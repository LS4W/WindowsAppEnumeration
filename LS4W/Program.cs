using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Globalization;
using TsudaKageyu;
using extensionMethods;

namespace LS4W
{

    class Program
    {

        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            bool exportBase64 = true;

            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            //App.Main();
            //global::App.Main();
            //LS4W.App.Main();

            List<WindowsProgram> installedPrograms = new List<WindowsProgram>(); 
            string registryKey_AppPaths = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

            //Get executables
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey_AppPaths))
            {
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

                        WindowsProgram app = new WindowsProgram();
                        app.executableLocation = rawLocation.ToString();
                        app.installLocation = rawPath.ToString();
                        app.iconPaths = new List<string>();


                        if (!File.Exists(app.executableLocation))
                            continue;

                        string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string iconPath = assemblyPath + @"\icons\";
                        if (!Directory.Exists(iconPath))
                        {
                            Directory.CreateDirectory(iconPath);
                        }

                        IconExtractor ie = new IconExtractor(app.executableLocation);
                        if (ie.Count == 0) //contains no icons
                            continue;

                        List<Icon> iconVariations = IconUtil.Split(ie.GetIcon(0)).ToList();
                        foreach(var (icon, index) in iconVariations.WithIndex()) 
                        {
                            string iconExportName = Path.GetFileName(app.executableLocation) + "_" + index + ".png";
                            Bitmap iconAsBitmap = icon.ToBitmap();
                            iconAsBitmap.Save(iconPath + iconExportName, ImageFormat.Png);
                            app.iconPaths.Add(iconPath + iconExportName);
                        }

                        if(exportBase64)
                        {
                            Icon largestIcon = iconVariations.OrderByDescending(icon => icon.Width).First();
                            Bitmap iconAsBitmap = largestIcon.ToBitmap();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                iconAsBitmap.Save(ms, ImageFormat.Png);
                                byte[] iconAsBytes = ms.ToArray();
                                app.iconB64 = Convert.ToBase64String(iconAsBytes);
                            }
                        }

                        installedPrograms.Add(app);
                    }
                }
            }

            string jsonString = JsonSerializer.Serialize(installedPrograms);
            Console.WriteLine(jsonString);
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            var path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false) path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null) return null;

                var assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }

    }
}

