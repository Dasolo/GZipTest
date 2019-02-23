namespace GZipTest
{
    using System.IO;

    internal class ThreadedGZip
    {
        private Stream input;

        private Stream output;

        internal static int BufferSize = 1024 * 1024 * 20;

        internal static void Copy(Stream input, Stream output)
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
            this.input = _input;
            this.output = _output;
        }

        public void CopyStreams()
        {
            Copy(input, output);
        }
    }
}
