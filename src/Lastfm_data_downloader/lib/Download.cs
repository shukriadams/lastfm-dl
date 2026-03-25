using System.Reflection;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Text;
using Lastfm_data_downloader.Porter_Packages.MadScience_ReflectionHelpers;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace Lastfm_data_downloader
{
    public class Download
    {
        /// <summary>
        /// 
        /// </summary>
        public void Work(string user, string cookiePath, DataTypes dataType, bool resume = true, int? forceStartPage = null, int pagePause = 5000)
        {
            string lastPageFilePath = "./working/lastpage";
            string incidentLogPath = "./working/incident-log.txt";

            System.IO.Directory.CreateDirectory("./working/scrobbles");

            if (pagePause < 5000)
            {
                Console.WriteLine($"Pause cannot be less than 5 seconds - please be polite and don't hammer last.fm. ");
                return;
            }

            if (!File.Exists(cookiePath))
            {
                Console.WriteLine($"Cookie file not found at path {cookiePath}");
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

            UserScrobblePagesResponse pagesLookupResponse = UserLib.GetScrobblePages(user);
            if (!pagesLookupResponse.Succeeded)
            {
                Console.WriteLine(pagesLookupResponse.Description);
                return;
            }


            int totalPages= pagesLookupResponse.Pages;
            Console.WriteLine($"User {user} has {totalPages} pages of scrobbles");

            // start processing backwards, oldest records first
            int currentPage = totalPages;
            int maxPageRetries = 5;

            if (forceStartPage != null)
            {
                if (resume)
                {
                    Console.WriteLine("Error : cannot both resume and force start page - use one only");
                    return;
                }

                if (forceStartPage < 0){
                    Console.WriteLine("ERROR : Page cannot be less than zero");
                    return;
                }

                if (forceStartPage > totalPages){
                    Console.WriteLine($"ERROR : cannot force page greater than existing max of {totalPages} from Last.fm");
                    return;
                }

                currentPage = forceStartPage.Value;
                Console.WriteLine($"Paging will start at user specified value {forceStartPage}");
            }


            // try to find last page file
            if (resume)
            {
                if (File.Exists(lastPageFilePath))
                {
                    string rawLastPage;
                    try 
                    {
                        rawLastPage = File.ReadAllText(lastPageFilePath);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"ERROR : reading last page cache file {lastPageFilePath}");
                        return;
                    }

                    try 
                    {
                        currentPage = Int32.Parse(rawLastPage);
                        if (currentPage > totalPages)
                        {
                            Console.WriteLine($"ERROR : last processed page is {currentPage} but maximum allowed page based on existing last.fm data is {totalPages}");
                            return;
                        }

                        Console.WriteLine($"Resuming import from last processed page {currentPage}");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return;
                    }
                }
            }


            while(currentPage > 0)
            {
                string currentPageSavePath = $"./working/scrobbles/page_{currentPage}.json";

                Console.WriteLine($"Processing page {currentPage}");

                ScrobblesOnPageResponse scrobblesOnPageResponse = UserLib.GetScrobbledOnPage(user, currentPage, cookie, pagePause);
                if (!scrobblesOnPageResponse.Succeeded)
                {
                    Console.WriteLine(scrobblesOnPageResponse.Description);
                    return;
                }

                try
                {
                    File.WriteAllText(currentPageSavePath, JsonConvert.SerializeObject(scrobblesOnPageResponse.Scrobbles, Formatting.Indented));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write scrobble data for page {lastPageFilePath}\n{ex.Message}");
                    return;
                }

                try 
                {
                    File.WriteAllText(lastPageFilePath, currentPage.ToString());
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to update last processed page file at path {lastPageFilePath}\n{ex.Message}");
                    return;
                }

                // write out scrobbles found on page 
                foreach(Scrobble scrobble in scrobblesOnPageResponse.Scrobbles)
                    Console.WriteLine($"Parsed scrobble : \"{scrobble.Artist}\" - {scrobble.Name} ({scrobble.Timestamp})");

                int percent = Percent.Calc(totalPages - currentPage, totalPages);
                Console.WriteLine($"Saved page {currentPage}/{totalPages} ({percent}%), pausing {pagePause} ms");

                Thread.Sleep(pagePause);
                currentPage --;
            }
        }
    }    
}