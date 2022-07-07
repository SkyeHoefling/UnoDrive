#if __ANDROID__ || __IOS__ || __MACOS__
using System;
using System.Text.Json.Serialization;
using Microsoft.Graph;

namespace UnoDrive.Models
{
	public class DriveItem
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("parentReference")]
		public ItemReference ParentReference { get; set; }

		[JsonPropertyName("size")]
		public Int64? Size { get; set; }

		[JsonPropertyName("lastModifiedDateTime")]
		public DateTimeOffset? LastModifiedDateTime { get; set; }

		[JsonPropertyName("thumbnails")]
		public Thumbnail[] Thumbnails { get; set; }

		[JsonPropertyName("folder")]
		public Folder Folder { get; set; }
	}
}
#endif