using System;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace UnoDrive.Mvvm
{
    public abstract class MvvmApplication : Application
    {
        public MvvmApplication()
        {
            Container = ConfigureDependencyInjection();
        }

        public IServiceProvider Container { get; }
        
        IServiceProvider ConfigureDependencyInjection()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }

        protected abstract void ConfigureServices(IServiceCollection services);
    }
}
