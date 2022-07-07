#if __ANDROID__ || __IOS__ || __MACOS__
using System;
using System.Text.Json.Serialization;
using Microsoft.Graph;

namespace UnoDrive.Models
{
	public class Folder
	{
		[JsonPropertyName("childCount")]
		public Int32? ChildCount { get; set; }
	}
}
#endif