namespace GZipTest
{
    using System;
    using System.Threading;
    using System.IO;
    using System.IO.Compression;

    internal class GZipThread: IDisposable
    {
        internal Thread Thread;

        internal int Number;

        internal byte[] inputData;

        public Stream resultStream;

        internal int bytesCount;


        internal CompressionMode compressionMode;

        internal Semaphore semaphore;

        private Semaphore threadsSemaphore;

        private void ThreadProc()
        {
            threadsSemaphore.WaitOne();
            if (compressionMode == CompressionMode.Compress)
            {
                var compressor = new GZipStream(resultStream, compressionMode);
                compressor.Write(inputData, 0, bytesCount);    
            }
            else
            {
                using (var inputStream = new MemoryStream(inputData)) {
                    using (var decompressor = new GZipStream(inputStream, compressionMode))
                    {
                        var buffer = new byte[bytesCount];
                        int bytesRead;
                        while ((bytesRead = decompressor.Read(buffer, 0, bytesCount)) > 0)
                        {
                            resultStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    inputStream.Close();
                }
            }
            semaphore.Release();
            threadsSemaphore.Release();
        }

        public GZipThread(int _number, byte[] _inputData, int _bytesCount, Semaphore _threadsSemaphore, CompressionMode _compressionMode)
        {
            this.Thread = new Thread(new ThreadStart(ThreadProc));
            this.Number = _number;
            this.inputData = _inputData;
            this.compressionMode = _compressionMode;
            this.resultStream = new MemoryStream();
            this.bytesCount = _bytesCount;
            this.threadsSemaphore = _threadsSemaphore;
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

        public void Dispose()
        {
            resultStream.Dispose();
            inputData = null;
        }
    }
}
