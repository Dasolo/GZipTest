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

        public ThreadedGZip(Stream _input, Stream _output, int _threadCount)
        {
            this.input = _input;
            this.output = _output;
            this.threadCount = _threadCount;
            this.zippers = new Dictionary<int, GZipThread>();
        }

        public void Start(CompressionMode compressionMode)
        {
            int BufferSize = (int)input.Length / threadCount;
            var buffer = new byte[BufferSize];
            input.Seek(0, SeekOrigin.Begin);
            int i = 0;
            while (input.Read(buffer, 0, BufferSize) > 0)
            {
                zippers.Add(i, new GZipThread(i, buffer, compressionMode));
                i++;
            }

            foreach (var zipper in zippers)
            {
                zipper.Value.Start();
            }
        }

        public void WaitAll()
        {
            int BufferSize = (int)input.Length / threadCount;
            var buffer = new byte[BufferSize];
            int bytesRead;
            int offset = 0;
            foreach (var zipper in zippers)
            {
                zipper.Value.Wait();
                var tempStream = zipper.Value.resultStream;
                tempStream.Seek(0, SeekOrigin.Begin);
                while ((bytesRead = tempStream.Read(buffer, 0, BufferSize)) > 0)
                {
                    output.Write(buffer, offset, bytesRead);
                    offset += bytesRead;
                    Console.WriteLine("Байтов записано {0}", bytesRead);
                }                
            }
        }
    }
}
