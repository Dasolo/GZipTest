namespace GZipTest
{
    using System.IO;
    using System.IO.Compression;
    using System.Threading;

    internal class GZipThread
    {
        internal Thread Thread;

        internal int Number;

        internal byte[] inputData;

        internal Stream _resultStream;

        internal int bytesCount;

        public Stream resultStream
        {
            get { return _resultStream; }
            set { _resultStream = value; }
        }

        internal CompressionMode compressionMode;

        internal Semaphore semaphore;

        private void ThreadProc()
        {
            if (compressionMode == CompressionMode.Compress)
            {
                var compressor = new GZipStream(_resultStream, compressionMode);
                compressor.Write(inputData, 0, bytesCount);
            }
            else
            {
                var inputStream = new MemoryStream(inputData);
                var decompressor = new GZipStream(inputStream, compressionMode);
                var buffer = new byte[bytesCount];
                int bytesRead;
                while ((bytesRead = decompressor.Read(buffer, 0, bytesCount)) > 0)
                {
                    _resultStream.Write(buffer, 0, bytesRead);
                }
            }
            semaphore.Release();
        }

        public GZipThread(int _number, byte[] _inputData, int _bytesCount, CompressionMode _compressionMode)
        {
            this.Thread = new Thread(new ThreadStart(ThreadProc));
            this.Number = _number;
            this.inputData = _inputData;
            this.compressionMode = _compressionMode;
            this._resultStream = new MemoryStream();
            this.bytesCount = _bytesCount;
            this.semaphore = new Semaphore(1, 1);
        }

        public void Start()
        {
            semaphore.WaitOne();
            this.Thread.Start();
        }

        public void Wait()
        {
            semaphore.WaitOne();
            semaphore.Release();
        }
    }
}
