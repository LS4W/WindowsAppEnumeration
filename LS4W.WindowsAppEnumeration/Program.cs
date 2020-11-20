using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using LS4W.WindowsAppEnumeration.Domain;
using Microsoft.Win32;
using TsudaKageyu;

namespace LS4W.WindowsAppEnumeration
{
    class Program
    {
        public const string AppPathRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

        static void Main(string[] args)
        {
            bool exportBase64 = true;

            List<WindowsApp> installedPrograms = new List<WindowsApp>();

            //Get executables
            using var key = Registry.LocalMachine.OpenSubKey(AppPathRegistryKey);
            foreach (string subkeyName in key.GetSubKeyNames())
            {
                using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                {
                    object rawLocation = subkey.GetValue("");
                    object rawPath = subkey.GetValue("Path");
                    if (rawLocation is null)
                        continue;
                    if (rawPath is null)
                        continue;

                    var app = new WindowsApp();
                    app.ExecutableLocation = rawLocation.ToString();
                    app.InstallLocation = rawPath.ToString();

                    if (!File.Exists(app.ExecutableLocation))
                        continue;

                    string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string iconPath = assemblyPath + @"\icons\";
                    if (!Directory.Exists(iconPath))
                    {
                        Directory.CreateDirectory(iconPath);
                    }

                    IconExtractor ie = new IconExtractor(app.ExecutableLocation);
                    if (ie.Count == 0) //contains no icons
                        continue;

                    var iconVariations = IconUtil.Split(ie.GetIcon(0));
                    foreach (var (icon, index) in iconVariations.WithIndex())
                    {
                        string iconExportName = $"{Path.GetFileName(app.ExecutableLocation)}_{index}.png";
                        var iconAsBitmap = icon.ToBitmap();
                        iconAsBitmap.Save(iconPath + iconExportName, ImageFormat.Png);
                        app.IconPaths.Add(iconPath + iconExportName);
                    }

                    if (exportBase64)
                    {
                        var largestIcon = iconVariations.OrderByDescending(icon => icon.Width).First();
                        var iconAsBitmap = largestIcon.ToBitmap();
                        using (MemoryStream ms = new MemoryStream())
                        {
                            iconAsBitmap.Save(ms, ImageFormat.Png);
                            byte[] iconAsBytes = ms.ToArray();
                            app.IconB64 = Convert.ToBase64String(iconAsBytes);
                        }
                    }

                    installedPrograms.Add(app);
                }
            }

            string jsonString = JsonSerializer.Serialize(installedPrograms);
            Console.WriteLine(jsonString);
        }
    }
}