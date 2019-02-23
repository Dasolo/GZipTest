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
            var outputFile = @"c:\temp\test.zip";
            var outFile = File.Create(outputFile);
            var inFile = new FileStream(inputFile, FileMode.Open);
            var Gzip = new ThreadedGZip(inFile, outFile, 20);
            var sw = Stopwatch.StartNew();
            Gzip.Start(CompressionMode.Compress);
            Gzip.WaitAll();
            Console.WriteLine("All end {0}", sw.ElapsedMilliseconds);
            Console.ReadLine();
            outFile.Close();
            inFile.Close();
        }
    }
}
