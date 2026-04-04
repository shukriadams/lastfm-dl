namespace Lastfm_dl
{
    public class UserScrobblePagesResponse : Response
    {
        public enum Errors
        {
            UserDoesNotfound,
            Unknown
        }

        public Errors? Error {get;set;}

        public int Pages {get;set;}
       
    }    
}