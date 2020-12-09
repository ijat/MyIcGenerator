using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MyIcGen
{
    class Program
    {
        static bool isDash = true;
        static bool isSplitAndCompressed = true;
        private static int numOfLines = 50000000;
        static string outname = "ic-gen.txt";
        
        static void Main(string[] args)
        {
            
            Console.WriteLine("[MyIcGen]");
            isDash = !(args.Length > 0 && args.Contains("--no-hyphen"));
            isSplitAndCompressed = !(args.Length > 0 && args.Contains("--no-split"));
            if (args.Length > 0 && !String.IsNullOrEmpty(args.Last()))
                outname = args.Last();

            Console.WriteLine($"With hyphen \"-\": {isDash}");
            Console.WriteLine($"With file split: {isSplitAndCompressed}");
            Console.WriteLine($"Process every # of lines: {numOfLines}");
            Console.WriteLine($"Output file: {outname}");

            string[] pbArray = new[] {
                "01",
                "02",
                "03",
                "04",
                "05",
                "06",
                "07",
                "08",
                "09",
                "10",
                "11",
                "12",
                "13",
                "14",
                "15",
                "16",
                "19",
                "21",
                "22",
                "23",
                "24",
                "25",
                "26",
                "27",
                "28",
                "29",
                "30",
                "31",
                "32",
                "33",
                "34",
                "35",
                "36",
                "37",
                "38",
                "39",
                "40",
                "41",
                "42",
                "43",
                "44",
                "45",
                "46",
                "47",
                "48",
                "49",
                "50",
                "51",
                "52",
                "53",
                "54",
                "55",
                "56",
                "57",
                "58",
                "59",
                "60",
                "61",
                "62",
                "63",
                "64",
                "65",
                "66",
                "67",
                "68",
                "71",
                "72",
                "74",
                "75",
                "76",
                "77",
                "78",
                "79",
                "82",
                "83",
                "84",
                "85",
                "86",
                "87",
                "88",
                "89",
                "90",
                "91",
                "92",
                "93",
                "98",
                "99",
            };
            
            Array.Sort(pbArray);
            
            // first part YYMMDD
            // DEFAULTS
            // List<string> tahun = GenerateBetweenNumber(0, 99, "0#");
            // List<string> bulan = GenerateBetweenNumber(1, 12, "0#");
            // List<string> hari = GenerateBetweenNumber(1, 31, "0#");
            // List<string> ending = GenerateBetweenNumber(0, 9999, "000#");
            List<string> tahun = GenerateBetweenNumber(0, 99, "0#");
            List<string> bulan = GenerateBetweenNumber(1, 12, "0#");
            List<string> hari = GenerateBetweenNumber(1, 31, "0#");

            // last part NNNN
            List<string> ending = GenerateBetweenNumber(0, 9999, "000#");

            List<string> fullbd = new List<string>();
            // generate string birthday
            foreach (var yy in tahun)
            {
                string fullyy = "";
                fullyy += yy;
                foreach (var mm in bulan)
                {
                    string fullmm = "";
                    fullmm += fullyy + mm;
                    foreach (var dd in hari)
                    {
                        string fulldd = "";
                        fulldd += fullmm + dd;
                        if (isDash)
                            fulldd += "-";
                        fullbd.Add(fulldd);
                    }
                }
            }

            List<string> fullbdpb = new List<string>();
            foreach (var tbd in fullbd)
            {
                foreach (var tpb in pbArray)
                {
                    string pb = tpb;
                    if (isDash)
                        pb += "-";
                    fullbdpb.Add(tbd + pb);
                }
            }

            long fulltotal = (long)fullbdpb.Count * (long)ending.Count;
            Console.WriteLine($"Total lines = {fulltotal}");
            Console.WriteLine($"Processing...");
            long fullcount = 0;
            int fileCount = 1;
            List<string> full = new List<string>();
            
            Stopwatch timeTaken = new Stopwatch();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var tbdpb in fullbdpb)
            {
                foreach (var tend in ending)
                {
                    full.Add(tbdpb + tend);

                    if (full.Count >= numOfLines)
                    {
                        WriteToFile(ref fileCount, full, fulltotal, ref fullcount);
                        stopwatch.Stop();
                        Console.WriteLine($"Complete write to output file in {stopwatch.Elapsed.TotalSeconds} s");
                        stopwatch.Restart();
                    }
                }
            }
            
            if ((fulltotal - fullcount) > 0)  
                WriteToFile(ref fileCount, full, fulltotal, ref fullcount);
            
            Console.WriteLine($"Done. Total generated {fullcount} in {timeTaken.Elapsed.TotalSeconds} s.");
        }

        private static void WriteToFile(ref int fileCount, List<string> full, long fulltotal, ref long fullcount)
        {
            if (isSplitAndCompressed)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        var demoFile = archive.CreateEntry($"{fileCount}-{outname}", CompressionLevel.Optimal);

                        using (var entryStream = demoFile.Open())
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            foreach (var ic in full)
                            {
                                streamWriter.WriteLine(ic);
                            }

                            streamWriter.Flush();
                        }
                    }

                    using (var fileStream = new FileStream($"{fileCount}-{outname}.zip", FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                        ++fileCount;
                    }
                }
            }
            else
            {
                File.AppendAllLines($"{outname}", full);
            }

            fullcount += full.Count;
            full.Clear();
            Decimal percent = (Convert.ToDecimal(fullcount) / Convert.ToDecimal(fulltotal)) * 100;
            Console.WriteLine($"Progress {percent.ToString("00.##")}% | Current lines {fullcount} | Total lines {fulltotal} | Available {fulltotal - fullcount}");
        }

        // tahun
        static List<string> GenerateBetweenNumber(int min, int max, string format)
        {
            List<string> generatedNumbers = new List<string>();
            
            for (int i = min; i <= max; i++)
            {
                generatedNumbers.Add(i.ToString(format));
            }
            
            return generatedNumbers;
        }
        
        
    }
}