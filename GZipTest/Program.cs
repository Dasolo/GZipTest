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
            var inputFile = @"c:\temp\test.bin";
            var outputFile = @"c:\temp\test.gz";
            var outfileDec = @"c:\temp\test1.bin";
            var outFile = File.Create(outputFile);
            var doutFile = File.Create(outfileDec);
            var inFile = new FileStream(inputFile, FileMode.Open);
            var Gzip = new ThreadedGZip(inFile, outFile, 12, CompressionMode.Compress);
            var sw = Stopwatch.StartNew();
            Gzip.Start();
            Console.WriteLine("All end {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            var unGzip = new ThreadedGZip(outFile, doutFile, 3, CompressionMode.Decompress);
            unGzip.Start();

            Console.WriteLine("All end {0}", sw.ElapsedMilliseconds);
            Console.ReadLine();
            outFile.Close();
            inFile.Close();
        }
    }
}
