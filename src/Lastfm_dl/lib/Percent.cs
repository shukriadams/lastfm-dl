namespace Lastfm_dl 
{
    // 
    public class Percent
    {
        public static int Calc(long first, long second, bool clamp = true)
        {
            // overflow check
            if (second == 0)
                return 0;

            double p = (double)first / (double)second;
            int percent = (int)System.Math.Round((double)(p * 100), 0);
            
            if (percent > 100 && clamp)
                percent = 100;

            return percent;
        }

    }
}