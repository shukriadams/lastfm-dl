namespace Lastfm_data_downloader
{
    public class SessionInitializeResponse : Response
    {
        /// existing scrobble that has already been downloaded. When updating an existing history set, stop when this scrobble is reached
        public Scrobble Limit {get; set;}

        public string Warning {get; set;}

        public Session Session  {get; set;}

        public bool IsSessionContinued {get; set;}
    }    
}