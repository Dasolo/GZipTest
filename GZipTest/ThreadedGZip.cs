namespace GZipTest
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    internal class ThreadedGZip: IDisposable
    {
        private Stream input;

        private Stream output;

        private Dictionary<int, GZipThread> zippers;

        private int maxThreadCount;

        private Semaphore threadsSemaphore;

        private Semaphore writeSemaphore;

        private CompressionMode compressionMode;

        private long inputLength;

        private int CalcBufferSize()
        {
            int maxBuffer = 1024 * 1024 * 200; //200mb
            return (Int32)((inputLength / maxThreadCount > maxBuffer) ? maxBuffer : inputLength / maxThreadCount);
        }

        private int GetLength(BinaryReader input)
        {
            var length = new byte[sizeof(int)];
            input.Read(length, 0, sizeof(int));
            return BitConverter.ToInt32(length, 0);
        }

        public ThreadedGZip(Stream _input, Stream _output, int _maxThreadCount, CompressionMode _compressionMode)
        {
            this.input = _input;
            this.output = _output;
            this.maxThreadCount = _maxThreadCount;
            this.threadsSemaphore = new Semaphore(maxThreadCount, maxThreadCount);
            this.writeSemaphore = new Semaphore(1, 1);
            this.compressionMode = _compressionMode;
            this.inputLength = input.Length;
            this.zippers = new Dictionary<int, GZipThread>();
        }

        public void Start()
        {

            int BufferSize = CalcBufferSize();
            input.Seek(0, SeekOrigin.Begin);
            int i = 0;
            int bytesRead;
            using (BinaryReader binaryReader = new BinaryReader(input))
            {
                if (compressionMode == CompressionMode.Decompress)
                {
                    BufferSize = GetLength(binaryReader);
                }
                var buffer = new byte[BufferSize];
                while ((bytesRead = binaryReader.Read(buffer, 0, BufferSize)) > 0)
                {
                    zippers.Add(i, new GZipThread(i, buffer, bytesRead, threadsSemaphore, compressionMode));

                        
                    i++;
                    if (compressionMode == CompressionMode.Decompress)
                    {
                        BufferSize = GetLength(binaryReader);
                    }
                    Console.WriteLine("Начинаем читать {0}", i);
                }
            }
            new Thread(WriteResults).Start();
            var zippersCount = zippers.Count;
            for(var j = 0; j < zippersCount; j++)
            {
                zippers[j].Start();
            }
            writeSemaphore.WaitOne();
            writeSemaphore.Release();
        }

        public void WriteResults()
        {
            writeSemaphore.WaitOne();
            var BufferSize = CalcBufferSize();
            var buffer = new byte[BufferSize];
            int bytesRead;
            var zippersCount = zippers.Count;
            BinaryWriter binaryWriter = new BinaryWriter(output);
            for(var i = 0; i < zippersCount; i++)
            { 
                zippers[i].Wait();
                var tempStream = zippers[i].resultStream;
                tempStream.Seek(0, SeekOrigin.Begin);
                    while ((bytesRead = tempStream.Read(buffer, 0, BufferSize)) > 0)
                    {
                        if (compressionMode == CompressionMode.Compress)
                        {
                            var length = BitConverter.GetBytes(bytesRead);
                            binaryWriter.Write(length, 0, sizeof(int));
                        }
                        binaryWriter.Write(buffer, 0, bytesRead);
                        Console.WriteLine("запись поток номер {0}", i);
                    }
                binaryWriter.Flush();
                zippers[i].Dispose();
                zippers.Remove(i);
                threadsSemaphore.Release();
            }
            writeSemaphore.Release();
        }

        public void Dispose()
        {
            zippers.Clear();
            input.Close();
            output.Close();
        }
    }
}
