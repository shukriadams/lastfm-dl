using System;
using System.IO;

namespace Lastfm_dl 
{
    public class CookieLib
    {
        public static CookieValidResponse IsCookieValid(string cookiePath)
        {
            // verify cookie file exists
            if (!File.Exists(cookiePath))
                return new CookieValidResponse{
                    Description = $"WARNING : cookie not found at expected location \"{cookiePath}\", scrobbles cannot be downloaded without this. See setup instructions at https://github.com/shukriadams/lastfm-dl"
                };
            
            // read cookie file 
            string cookie = string.Empty;
            try 
            {
                cookie = File.ReadAllText(cookiePath);
            }
            catch(Exception ex)
            {
                return new CookieValidResponse {
                    Description = $"ERROR : could not read read cookie at path \"{cookiePath}\" got : {ex.Message}"
                };
            }

            // trying to access any page greater than 1, if we get status  200 back we are allowed to 
            // view page so cookie must be valid. if status is anything else (normally 302) we are being
            // redirected to login page
            PageRequest cookieRequest = new PageRequest($"https://www.last.fm/home");
            cookieRequest.SetCookie = cookie;
            PageResponse cookeResponse= cookieRequest.Execute();
            if (cookeResponse.StatusCode != 200)
                return new CookieValidResponse {
                    Description = $"ERROR : cookie at path \"{cookiePath}\" is invalid (code {cookeResponse.StatusCode})."
                };

            return new CookieValidResponse {
                Succeeded = true,
                Cookie = cookie,
                IsValid = true
            };
        }
    }
}