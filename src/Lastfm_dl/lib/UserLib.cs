using System.Net;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Lastfm_dl
{
    public class UserLib
    {
        /// <summary>
        /// Finds how many pages of scrobbles user has
        /// </summary>
        public static UserScrobblePagesResponse GetScrobblePages(string user)
        {
            
            WebClient client = new WebClient();

            string paginationRaw;
            
            try 
            {
                paginationRaw = client.DownloadString($"https://www.last.fm/user/{user}/library");
            }
            catch(WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                if (response != null && (int)response.StatusCode == 404)
                    return new UserScrobblePagesResponse { 
                        Error = UserScrobblePagesResponse.Errors.UserDoesNotfound,
                        Description = $"User \"{user}\" does not exist on last.fm"
                    };

                return new UserScrobblePagesResponse { 
                    Error = UserScrobblePagesResponse.Errors.Unknown,
                    Description = ex.Message
                };
            }
            catch(Exception ex)
            {
                return new UserScrobblePagesResponse { 
                    Error = UserScrobblePagesResponse.Errors.Unknown,
                    Description = ex.Message
                };
            }

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(paginationRaw);

            HtmlNode pagination = htmlDoc.DocumentNode.SelectSingleNode("//ul[contains(@class, 'pagination-list')]");
            HtmlNodeCollection pages = pagination.SelectNodes(".//li[contains(@class, 'pagination-page')]");
            if (pages.Count == 0)
                return new UserScrobblePagesResponse { 
                    Error = UserScrobblePagesResponse.Errors.Unknown,
                    Description = "No pages found for user stats - exiting"
                };

            var lastPagination = pages[pages.Count -1];
            if (lastPagination == null)
                return new UserScrobblePagesResponse { 
                    Error = UserScrobblePagesResponse.Errors.Unknown,
                    Description = "Failed to get last page"
                };

            int totalPages = 0;

            try 
            {
                totalPages = Int32.Parse(lastPagination.InnerText);
                return new UserScrobblePagesResponse{ 
                    Pages = totalPages, 
                    Succeeded = true 
                };
            }
            catch (Exception ex)
            {
                return new UserScrobblePagesResponse { 
                    Error = UserScrobblePagesResponse.Errors.Unknown,
                    Description = $"page value \"{lastPagination.InnerText}\" likely not a valid integer.\n{ex.Message}"
                };
            }            
        }


        ///
        /// 
        /// 
        public static ScrobblesOnPageResponse GetScrobblesOnPage(string user, int page, string cookie, int pagePause)
        {
            WebClient client = new WebClient();
            int pageRetries = 0;
            int maxPageRetries = 5;

            while(pageRetries <= maxPageRetries)
            {
                string pageUrl = $"https://www.last.fm/user/{user}/library?page={page}";
                client.Headers.Add(HttpRequestHeader.Cookie, cookie);
                string raw ="";
                try
                {
                    raw = client.DownloadString(pageUrl);
                }
                catch (WebException ex)
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                    if (response != null && (int)response.StatusCode == 404)
                    {
                        return new ScrobblesOnPageResponse { 
                            Description = $"User \"{user}\" does not exist on last.fm"
                        };
                    }

                    using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        Console.WriteLine($"{ex}\n" +
                            $"Url : {pageUrl}.Body is : \n" +
                            $"{r.ReadToEnd()}\n" +
                            "Sleeping, then retrying");

                        pageRetries ++;
                        Thread.Sleep(pagePause);
                        continue;
                    }
                }                
                catch(Exception ex)
                {
                    Console.WriteLine($"{ex}\n" + 
                        $"Url : {pageUrl}\n" +
                        "Sleeping, then retrying");

                    pageRetries ++;
                    Thread.Sleep(pagePause);
                    continue;
                }

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(raw);

                HtmlNodeCollection charts = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'chartlist')]");
                if (charts == null)
                {
                    // pause and retry this page
                    Console.WriteLine($"No chart on page {page}, attempt {pageRetries} ({DateTime.Now}).");
                    pageRetries ++;
                    Thread.Sleep(pagePause);
                    continue;
                }

                List<Scrobble> scrobbles = new List<Scrobble>();

                bool nodata = false;
                foreach(HtmlNode chart in charts)
                {
                    if (chart.SelectNodes(".//tr[@data-scrobble-row]") == null)
                    {
                        nodata = true;
                    }
                }

                if (nodata)
                {
                    // pause and retry this page
                    Console.WriteLine($"No plays on page {page}, attempt {pageRetries} ({DateTime.Now}).");
                    pageRetries ++;
                    Thread.Sleep(pagePause);
                    continue;
                }


                foreach(HtmlNode chart in charts)
                {
                    HtmlNodeCollection plays = chart.SelectNodes(".//tr[@data-scrobble-row]");

                    foreach(HtmlNode play in plays)
                    {
                        Scrobble scrobble = new Scrobble();

                        scrobble.Artist = WebUtility.HtmlDecode(play.SelectSingleNode("td[contains(@class, 'chartlist-artist')]").InnerText.Trim());
                        scrobble.Name = WebUtility.HtmlDecode(play.SelectSingleNode("td[contains(@class, 'chartlist-name')]").InnerText.Trim());
                        scrobble.Timestamp = play.SelectSingleNode("td[contains(@class, 'chartlist-timestamp')]").SelectSingleNode(".//span").Attributes["title"].Value;
                        scrobble.TimestampDT = DateTime.Parse(scrobble.Timestamp);

                        HtmlNode imageElement = play.SelectSingleNode("td[contains(@class, 'chartlist-image')]").SelectSingleNode(".//img");
                        if (imageElement != null)
                            scrobble.Image = imageElement.Attributes["src"].Value;
       
                        scrobbles.Add(scrobble);
                    } 
                }            

                return new ScrobblesOnPageResponse{
                    Scrobbles = scrobbles,
                    Succeeded = true
                };
            }

            return new ScrobblesOnPageResponse{
                Description = $"ERROR : Too many retries on page {page}, exiting"
            };
        }

        public static CookieValidResponse IsCookieValid(string user, string cookie)
        {
            // trying to access any page greater than 1, if we get status  200 back we are allowed to 
            // view page so cookie must be valid. if status is anything else (normally 302) we are being
            // redirected to login page
            
            PageRequest pageRequest = new PageRequest($"https://www.last.fm/user/{user}/library?page=2");
            pageRequest.SetCookie = cookie;
            PageResponse response = pageRequest.Execute();

            // dont bother with details of error, this is a simple request that will fail
            // only if lastfm is unreachable
            if (!response.Succeeded)
                return new CookieValidResponse {
                    Description = response.Description
                };

            return new CookieValidResponse {
                Succeeded = true,
                IsValid = response.StatusCode == 200
            };
        }
    }    
}