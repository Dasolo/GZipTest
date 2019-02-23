using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace GZipTest
{
    class Program
    {
        static int BufferSize = 1024 * 1024 * 20; //20mb

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

        static void Main(string[] args)
        {
            var inputFile = @"c:\temp\test.bin";
            var outputFile = @"c:\temp\test.zip";
            Stopwatch sw = Stopwatch.StartNew();
            var outFile = File.Create(outputFile);
            var inFile = new FileStream(inputFile, FileMode.Open);
            var innerStream = new MemoryStream();
            var innerStream2 = new MemoryStream();
            Console.WriteLine(sw.ElapsedMilliseconds);
            var zipper = new GZipStream(innerStream, CompressionMode.Compress);
            var fileZipper = new GZipStream(outFile, CompressionMode.Compress);
            sw.Reset();
            sw.Start();
            Copy(inFile, outFile);
            Console.WriteLine("запись из файла в файл {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            Copy(inFile, innerStream);
            Console.WriteLine("запись из файла в поток {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            Copy(innerStream, innerStream2);
            Console.WriteLine("запись из потока в поток {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            Copy(inFile, zipper);
            Console.WriteLine("упакова из файла в поток {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            Copy(inFile, fileZipper);
            Console.WriteLine("упаковка из файла в файл {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            Copy(innerStream, zipper);
            Console.WriteLine("упаковка из потока в поток {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            Copy(innerStream, fileZipper);
            Console.WriteLine("упаковка из потока в файл {0}", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            zipper.Close();
            outFile.Close();
            inFile.Close();
            Console.ReadLine();

        }
 
    }

}
