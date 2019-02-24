namespace GZipTest
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;

    internal class Program
    {
        internal static void Main(string[] args)
        {
            var command = args[0];
            var input = args[1];
            var output = args[2];

            var inFile = new FileStream(input, FileMode.Open);
            var outFile = File.Create(output);
            CompressionMode compressionMode;
            if (command == "compress")
                compressionMode = CompressionMode.Compress;
            else if (command == "decompress") 
                compressionMode = CompressionMode.Decompress;
            else
            {
                Console.WriteLine("Gzip compress — сжатие");
                Console.WriteLine("Gzip decompress — распаковка");
                return;
            }
            using (var Gzip = new ThreadedGZip(inFile, outFile, 8, compressionMode))
                {
                    Gzip.Start();
                }

        }
    }
}
