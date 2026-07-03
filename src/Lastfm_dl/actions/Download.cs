using System.Reflection;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Lastfm_dl.Porter_Packages.MadScience_ReflectionHelpers;

namespace Lastfm_dl
{
    public class Download
    {
        /// <summary>
        /// 
        /// </summary>
        public void Work(
            string user, 
            string cookiePath, 
            string workPath,
            int pagePause = 5000,
            bool ignorePageCount = false,
            bool additive = true,
            bool clearSession = false,
            int? forceStartPage = null, 
            int? forceStopPage = null
            )
        {
            if (pagePause < 5000)
            {
                Console.WriteLine($"Pause cannot be less than 5 seconds - please be polite and don't hammer last.fm. ");
                return;
            }

            // verify directory structure
            Response pathSetupResponse = PathLib.SetupWorkDirectory(workPath);
            if (!pathSetupResponse.Succeeded)
            {
                Console.WriteLine(pathSetupResponse.Description);
                return;
            }
            Console.WriteLine("Directory structure passed. validation passed");


            // validate cookie
            CookieValidResponse cookieValidResponse = CookieLib.IsCookieValid(cookiePath);
            if (!cookieValidResponse.IsValid)
            {
                Console.WriteLine(cookieValidResponse.Description);
                return;
            }
            Console.WriteLine("Cookie validation passed");


            // get nr of user pages, this also verifies that user exists on lastfm. Note that we need user 
            // page count to process session
            UserScrobblePagesResponse pagesLookupResponse = UserLib.GetScrobblePages(user);
            if (!pagesLookupResponse.Succeeded)
            {
                Console.WriteLine(pagesLookupResponse.Description);
                return;
            }
            Console.WriteLine("Queried user history");

            // create session or update if exists already
            SessionLib sessionLib = new SessionLib();
            SessionInitializeResponse sessionInitializeResponse = sessionLib.Initialize(
                currentPagesCount : pagesLookupResponse.Pages,
                ignorePagesMismatch : ignorePageCount,
                user: user,
                path: workPath,
                clearSession : clearSession);
            
            if (!sessionInitializeResponse.Succeeded)
            {
                Console.WriteLine($"ERROR : Session init failed {sessionInitializeResponse.Description}");
                return;
            }
            Console.WriteLine("Session initialized");


            string sessionPath = PathLib.SessionPath(workPath);
            

            // If continuing a session, we continue at the last processed page number. If the user has new 
            // scrobbles the page nr continued from will not be on the same point in the user's history, resulting in lost 
            // scrobbles. Warn user of that.
            int pagesWhenSessionCreated = sessionInitializeResponse.Session.TotalPages;
            if (!ignorePageCount && pagesWhenSessionCreated != pagesLookupResponse.Pages)
            {
                Console.WriteLine($"Current session was started when lastfm total scrobble playcount was {pagesWhenSessionCreated}, but current total page count is {pagesLookupResponse.Pages}. Aborting download. Use --ignore to ignore this error, or --clear to start a new session. Note that you the greater the page mismatch, the more plays you will lose from your history.");
                return;
            }
                
                

            string warning = string.Empty;
            if (pagesWhenSessionCreated != pagesLookupResponse.Pages)
                warning = $"Current session was started when lastfm total scrobble playcount was {pagesWhenSessionCreated}, but current total page count is {pagesLookupResponse.Pages}. You have opted to ignore this mismatch. The greater the page mismatch, the more plays you will lose from your history.";



            if (!string.IsNullOrEmpty(sessionInitializeResponse.Warning))
                Console.WriteLine(sessionInitializeResponse.Warning);

            int totalPages = pagesLookupResponse.Pages;

            Console.WriteLine($"User {user} has {totalPages} pages of scrobbles");
            
            if (sessionInitializeResponse.IsSessionContinued)
                Console.WriteLine($"Incomplete session found, created {sessionInitializeResponse.Session.Started}, on page {sessionInitializeResponse.Session.CurrentPage} of {sessionInitializeResponse.Session.TotalPages}, will continue from this.");

            if (sessionInitializeResponse.Limit != null)
                Console.WriteLine($"This import will add to an existing scrobble download, and will stop at {sessionInitializeResponse.Limit}");
 
            // start processing forwards, newest records first. A session's default starting page will be 1
            int currentPage = sessionInitializeResponse.Session.CurrentPage;

            if (forceStopPage.HasValue && forceStopPage.Value > totalPages)
            {
                Console.WriteLine($"Error : Forced stop page is {forceStopPage.Value}, which is greater than the number of pages this user has ({totalPages})");
                return;
            }
            
            if (forceStopPage.HasValue)
                Console.WriteLine($"Forced stop page set to {forceStopPage.Value}, will not process more than this.");

            Console.WriteLine($"your data will be downloaded in path \"{workPath}\"");


            int updatedScrobbles = 0;
            Scrobble lastNewScrobble = null;
            Scrobble firstNewScrobble = null;
            bool isLimitMode = sessionInitializeResponse.Limit != null;
            int expectedPagesToProcess = sessionInitializeResponse.Pages == 0 ? totalPages : totalPages - sessionInitializeResponse.Pages;
            if (sessionInitializeResponse.Session != null)
                expectedPagesToProcess = expectedPagesToProcess - sessionInitializeResponse.Session.CurrentPage;

            while(currentPage < totalPages)
            {
                if (forceStopPage.HasValue && currentPage > forceStopPage.Value)
                {
                    Console.WriteLine($"Reached forced stop page {forceStopPage.Value}.");
                    break;
                }

                string currentPageSavePath = Path.Join(PathLib.ScrobblesPath(workPath), $"page_{currentPage}.json");

                ScrobblesOnPageResponse scrobblesOnPageResponse = UserLib.GetScrobblesOnPage(user, currentPage, cookieValidResponse.Cookie, pagePause);
                if (!scrobblesOnPageResponse.Succeeded)
                {
                    Console.WriteLine(scrobblesOnPageResponse.Description);
                    return;
                }

                // transfer scrobble to write buffer, cut off at previously reached end of scrobbles if necessary. this is how 
                // addative downloading is done
                List<Scrobble> writePage = new List<Scrobble>();
                bool limitReached = false;

                // process scrobbles on current page
                if (isLimitMode == null)
                {
                    // not doing limit mode, use all scrobbles on page
                    writePage = scrobblesOnPageResponse.Scrobbles.ToList();
                } 
                else
                {
                    // check for limit - limit is the last scrobble stored in the aleady-saved scrobble file on disk.
                    // if we are downloading only latest scrobbles, we look for and stop at limit
                    scrobblesOnPageResponse.Scrobbles = scrobblesOnPageResponse.Scrobbles.OrderByDescending(s => s.TimestampDT).ToList();

                    foreach(Scrobble scrobble in scrobblesOnPageResponse.Scrobbles)
                    {
                        if (sessionInitializeResponse.Limit != null &&
                            sessionInitializeResponse.Limit.Artist == scrobble.Artist &&
                            sessionInitializeResponse.Limit.Name == scrobble.Name &&
                            sessionInitializeResponse.Limit.Timestamp == scrobble.Timestamp){
                                limitReached = true;
                                break;
                        }

                        if (firstNewScrobble == null)
                            firstNewScrobble = scrobble;

                        updatedScrobbles ++;
                        lastNewScrobble = scrobble;
                        writePage.Add(scrobble);
                    }
                }
 

                try
                {
                    File.WriteAllText(currentPageSavePath, JsonConvert.SerializeObject(writePage, Formatting.Indented));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR : failed to write scrobble data\n{ex.Message}");
                    return;
                }


                // write out scrobbles found on this page 
                using(ConsoleWriteSingleLine writeToSameLine = new ConsoleWriteSingleLine())
                foreach(Scrobble scrobble in writePage)
                {
                    writeToSameLine.Write($"Parsed scrobble : \"{scrobble.Artist}\" - {scrobble.Name} ({scrobble.Timestamp})");
                    Thread.Sleep(100);
                }


                int percent = Percent.Calc(currentPage, expectedPagesToProcess);
                Console.WriteLine($"Processed page {currentPage}/{expectedPagesToProcess} ({percent}%), pausing {pagePause} ms");

                if (limitReached){
                    Console.WriteLine($"Reached previously downloaded scrobble {sessionInitializeResponse.Limit}, stopping here. Imported {updatedScrobbles} new scrobbles.");
                    if (lastNewScrobble != null && firstNewScrobble != null && sessionInitializeResponse.Limit != null)
                        Console.WriteLine($"New scrobbles started at {firstNewScrobble}, ended at {lastNewScrobble}. The latter lined up against previously downloaded scrobble {sessionInitializeResponse.Limit}.");

                    break;
                }

                Thread.Sleep(pagePause);
                currentPage ++;
                sessionInitializeResponse.Session.CurrentPage = currentPage;
                sessionLib.Update(
                    path : workPath,
                    session : sessionInitializeResponse.Session);
            }

            // collate
            Collate collate = new Collate();
            Response collateResponse = collate.Work(appendToExisting: additive, 
                user: user, 
                pages : totalPages,
                path : workPath);

            if (!collateResponse.Succeeded)
            {
                Console.WriteLine($"ERROR : {collateResponse.Description}");
                return;
            }

            // wipe session
            sessionLib.Remove(workPath);

            Console.WriteLine("Finished downloading.");
        }
    }    
}