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
            var sw = Stopwatch.StartNew();
            using (var Gzip = new ThreadedGZip(inFile, outFile, 15, CompressionMode.Compress))
            {
                Gzip.Start();
            }
            Console.ReadLine();
            Console.WriteLine("All end {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            var dinFile = new FileStream(outputFile, FileMode.Open);
            using (var unGzip = new ThreadedGZip(dinFile, doutFile, 4, CompressionMode.Decompress))
            {
                unGzip.Start();
            }
            outFile.Close();
            inFile.Close();
            dinFile.Close();
            doutFile.Dispose();
            Console.WriteLine("All end {0}", sw.ElapsedMilliseconds);
            Console.ReadLine();

        }
    }
}
