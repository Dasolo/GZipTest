namespace GZipTest
{
    using System;
    using System.Threading;

    internal class GZipThread
    {
        internal Thread Thread;

        internal int Number;

        private void ThreadProc()
        {
            Thread.Sleep(1);
            Console.WriteLine("thread {0} work", Number);
        }

        public GZipThread(int _number)
        {
            this.Thread = new Thread(new ThreadStart(ThreadProc));
            this.Number = _number;
        }

        public void Start()
        {
            this.Thread.Start();
        }
    }
}
