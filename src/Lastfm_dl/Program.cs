using Lastfm_dl.Porter_Packages.Madscience_CommandLineSwitches;

namespace Lastfm_dl
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            {
                CommandLineSwitches switches = new CommandLineSwitches();
                
                switches.Add(new Argument("version", typeof(string)) { LongName = "version", ShortName = "v" });
                switches.Add(new Argument("download", typeof(string)) { LongName = "download", ShortName = "d" });
                switches.Add(new Argument("user", typeof(string)) { LongName = "user", ShortName = "u" });
                switches.Add(new Argument("cookie", typeof(string)) { LongName = "cookie", ShortName = "c" });
                switches.Add(new Argument("limit", typeof(int?)) { LongName = "limit", ShortName = "l" });
                switches.Add(new Argument("ignorePageCount", typeof(bool)) { LongName = "ignorePageCount", DefaultValue = "false" });

                Console.WriteLine("Lastfm-dl");

                BindResponse bindResponse = switches.Bind(
                    args, 
                    validate : true);

                if (!bindResponse.Succeeded)
                {
                    Console.WriteLine($"Error :\n{bindResponse.Description}");
                    System.Environment.Exit(1);
                }

                string command = null;
                if (switches.IsSet("version"))
                    command = "version";
                else if (switches.IsSet("download"))
                    command = "download";

                if (command == null)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("Download scrobbles : --download|-d --user|-u <Lastfm username> --cookie|-c <path to file with last.fm cookie text>");
                }

                if (command == "version")
                {
                    Version version = new Version();
                    version.Work();
                }

                if (command == "download")
                {
                    if (!switches.IsSet("user"))
                    {
                        Console.WriteLine($"Error: User required. Use --user|-u <lastfm username>");
                        System.Environment.Exit(1);
                    }

                    if (!switches.IsSet("cookie"))
                    {
                        Console.WriteLine($"Error: Cookie file path required. Use --cookie|-c <lastfm username>. Get cookie by logging onto Lastfm in your browser, then extracting cookie header.");
                        System.Environment.Exit(1);
                    }

                    Download download = new Download();
                    download.Work(
                        user : switches.Get<string>("user"), 
                        cookiePath : switches.Get<string>("cookie"),
                        forceStopPage : switches.Get<int?>("limit"),
                        ignorePageCount : switches.Get<bool>("ignorePageCount"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}