namespace Lastfm_data_downloader
{
    public class UserScrobblePagesResponse
    {
        public enum Errors
        {
            UserDoesNotfound,
            Unknown
        }
        
        public bool Succeeded {get;set;}

        public Errors? Error {get;set;}

        public int Pages {get;set;}
        
        public string Description {get;set;}
    }    
}