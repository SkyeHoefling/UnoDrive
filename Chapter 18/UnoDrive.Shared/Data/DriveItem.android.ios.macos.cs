#if __ANDROID__ || __IOS__ || __MACOS__
using System;
using Newtonsoft.Json;
using Microsoft.Graph;

namespace UnoDrive.Models
{
	[JsonObject]
	public class DriveItem
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public ItemReference ParentReference { get; set; }
		public Int64? Size { get; set; }
		public DateTimeOffset? LastModifiedDateTime { get; set; }
		public Thumbnail[] Thumbnails { get; set; }
		public Folder Folder { get; set; }
	}
}
#endif