namespace Lastfm_data_downloader
{
    public class Scrobble 
    {
       
        public string Name {get;set;}

        public string Artist {get;set;}

        ///
        /// Timestamp is accurate down to day. Identicaly timestamps for different tracks on the same
        /// user profile have been observed in the wild.
        /// 
        public string Timestamp {get;set;}

        public string Image {get;set;}

        public override string ToString()
        {
            return $"\"{this.Name}\" - {this.Artist} ({this.Timestamp})";   
        }
    }
}