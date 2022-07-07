#if __ANDROID__ || __IOS__ || __MACOS__
using System;
using System.Text.Json.Serialization;
using Microsoft.Graph;

namespace UnoDrive.Models
{
	public class Thumbnail
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("large")]
		public ThumbnailImage Large { get; set; }

		[JsonPropertyName("medium")]
		public ThumbnailImage Medium { get; set; }

		[JsonPropertyName("small")]
		public ThumbnailImage Small { get; set; }
	}
}
#endif