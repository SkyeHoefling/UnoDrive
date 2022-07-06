using Microsoft.Extensions.DependencyInjection;

namespace UnoDrive.Logging
{
	public static class LoggingExtensions
    {
		public static void AddLoggingForUnoDrive(this IServiceCollection services)
		{
			LoggingConfiguration configuration = new LoggingConfiguration();
			configuration.ConfigureLogging(services);
		}
	}
}
