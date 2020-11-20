using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using LS4W.WindowsAppEnumeration.Domain;
using Microsoft.Win32;
using TsudaKageyu;

namespace LS4W.WindowsAppEnumeration
{
    public class AppEnumerator
    {
        private readonly Configuration _config;

        public AppEnumerator(Configuration config)
        {
            _config = config;
        }

        public IEnumerable<WindowsApp> GetWindowsApps()
        {
            foreach (var app in GetExecutablePaths())
            {
                var icons = Icon.ExtractAssociatedIcon(app.ExecutableLocation);
                var iconVariations = IconUtil.Split(icons);
                app.IconPaths = CopyIcons(iconVariations, app.ExecutableLocation);
                app.IconB64 = ConvertLargestIconToBase64(iconVariations);

                yield return app;
            }
        }


        private IEnumerable<WindowsApp> GetExecutablePaths()
        {
            using var key = Registry.LocalMachine.OpenSubKey(_config.AppPathRegistryKey);
            foreach (var subkeyName in key.GetSubKeyNames())
            {
                using var subkey = key.OpenSubKey(subkeyName);
                var rawLocation = subkey?.GetValue("");
                var rawPath = subkey?.GetValue("Path");
                if (!(rawLocation is string location) || !(rawPath is string path))
                    continue;
                if (!File.Exists(location))
                    continue;
                yield return new WindowsApp
                {
                    ExecutableLocation = location,
                    InstallLocation = path
                };
            }
        }

        private List<string> CopyIcons(IEnumerable<Icon> icons, string executableLocation)
        {
            var paths = new List<string>();

            if (!_config.CopyIcon)
                return paths;

            foreach (var (icon, index) in icons.WithIndex())
            {
                var iconExportName = $"{Path.GetFileName(executableLocation)}_{index}.png";
                var iconAsBitmap = icon.ToBitmap();
                iconAsBitmap.Save(_config.IconCopyPath + iconExportName, ImageFormat.Png);
                paths.Add($"{_config.IconCopyPath}{iconExportName}");
            }

            return paths;
        }

        private string ConvertLargestIconToBase64(IEnumerable<Icon> icons)
        {
            if (!_config.ExportAsBase64)
                return null;
            var largestIcon = icons.OrderByDescending(icon => icon.Width).First();
            var iconAsBitmap = largestIcon.ToBitmap();
            using var ms = new MemoryStream();

            iconAsBitmap.Save(ms, ImageFormat.Png);
            var iconAsBytes = ms.ToArray();
            return Convert.ToBase64String(iconAsBytes);
        }
    }
}