using System;
using Windows.ApplicationModel.Core;
using Windows.Networking.Connectivity;
using Windows.UI.Core;

#if HAS_UNO_SKIA_WPF
using System.Runtime.InteropServices;
#endif

namespace UnoDrive.Services
{
	public interface INetworkConnectivityService
	{
		event NetworkStatusChangedEventHandler NetworkStatusChanged;
		NetworkConnectivityLevel Connectivity { get; }
	}


	public class NetworkConnectivityService : INetworkConnectivityService
	{
		public NetworkConnectivityService()
		{
			NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
		}

#if HAS_UNO_SKIA_WPF
		[DllImport("wininet.dll")]
		extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
#endif

		public NetworkConnectivityLevel Connectivity
		{
			get
			{
				var profile = NetworkInformation.GetInternetConnectionProfile();
				if (profile == null)
					return NetworkConnectivityLevel.None;

				// This is most likely broken for all SKIA target heads.
#if HAS_UNO_SKIA_WPF
				return InternetGetConnectedState(out _, 0) ?
					NetworkConnectivityLevel.InternetAccess : NetworkConnectivityLevel.None;
#else
				return profile.GetNetworkConnectivityLevel();
#endif
			}
		}

		public event NetworkStatusChangedEventHandler NetworkStatusChanged;

		async void OnNetworkStatusChanged(object sender)
		{
			if (NetworkStatusChanged == null)
				return;

			var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
			if (dispatcher == null)
				throw new InvalidOperationException("Unable to find main thread");

			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => NetworkStatusChanged?.Invoke(sender));
		}
	}
}