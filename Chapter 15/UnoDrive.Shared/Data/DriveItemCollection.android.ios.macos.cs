#if __ANDROID__ || __IOS__ || __MACOS__
using System.Text.Json.Serialization;

namespace UnoDrive.Models
{
    public class DriveItemCollection
    {
		[JsonPropertyName("value")]
		public DriveItem[] Value { get; set; }
    }
}
#endif