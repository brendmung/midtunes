using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Window;
using AndroidX.Activity;
using AndroidX.AppCompat.App;
using AndroidX.Core.OS;
using DeepSound.Activities.SettingsUser;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Utils;
using System;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Base
{
    [Activity]
    public class BaseActivity : AppCompatActivity
    {
        #region General

        public void InitBackPressed(string pageName = "")
        {
            try
            {
                if (BuildCompat.IsAtLeastT && Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, new BackCallAppBase2(this, pageName));
                }
                else
                {
                    OnBackPressedDispatcher.AddCallback(new BackCallAppBase1(this, pageName, true));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                //Glide.With(this).OnTrimMemory(level);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                // Glide.With(this).OnLowMemory();
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        SharedPref.ApplyTheme(SharedPref.LightMode);
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        SharedPref.ApplyTheme(SharedPref.DarkMode);
                        break;
                }

                Delegate.SetLocalNightMode(DeepSoundTools.IsTabDark() ? AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion 
    }

    public class BackCallAppBase1 : OnBackPressedCallback
    {
        private readonly Activity Activity;
        private readonly string PageName;

        public BackCallAppBase1(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public BackCallAppBase1(bool enabled) : base(enabled)
        {
        }

        public BackCallAppBase1(Activity activity, string pageName, bool enabled) : base(enabled)
        {
            Activity = activity;
            PageName = pageName;
        }

        public override void HandleOnBackPressed()
        {
            try
            {
                if (string.IsNullOrEmpty(PageName))
                {
                    // Back is pressed... Finishing the activity
                    Activity?.Finish();
                }
                else
                {
                    BackCallDeepSoundTools.OnBackPressed(Activity, PageName);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class BackCallAppBase2 : Object, IOnBackInvokedCallback
    {
        private readonly Activity Activity;
        private readonly string PageName;

        public BackCallAppBase2(Activity activity, string pageName)
        {
            Activity = activity;
            PageName = pageName;
        }

        public void OnBackInvoked()
        {
            try
            {
                if (string.IsNullOrEmpty(PageName))
                {
                    // Back is pressed... Finishing the activity
                    Activity?.Finish();
                }
                else
                {
                    BackCallDeepSoundTools.OnBackPressed(Activity, PageName);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public static class BackCallDeepSoundTools
    {
        public static void OnBackPressed(Activity activity, string pageName)
        {
            try
            {
                if (string.IsNullOrEmpty(pageName))
                {
                    // Back is pressed... Finishing the activity
                    activity?.Finish();
                }
                else switch (pageName)
                    {
                        case "HomeActivity":
                            {
                                var subActivity = activity as HomeActivity;
                                subActivity?.BackPressed();
                                break;
                            }

                    }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}