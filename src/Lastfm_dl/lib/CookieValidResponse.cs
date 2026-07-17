namespace Lastfm_dl
{
    public class CookieValidResponse : Response
    {
        public bool IsValid {get;set;}
        
        /// <summary>
        /// If cookie check is valid, this field contains the cookie string
        /// </summary>
        public string Cookie {get;set;}

        public override string ToString()
        {
            return $"IsValid:{this.IsValid}\n" +
                $"Cookie:{this.Cookie}\n" +
                base.ToString();
        }
    }    
}