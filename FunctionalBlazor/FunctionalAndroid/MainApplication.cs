using Android.App;
using Android.Runtime;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using Xamarin.Essentials;
#nullable enable

namespace FunctionalAndroid
{
    [Application(Theme = "@style/AppTheme")]
    public class MainApplication : Android.App.Application
    {
        public static IServiceProvider? RootServiceProvider { get; private set; }

        public MainApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

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
        }
    }
}