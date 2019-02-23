using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureOrganizer
{
    public class Settings
    {
        public string ArchivePath { get; set; }
        public string DuplicatePath { get; set; }
        public string KeepPath { get; set; }
        public string StatusFile { get; set; }
    }
}
