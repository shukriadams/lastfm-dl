using System;

namespace Lastfm_dl
{
    public class Initialize
    {
        public void Work(string workPath, string cookiePath)
        {
            Console.WriteLine("initializing ... ");
            Response workSetupResponse = PathLib.SetupWorkDirectory(workPath);
            if (!workSetupResponse.Succeeded)
            {
                Console.WriteLine(workSetupResponse.Description);
                Environment.Exit(1);
            }

            Console.WriteLine($"scrobbles will be stored in \"{workPath}\"");

            // check cookie, get current page count of user
            CookieValidResponse cookieValidResponse = CookieLib.IsCookieValid(cookiePath);
            if (!cookieValidResponse.IsValid)
            {
                Console.WriteLine(cookieValidResponse.Description);
                return;
            }
            Console.WriteLine("cookie access to last.fm verified");

            // get info on existing session, non-destructively
            SessionGetResponse sessionResponse = new SessionLib().GetSession(workPath);
            if (!sessionResponse.Succeeded)
            {
                Console.WriteLine(sessionResponse.Description);
                return;
            }

            // print current session if any
            if (sessionResponse.Session != null)
                Console.WriteLine($"found an existing session started {sessionResponse.Session.Started}, this will be continued on next download");

            // print current data on disk if any
            if (sessionResponse.Collation != null)
                Console.WriteLine($"existing scrobble file found for user {sessionResponse.Collation.User}, {sessionResponse.Collation.ScrobbleCount} scrobbles already downloaded");
        }
    }
}