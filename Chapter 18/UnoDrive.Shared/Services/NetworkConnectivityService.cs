using System;
using Windows.ApplicationModel.Core;
using Windows.Networking.Connectivity;
using Windows.UI.Core;

namespace UnoDrive.Services
{
	public class NetworkConnectivityService : INetworkConnectivityService
    {
		public NetworkConnectivityService()
		{
			NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
		}

		public NetworkConnectivityLevel Connectivity
		{
			get
			{
				// WORKAROUND - network profile is always null in Skia Linux. Once https://github.com/unoplatform/uno/pull/8621 is merged this can be removed
				// WORKAROUND - network profile is always 'InternetAccess' in WASM windows
#if __WASM__ || (HAS_UNO_SKIA && !HAS_UNO_SKIA_WPF)
				try
				{
					using (var ping = new System.Net.NetworkInformation.Ping())
					{
						var reply = ping.Send(Uno.WinRTFeatureConfiguration.NetworkInformation.ReachabilityHostname);
						return reply?.Status == System.Net.NetworkInformation.IPStatus.Success ? NetworkConnectivityLevel.InternetAccess : NetworkConnectivityLevel.None;
					}
				}
				catch
				{
					// In case exception is thrown, assume
					// connection was not possible
					return NetworkConnectivityLevel.None;
				}
#else
				var profile = NetworkInformation.GetInternetConnectionProfile();
				if (profile == null)
				{
					return NetworkConnectivityLevel.None;
				}

				return profile.GetNetworkConnectivityLevel();
#endif
			}
		}

		public event EventHandler NetworkStatusChanged;

		// This api is invoked from a background thread.
		// We must ensure that it is marshaled to the
		// main UI thread as consumers will want to update
		// the UI on network status change.
		async void OnNetworkStatusChanged(object sender)
		{
			if (NetworkStatusChanged == null)
			{
				return;
			}

			var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
			if (dispatcher == null)
			{
				throw new InvalidOperationException("Unable to find main thread");
			}

			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => NetworkStatusChanged?.Invoke(sender, new EventArgs()));
		}
	}
}
