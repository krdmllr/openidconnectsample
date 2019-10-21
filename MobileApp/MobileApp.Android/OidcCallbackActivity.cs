
using System; 
using Android.App;
using Android.Content;
using Android.OS;

namespace MobileApp.Droid
{
    [Activity(Label = "OidcCallbackActivity")]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "xamarinclient")] 
    public class OidcCallbackActivity : Activity
    {
        public static event Action<string> Callbacks;

        public OidcCallbackActivity()
        {
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            System.Diagnostics.Debug.WriteLine("OIDC: " + Intent.DataString);

            Callbacks?.Invoke(Intent.DataString);

            Finish();
        }
    }
}