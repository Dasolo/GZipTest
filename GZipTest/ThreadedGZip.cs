namespace GZipTest
{
    using System.Collections.Generic;
    using System.IO;

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

        public void Start(int threadCount)
        {
            for (var i = 0; i < threadCount; i++)
            {
                zippers.Add(i, new GZipThread(i));
            }

            foreach (var zipper in zippers)
            {
                zipper.Value.Start();
            }
        }
    }
}
