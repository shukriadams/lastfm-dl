using System;

namespace Lastfm_data_downloader
{
    public class DirectoryHelper
    {
        /// <summary>
        /// Generates a path for an item based on the sequence of chars in name. Used to spread files over 
        /// a directory tree.
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetPath(string itemName, int depth)
        {
            string path = string.Empty;
            if (itemName.Length < depth)
                throw new Exception($"{itemName} cannot be less than depth {depth}");

            for (int i = 0 ; i < depth; i++)
                path += $"{Path.DirectorySeparatorChar}{itemName.Substring(i, 1)}";
            
            return path;
        }
    }    
}
