using System;
using System.IO;

namespace GZipTest
{
    class ThreadedGZip
    {
        private Stream Input;
        private Stream Output;
        static int BufferSize = 1024 * 1024 * 20;
        static void Copy(Stream input, Stream output)
        {
            var buffer = new byte[BufferSize];
            int bytesRead;
            input.Seek(0, SeekOrigin.Begin);
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }


        public ThreadedGZip(Stream _input, Stream _output)
        {
            this.Input = _input;
            this.Output = _output;
        }

        public void CopyStreams()
        {
            Copy(Input, Output);
        } 
    }
}
