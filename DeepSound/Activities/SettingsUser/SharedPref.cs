using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepSound.Activities.SettingsUser
{
    public static class SharedPref
    {
        #region Variables Basic

        public static ISharedPreferences SharedData, InAppReview;
        public static readonly string LightMode = "light";
        public static readonly string DarkMode = "dark";
        public static readonly string DefaultMode = "default";

        private const string InterestedGenres = "INTERESTED_GENRES";
        private const string InterestedGenresSeparator = ",";

        public static readonly string PrefKeyInAppReview = "In_App_Review";

        public static readonly string PrefKeyOptimizationApp = "Optimization_App";

        #endregion

        public static void Init()
        {
            try
            {
                SharedData = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                InAppReview = Application.Context.GetSharedPreferences("In_App_Review", FileCreationMode.Private);

                UserDetails.IsOptimizationApp = SharedData.GetBoolean(PrefKeyOptimizationApp, false);

                string getValue = SharedData.GetString("Night_Mode_key", string.Empty);
                ApplyTheme(getValue);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public static void ApplyTheme(string themePref)
        {
            try
            {
                if (themePref == LightMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    AppSettings.SetTabDarkTheme = TabTheme.Light;
                }
                else if (themePref == DarkMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    AppSettings.SetTabDarkTheme = TabTheme.Dark;
                }
                else if (themePref == DefaultMode)
                {
                    AppCompatDelegate.DefaultNightMode = (int)Build.VERSION.SdkInt >= 29 ? AppCompatDelegate.ModeNightFollowSystem : AppCompatDelegate.ModeNightAutoBattery;

                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;

                    if (currentNightMode == UiMode.NightYes) // Night mode is active, we're using dark theme
                    {
                        AppSettings.SetTabDarkTheme = TabTheme.Dark;
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    }
                    else  // Night mode is not active, we're using the light theme
                    {
                        AppSettings.SetTabDarkTheme = TabTheme.Light;
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    }
                }
                else
                {
                    switch (AppSettings.SetTabDarkTheme)
                    {
                        case TabTheme.Dark:
                            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                            AppSettings.SetTabDarkTheme = TabTheme.Dark;
                            break;
                        case TabTheme.Light:
                            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                            AppSettings.SetTabDarkTheme = TabTheme.Light;
                            break;
                        default:
                            {
                                var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;

                                if (currentNightMode == UiMode.NightYes) // Night mode is active, we're using dark theme
                                {
                                    AppSettings.SetTabDarkTheme = TabTheme.Dark;
                                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                                }
                                else  // Night mode is not active, we're using the light theme
                                {
                                    AppSettings.SetTabDarkTheme = TabTheme.Light;
                                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void StoreInterestedGenresValue(List<int> interestedGenres)
        {
            try
            {
                var data = "";

                data = interestedGenres.Aggregate(data, (current, item) => current + (item + InterestedGenresSeparator));
                SharedData?.Edit()?.PutString(InterestedGenres, data)?.Commit();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static List<int> GetInterestedGenresValue()
        {
            try
            {
                var data = SharedData?.GetString(InterestedGenres, string.Empty);
                if (!string.IsNullOrEmpty(data))
                {
                    var st = new StringTokenizer(data, InterestedGenresSeparator);
                    var result = new List<int>();
                    while (st.HasMoreTokens)
                    {
                        result.Add(int.Parse(st.NextToken()));
                    }

                    return result;
                }
                return new List<int>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new List<int>();
            }
        }
    }
}
