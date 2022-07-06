#if __ANDROID__
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