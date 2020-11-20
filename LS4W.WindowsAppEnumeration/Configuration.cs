using System.IO;
using System.Reflection;

namespace LS4W.WindowsAppEnumeration
{
    public class Configuration
    {
        public bool ExportAsBase64 { get; set; }
        public bool CopyIcon { get; set; }
        public string IconCopyPath { get; set; }
        public string AppPathRegistryKey { get; set; } = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

        private static string GetIconDefaultCopyPath()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var iconPath = assemblyPath + @"\icons\";
            if (!Directory.Exists(iconPath))
            {
                Directory.CreateDirectory(iconPath);
            }

            return iconPath;
        }

        //We can deal with all the args and stuff in here, for now I am hard coding defaults
        public static Configuration GetConfiguration(string[] args)
        {
            var config = new Configuration
            {
                ExportAsBase64 = true,
                CopyIcon = true
            };
            config.IconCopyPath = GetIconDefaultCopyPath();
            return config;
        }
    }
}