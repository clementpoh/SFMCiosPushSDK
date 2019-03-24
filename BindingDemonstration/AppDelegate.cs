using Foundation;
using UIKit;
using UserNotifications;
using System;

using SfmcSdkBinding;

namespace BindingDemonstration {
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate, IUNUserNotificationCenterDelegate {

		public override UIWindow Window {
			get;
			set;
		}

		private class UrlHandler : MarketingCloudSDKURLHandlingDelegate {
			public override void HandleURL(NSUrl url, string type) {
				Console.WriteLine(string.Format("HandleURL: {0} {1}", type, url));

				if (UIApplication.SharedApplication.CanOpenUrl(url)) {
					Console.WriteLine(string.Format("Can open: " + url));
					var options = new NSDictionary();
					UIApplication.SharedApplication.OpenUrl(url, options, (success) => Console.WriteLine("OpenURL: " + success));
				} else {
					Console.WriteLine("Cannot open URL: " + url);
				}
			}
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions) {
            if (MarketingCloudSDK.SharedInstance.Configure(out NSError configError)) {
				MarketingCloudSDK.SharedInstance.SetDebugLoggingEnabled(true);
				MarketingCloudSDK.SharedInstance.StartWatchingLocation();
				MarketingCloudSDK.SharedInstance.SetURLHandlingDelegate(new UrlHandler());
				MarketingCloudSDK.SharedInstance.SetContactKey("iOS Device");

				// For iOS 10 display notification (sent via APNS)
				if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
					UNUserNotificationCenter.Current.Delegate = this;

					var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
					UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) => {
						Console.WriteLine("Push Authorised: " + granted);

						if (error == null) {
							InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
						} else {
							Console.WriteLine("Error registering remote notifications: " + error);
						}
					});
				} else {
					InvokeOnMainThread(() => {
						var notificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
						var settings = UIUserNotificationSettings.GetSettingsForTypes(notificationTypes, null);
						UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
					});
				}
			} else {
				Console.WriteLine("Initialisation Unsuccessful" + configError);
			}

			if (UIApplication.SharedApplication.BackgroundRefreshStatus == UIBackgroundRefreshStatus.Available) {
				Console.WriteLine("Enabling background refresh");
				UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);
			}

			return true;
		}

		[Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken) {
			Console.WriteLine("didRegisterForRemoteNotificationsWithDeviceToken: " + deviceToken);
			MarketingCloudSDK.SharedInstance.SetDeviceToken(deviceToken);
		}

		[Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error) {
			Console.WriteLine("didFailToRegisterForRemoteNotificationsWithError: " + error);
		}

		[Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
		public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler) {
			Console.WriteLine("DidReceiveNotificationResponse: " + response);
			
            MarketingCloudSDK.SharedInstance.SetNotificationRequest(response.Notification.Request);
			completionHandler?.Invoke();
		}

		[Export("application:didReceiveRemoteNotification:fetchCompletionHandler:")]
		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler) {
			Console.WriteLine("DidReceiveRemoteNotification: " + userInfo);

			var request = UNNotificationRequest.FromIdentifier(new NSUuid().ToString(), new UNMutableNotificationContent { UserInfo = userInfo }, null);
			MarketingCloudSDK.SharedInstance.SetNotificationRequest(request);

			completionHandler.Invoke(UIBackgroundFetchResult.NewData);
		}
	}
}

