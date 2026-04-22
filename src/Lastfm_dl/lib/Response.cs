namespace Lastfm_dl
{
    public class Response
    {
        public bool Succeeded {get;set;}
        
        public string Description {get;set;}

        public override string ToString()
        {
            return $"Succeeded:{this.Succeeded}\nDescription:{this.Description}";
        }
    }    
}