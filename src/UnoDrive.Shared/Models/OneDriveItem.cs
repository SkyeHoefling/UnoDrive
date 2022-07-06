using System;

namespace UnoDrive.Models
{
	public enum OneDriveItemType
	{
		File = 0,
		Folder = 1
	}

	public class OneDriveItem
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public DateTime Modified { get; set; }
		public string ModifiedBy { get; set; }
		public string FileSize { get; set; }
		public string Sharing { get; set; }
		public OneDriveItemType Type { get; set; }
	}
}
