#if __ANDROID__ || __IOS__ || __MACOS__
using System;
using Newtonsoft.Json;
using Microsoft.Graph;

namespace UnoDrive.Models
{
	[JsonObject]
	public class Thumbnail
	{
		public string Id { get; set; }
		public ThumbnailImage Large { get; set; }
		public ThumbnailImage Medium { get; set; }
		public ThumbnailImage Small { get; set; }
	}
}
#endif