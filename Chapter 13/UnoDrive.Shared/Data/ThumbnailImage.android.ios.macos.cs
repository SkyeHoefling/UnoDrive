#if __ANDROID__ || __IOS__ || __MACOS__
using System;
using System.Text.Json.Serialization;
using Microsoft.Graph;

namespace UnoDrive.Models
{
	public class ThumbnailImage
	{
		[JsonPropertyName("height")]
		public int Height { get; set; }

		[JsonPropertyName("width")]
		public int Width { get; set; }

		[JsonPropertyName("url")]
		public string Url { get; set; }
	}
}
#endif