using System;
using System.Text.Json;

namespace LS4W.WindowsAppEnumeration
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = Configuration.GetConfiguration(args);
            var appEnumerator = new AppEnumerator(configuration);
            var installedPrograms = appEnumerator.GetWindowsApps();
            string jsonString = JsonSerializer.Serialize(installedPrograms);
            Console.WriteLine(jsonString);
        }
    }
}