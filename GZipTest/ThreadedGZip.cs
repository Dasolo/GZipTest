namespace GZipTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    internal class ThreadedGZip
    {
        private Stream input;

        private Stream output;

        private Dictionary<int, GZipThread> zippers;

        private int threadCount;

        private CompressionMode compressionMode;

        private int CalcBufferSize()
        {
            int maxBuffer = Int32.MaxValue - 1;
            return (Int32)((input.Length / threadCount > maxBuffer) ? maxBuffer : input.Length / threadCount);
        }

        private int GetLength(Stream input)
        {
            var length = new byte[sizeof(int)];
            input.Read(length, 0, sizeof(int));
            return BitConverter.ToInt32(length, 0);
        }

        public ThreadedGZip(Stream _input, Stream _output, int _threadCount, CompressionMode _compressionMode)
        {
            this.input = _input;
            this.output = _output;
            this.threadCount = _threadCount;
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
                zippers.Add(i, new GZipThread(i, buffer, bytesRead, compressionMode));
                i++;
                if (compressionMode == CompressionMode.Decompress)
                {
                    BufferSize = GetLength(input);
                }
            }

            foreach (var zipper in zippers)
            {
                zipper.Value.Start();
            }
            WriteResults();
        }

        public void WriteResults()
        {
            int BufferSize = (int)input.Length / threadCount;
            var buffer = new byte[BufferSize];
            int bytesRead;
            foreach (var zipper in zippers)
            {
                zipper.Value.Wait();
                var tempStream = zipper.Value.resultStream;
                tempStream.Seek(0, SeekOrigin.Begin);
                while ((bytesRead = tempStream.Read(buffer, 0, BufferSize)) > 0)
                {
                    if (compressionMode == CompressionMode.Compress)
                    {
                        var length = BitConverter.GetBytes(bytesRead);
                        output.Write(length, 0, sizeof(int));
                    }
                    output.Write(buffer, 0, bytesRead);
                }                
            }
        }
    }
}
