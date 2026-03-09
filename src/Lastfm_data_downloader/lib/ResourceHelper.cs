using System.Reflection;

namespace Lastfm_data_downloader.Porter_Packages.MadScience_ReflectionHelpers
{
    /// <summary>
    /// A collection of static helper methods for working with reflection-based resources.
    /// </summary>
    public class ResourceHelper
    {
        /// <summary>
        /// Returns the contents of an embedded string resource from a given Assembly.
        /// </summary>
        public static string ReadStringResourceFromCallingAssembly(string resourceFullname)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            return ReadStringResource(assembly, resourceFullname);
        }

      
        /// <summary>
        /// Returns the contents of an embedded string resource from a given Assembly.
        /// </summary>
        public static string ReadStringResource(Assembly assembly, string resourceFullname)
        {

            using (Stream stream = assembly.GetManifestResourceStream(resourceFullname))
            {
                if (stream == null)
                    throw new Exception($"Failed to load resource {resourceFullname} in assembly {assembly.FullName}.");

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}