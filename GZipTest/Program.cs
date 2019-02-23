using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = @"c:\temp\test.bin";
            var outputFile = @"c:\temp\test.zip";
            var outFile = File.Create(outputFile);
            var inFile = new FileStream(inputFile, FileMode.Open);
            var Gzip = new ThreadedGZip(inFile, outFile);
            Gzip.CopyStreams();
            outFile.Close();
            inFile.Close();
        }
 
    }

}
