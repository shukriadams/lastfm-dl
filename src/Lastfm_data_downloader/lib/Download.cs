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
        public static MemoryStream StreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }
        
        private string ToHex(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();

            foreach (byte b in bytes)
                s.Append(b.ToString("x2").ToLower());

            return s.ToString();
        }

        public string Hash(string str)
        {
            Stream stream = StreamFromString(str);
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(stream);
                return ToHex(hash);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Work(string user, string cookiePath, DataTypes dataType, bool resume = true, int? forcePage = null, int pagePause = 5000)
        {
            string lastPageFilePath = "./working/lastpage";
            string incidentLogPath = "./working/incident-log.txt";

            System.IO.Directory.CreateDirectory("./working/scrobbles");

            if (pagePause < 5000)
            {
                Console.WriteLine($"Pause cannot be less than 2 seconds - don't hammer last.fm. ");
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

            // calculate pages
            WebClient client = new WebClient();
            string paginationRaw = client.DownloadString($"https://www.last.fm/user/{user}/library");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(paginationRaw);

            var pagination = htmlDoc.DocumentNode.SelectSingleNode("//ul[contains(@class, 'pagination-list')]");
            var pages = pagination.SelectNodes(".//li[contains(@class, 'pagination-page')]");
            if (pages.Count == 0)
            {
                Console.WriteLine("No pages found for user stats - exiting");
                return;   
            }
            var lastPagination = pages[pages.Count -1];
            if (lastPagination == null)
            {
                Console.WriteLine("Failed to get last page");
                return;
            }

            int lastPage = 0;
            try 
            {
                lastPage = Int32.Parse(lastPagination.InnerText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"page value \"{lastPagination.InnerText}\" likely not a valid integer.\n{ex.Message}");
                return;
            }

            Console.WriteLine($"User {user} has {lastPage} pages of plays");

            // start processing backwards, oldest records first
            int currentPage = lastPage;
            int maxPageRetries = 5;

            if (forcePage != null)
            {
                if (resume)
                {
                    Console.WriteLine("Error : cannot both resume and force page - enable one only");
                    return;
                }

                if (forcePage < 0){
                    Console.WriteLine("ERROR : Page cannot be less than zero");
                    return;
                }

                if (forcePage > lastPage){
                    Console.WriteLine($"ERROR : cannot force page greater than existing max of {lastPage} from Last.fm");
                    return;
                }

                currentPage = forcePage.Value;
                Console.WriteLine($"Paging will start at user specified value {forcePage}");
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
                        if (currentPage > lastPage)
                        {
                            Console.WriteLine($"ERROR : last processed page is {currentPage} but maximum allowed page based on existing last.fm data is {lastPage}");
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

            int pageRetries = 0;

            while(currentPage > 0)
            {
                client = new WebClient();

                if (pageRetries > maxPageRetries)
                {
                    Console.WriteLine($"ERROR : Too many retries on page {currentPage}, exiting");
                    return;
                }

                string currentPageSavePath = $"./working/scrobbles/page_{currentPage}.json";
                if (File.Exists(currentPageSavePath))
                {
                    currentPage --;
                    pageRetries = 0;
                    Console.WriteLine($"Page {currentPage} already processed, skipping");
                    continue;
                }
                Console.WriteLine($"Processing page {currentPage}");

                string pageUrl = $"https://www.last.fm/user/{user}/library?page={currentPage}";
                client.Headers.Add(HttpRequestHeader.Cookie, cookie);
                string raw ="";
                try
                {
                    raw = client.DownloadString(pageUrl);
                }
                catch (WebException ex)
                {
                    using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string body = r.ReadToEnd();
                        Console.WriteLine(ex);
                        Console.WriteLine($"Url : {pageUrl}.Body is : ");
                        Console.WriteLine(body);
                        Console.WriteLine("Sleeping, then retrying");
                        File.AppendAllText(incidentLogPath, $"Unexpected error reading page {currentPage}, attempt {pageRetries} ({DateTime.Now}).{Environment.NewLine}");
                        File.AppendAllText(incidentLogPath, $"{ex}{Environment.NewLine}");
                        pageRetries ++;
                        Thread.Sleep(pagePause);
                        continue;
                    }
                }                
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine($"Url : {pageUrl}");
                    Console.WriteLine("Sleeping, then retrying");
                    File.AppendAllText(incidentLogPath, $"Unexpected error reading page {currentPage}, attempt {pageRetries} ({DateTime.Now}).{Environment.NewLine}");
                    File.AppendAllText(incidentLogPath, $"{ex}{Environment.NewLine}");
                    pageRetries ++;

                    Thread.Sleep(pagePause);
                    continue;
                }

                htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(raw);

                HtmlNodeCollection charts = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'chartlist')]");
                if (charts == null)
                {

                    // pause and retry this page
                    Thread.Sleep(pagePause);
                    File.AppendAllText(incidentLogPath, $"No chart on page {currentPage}, attempt {pageRetries} ({DateTime.Now}).");
                    pageRetries ++;
                    continue;
                }

                int id = 0;
                List<Scrobble> scrobbles = new List<Scrobble>();

                foreach(var chart in charts)
                {
                    HtmlNodeCollection plays = chart.SelectNodes(".//tr[@data-scrobble-row]");
                    if (plays == null)
                    {
                        // pause and retry this page
                        Thread.Sleep(pagePause);
                        File.AppendAllText(incidentLogPath, $"No plays on page {currentPage}, attempt {pageRetries} ({DateTime.Now}).");
                        pageRetries ++;
                        continue;
                    }

                    foreach(var play in plays)
                    {
                        Scrobble scrobble = new Scrobble();

                        scrobble.ScrobbleId = play.Attributes["data-edit-scrobble-id"].Value;
                        scrobble.Artist = play.SelectSingleNode("td[contains(@class, 'chartlist-artist')]").InnerText.Trim();
                        scrobble.Name = play.SelectSingleNode("td[contains(@class, 'chartlist-name')]").InnerText.Trim();
                        scrobble.Timestamp = play.SelectSingleNode("td[contains(@class, 'chartlist-timestamp')]").SelectSingleNode(".//span").Attributes["title"].Value;
                        scrobble.Index = id;
                        scrobble.Page = currentPage;

                        scrobble.Artist = WebUtility.HtmlDecode(scrobble.Artist);
                        scrobble.Name = WebUtility.HtmlDecode(scrobble.Name);

                        var imageElement = play.SelectSingleNode("td[contains(@class, 'chartlist-image')]").SelectSingleNode(".//img"); //;
                        var image = String.Empty;
                        if (imageElement != null)
                            scrobble.Image = imageElement.Attributes["src"].Value;
        
                        Console.WriteLine($"Parsed scrobble : {scrobble.Artist}-{scrobble.Name} ({scrobble.Timestamp})");
                        scrobbles.Add(scrobble);
                        id ++;
                    } 

                }

                int percent = Percent.Calc(lastPage - currentPage,lastPage);
                File.WriteAllText(currentPageSavePath, JsonConvert.SerializeObject(scrobbles, Formatting.Indented));
                Console.WriteLine($"Saved page {currentPage}/{lastPage} ({percent}%), pausing {pagePause} ms");

                try 
                {
                    File.WriteAllText(lastPageFilePath, currentPage.ToString());
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to update last processed page file at path {lastPageFilePath}\n{ex.Message}");
                    return;
                }

                Thread.Sleep(pagePause);
                pageRetries = 0;
                currentPage --;
            }
        }
    }    
}