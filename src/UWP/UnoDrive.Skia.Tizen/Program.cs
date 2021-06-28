using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace UnoDrive.Skia.Tizen
{
	class Program
{
	static void Main(string[] args)
	{
		var host = new TizenHost(() => new UnoDrive.App(), args);
		host.Run();
	}
}
}
