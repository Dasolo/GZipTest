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

        internal Stream resultStream;

        internal CompressionMode compressionMode;

        internal Mutex mutex;

        private void ThreadProc()
        {
            mutex.WaitOne();
            using (var compressor = new GZipStream(resultStream, compressionMode))
            {
                compressor.Write(inputData, 0, inputData.Length);
            }
            mutex.ReleaseMutex();
        }

        public GZipThread(int _number, byte[] _inputData, CompressionMode _compressionMode)
        {
            this.Thread = new Thread(new ThreadStart(ThreadProc));
            this.Number = _number;
            this.inputData = _inputData;
            this.compressionMode = _compressionMode;
            this.resultStream = new MemoryStream();
            this.mutex = new Mutex();
        }

        public void Start()
        {
            this.Thread.Start();
        }

        public void Wait()
        {
            mutex.WaitOne();
            mutex.ReleaseMutex();
        }
    }
}
