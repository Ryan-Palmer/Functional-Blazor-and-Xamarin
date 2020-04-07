using System;
using Foundation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using UIKit;
using Xamarin.Essentials;
#nullable enable
namespace FunctionaliOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {
        public static IServiceProvider? RootServiceProvider { get; private set; }

        [Export("window")]
        public UIWindow Window { get; set; }

        [Export("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method
            RootServiceProvider =
                new HostBuilder()
                    .ConfigureHostConfiguration(c =>
                    {
                        c.AddCommandLine(new string[] { $"ContentRoot={FileSystem.AppDataDirectory}" });
                    })
                    .ConfigureServices((ctx, svcs) =>
                    {
                        Startup.ConfigureServices(ctx, svcs);
                    })
                    .ConfigureLogging(l => l.AddConsole(o =>
                    {
                        o.DisableColors = true;
                    }))
                    .Build()
                    .Services;
            return true;
        }

        // UISceneSession Lifecycle

        [Export("application:configurationForConnectingSceneSession:options:")]
        public UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
        {
            // Called when a new scene session is being created.
            // Use this method to select a configuration to create the new scene with.
            return UISceneConfiguration.Create("Default Configuration", connectingSceneSession.Role);
        }

        [Export("application:didDiscardSceneSessions:")]
        public void DidDiscardSceneSessions(UIApplication application, NSSet<UISceneSession> sceneSessions)
        {
            // Called when the user discards a scene session.
            // If any sessions were discarded while the application was not running, this will be called shortly after `FinishedLaunching`.
            // Use this method to release any resources that were specific to the discarded scenes, as they will not return.
        }
    }
}

