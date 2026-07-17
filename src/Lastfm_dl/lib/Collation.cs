namespace Lastfm_dl 
{
    public class Collation
    {
        public IEnumerable<Scrobble> Scrobbles { get;set; } = new Scrobble[] { };
        
        public int ScrobbleCount { get;set; }

        /// number of pages of scrobbles processed. Use this as a convenient progress start
        /// when doing a continuation import.        
        public int Pages { get;set; }

        public DateTime Date { get;set; }

        public string User {get;set;}
    }

}