#if __WASM__
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnoDrive.Logging
{
	public partial class LoggingConfiguration
    {
		public partial void ConfigureLogging(IServiceCollection services)
		{
			services.AddLogging(builder =>
			{
				builder
					.ClearProviders()
					.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider())
#if DEBUG
					.AddFilter("UnoDrive", LogLevel.Information)
#else
					.AddFilter("UnoDrive", LogLevel.Debug)
#endif
					.AddFilter("Uno", LogLevel.Debug)
					.AddFilter("Windows", LogLevel.Debug)
					.AddFilter("Microsoft", LogLevel.Debug)
					.AddDebug();
			});
		}
    }
}
#endif