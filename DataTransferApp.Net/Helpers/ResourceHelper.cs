using System.IO;
using System.Reflection;

namespace DataTransferApp.Net.Helpers
{
    /// <summary>
    /// Helper class for loading embedded resources.
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Loads the content of an embedded resource as a string.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource.</param>
        /// <returns>The content of the resource as a string, or an empty string if not found.</returns>
        public static string LoadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return string.Empty;
        }
    }
}