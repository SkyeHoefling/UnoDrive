#if __ANDROID__
using System;
using Newtonsoft.Json;
using Microsoft.Graph;

namespace UnoDrive.Models
{
	[JsonObject]
	public class ThumbnailImage
	{
		public int Height { get; set; }
		public int Width { get; set; }
		public string Url { get; set; }
	}
}
#endif