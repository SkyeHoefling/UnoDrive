#if __ANDROID__ || __IOS__ || __MACOS__
using Newtonsoft.Json;

namespace UnoDrive.Models
{
	[JsonObject]
    public class DriveItemCollection
    {
		[JsonProperty("value")]
		public DriveItem[] Value { get; set; }
    }
}
#endif