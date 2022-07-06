using System;
using LiteDB;
using Microsoft.UI.Xaml.Media;

namespace UnoDrive.Data
{
	public class OneDriveItem
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string PathId { get; set; }
		public DateTime Modified { get; set; }
		public string FileSize { get; set; }
		public OneDriveItemType Type { get; set; }
		public string ThumbnailPath { get; set; }

		[BsonIgnore]
		public ImageSource ThumbnailSource { get; set; }
	}
}
