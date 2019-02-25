namespace GZipTest
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;

    internal class GZipThread : IDisposable
    {
        private Thread Thread;

        private int Number;

        private byte[] inputData;

        public Stream resultStream;

        private int bytesCount;

        private Boolean isZipped;

        private CompressionMode compressionMode;

        private Semaphore semaphore;

        private Semaphore threadsSemaphore;

        private void ThreadProc()
        {
            if (compressionMode == CompressionMode.Compress)
            {
                using (var compressor = new GZipStream(resultStream, compressionMode, true))
                {
                    compressor.Write(inputData, 0, bytesCount);
                    Console.WriteLine("Было {0} стало {1} {2} {3}", bytesCount, resultStream.Length, compressionMode, Number);
                }

            }
            else
            {
                using (var inputStream = new MemoryStream(inputData))
                {
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
            this.isZipped = true;
            semaphore.Release();
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
            this.isZipped = false;
            this.semaphore = new Semaphore(1, 1);
        }

        public void Start()
        {
            threadsSemaphore.WaitOne();
            semaphore.WaitOne();
            this.Thread.Start();
        }

        public void Wait()
        {
            while (!isZipped)
            {
                Thread.Sleep(100);
            }
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
