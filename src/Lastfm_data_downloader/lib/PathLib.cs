namespace Lastfm_data_downloader 
{
    // 
    public class PathLib
    {
        public static string WorkPath
        {
            get
            {
                return "./working";
            }
        }

        public static string SessionPath
        {
            get
            {
                return "./working/session";
            }
        }

        public static string ScrobblesPath
        {
            get
            {
                return "./working/session/scrobbles";
            }
        }

        public static string CollatedFilePath
        {
            get{
                return "./working/all_scrobbles.json";
            }
        }
    }
}