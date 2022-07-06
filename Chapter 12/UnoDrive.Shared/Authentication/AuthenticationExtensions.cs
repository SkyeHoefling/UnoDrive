using Microsoft.Extensions.DependencyInjection;

namespace UnoDrive.Authentication
{
	public static class AuthenticationExtensions
    {
		public static void AddAuthentication(this IServiceCollection services)
		{
			AuthenticationConfiguration configuration = new AuthenticationConfiguration();
			configuration.ConfigureAuthentication(services);
		}
	}
}
