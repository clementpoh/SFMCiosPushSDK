using System;
using Foundation;
using UIKit;
using UserNotifications;

namespace NotificationExtension {
	[Register("NotificationService")]
	public class NotificationService : UNNotificationServiceExtension {
		Action<UNNotificationContent> ContentHandler { get; set; }
		UNMutableNotificationContent Content { get; set; }

		protected NotificationService(IntPtr handle) : base(handle) {
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void DidReceiveNotificationRequest(UNNotificationRequest request, Action<UNNotificationContent> contentHandler) {
			Console.WriteLine("NotificationExtension: DidReceiveNotificationRequest");

			ContentHandler = contentHandler;
			Content = request.Content.MutableCopy() as UNMutableNotificationContent;

			var urlString = request.Content.UserInfo.ObjectForKey(new NSString("_mediaUrl"));
			var url = new NSUrl(urlString.ToString());

			if (url == null) {
				Console.WriteLine("Returning original content");
				ContentHandler(Content);
				return;
			}

			Console.WriteLine("Downloading media");
			// Start a download task to handle the download of the media
			NSUrlSession.SharedSession.CreateDownloadTask(url, (location, response, error) => {
				Console.WriteLine("Downloaded: " + location);

				// Download was successful, attempt save the media file
				var localUrl = NSUrl.CreateFileUrl(new string[] { location.RemoveLastPathComponent().Path, url.LastPathComponent });
				Console.WriteLine("Save Locaton: " + localUrl);

				// Remove any existing file with the same name
				NSFileManager.DefaultManager.Remove(localUrl, out NSError removeError);
				Console.WriteLine(string.Format("Remove: {0}", removeError == null ? "Successful" : removeError.ToString()));

				// Move the downloaded file from the temporary location to a new file
				if (NSFileManager.DefaultManager.Move(location, localUrl, out NSError moveError)) {
					// Create attachment
					var attachment = UNNotificationAttachment.FromIdentifier("image", localUrl, new UNNotificationAttachmentOptions(), out NSError attachError);
					Console.WriteLine(string.Format("Attach: {0}", attachError == null ? "Successful" : attachError.ToString()));

					// Modify contents
					Content.Attachments = new UNNotificationAttachment[] { attachment };

					// Display notification
					ContentHandler(Content);
				} else {
					Console.WriteLine("Move Error: " + moveError);

					// Revert to the original content
					ContentHandler(Content);
				}
			}).Resume();
		}

		public override void TimeWillExpire() {
			// Called just before the extension will be terminated by the system.
			Console.WriteLine("NotificationService: TimeWillExpire");

			// Use the alt-text if there's no time left
			var altText = Content.UserInfo.ObjectForKey(new NSString("_mediaAlt"));
			Content.Body = altText.ToString() ?? Content.Body;

			ContentHandler(Content);
		}
	}
}
