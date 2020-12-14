using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace MyIcGen
{
    public class Options
    {
        [Option("no-hyphen", Required = false, HelpText = "Remove hyphen (-) in output")]
        public bool noHyphen { get; set; }
        
        [Option("no-split", Required = false, HelpText = "Disable split and zip. Output will be generated in a single file")]
        public bool noSplit { get; set; }
        
        [Option('o',"out", Required = true, HelpText = "Output filename")]
        public string filename { get; set; }
        
        [Option("min-year", HelpText = "Min year", Default = 0)]
        public int yStart { get; set; }
        
        [Option("max-year", HelpText = "Max year", Default = 99)]
        public int yEnd { get; set; }
        
        [Option("min-month", HelpText = "Min month", Default = 1)]
        public int mStart { get; set; }
        
        [Option("max-month", HelpText = "Max month", Default = 12)]
        public int mEnd { get; set; }
        
        [Option("min-day", HelpText = "Min day", Default = 1)]
        public int dStart { get; set; }
        
        [Option("max-day", HelpText = "Max day", Default = 31)]
        public int dEnd { get; set; }
        
        [Option("min-special", HelpText = "Special number of IC (last 4 digit) min", Default = 0)]
        public int sStart { get; set; }
        
        [Option("max-special", HelpText = "Special number of IC (last 4 digit) max", Default = 9999)]
        public int sEnd { get; set; }

        [Option("place-of-birth", Separator=',', HelpText = "(Default list included) Place of birth (2 digits). Seperated by comma. E.g. 01,10,14,99")]
        public IEnumerable<string> pb { get; set; }
        
        [Option("lines", HelpText = "Number of lines processed in memory before write to output", Default = 50000000)]
        public int numLines { get; set; }
        
        [Usage(ApplicationAlias = "MyIcGen")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Generate list of Malaysian IC", new Options { filename = "out.txt" })
                };
            }
        }
    }
    
    class Program
    {
        //in case of errors or --help or --version
        static int HandleParseError(IEnumerable<Error> errs)
        {
            var result = -2;
            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
                result = -1;
            return result;
        }
        
        static string[] pbArray = new[] {
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

        static int Run(Options o)
        {
            if (o.pb.Any())
                pbArray = o.pb.ToArray();
            
            Console.WriteLine("[MyIcGen]");
            Console.WriteLine($"No hyphen \"-\": {o.noHyphen}");
            Console.WriteLine($"No file split: {o.noSplit}");
            Console.WriteLine($"Process every # of lines: {o.numLines}");
            Console.WriteLine($"Year: {o.yStart}-{o.yEnd}");
            Console.WriteLine($"Month: {o.mStart}-{o.mEnd}");
            Console.WriteLine($"Day: {o.dStart}-{o.dEnd}");
            Console.WriteLine($"Place of Birth (PB): {String.Join(",", pbArray)}");
            Console.WriteLine($"Special number: {o.sStart}-{o.sEnd}");
            Console.WriteLine($"Output file: {o.filename}");

            Array.Sort(pbArray);
            
            // first part YYMMDD
            // DEFAULTS
            // List<string> tahun = GenerateBetweenNumber(0, 99, "0#");
            // List<string> bulan = GenerateBetweenNumber(1, 12, "0#");
            // List<string> hari = GenerateBetweenNumber(1, 31, "0#");
            // List<string> ending = GenerateBetweenNumber(0, 9999, "000#");
            List<string> tahun = GenerateBetweenNumber(o.yStart, o.yEnd, "0#");
            List<string> bulan = GenerateBetweenNumber(o.mStart, o.mEnd, "0#");
            List<string> hari = GenerateBetweenNumber(o.dStart, o.dEnd, "0#");

            // last part NNNN
            List<string> ending = GenerateBetweenNumber(o.sStart, o.sEnd, "000#");

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
                        if (!o.noHyphen)
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
                    if (!o.noHyphen)
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

                    if (full.Count >= o.numLines)
                    {
                        WriteToFile(ref fileCount, full, fulltotal, ref fullcount, o);
                        stopwatch.Stop();
                        Console.WriteLine($"Complete write to output file in {stopwatch.Elapsed.TotalSeconds} s");
                        stopwatch.Restart();
                    }
                }
            }
            
            if ((fulltotal - fullcount) > 0)  
                WriteToFile(ref fileCount, full, fulltotal, ref fullcount, o);
            
            Console.WriteLine($"Done. Total generated {fullcount} in {timeTaken.Elapsed.TotalSeconds} s.");
            return 0;
        }

        private static void WriteToFile(ref int fileCount, List<string> full, long fulltotal, ref long fullcount, 
            Options o)
        {
            if (!o.noSplit)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        var demoFile = archive.CreateEntry($"{fileCount}-{o.filename}", CompressionLevel.Optimal);

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

                    using (var fileStream = new FileStream($"{fileCount}-{o.filename}.zip", FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                        ++fileCount;
                    }
                }
            }
            else
            {
                File.AppendAllLines($"{o.filename}", full);
            }

            fullcount += full.Count;
            full.Clear();
            Decimal percent = (Convert.ToDecimal(fullcount) / Convert.ToDecimal(fulltotal)) * 100;
            Console.WriteLine($"Progress {percent.ToString("00.##")}% | Current lines {fullcount} | Total lines {fulltotal} | Available {fulltotal - fullcount}");
        }
        
        static List<string> GenerateBetweenNumber(int min, int max, string format)
        {
            if (max < min)
                throw new Exception($"Invalid range. Max {max} is lower than min {min}.");
            
            List<string> generatedNumbers = new List<string>();
            
            for (int i = min; i <= max; i++)
            {
                generatedNumbers.Add(i.ToString(format));
            }
            
            return generatedNumbers;
        }
        
        static void Main(string[] args)
        {
            Environment.Exit(Parser.Default.ParseArguments<Options>(args).MapResult(Run, HandleParseError));
        }
    }
}