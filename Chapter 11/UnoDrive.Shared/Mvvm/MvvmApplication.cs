using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace UnoDrive.Mvvm
{
	public abstract class MvvmApplication : Application
	{
		public MvvmApplication()
		{
			Current = this;
			Container = ConfigureDependencyInjection();
		}

		public static new MvvmApplication Current { get; private set; }
		public IServiceProvider Container { get; }

		IServiceProvider ConfigureDependencyInjection()
		{
			ServiceCollection services = new ServiceCollection();
			ConfigureServices(services);
			return services.BuildServiceProvider();
		}

		protected abstract void ConfigureServices(IServiceCollection services);
    }
}
