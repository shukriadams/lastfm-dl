namespace Lastfm_dl 
{
    public class Collation
    {
        public IEnumerable<Scrobble> Scrobbles { get;set; } = new Scrobble[] { };
        public int ScrobbleCount { get;set; }
        public DateTime Date { get;set; }
    }

}