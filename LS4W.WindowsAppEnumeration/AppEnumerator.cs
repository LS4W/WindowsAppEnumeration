using System;
using System.Collections.Generic;
using System.IO;
using LS4W.WindowsAppEnumeration.Domain;
using Microsoft.Win32;
using Toolbelt.Drawing;

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
                using var ms = new MemoryStream();
                IconExtractor.Extract1stIconTo(app.ExecutableLocation, ms);
                app.IconPaths = CopyIcons(ms, app.ExecutableLocation);
                app.IconB64 = ConvertIconToBase64(ms);

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

        private List<string> CopyIcons(MemoryStream ms, string executableLocation)
        {
            var paths = new List<string>();

            if (!_config.CopyIcon)
                return paths;

            ms.Seek(0, SeekOrigin.Begin);
            var path = $"{Path.GetFileName(executableLocation)}.png";
            using var fileStream = File.Create(path);
            ms.CopyTo(fileStream);
            fileStream.Close();
            paths.Add(path);

            return paths;
        }

        private string ConvertIconToBase64(MemoryStream ms)
        {
            if (!_config.ExportAsBase64)
                return null;

            ms.Seek(0, SeekOrigin.Begin);
            var iconAsBytes = ms.ToArray();
            return Convert.ToBase64String(iconAsBytes);
        }
    }
}