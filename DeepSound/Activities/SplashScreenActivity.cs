using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using DeepSound.Activities.Default;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using DeepSound.Library.OneSignalNotif.Models;
using DeepSound.SQLite;
using Exception = System.Exception;

namespace DeepSound.Activities
{
    [Activity(Icon = "@mipmap/icon", MainLauncher = true, NoHistory = true, Theme = "@style/SplashScreenTheme", Exported = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault, Intent.CategoryAppBrowser }, DataSchemes = new[] { "http", "https", "app" }, DataHosts = new[] { "@string/ApplicationUrlWeb", "@string/ApplicationShortUrl" }, AutoVerify = true)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault, Intent.CategoryAppBrowser }, DataSchemes = new[] { "http", "https", "app" }, DataHosts = new[] { "@string/ApplicationUrlWeb", "@string/ApplicationShortUrl" }, DataPathPrefixes = new[] { "/track/", "/reset-password/", "ref=" }, AutoVerify = true)]
    public class SplashScreenActivity : AppCompatActivity
    {
        #region Variables Basic

        public static bool SplashWasShown = true;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                {
                    AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);
                }

                base.OnCreate(savedInstanceState);

                Task startupWork = new Task(FirstRunExcite);
                startupWork.Start();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FirstRunExcite()
        {
            try
            {
                if (!string.IsNullOrEmpty(AppSettings.Lang))
                {
                    LangController.SetApplicationLang(this, AppSettings.Lang);
                }
                else
                {
#pragma warning disable 618
                    UserDetails.LangName = (int)Build.VERSION.SdkInt < 25 ? Resources?.Configuration?.Locale?.Language.ToLower() : Resources?.Configuration?.Locales.Get(0)?.Language.ToLower() ?? Resources?.Configuration?.Locale?.Language.ToLower();
#pragma warning restore 618
                    LangController.SetApplicationLang(this, UserDetails.LangName);
                }

                if (Intent?.Data != null)
                {
                    if (Intent.Data.ToString()!.Contains("track"))
                    {
                        //https://demo.deepsoundscript.com/track/uxIRQ5QcwLnf8oh
                        var trackId = Intent.Data.ToString()!.Split("/track/")?.LastOrDefault()?.Replace("/", "") ?? "";

                        var intent = new Intent(this, typeof(HomeActivity));
                        var dataNotification = new OsObject.OsNotificationObject() { TrackId = trackId, Type = "track" };
                        intent.PutExtra("OsNotificationObject", JsonConvert.SerializeObject(dataNotification));
                        switch (UserDetails.Status)
                        {
                            case "Active":
                                UserDetails.IsLogin = true;
                                StartActivity(intent);
                                break;
                            case "Pending":
                                UserDetails.IsLogin = false;
                                StartActivity(intent);
                                break;
                            default:
                                StartActivity(new Intent(this, typeof(FirstActivity)));
                                break;
                        }
                    }
                    else if (Intent.Data.ToString()!.Contains("/reset-password"))
                    {
                        //https://demo.deepsoundscript.com/reset-password/76c2b38d1e5d564bbb58577310a2f7d884f6d7d6
                        var code = Intent.Data.ToString()!.Split("/reset-password/")?.LastOrDefault()?.Replace("/", "") ?? "";

                        var intent = new Intent(this, typeof(ResetPasswordActivity));
                        intent.PutExtra("EmailCode", code);
                        StartActivity(intent);
                    }
                    else if (Intent.Data.ToString()!.Contains("ref=") && !string.IsNullOrEmpty(UserDetails.AccessToken))
                    {
                        //https://demo.deepsoundscript.com/?ref=admin 
                        var referral = Intent.Data.ToString()!.Split("?ref=")?.LastOrDefault() ?? "";

                        var intent = new Intent(this, typeof(RegisterActivity));
                        intent.PutExtra("Referral", referral);
                        StartActivity(intent);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                        {
                            switch (UserDetails.Status)
                            {
                                case "Active":
                                    UserDetails.IsLogin = true;
                                    StartActivity(new Intent(this, typeof(HomeActivity)));
                                    break;
                                case "Pending":
                                    UserDetails.IsLogin = false;
                                    StartActivity(new Intent(this, typeof(HomeActivity)));
                                    break;
                                default:
                                    StartActivity(new Intent(this, typeof(FirstActivity)));
                                    break;
                            }
                        }
                        else
                        {
                            switch (UserDetails.Status)
                            {
                                case "Active":
                                case "Pending":
                                    StartActivity(new Intent(this, typeof(HomeActivity)));
                                    break;
                                default:
                                    StartActivity(new Intent(this, typeof(FirstActivity)));
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                    {
                        switch (UserDetails.Status)
                        {
                            case "Active":
                                UserDetails.IsLogin = true;
                                StartActivity(new Intent(this, typeof(HomeActivity)));
                                break;
                            case "Pending":
                                UserDetails.IsLogin = false;
                                StartActivity(new Intent(this, typeof(HomeActivity)));
                                break;
                            default:
                                StartActivity(new Intent(this, typeof(FirstActivity)));
                                break;
                        }
                    }
                    else
                    {
                        switch (UserDetails.Status)
                        {
                            case "Active":
                            case "Pending":
                                StartActivity(new Intent(this, typeof(HomeActivity)));
                                break;
                            default:
                                StartActivity(new Intent(this, typeof(FirstActivity)));
                                break;
                        }
                    }
                }

                OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}