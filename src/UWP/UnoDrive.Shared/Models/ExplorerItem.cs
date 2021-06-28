using System;

namespace UnoDrive.Models
{
    public class ExplorerItem
    {
        public string Name { get; set; }
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string FileSize { get; set; }
        public string Sharing { get; set; }
    }
}
