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
                
                string command = null;
                if (switches.Contains("version") || switches.Contains("v"))
                    command = "version";
                
                if (command == null || switches.Contains("help") || switches.Contains("h"))
                {
                    Console.WriteLine("Usage:");
                }

                command = "download";

                if (command == "download")
                {
                    string user = File.ReadAllText("./user"); // dev only
                    Download download = new Download();
                    download.Work(user, DataTypes.Scrobbles);
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