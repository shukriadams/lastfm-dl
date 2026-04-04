namespace Lastfm_dl 
{
    public class WriteToSameLine : IDisposable
    {
        private int length = 0;

        private string Pad(string text)
        {
            string padding = String.Empty;
            
            for (int i = 0 ; i < length - text.Length ; i ++)
                padding += " ";

            if (text.Length > length)
                length = text.Length;

            return text + padding;
        }

        public void Dispose()
        {
            // must break to next line or next console write will be on same line as this class' output
            Console.WriteLine(string.Empty);
        }

        public void Write(string text)
        {
            Console.Write($"\r{Pad(text)}");
        }
    }
}