using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Com.Facebook;
using Com.Facebook.Login;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Default;
using DeepSound.Activities.SettingsUser;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignalNotif;
using DeepSound.Service;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Console = System.Console;
using Exception = System.Exception;

namespace DeepSound.Helpers.Controller
{
    internal static class ApiRequest
    {
        public static async Task GetSettings_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await Current.GetOptionsAsync();
                if (apiStatus == 200)
                {
                    if (respond is OptionsObject result)
                    {
                        ListUtils.SettingsSiteList = null;
                        ListUtils.SettingsSiteList = result.DataOptions;

                        AppSettings.OneSignalAppId = result.DataOptions.AndroidMPushId;

                        //Blog Categories
                        var listBlog = result.DataOptions.BlogCategories?.Select(cat => new Classes.Categories
                        {
                            CategoriesId = cat.Key,
                            CategoriesName = Methods.FunString.DecodeString(cat.Value),
                            CategoriesColor = "#ffffff",
                        }).ToList();

                        CategoriesController.ListCategoriesBlog.Clear();
                        CategoriesController.ListCategoriesBlog = listBlog?.Count switch
                        {
                            > 0 => new ObservableCollection<Classes.Categories>(listBlog),
                            _ => CategoriesController.ListCategoriesBlog
                        };

                        //Products Categories
                        var listProducts = result.DataOptions.ProductsCategories?.Select(cat => new Classes.Categories
                        {
                            CategoriesId = cat.Key,
                            CategoriesName = Methods.FunString.DecodeString(cat.Value),
                            CategoriesColor = "#ffffff",
                        }).ToList();

                        CategoriesController.ListCategoriesProducts.Clear();
                        CategoriesController.ListCategoriesProducts = listProducts?.Count switch
                        {
                            > 0 => new ObservableCollection<Classes.Categories>(listProducts),
                            _ => CategoriesController.ListCategoriesProducts
                        };

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertOrUpdateSettings(result.DataOptions);
                    }
                    else Methods.DisplayReportResult(context, respond);
                }
            }
        }

        public static async Task<ProfileObject> GetInfoData(Activity context, string userId)
        {
            if (!UserDetails.IsLogin || userId == "0")
                return null;

            if (Methods.CheckConnectivity())
            {
                string fetch = "followers,following,latest_songs";
                if (AppSettings.ShowStations)
                    fetch += ",stations";

                var (apiStatus, respond) = await RequestsAsync.User.ProfileAsync(userId, fetch);
                if (apiStatus == 200)
                {
                    if (respond is ProfileObject result)
                    {
                        if (userId == UserDetails.UserId.ToString())
                        {
                            if (result.Data != null)
                            {
                                UserDetails.Avatar = result.Data.Avatar;
                                UserDetails.Username = result.Data.Username;
                                UserDetails.IsPro = result.Data.IsPro.ToString();
                                UserDetails.Url = result.Data.Url;
                                UserDetails.FullName = result.Data.Name;

                                if (result.Details != null)
                                    result.Data.Details = result.Details;

                                ListUtils.MyUserInfoList = new ObservableCollection<UserDataObject> { result.Data };

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrUpdate_DataMyInfo(result.Data);

                                //HomeActivity.GetInstance()?.RunOnUiThread(() =>
                                //{
                                //    try
                                //    {
                                //        if (AppSettings.ShowGoPro && result.Data.IsPro != 1) return;
                                //        var HomeFragmentProIcon = HomeActivity.GetInstance()?.HomeFragment?.ProIcon;
                                //        if (HomeFragmentProIcon != null)
                                //            HomeFragmentProIcon.Visibility = ViewStates.Gone;
                                //    }
                                //    catch (Exception e)
                                //    {
                                //        Methods.DisplayReportResultTrack(e);
                                //    }
                                //});
                            }
                             
                            return result;
                        }

                        return result;
                    }
                }
                else Methods.DisplayReportResult(context, respond);
            }

            return null;
        }

        public static async Task<(int?, int?)> GetCountNotifications()
        {
            var (respondCode, respondString) = await RequestsAsync.Common.GetCountNotificationsAsync(UserDetails.DeviceId);
            if (respondCode.Equals(200))
            {
                if (respondString is NotificationsCountObject notifications)
                {
                    return (notifications.Count, notifications.Msgs);
                }
            }
            return (0, 0);
        }

        public static async Task GetGenres_Api()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.User.GenresAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is GenresObject result)
                        {
                            ListUtils.GenresList.Clear();
                            ListUtils.GenresList = new ObservableCollection<GenresObject.DataGenres>(result.Data);

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdate_Genres(ListUtils.GenresList);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task GetMyPlaylist_Api()
        {
            try
            {
                if (!UserDetails.IsLogin)
                    return;

                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Playlist.GetPlaylistAsync(UserDetails.UserId.ToString());
                    if (apiStatus.Equals(200))
                    {
                        if (respond is PlaylistObject result)
                        {
                            ListUtils.PlaylistList = new ObservableCollection<PlaylistDataObject>(result.Playlist);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task GetMyAlbums_Api()
        {
            try
            {
                if (!UserDetails.IsLogin)
                    return;

                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.User.GetUserAlbumsAsync(UserDetails.UserId.ToString());
                    if (apiStatus.Equals(200))
                    {
                        if (respond is AlbumsObject result)
                        {
                            ListUtils.AlbumList = new ObservableCollection<DataAlbumsObject>(result.Data);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task GetPrices_Api()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Common.GetPricesAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is PricesObject result)
                        {
                            ListUtils.PriceList.Clear();
                            ListUtils.PriceList = new ObservableCollection<PricesObject.DataPrice>(result.Data);

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdate_Price(ListUtils.PriceList);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task<string> GetTimeZoneAsync()
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync("http://ip-api.com/json/");
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<TimeZoneObject>(json);
                return data?.Timezone ?? "UTC";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "UTC";
            }
        }

        public static async Task LoadFavorites()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.User.GetFavoriteAsync(UserDetails.UserId.ToString());
                    if (apiStatus == 200)
                    {
                        if (respond is GetSoundObject result)
                        {
                            result.Data.SoundList = DeepSoundTools.ListFilter(result.Data?.SoundList);

                            var respondList = result.Data?.SoundList?.Count;
                            if (respondList > 0)
                            {
                                ListUtils.FavoritesList = result.Data?.SoundList;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task LoadLiked()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.User.GetLikedAsync(UserDetails.UserId.ToString());
                    if (apiStatus == 200)
                    {
                        if (respond is GetSoundObject result)
                        {
                            var respondList = result.Data?.SoundList?.Count;
                            if (respondList > 0)
                            {
                                ListUtils.LikedSongs = result.Data?.SoundList;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static bool RunLogout;

        public static async void Delete(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    Constant.IsLoggingOut = true;
                    RunLogout = true;

                    await RemoveData("Delete");

                    context?.RunOnUiThread(() =>
                    {
                        try
                        {
                            OneSignalNotification.Instance.UnRegisterNotificationDevice();

                            Methods.Path.DeleteAll_FolderUser();

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.DropAll();

                            Runtime.GetRuntime()?.RunFinalization();
                            Runtime.GetRuntime()?.Gc();
                            TrimCache(context);

                            ListUtils.ClearAllList();

                            UserDetails.ClearAllValueUserDetails();

                            dbDatabase.CheckTablesStatus();

                            context.StopService(new Intent(context, typeof(AppApiService)));

                            SharedPref.SharedData?.Edit()?.Clear()?.Commit();
                            SharedPref.InAppReview?.Edit()?.Clear()?.Commit();

                            Intent intent = new Intent(context, typeof(FirstActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            context.StartActivity(intent);
                            context.FinishAffinity();
                            context.Finish();

                            RunLogout = false;
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void Logout(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    Constant.IsLoggingOut = true;
                    RunLogout = true;

                    await RemoveData("Logout");

                    context?.RunOnUiThread(() =>
                    {
                        try
                        {
                            OneSignalNotification.Instance.UnRegisterNotificationDevice();

                            Methods.Path.DeleteAll_FolderUser();

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.DropAll();

                            Runtime.GetRuntime()?.RunFinalization();
                            Runtime.GetRuntime()?.Gc();
                            TrimCache(context);
                              
                            ListUtils.ClearAllList();

                            UserDetails.ClearAllValueUserDetails();
                             
                            dbDatabase.CheckTablesStatus();

                            context.StopService(new Intent(context, typeof(AppApiService))); 

                            SharedPref.SharedData?.Edit()?.Clear()?.Commit();
                            SharedPref.InAppReview?.Edit()?.Clear()?.Commit();

                            Intent intent = new Intent(context, typeof(FirstActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            context.StartActivity(intent);
                            context.FinishAffinity();
                            context.Finish();
                           
                            RunLogout = false;
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static void TrimCache(Activity context)
        {
            try
            {
                File dir = context.CacheDir;
                if (dir != null && dir.IsDirectory)
                {
                    DeleteDir(dir);
                }

                context.DeleteDatabase(AppSettings.DatabaseName + "_.db");
                context.DeleteDatabase(SqLiteDatabase.PathCombine);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static bool DeleteDir(File dir)
        {
            try
            {
                if (dir == null || !dir.IsDirectory) return dir != null && dir.Delete();
                string[] children = dir.List();
                if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
                {
                    return false;
                }

                // The directory is now empty so delete it
                return dir.Delete();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private static async Task RemoveData(string type)
        {
            try
            {
                if (type == "Logout")
                {
                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { RequestsAsync.Auth.LogoutAsync });
                    }
                }
                else if (type == "Delete")
                {
                    Methods.Path.DeleteAll_FolderUser();

                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.DeleteAccountAsync(UserDetails.UserId.ToString(), UserDetails.Password) });
                    }
                }

                await Task.Delay(500);

                try
                {
                    if (AppSettings.ShowGoogleLogin && LoginActivity.MGoogleSignInClient != null)
                        if (Auth.GoogleSignInApi != null)
                        {
                            LoginActivity.MGoogleSignInClient.SignOut();
                            LoginActivity.MGoogleSignInClient = null;
                        }

                    if (AppSettings.ShowFacebookLogin)
                    {
                        var accessToken = AccessToken.CurrentAccessToken;
                        var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                        if (isLoggedIn && Profile.CurrentProfile != null)
                        {
                            LoginManager.Instance.LogOut();
                        }
                    }

                    AlbumsFragment.Instance?.MAdapter?.SoundsList?.Clear();
                    Constant.Player?.Release();
                    GC.Collect();
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}