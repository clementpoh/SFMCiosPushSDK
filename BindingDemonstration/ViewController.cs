using Foundation;
using SfmcSdkBinding;
using System;
using UIKit;
using Xamarin.Essentials;

namespace BindingDemonstration {
	public partial class ViewController : UIViewController {
		public ViewController(IntPtr handle) : base(handle) {
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();

			StatusText.Text += string.Format("Push Enabled: {0}\n", MarketingCloudSDK.SharedInstance.PushEnabled);
			StatusText.Text += string.Format("SDK State:\n {0}\n", MarketingCloudSDK.SharedInstance.GetSDKState());
			StatusText.Text += string.Format("SDK Location:\n {0}\n", MarketingCloudSDK.SharedInstance.LastKnownLocation());

			InvokeInBackground(async () => {
				var location = await Geolocation.GetLastKnownLocationAsync();
				InvokeOnMainThread(() => StatusText.Text += "Last Known Location:\n" + location);
			});
		}

		public override void DidReceiveMemoryWarning() {
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}