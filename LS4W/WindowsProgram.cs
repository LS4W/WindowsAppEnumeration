using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LS4W
{
    class WindowsProgram
    {
        public string executableLocation { get; set; }
        public string installLocation { get; set; }
        public string publisherName { get; set; }
        public string applicationName { get; set; }
        public List<string> iconPaths { get; set; }
        public string iconB64 { get; set; }
    }
}
