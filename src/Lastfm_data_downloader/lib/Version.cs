using System.Reflection;
using Lastfm_data_downloader.Porter_Packages.MadScience_ReflectionHelpers;

namespace Lastfm_data_downloader
{
    public class Version
    {
        /// <summary>
        /// 
        /// </summary>
        public void Work()
        {
            string currentVersion = ResourceHelper.ReadStringResourceFromCallingAssembly("Lastfm_data_downloader.currentVersion.txt");
            Console.WriteLine($"version : {currentVersion}");
        }
    }    
}