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
            string path,
            int pagePause = 5000,
            bool ignorePageCount = false,
            bool additive = true,
            bool clearSession = false,
            int? forceStartPage = null, 
            int? forceStopPage = null
            )
        {
            string lastPageFilePath = Path.Join(PathLib.SessionPath(path), "lastpage");

            if (pagePause < 5000)
            {
                Console.WriteLine($"Pause cannot be less than 5 seconds - please be polite and don't hammer last.fm. ");
                return;
            }

            // verify cookie state
            if (!File.Exists(cookiePath))
            {
                Console.WriteLine($"Cookie file not found at path {cookiePath}");
                return;
            }

            // verify path parent exists, and that path is not a file
            if (path == string.Empty)
            {
                Console.WriteLine($"Path cannot be empty");
                return;
            }

            // convert to absolute path
            string pathAbsolute = Path.GetFullPath(path);
            string pathParent = Path.GetDirectoryName(pathAbsolute);

            if (!Directory.Exists(pathParent))
            {
                Console.WriteLine($"Directory \"{path}\" cannot be created because \"{pathParent}\" does not exist.");
                return;                
            }

            if(File.Exists(pathAbsolute))
            {
                Console.WriteLine($"{path} is a file, this must be a directory.");
                return;
            }
            

            string cookie = string.Empty;
            try 
            {
                cookie = File.ReadAllText(cookiePath);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error reading cookie at path {cookiePath}");
                Console.WriteLine(ex.Message);
                return;
            }

            // verify cookie - request last.fm/home, if we get a 302 instead of 200, we have been redirect to default
            // landing page and cookie is therefore not valid
            CookieValidResponse cookieValidResponse = UserLib.IsCookieValid(user, cookie);
            if (!cookieValidResponse.IsValid)
            {
                Console.WriteLine($"Cookie string in file {cookiePath} was not accepted by last.fm");
                return;
            }

            // get nr of user pages, this also verifies user exists
            UserScrobblePagesResponse pagesLookupResponse = UserLib.GetScrobblePages(user);
            if (!pagesLookupResponse.Succeeded)
            {
                Console.WriteLine(pagesLookupResponse.Description);
                return;
            }

            // determine state of current session, if any
            SessionLib sessionLib = new SessionLib();
            SessionInitializeResponse sessionInitializeResponse = sessionLib.Initialize(
                currentPagesCount : pagesLookupResponse.Pages,
                ignorePagesMismatch : ignorePageCount,
                path: path,
                clearSession : clearSession);

            if (!sessionInitializeResponse.Succeeded)
            {
                Console.WriteLine($"ERROR : Session init failed {sessionInitializeResponse.Description}");
                return;
            }

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

            Console.WriteLine($"your data will be downloaded in path \"{pathAbsolute}\"");

            int updatedScrobbles = 0;
            Scrobble lastNewScrobble = null;
            Scrobble firstNewScrobble = null;

            while(currentPage < totalPages)
            {
                if (forceStopPage.HasValue && currentPage > forceStopPage.Value)
                {
                    Console.WriteLine($"Reached forced stop page {forceStopPage.Value}.");
                    break;
                }

                string currentPageSavePath = $"{PathLib.ScrobblesPath(path)}/page_{currentPage}.json";

                ScrobblesOnPageResponse scrobblesOnPageResponse = UserLib.GetScrobblesOnPage(user, currentPage, cookie, pagePause);
                if (!scrobblesOnPageResponse.Succeeded)
                {
                    Console.WriteLine(scrobblesOnPageResponse.Description);
                    return;
                }

                // transfer scrobble to write buffer, cut off at previously reached end of scrobbles if necessary. this is how 
                // addative downloading is done
                List<Scrobble> writePage = new List<Scrobble>();
                bool limitReached = false;
                
                if (sessionInitializeResponse.Limit == null)
                {
                    writePage = scrobblesOnPageResponse.Scrobbles.ToList();
                } 
                else
                {
                    scrobblesOnPageResponse.Scrobbles = scrobblesOnPageResponse.Scrobbles.OrderByDescending(s => s.TimestampDT).ToList();

                    foreach(Scrobble scrobble in scrobblesOnPageResponse.Scrobbles)
                    {
                        if (sessionInitializeResponse.Limit.Artist == scrobble.Artist &&
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
                    Console.WriteLine($"ERROR : failed to write scrobble data for page {lastPageFilePath}\n{ex.Message}");
                    return;
                }

                try 
                {
                    File.WriteAllText(lastPageFilePath, currentPage.ToString());
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"ERROR : failed to update last processed page file at path {lastPageFilePath}\n{ex.Message}");
                    return;
                }

                // write out scrobbles found on page 
                using(WriteToSameLine writeToSameLine = new WriteToSameLine())
                foreach(Scrobble scrobble in writePage)
                {
                    writeToSameLine.Write($"Parsed scrobble : \"{scrobble.Artist}\" - {scrobble.Name} ({scrobble.Timestamp})");
                    Thread.Sleep(100);
                }


                int percent = Percent.Calc(currentPage, totalPages);
                Console.WriteLine($"Processed page {currentPage}/{totalPages} ({percent}%), pausing {pagePause} ms");

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
                    path : path,
                    session : sessionInitializeResponse.Session);
            }

            // collate
            Collate collate = new Collate();
            Response collateResponse = collate.Work(additive, path);
            if (!collateResponse.Succeeded)
            {
                Console.WriteLine($"ERROR : {collateResponse.Description}");
                return;
            }

            // wipe session
            sessionLib.Remove(path);

            Console.WriteLine("Finished downloading.");
        }
    }    
}