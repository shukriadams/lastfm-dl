using System.Collections.Generic;

namespace Lastfm_data_downloader
{
    public class ScrobblesOnPageResponse
    {
        public enum Errors
        {
            UserDoesNotfound,
            Unknown
        }
        
        public bool Succeeded {get;set;}

        public Errors? Error {get;set;}

        public string Description {get;set;}

        public IList<Scrobble> Scrobbles {get;set;} = new List<Scrobble>();

    }    
}