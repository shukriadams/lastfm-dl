namespace Lastfm_dl
{
    public class Scrobble 
    {
        
        public int Page {get;set;}

        // position of scobble on page - crosses date blocks
        public int Index {get;set;}

        public string Name {get;set;}

        public string Artist {get;set;}

        ///
        /// Timestamp is accurate down to day. Identicaly timestamps for different tracks on the same
        /// user profile have been observed in the wild.
        /// 
        public string Timestamp {get;set;}

        public DateTime TimestampDT {get;set;}

        public string Image {get;set;}

        public override string ToString()
        {
            return $"\"{this.Name}\" - {this.Artist} ({this.Timestamp})";   
        }
    }
}