using System.Collections.Generic;

namespace LS4W.WindowsAppEnumeration.Domain
{
    public class WindowsApp
    {
        public WindowsApp()
        {
            IconPaths = new List<string>();
        }
        public string ExecutableLocation { get; set; }
        public string InstallLocation { get; set; }
        public string PublisherName { get; set; }
        public string ApplicationName { get; set; }
        public List<string> IconPaths { get; set; }
        public string IconB64 { get; set; }
    }
}
