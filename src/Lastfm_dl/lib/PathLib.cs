namespace Lastfm_dl 
{
    // 
    public class PathLib
    {
        public static string SessionPath(string root)
        {
            return Path.Join(root, "lastfm_dl", "session");
        }

        public static string ScrobblesPath(string root)
        {
            return Path.Join(SessionPath(root), "scrobbles");
        }

        public static string CollatedFilePath(string root)
        {
            return Path.Join(root, "lastfm_dl", "all_scrobbles.json" );
        }
    }
}