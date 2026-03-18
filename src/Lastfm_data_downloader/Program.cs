using Lastfm_data_downloader.Porter_Packages.Madscience_CommandLineSwitches;

namespace Lastfm_data_downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            {
                CommandLineSwitches switches = new CommandLineSwitches(args);

                Console.WriteLine("Lastfm downloader");

                if (switches.InvalidArguments.Any())
                {
                    Console.WriteLine("ERROR : invalid switch(es):");
                    foreach(var r in switches.InvalidArguments)
                        Console.WriteLine(r);

                    System.Environment.Exit(1);
                }

                Console.WriteLine("args:" + String.Join(",", switches.Arguments));
                
                string command = null;
                if (switches.Contains("version") || switches.Contains("v"))
                    command = "version";
                else if (switches.Contains("download") || switches.Contains("d"))
                    command = "download";
                else if (switches.Contains("collate"))
                    command = "collate";
                
                //Console.WriteLine("command : " + command);

                if (command == null || switches.Contains("help") || switches.Contains("h"))
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("Download data : --download|-d --user|-u <Lastfm username> (optional: --page±-p <PAGE>)");
                }

                if (command == "download")
                {
                    if (!switches.Contains("user") && !switches.Contains("u")){
                        Console.WriteLine($"Error: User required. Use --user|-u <lastfm username>");
                        System.Environment.Exit(1);
                    }

                    if (!switches.Contains("cookie") && !switches.Contains("c")){
                        Console.WriteLine($"Error: Cookie file path required. Use --cookie|-c <lastfm username>. Get cookie by logging onto Lastfm in your browser, then extracting cookie header.");
                        System.Environment.Exit(1);
                    }

                    Download download = new Download();
                    download.Work(switches.Get("user", "u"), switches.Get("cookie", "c"), DataTypes.Scrobbles);
                }

                if (command == "collate")
                {
                    Collate collate = new Collate();
                    collate.Work();
                }

                if (command == "version")
                {
                    Version version = new Version();
                    version.Work();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}