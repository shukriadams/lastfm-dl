using System.Reflection;
using Lastfm_dl.Porter_Packages.MadScience_ReflectionHelpers;

namespace Lastfm_dl
{
    public class Version
    {
        /// <summary>
        /// 
        /// </summary>
        public void Work()
        {
            string currentVersion = ResourceHelper.ReadStringResourceFromCallingAssembly("Lastfm_dl.currentVersion.txt");
            Console.WriteLine($"version : {currentVersion}");
        }
    }    
}