using System;

namespace Lastfm_dl 
{
    public class RandomLib
    {
        public string String(int minLength, int maxLength)
        {
            const string input = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            Random rnd = new Random();
            int length = rnd.Next(minLength, maxLength);

            return new string(Enumerable.Repeat(input, length)
                .Select(s => s[rnd.Next(s.Length)])
                .ToArray());
        }
    }

}
