using ModernModellingWarfare.Assets;
using ModernModellingWarfare.Shared;
using PhilLibX;
using System;
using System.IO;
using System.Reflection;

namespace ModernModellingWarfare
{
    internal unsafe class Program
    {
        /// <summary>
        /// Processes a Fast File
        /// </summary>
        static void ProcessFastFile(string file)
        {
            Printer.WriteLine("LOADER", $"Loading: {Path.GetFileName(file)}...");

            using var ffStream = new FastFileStream(file);

            using(var f = File.Create("test.dat"))
            {
                ffStream.CopyTo(f);
            }

            ffStream.Position = 0;

            using var zone = new Zone();

            try
            {
                zone.Load(ffStream);

                Printer.WriteLine("LOADER", $"Loaded {Path.GetFileName(file)}");

                Global.VerbosePrint($"Assets    : {zone.AssetList->AssetCount}");
                Global.VerbosePrint($"Strings   : {zone.AssetList->StringList.Count}");
                Global.VerbosePrint($"Memory    : {ffStream.Length}");

                foreach (string warning in zone.Warnings)
                    Global.VerbosePrint($"WARNING: {warning}");


                for (int i = 0; i < zone.AssetList->AssetCount; i++)
                {
                    if (zone.AssetList->Assets[i].Type == 9)
                    {
                        var xmodel = (XModelHandler.XModel*)zone.AssetList->Assets[i].Header;
                        XModelParser.ConvertLOD(zone, xmodel, xmodel->LodInfo0);
                    }
                    else if (zone.AssetList->Assets[i].Type == 7)
                    {
                        var xanim = (XAnimHandler.XAnim*)zone.AssetList->Assets[i].Header;
                        XAnimHandler.ConvertXAnim(zone, xanim);
                    }

                }
            }
#if DEBUG
            catch (Exception e)
            {
                zone.DumpZoneMemory();
                ffStream.Position = 0;
                using var w = File.Create("Test.dat");
                ffStream.CopyTo(w);

                Printer.WriteLine("ERROR", "An error has occured:", ConsoleColor.DarkRed);
                foreach (string split in e.ToString().Split('\n'))
                    Printer.WriteLine("ERROR", split);
#else
            catch
            {
#endif
                Printer.WriteLine("WARNING", "Failed to load Fast File.", ConsoleColor.DarkRed);
            }
        }

        static bool TryCopyOodleDLL()
        {
            var expectedOodleDLL = Path.Combine(Global.WorkingDirectory, "Oodle.dll");

            if (File.Exists(expectedOodleDLL))
                return true;

            var oodleDLL = Path.Combine(MaterialCacheHandler.GameDirectory, "oo2core_7_win64.dll");

            if(File.Exists(oodleDLL))
            {
                File.Copy(oodleDLL, expectedOodleDLL);
                return File.Exists(expectedOodleDLL);
            }

            return false;
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(Math.Min(128, Console.LargestWindowWidth), Math.Min(50, Console.LargestWindowHeight));

            Printer.WriteLine("INIT", $"--------------------------");
            Printer.WriteLine("INIT", $"ModernModellingWarfare");
            Printer.WriteLine("INIT", $"FastFast Model Export for Modern Warfare");
            Printer.WriteLine("INIT", $"Version {Assembly.GetExecutingAssembly().GetName().Version}");
            Printer.WriteLine("INIT", $"By Scobalula");
            Printer.WriteLine("INIT", $"Donate: paypal.me/Scobalula");
            Printer.WriteLine("INIT", $"--------------------------");

            if(!MaterialCacheHandler.Initialize())
            {
                Printer.WriteLine("WARNING", "Failed to initialize Material Handler. Image/Material export disabled.");
            }

            if(!TryCopyOodleDLL())
            {
                Printer.WriteLine("INIT", $"Failed to locate Oodle DLL, please ensure your game directory is set up!");
                Console.ReadLine();
                Environment.Exit(-100000);
            }

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    try
                    {
                        if (args[i] == "--imageformat")
                        {
                            MaterialCacheHandler.ImageExtension = args[i + 1];
                        }
                        else if (Directory.Exists(args[i]))
                        {
                            foreach (var dirFile in Directory.EnumerateFiles(args[i]))
                            {
                                if (Path.GetExtension(dirFile) == ".ff")
                                {
                                    ProcessFastFile(args[i]);
                                }
                            }
                        }
                        else
                        {
                            if (Path.GetExtension(args[i]) == ".ff")
                            {
                                ProcessFastFile(args[i]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Printer.WriteLine("ERROR", "An error has occured:", ConsoleColor.DarkRed);
                        foreach (string split in ex.ToString().Split('\n'))
                            Printer.WriteLine("ERROR", split);
                    }
                }
            }
            else
            {
                Printer.WriteLine("ERROR", "No files provided, see repo for usage info.", ConsoleColor.DarkRed);
            }

            Printer.WriteLine("DONE", "Execution complete, press Enter to exit");
            Console.ReadLine();
        }
    }
}
