﻿#if __ANDROID__ || __IOS__ || __MACOS__
using System;
using Newtonsoft.Json;
using Microsoft.Graph;

namespace UnoDrive.Models
{
	[JsonObject]
	public class Folder
	{
		public Int32? ChildCount { get; set; }
	}
}
#endif