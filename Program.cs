using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace MyIcGen
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isDash = true;
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
            long fullcount = 0;
            int fileCount = 1;
            List<string> full = new List<string>();
            foreach (var tbdpb in fullbdpb)
            {
                foreach (var tend in ending)
                {
                    full.Add(tbdpb + tend);

                    if (full.Count >= 50000000)
                    {
                        WriteToFile(ref fileCount, full, fulltotal, ref fullcount);
                    }
                }
            }
            
            WriteToFile(ref fileCount, full, fulltotal, ref fullcount);
            fullcount += full.Count;
            Decimal percent = (Convert.ToDecimal(fullcount) / Convert.ToDecimal(fulltotal)) * 100;
            Console.WriteLine($"Percent {percent.ToString("00.##")}%");

            Console.WriteLine($"Done. Total generated {fullcount}.");
        }

        private static void WriteToFile(ref int fileCount, List<string> full, long fulltotal, ref long fullcount)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = archive.CreateEntry($"ic-generated-{fileCount}.txt", CompressionLevel.Optimal);

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

                using (var fileStream = new FileStream($"ic-generated-{fileCount}.zip", FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                    ++fileCount;
                }
            }

            fullcount += full.Count;
            full.Clear();
            Decimal tpercent = (Convert.ToDecimal(fullcount) / Convert.ToDecimal(fulltotal)) * 100;
            Console.WriteLine($"Percent {tpercent.ToString("00.0000#")}%");
        }

        // tahun
        static List<string> GenerateBetweenNumber(int min, int max, string format)
        {
            //int count = 0;
            List<string> generatedNumbers = new List<string>();
            
            for (int i = min; i <= max; i++)
            {
                generatedNumbers.Add(i.ToString(format));
                //++count;
            }
            //generatedNumbers.ForEach(Console.WriteLine);
            //Console.WriteLine($"Total count: {count}");
            return generatedNumbers;
        }
    }
}