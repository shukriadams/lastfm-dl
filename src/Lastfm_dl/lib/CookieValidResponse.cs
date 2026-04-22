using HtmlAgilityPack;

namespace Lastfm_dl
{
    public class CookieValidResponse : Response
    {
        public bool IsValid {get;set;}
        
        public override string ToString()
        {
            return $"IsValid:{this.IsValid}\n" + base.ToString();
        }
    }    
}