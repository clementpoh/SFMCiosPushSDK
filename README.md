# Description

Xamarin binding for the version 6.1.4 of the Salesforce Marketing Cloud iOS
Mobile Push SDK and an accompanying app to demonstrate the basic features of
the SDK.

The SfmcSdkBinding project is the Xamarin binding, this can just be included as
a reference in another Xamarin project. It's important to note the SDK
communicates through APNs, which does not work on iOS simulators.

The BindingDemonstration project is the demonstration app; it’s a bare-bones
app with minimal features to keep things simple. The instance specific
Marketing Cloud configurations are in MarketingCloudSDKConfiguration.json.

NotificationExtension is a Notification Extension Service to enable rich media
notifications in the Binding Demonstration app.

Most of the configuraton occurs in the AppDelegate of the app, where the SDK is
initialised and configured there and the appropriate lifecycle call backs are
handled.  NotificationService.cs is the only real file of note in the
NotificationExtension; however it’s important that the targets of the service
extension and iOS app match in their Info.plist files.

ApiDefinitions.cs in the SfmcSdkBinding project can be updated to rename or
reconfigure the interfaces and methods between the Objective C and the C#
bindings if necessary.
