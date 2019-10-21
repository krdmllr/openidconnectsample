using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using Acr.UserDialogs;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Shared;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace MobileApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private string _currentAccessToken;
        private string _currentRefreshToken;

        public MainPage()
        {
            InitializeComponent();
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
        }

        private async void StartAuthenticationClicked(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading();
                var browser = DependencyService.Get<IBrowser>();

                var options = new OidcClientOptions
                {
                    Authority = Constants.Authority,
                    ClientId = Constants.MobileClientId,
                    Scope = "openid profile email api offline_access",
                    RedirectUri = "xamarinclient://callback",
                    Browser = browser,

                    ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
                };

                var client = new OidcClient(options);
                var loginResult = await client.LoginAsync(new LoginRequest());

                StoreAndShowData(loginResult.AccessToken, loginResult.IdentityToken, loginResult.RefreshToken);

                RefreshTokenButton.IsEnabled = true;
                ClaimsSection.Clear();
                loginResult.User.Claims.ForEach(x => ClaimsSection.Add(new TextCell
                {
                    Text = x.Type,
                    Detail = x.Value
                }));
                Device.BeginInvokeOnMainThread(async () => await UserDialogs.Instance.AlertAsync("Authenticated"));
            }
            finally
            {
                Device.BeginInvokeOnMainThread(()=> UserDialogs.Instance.HideLoading());
            }
        }

        private void StoreAndShowData(string accessToken, string idToken, string refreshToken)
        {
            _currentAccessToken = accessToken;
            _currentRefreshToken = refreshToken;

            TokenSection.Clear();
            foreach (var token in new Dictionary<string, string>
            {
                { "Access token", accessToken },
                { "Id token", idToken },
                { "Refresh token", refreshToken },
            })
            {
                TokenSection.Add(new ViewCell
                { 
                    View = new StackLayout()
                    {
                        Children =
                        {
                            new Label
                            {
                                FontAttributes = FontAttributes.Bold,
                                Text = token.Key
                            },
                            new Editor()
                            {
                                Text = token.Value
                            }
                        }
                    } 
                });
                TokenSection.Last().ForceUpdateSize();
            } 
        }

        private async void RefreshToken(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading();
                var client = new HttpClient();

                var refreshResult = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = Constants.Authority + "connect/token",
                    ClientId = Constants.MobileClientId,
                    RefreshToken = _currentRefreshToken
                });

                StoreAndShowData(refreshResult.AccessToken, refreshResult.IdentityToken, refreshResult.RefreshToken);
                await UserDialogs.Instance.AlertAsync("Refreshed token");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void RequestButtonClicked(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading();
                var client = new HttpClient() // Dont do this at home! HttpClient should not get initialized for each request, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.0
                {
                    BaseAddress = new Uri(Constants.SampleApiUrl)
                };
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_currentAccessToken}");
                var text = await client.GetStringAsync("api/sample");
                RequestResultCell.Text = text;
                await UserDialogs.Instance.AlertAsync(text, "Test data loaded");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
    }
}
