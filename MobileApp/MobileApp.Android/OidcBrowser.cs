using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.CustomTabs;
using IdentityModel.OidcClient.Browser;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Application = Android.App.Application;

namespace MobileApp.Droid
{
    public class OidcBrowser : IBrowser
    {
        private readonly Activity _context;
        private readonly CustomTabsActivityManager _manager; 

        public OidcBrowser()
        {
            _context = Application.Context.GetActivity();
            _manager = new CustomTabsActivityManager(MainActivity.CurrentActivity);
        }

        public Task<BrowserResult> InvokeAsync(BrowserOptions options)
        {
            var task = new TaskCompletionSource<BrowserResult>();

            var builder = new CustomTabsIntent.Builder(_manager.Session)
                .SetToolbarColor(Color.Accent.ToAndroid())
                .SetShowTitle(true)
                .EnableUrlBarHiding();

            var customTabsIntent = builder.Build();

            // ensures the intent is not kept in the history stack, which makes
            // sure navigating away from it will close it
            customTabsIntent.Intent.AddFlags(ActivityFlags.NoHistory);

            Action<string> callback = null;
            callback = url =>
            {
                OidcCallbackActivity.Callbacks -= callback;

                task.SetResult(new BrowserResult()
                {
                    Response = url
                });
            };

            OidcCallbackActivity.Callbacks += callback;

            customTabsIntent.LaunchUrl(MainActivity.CurrentActivity, Android.Net.Uri.Parse(options.StartUrl));

            return task.Task;
        }
    }
}