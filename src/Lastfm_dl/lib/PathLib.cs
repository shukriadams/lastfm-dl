using System;
using System.IO;

namespace Lastfm_dl 
{
    // 
    public class PathLib
    {
        public static string WorkingPath(string root)
        {
            return Path.Join(root, "lastfmdl");
        }

        public static string CookiePath(string root)
        {
            return Path.Join(WorkingPath(root), "cookie");
        }

        public static string SessionPath(string workPath)
        {
            return Path.Join(workPath, "session");
        }

        public static string ScrobblesPath(string workPath)
        {
            return Path.Join(SessionPath(workPath), "scrobbles");
        }

        public static string CollatedFilePath(string workPath)
        {
            return Path.Join(workPath, "scrobbles.json" );
        }

        /// <summary>
        /// 
        /// </summary>
        public static Response SetupWorkDirectory(string workPath)
        {
            // safety test, ensure workPath not empty, this shouldn't happen
            if (workPath == string.Empty)
                throw new Exception($"Path cannot be empty");

            // convert to absolute path
            string workPathAbsolute = Path.GetFullPath(workPath);
            string pathParent = Path.GetDirectoryName(workPathAbsolute);

            if (File.Exists(workPath))
                return new Response{
                    Description = $"\"{workPath}\" is a file, it must be a directory."
                };                

            // always create work dir if not exists
            if (!Directory.Exists(workPath))
            {
                Directory.CreateDirectory(workPath);
                Console.WriteLine($"Created work path \"{workPath}\"");
            }

            return new Response {
                Succeeded = true,
                Description = $"Working directory verified."
            };

        }
    }
}