using HtmlAgilityPack;

namespace Lastfm_dl
{
    public class PageResponse : Response
    {
        public int StatusCode {get;set;}
        
        public string BodyRaw {get;set;}

        public HtmlDocument Body {get;set;}

        public bool TooManyRequests {get;set;}

        public bool NotFound {get;set;}

        public override string ToString()
        {
            return $"StatusCode:{this.StatusCode}\n" + base.ToString();
        }

    }    
}