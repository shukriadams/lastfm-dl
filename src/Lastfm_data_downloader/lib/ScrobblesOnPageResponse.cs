using System.Collections.Generic;

namespace Lastfm_data_downloader
{
    public class ScrobblesOnPageResponse : Response
    {
        public enum Errors
        {
            UserDoesNotfound,
            Unknown
        }
        
        public Errors? Error {get;set;}


        public IList<Scrobble> Scrobbles {get;set;} = new List<Scrobble>();

    }    
}