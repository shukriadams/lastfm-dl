namespace Lastfm_dl 
{
    // 
    public class Percent
    {
        public static int Calc(long first, long second)
        {
            // overflow check
            if (second == 0)
                return 0;

            double p = (double)first / (double)second;
            return (int)System.Math.Round((double)(p * 100), 0);
        }

    }
}