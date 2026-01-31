using System.Reflection;

namespace DataTransferApp.Net.Helpers
{
    public static class VersionHelper
    {
        private static Version? _version;

        /// <summary>
        /// Gets the current application version from the executing assembly.
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            if (_version == null)
            {
                _version = Assembly.GetExecutingAssembly().GetName().Version;
            }

            return _version?.ToString(3) ?? "1.3.2";
        }

        /// <summary>
        /// Gets the current application version with 'v' prefix.
        /// </summary>
        /// <returns></returns>
        public static string GetVersionWithPrefix()
        {
            return $"v{GetVersion()}";
        }
    }
}