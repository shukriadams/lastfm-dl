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
        public void Work(string user, DataTypes dataType)
        {
            int pagePause = 10000; // ms to wait between page pulls. Don't hammer.
            System.IO.Directory.CreateDirectory("./working");

            // you need a valid auth cookie from a current lastfm loing to access
            string cookie = File.ReadAllText("./cookie");

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
            int lastPage = Int32.Parse(lastPagination.InnerText);

            Console.WriteLine($"{lastPage} pages detected");

            // start processing backwards, oldest records are last
            int currentPage = lastPage;
            while(currentPage > 0)
            {
                client = new WebClient();

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
                        Thread.Sleep(pagePause);
                        continue;
                    }
                }                
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine($"Url : {pageUrl}");
                    Console.WriteLine("Sleeping, then retrying");
                    Thread.Sleep(pagePause);
                    continue;
                }

                htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(raw);
                var chart = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'chartlist')]");
                
                // work backwards, we stop all processing when we encounter a record that has already been processed
                var plays = chart.SelectNodes(".//tr[@data-scrobble-row]").Reverse();

                int id = 0;
                foreach(var play in plays)
                {
                    Scrobble scrobble = new Scrobble();

                    scrobble.ScrobbleId = play.Attributes["data-edit-scrobble-id"].Value;
                    scrobble.Artist = play.SelectSingleNode("td[contains(@class, 'chartlist-artist')]").InnerText.Trim();
                    scrobble.Name = play.SelectSingleNode("td[contains(@class, 'chartlist-name')]").InnerText.Trim();
                    scrobble.Timestamp = play.SelectSingleNode("td[contains(@class, 'chartlist-timestamp')]").SelectSingleNode(".//span").Attributes["title"].Value;
                    scrobble.Id = Hash(scrobble.Timestamp + "__" + scrobble.Artist + "__" + scrobble.Name);

                    string itemPath = Path.Join("./working", DirectoryHelper.GetPath(scrobble.Id, 5), $"{scrobble.Id}.json");

                    if (File.Exists(itemPath))
                    {
                        Console.WriteLine($"Scrobble {scrobble.Id} (based on signature {scrobble.Timestamp} {scrobble.Artist} {scrobble.Name}) already exists, skipping");
                        continue;

                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(itemPath));
                    Console.WriteLine(itemPath);


                    var imageElement = play.SelectSingleNode("td[contains(@class, 'chartlist-image')]").SelectSingleNode(".//img"); //;
                    var image = String.Empty;
                    if (imageElement != null)
                        scrobble.Image = imageElement.Attributes["src"].Value;

                    File.WriteAllText(itemPath, JsonConvert.SerializeObject(scrobble, Formatting.Indented));
                    Console.WriteLine($"Created scrobble {scrobble.Id}:{scrobble.Artist}-{scrobble.Name} ({scrobble.Timestamp})");
                } 

                Console.WriteLine($"Processed page {currentPage}, pausing {pagePause} ms");
                Thread.Sleep(pagePause);
                currentPage --;
            }
        }
    }    
}