namespace GZipTest
{
    using System.IO;

    internal class Program
    {
        internal static void Main(string[] args)
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
