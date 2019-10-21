using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
using IdentityModel.OidcClient.Browser;
using SafariServices;
using UIKit;

namespace MobileApp.iOS
{
    public class iOSOidcBrowser : IBrowser
    {
        private ASWebAuthenticationSession _authenticationSession;

        public Task<BrowserResult> InvokeAsync(BrowserOptions options)
        {
            // The task will be finished when the result of the authentication comes in
            var wait = new TaskCompletionSource<BrowserResult>();

            _authenticationSession = new ASWebAuthenticationSession(
                            new NSUrl(options.StartUrl),
                            options.EndUrl,
                            (callbackUrl, error) =>
                            {
                                if (error != null)
                                {
                                    var errorResult = new BrowserResult
                                    {
                                        ResultType = BrowserResultType.UserCancel,
                                        Error = error.ToString()
                                    };

                                    wait.SetResult(errorResult);
                                }
                                else
                                {
                                    var result = new BrowserResult
                                    {
                                        ResultType = BrowserResultType.Success,
                                        Response = callbackUrl.AbsoluteString
                                    };

                                    Debug.WriteLine("Callback: " + callbackUrl.AbsoluteString);

                                    wait.SetResult(result);
                                }
                            });

            // iOS 13 requires the PresentationContextProvider set
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 1))
            {
                _authenticationSession.PresentationContextProvider = new PresentationContextProviderToSharedKeyWindow();
            }


            _authenticationSession.Start();
            return wait.Task;
        } 

        class PresentationContextProviderToSharedKeyWindow : NSObject, IASWebAuthenticationPresentationContextProviding
        {
            public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
            {
                return UIApplication.SharedApplication.KeyWindow;
            }
        }
    }
}