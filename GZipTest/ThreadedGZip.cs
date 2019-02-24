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
            int BufferSize = (int)input.Length / threadCount;
            var buffer = new byte[BufferSize];
            input.Seek(0, SeekOrigin.Begin);
            int i = 0;
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, BufferSize)) > 0)
            {
                zippers.Add(i, new GZipThread(i, buffer, bytesRead, compressionMode));
                i++;
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
                    output.Write(buffer, 0, bytesRead);
                }                
            }
        }
    }
}
