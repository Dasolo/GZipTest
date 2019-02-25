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

        private int maxThreadCount;

        private Semaphore threadsSemaphore;

        private CompressionMode compressionMode;

        private long inputLength;

        private int CalcBufferSize()
        {
            int maxBuffer = 1024 * 1024 * 20; //200mb
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
            this.compressionMode = _compressionMode;
            this.inputLength = input.Length;
        }

        public void Start()
        {

            int BufferSize = CalcBufferSize();
            input.Seek(0, SeekOrigin.Begin);
            output.Seek(0, SeekOrigin.Begin);
            int i = 0;
            int bytesRead;
            GZipThread zipper = null;
            using (var binaryWriter = new BinaryWriter(output))
            {
                using (BinaryReader binaryReader = new BinaryReader(input))
                {
                    if (compressionMode == CompressionMode.Decompress)
                    {
                        BufferSize = GetLength(binaryReader);
                    }
                    var buffer = new byte[BufferSize];

                    while ((bytesRead = binaryReader.Read(buffer, 0, BufferSize)) > 0)
                    {
                        zipper = new GZipThread(i++, buffer, bytesRead, threadsSemaphore, zipper, binaryWriter, compressionMode);
                        Console.WriteLine("Старт потока {0}", i - 1);
                        zipper.Start();

                        if (compressionMode == CompressionMode.Decompress)
                        {
                            BufferSize = GetLength(binaryReader);
                        }
                        Console.WriteLine("Начинаем читать {0}", i - 1);
                    }
                }
                zipper.Wait();
                binaryWriter.Flush();
            }
            Console.ReadLine();
        }


        public void Dispose()
        {
            input.Close();
            output.Close();
        }
    }
}
