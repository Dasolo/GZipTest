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

        private CompressionMode compressionMode;

        private int CalcBufferSize()
        {
            int maxBuffer = 1024 * 1024 * 200; //200mb
            return (Int32)((input.Length / maxThreadCount > maxBuffer) ? maxBuffer : input.Length / maxThreadCount);
        }

        private int GetLength(Stream input)
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
            this.zippers = new Dictionary<int, GZipThread>();
        }

        public void Start()
        {
            int BufferSize = CalcBufferSize();
            input.Seek(0, SeekOrigin.Begin);
            if (compressionMode == CompressionMode.Decompress)
            {
                BufferSize = GetLength(input);
            }
            var buffer = new byte[BufferSize];
            int i = 0;
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, BufferSize)) > 0)
            {
                zippers.Add(i, new GZipThread(i, buffer, bytesRead, threadsSemaphore, compressionMode));
                i++;
                if (compressionMode == CompressionMode.Decompress)
                {
                    BufferSize = GetLength(input);
                }
            }
            buffer = null;
            foreach (var zipper in zippers)
            {
                zipper.Value.Start();
            }
            WriteResults();
        }

        public void WriteResults()
        {
            int BufferSize = (int)input.Length / maxThreadCount;
            var buffer = new byte[BufferSize];
            int bytesRead;
            var zippersCount = zippers.Count;
            using (BufferedStream bufferStream = new BufferedStream(output))
            {
                for (var i = 0; i < zippersCount; i++)
                {
                    var zipper = zippers[i];
                    zipper.Wait();
                    var tempStream = zipper.resultStream;
                    tempStream.Seek(0, SeekOrigin.Begin);
                    while ((bytesRead = tempStream.Read(buffer, 0, BufferSize)) > 0)
                    {

                        if (compressionMode == CompressionMode.Compress)
                        {
                            var length = BitConverter.GetBytes(bytesRead);
                            bufferStream.Write(length, 0, sizeof(int));
                        }
                        bufferStream.Write(buffer, 0, bytesRead);

                    }
                    zippers.Remove(i);
                }
            }
        }

        public void Dispose()
        {
            zippers.Clear();
            input.Close();
            output.Close();
        }
    }
}
