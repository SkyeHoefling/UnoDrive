using System;
using System.ComponentModel.DataAnnotations;

namespace UnoDrive.Data
{
	public enum OneDriveItemType
	{
		File = 0,
		Folder = 1
	}

	public class OneDriveItem
	{
		// Android, iOS, MacOS - https://stackoverflow.com/questions/67364599/is-not-surpported-system-componentmodel-dataannotations-in-uno-platform
		// Add system reference to `System.ComponentModel.DataAnnotations`
		[Key]
		public string Id { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string PathId { get; set; }
		public DateTime Modified { get; set; }
		public string ModifiedBy { get; set; }
		public string FileSize { get; set; }
		public string Sharing { get; set; }
		public OneDriveItemType Type { get; set; }
		public string ThumbnailPath { get; set; }
	}
}
