namespace GZipTest
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    internal class ThreadedGZip
    {
        private Stream input;

        private Stream output;

        private Dictionary<int, GZipThread> zippers;

        public ThreadedGZip(Stream _input, Stream _output)
        {
            this.input = _input;
            this.output = _output;
            this.zippers = new Dictionary<int, GZipThread>();
        }

        public void Start(int threadCount, CompressionMode compressionMode)
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
            foreach (var zipper in zippers)
            {
                zipper.Value.Wait();
            }
        }
    }
}
