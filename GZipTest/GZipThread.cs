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

        private BinaryWriter output;

        public GZipThread previous;

        internal int bytesCount;

        private int offset;
        private Boolean isZipped; 
        

        internal CompressionMode compressionMode;

        internal Semaphore semaphore;

        private Semaphore threadsSemaphore;

        private void Write()
        {
            var BufferSize = 1024 * 1024 * 200;
            var buffer = new byte[BufferSize];
            int bytesRead;
            if (previous != null)
                offset = previous.offset;
            else
                offset = 0;
            resultStream.Seek(0, SeekOrigin.Begin);
            while ((bytesRead = resultStream.Read(buffer, 0, BufferSize)) > 0)
            {
                output.Seek(offset, SeekOrigin.Current);
                if (compressionMode == CompressionMode.Compress)
                {
                    var length = BitConverter.GetBytes(bytesRead);
                    output.Write(length, 0, sizeof(int));
                    offset += sizeof(int);
                }
                output.Write(buffer, 0, bytesRead);
                offset += bytesRead;
                output.Flush();
                Console.WriteLine("запись поток номер {0} {1}", Number, bytesRead);
            }
            threadsSemaphore.Release();
        }

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
            if (previous != null)
                previous.Wait();
            Write();
            this.isZipped = true;
        }

        public GZipThread(int _number, byte[] _inputData, int _bytesCount, Semaphore _threadsSemaphore, GZipThread _previous, BinaryWriter _output, CompressionMode _compressionMode)
        {
            this.Thread = new Thread(new ThreadStart(ThreadProc));
            this.Number = _number;
            this.inputData = _inputData;
            this.compressionMode = _compressionMode;
            this.resultStream = new MemoryStream();
            this.bytesCount = _bytesCount;
            this.threadsSemaphore = _threadsSemaphore;
            this.previous = _previous;
            this.output = _output;
            this.isZipped = false;
            this.semaphore = new Semaphore(1, 1);
        }

        public void Start()
        {
            threadsSemaphore.WaitOne();
            this.Thread.Start();
        }

        public void Wait()
        {
            while (!isZipped)
            {
                Thread.Sleep(100);
            }
        }

        public void Dispose()
        {
            resultStream.Dispose();
            inputData = null;
        }
    }
}
