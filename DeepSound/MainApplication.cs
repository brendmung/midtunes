//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using AndroidX.Lifecycle;
using Bumptech.Glide;
using DeepSound.Activities;
using DeepSound.Activities.SettingsUser;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient;
using Firebase;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.CurrentActivity;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DeepSound.Activities.Tabbes;
using Xamarin.Android.Net;
using Exception = System.Exception;

namespace DeepSound
{
    //You can specify additional application information in this attribute
    [Application(UsesCleartextTraffic = true)]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        private static MainApplication Instance;
        public Activity Activity;
        public MainApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {

        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                //A great place to initialize Xamarin.Insights and Dependency Services!
                RegisterActivityLifecycleCallbacks(this);
                Instance = this;

                //Bypass Web Errors 
                //======================================
                if (AppSettings.TurnSecurityProtocolType3072On)
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    var client = new HttpClient(new AndroidMessageHandler());
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.SystemDefault;
                    Console.WriteLine(client);
                }

                //If you are Getting this error >>> System.Net.WebException: Error: TrustFailure /// then Set it to true
                if (AppSettings.TurnTrustFailureOnWebException)
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                JsonConvert.DefaultSettings = () => UserDetails.JsonSettings;

                InitializeDeepSound.Initialize(AppSettings.Cert, PackageName, AppSettings.TurnSecurityProtocolType3072On, new MyReportModeApp());

                var sqLiteDatabase = new SqLiteDatabase();
                sqLiteDatabase.CheckTablesStatus();
                sqLiteDatabase.Get_data_Login_Credentials();

                new Handler(Looper.MainLooper).Post(new Runnable(FirstRunExcite));
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
                //Init Settings
                SharedPref.Init();

                Methods.AppLifecycleObserver appLifecycleObserver = new Methods.AppLifecycleObserver();
                ProcessLifecycleOwner.Get().Lifecycle.AddObserver(appLifecycleObserver);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SecondRunExcite(Activity context)
        {
            try
            {
                AdsGoogle.InitializeAdsGoogle.Initialize(context);

                if (AppSettings.ShowFbBannerAds || AppSettings.ShowFbInterstitialAds || AppSettings.ShowFbRewardVideoAds)
                    InitializeFacebook.Initialize(this);

                AdsAppLovin.Initialize(this);

                ClassMapper.SetMappers();

                //App restarted after crash
                AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                //TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

                AppCompatDelegate.CompatVectorFromResourcesEnabled = true;
                FirebaseApp.InitializeApp(this);

                //CrossCurrentActivity.Current.Init(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(SplashScreenActivity));
                intent.AddCategory(Intent.CategoryHome);
                intent.PutExtra("crash", true);
                intent.SetAction(Intent.ActionMain);
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);

                PendingIntent pendingIntent = PendingIntent.GetActivity(GetInstance().BaseContext, 0, intent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.OneShot | PendingIntentFlags.Immutable : PendingIntentFlags.OneShot);
                AlarmManager mgr = (AlarmManager)GetInstance()?.BaseContext?.GetSystemService(AlarmService);
                mgr?.Set(AlarmType.Rtc, JavaSystem.CurrentTimeMillis() + 100, pendingIntent);

                Activity.Finish();
                JavaSystem.Exit(2);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                //var message = e.Exception.Message;
                var stackTrace = e.Exception.StackTrace;

                Methods.DisplayReportResult(Activity, stackTrace);
                Console.WriteLine(e.Exception);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                //var message = e;
                Methods.DisplayReportResult(Activity, e);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public static MainApplication GetInstance()
        {
            return Instance;
        }

        public override void OnTerminate() // on stop
        {
            try
            {
                base.OnTerminate();
                UnregisterActivityLifecycleCallbacks(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityDestroyed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPaused(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityResumed(Activity activity)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            Activity = activity;
        }

        public void OnActivityStarted(Activity activity)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityStopped(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostCreated(Activity activity, Bundle savedInstanceState)
        {
            Activity = activity;
        }

        public void OnActivityPostDestroyed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostPaused(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostResumed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostSaveInstanceState(Activity activity, Bundle outState)
        {
            Activity = activity;
        }

        public void OnActivityPostStarted(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPostStopped(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPreCreated(Activity activity, Bundle savedInstanceState)
        {
            Activity = activity;
        }

        public void OnActivityPreDestroyed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPrePaused(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPreResumed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPreSaveInstanceState(Activity activity, Bundle outState)
        {
            Activity = activity;
        }

        public void OnActivityPreStarted(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPreStopped(Activity activity)
        {
            Activity = activity;
        }

        public override void OnLowMemory()
        {
            try
            {
                base.OnLowMemory();
                Glide.With(this).OnLowMemory();
                GC.Collect(GC.MaxGeneration);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
                Glide.With(this).OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class MyReportModeApp : IReportModeCallBack
    {
        public void OnErrorReportMode(ReportModeObject modeObject)
        {
            try
            {
                if (AppSettings.SetApisReportMode)
                {
                    if (modeObject.Type == "Error")
                    {
                        Methods.DisplayReportResultTrack(modeObject.Exception);
                    }
                    else
                    {
                        string text = "ReportMode >> Member name: " + modeObject.MemberName;
                        text += "\n \n ReportMode >> Parameters Request : " + modeObject.RequestApi;
                        text += "\n \n ReportMode >> Response Api : " + modeObject.ResponseJson;

                        Methods.DialogPopup.InvokeAndShowDialog(HomeActivity.GetInstance(), "ReportMode", text, "Close");
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}