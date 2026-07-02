using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;

namespace Lastfm_dl
{
    /* 
        Wraps http lookup, lets us repeat a lookup a few times - lastfm often returns a 600, and doing the call
        again gets us past it. Also abstracts away a lot of the rubbish around httpclient and the many lines 
        of trash code needed to make it do simple things. 
    */
    public class PageRequest
    {

        #region FIELDS

        private string _url;

        private string _lastFailedPage;

        private string _error = string.Empty;

        #endregion


        #region PROPERTIES

        public int MaxRetries { get; set; } = 5;
        
        public string SetCookie { get; set; }

        public int Pause { get; set; } = 2000;
 
        public bool AllowAutoRedirect { get; set; }

        #endregion


        #region CTORS

        public PageRequest(string url)
        {
            _url = url;
        }    

        #endregion


        #region METHODS

        public PageResponse Execute()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = this.AllowAutoRedirect;
            HttpClient client = new HttpClient(httpClientHandler);

            int pageRetries = 0;
        
            while(pageRetries <= this.MaxRetries)
            {
                string raw = "";

                try
                {
                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, _url);

                    if (this.SetCookie != null)
                    {
                        // strip out all new lines, headers don't allow these
                        this.SetCookie = this.SetCookie
                            .Replace("\n", String.Empty)
                            .Replace("\r", String.Empty)
                            .Replace("\t", String.Empty);

                        req.Headers.Add("Cookie", this.SetCookie);
                    }

                    HttpResponseMessage resp = client.Send(req);
                    int statuscode = (int)resp.StatusCode;
                    // last fm is having a moment, try again
                    if (statuscode == 600)
                    {
                        Console.WriteLine("Lastfm is busy, hold on, trying again ...");
                        pageRetries ++;
                        Thread.Sleep(this.Pause);
                        continue;
                    }

                    using (Stream content = resp.Content.ReadAsStream())
                    {
                        raw = StreamsLib.StreamToString(content);
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(raw);

                        return new PageResponse{
                            Body = htmlDoc,
                            StatusCode = statuscode, 
                            BodyRaw = raw,
                            Succeeded = true
                        };
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                    if (response == null)
                    {
                        _error += "Failed to cast ex to HttpWebResponse\n";
                    }
                    else 
                    {
                        int status = (int)response.StatusCode;

                        // abort immediately on 404, no need to retry
                        if (status == 404)
                            return new PageResponse { 
                                NotFound = true,
                                Description = "Not found"
                            };

                        using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            _lastFailedPage = r.ReadToEnd();
                            _error += $"Attempt {pageRetries}, status {status}\n";
                        }
                    }

                    pageRetries ++;
                    Thread.Sleep(this.Pause);
                    continue;
                }                
                catch(Exception ex)
                {
                    _error += ex.ToString();
                    pageRetries ++;
                    Thread.Sleep(this.Pause);
                    continue;
                }
            }        

            return new PageResponse {
                TooManyRequests = true,
                Description = $"Failed after too many requests{_error}"
            };
        }

        #endregion
    }
}