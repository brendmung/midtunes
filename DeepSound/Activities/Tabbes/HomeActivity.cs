//###############################################################
// Author >> Elin Doughouz
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using AndroidX.Activity.Result;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.OS;
using AndroidX.SlidingPaneLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Canhub.Cropper;
using Com.Google.Android.Play.Core.Install.Model;
using Com.Sothree.Slidinguppanel;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Artists;
using DeepSound.Activities.Base;
using DeepSound.Activities.Chat;
using DeepSound.Activities.Library;
using DeepSound.Activities.Library.Listeners;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.SettingsUser;
using DeepSound.Activities.SettingsUser.General;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Tabbes.Fragments;
using DeepSound.Activities.Upload;
using DeepSound.Activities.UserProfile;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignalNotif;
using DeepSound.Library.OneSignalNotif.Models;
using DeepSound.Service;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Classes.Story;
using DeepSoundClient.Classes.Tracks;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Google.Android.Material.AppBar;
using Google.Android.Material.Dialog;
using Java.IO;
using Newtonsoft.Json;
using Q.Rorbin.Badgeview;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActivityResult = Com.Google.Android.Play.Core.Install.Model.ActivityResult;
using Console = System.Console;
using Exception = System.Exception;
using Math = System.Math;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class HomeActivity : AppCompatActivity, SlidingPaneLayout.IPanelSlideListener, SlidingUpPanelLayout.IPanelSlideListener, AppBarLayout.IOnOffsetChangedListener, IActivityResultCallback
    {
        #region Variables Basic

        private static HomeActivity Instance;
        public SlidingUpPanelLayout SlidingUpPanel;
        private ViewStub FullScreenPlayerViewStub;
        public HomeFragment HomeFragment;
        public TrendingFragment TrendingFragment;
        public FavoritesFragment FavoritesFragment;
        public LibraryFragment LibraryFragment;
        public ProfileFragment ProfileFragment;
        private LinearLayout NavigationTabBar;
        public CustomNavigationController FragmentBottomNavigator;
        private PowerManager.WakeLock Wl;
        public SoundController SoundController;
        private string TypeImage = "";
        public LibrarySynchronizer LibrarySynchronizer;
        private readonly Handler ExitHandler = new Handler(Looper.MainLooper);
        private bool RecentlyBackPressed;
        private DialogGalleryController GalleryController;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                if (ShouldReturn())
                    return;

                base.OnCreate(savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);

                Methods.App.FullScreenApp(this);

                Delegate.SetLocalNightMode(DeepSoundTools.IsTabDark() ? AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                AddFlagsWakeLock();

                // Create your application here
                SetContentView(Resource.Layout.TabbedMainLayout);

                Instance = this;
                Constant.Context = this;

                LibrarySynchronizer = new LibrarySynchronizer(this);

                //Get Value 
                InitComponent();
                SetupBottomNavigationView();
                InitBackPressed("HomeActivity");

                SoundController = new SoundController(this);
                SoundController.InitializeUi();

                Task.Factory.StartNew(() =>
                {
                    MainApplication.GetInstance()?.SecondRunExcite(this);
                    GetGeneralAppData();
                });

                GetOneSignalNotification();
                SetService();
                CheckOptimization();
                GalleryController = new DialogGalleryController(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CheckOptimization()
        {
            try
            {
                if (!AppSettings.EnableOptimizationApp || UserDetails.IsOptimizationApp) return;
                if (Build.VERSION.SdkInt < BuildVersionCodes.M) return;

                var pm = (PowerManager)ApplicationContext?.GetSystemService(Context.PowerService);
                if (pm == null) return;

                bool result = pm.IsIgnoringBatteryOptimizations(PackageName);
                if (result)
                    return;

                UserDetails.IsOptimizationApp = true;
                SharedPref.SharedData?.Edit()?.PutBoolean(SharedPref.PrefKeyOptimizationApp, true)?.Commit();

                var intent = new Intent();
                if (pm.IsIgnoringBatteryOptimizations(PackageName))
                {
                    intent.SetAction(Settings.ActionIgnoreBatteryOptimizationSettings);
                }
                else
                {
                    intent.SetAction(Settings.ActionRequestIgnoreBatteryOptimizations);
                    intent.SetData(Uri.Parse("package:" + PackageName));
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                OffWakeLock();
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
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                if (!Constant.IsLoggingOut && !Constant.IsChangingTheme || Constant.IsPlayed)
                {
                    Intent intent = new Intent(this, typeof(PlayerService));
                    intent.SetAction(PlayerService.ActionStop);

                    ContextCompat.StartForegroundService(this, intent);

                    StopService(intent);
                }

                Constant.IsLoggingOut = false;
                Constant.IsChangingTheme = false;

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

                FragmentBottomNavigator?.DisableAllNavigationButton();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                SlidingUpPanel = FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);
                SlidingUpPanel.AddPanelSlideListener(this);

                FullScreenPlayerViewStub = FindViewById<ViewStub>(Resource.Id.viewStubFullScreenPlayer);
                switch (AppSettings.PlayerTheme)
                {
                    case PlayerTheme.Theme1:
                        FullScreenPlayerViewStub.LayoutResource = Resource.Layout.FullScreenPlayerLayout;
                        break;
                    case PlayerTheme.Theme2:
                        FullScreenPlayerViewStub.LayoutResource = Resource.Layout.FullScreenPlayer2Layout;
                        break;
                }
                FullScreenPlayerViewStub.Inflate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static HomeActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public void SetToolBar(Toolbar toolbar, string title, bool showIconBack = true)
        {
            try
            {
                if (toolbar != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        toolbar.Title = title;

                    toolbar.SetTitleTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(showIconBack);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitBackPressed(string pageName)
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

        #endregion

        #region Functions ¯\_(ツ)_/¯

        private async void GetOneSignalNotification()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) == Permission.Granted)
                    {
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                    }
                    else
                    {
                        ActivityCompat.RequestPermissions(this, new[]
                        {
                            Manifest.Permission.PostNotifications
                        }, 16248);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(UserDetails.DeviceId))
                        OneSignalNotification.Instance.RegisterNotificationDevice(this);
                }

                string osNotificationObject = Intent?.GetStringExtra("OsNotificationObject") ?? "";
                if (!string.IsNullOrEmpty(osNotificationObject))
                {
                    var dataNotification = JsonConvert.DeserializeObject<OsObject.OsNotificationObject>(osNotificationObject);
                    if (dataNotification == null)
                        return;

                    switch (dataNotification.Type)
                    {
                        case "user":
                            OpenProfile(dataNotification.UserData.Id, dataNotification.UserData);
                            break;
                        case "track":
                        {
                            var (apiStatus, respond) = await RequestsAsync.Tracks.GetTrackInfoAsync(dataNotification.TrackId);
                            if (apiStatus.Equals(200))
                            {
                                if (respond is GetTrackInfoObject result)
                                {
                                    Constant.PlayPos = 0;
                                    SoundController?.StartPlaySound(result.Data, new ObservableCollection<SoundDataObject> { result.Data });
                                }
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

        public async Task GetCountNotifications()
        {
            try
            {
                var (countNotifications, countMessages) = await ApiRequest.GetCountNotifications();
                if (HomeFragment?.NotificationIcon != null && countNotifications != 0 && countNotifications != UserDetails.CountNotificationsStatic)
                {
                    UserDetails.CountNotificationsStatic = Convert.ToInt32(countNotifications);
                    ShowOrHideBadgeViewIcon(UserDetails.CountNotificationsStatic, true);
                }
                else
                {
                    ShowOrHideBadgeViewIcon();
                }

                Console.WriteLine(countMessages);
                //if (tabMessages != null && countMessages != 0 && countMessages != CountMessagesStatic)
                //{
                //    RunOnUiThread(() =>
                //    {
                //        try
                //        {
                //            CountMessagesStatic = countMessages;
                //            tabMessages.BadgeTitle = countMessages.ToString();
                //            tabMessages.UpdateBadgeTitle(countMessages.ToString());
                //            tabMessages.ShowBadge();
                //        }
                //        catch (Exception e)
                //        {
                //            Methods.DisplayReportResultTrack(e);
                //        }
                //    });
                //}  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowOrHideBadgeViewIcon(int countNotifications = 0, bool show = false)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (HomeFragment?.NotificationIcon != null)
                            {
                                int gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                QBadgeView badge = new QBadgeView(this);
                                badge.BindTarget(HomeFragment?.NotificationIcon);
                                badge.SetBadgeNumber(countNotifications);
                                badge.SetBadgeGravity(gravity);
                                badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                //badge.SetGravityOffset(10, true);
                            }
                        }
                        else
                        {
                            if (HomeFragment?.NotificationIcon != null && countNotifications != 0 && countNotifications != UserDetails.CountNotificationsStatic)
                            {
                                new QBadgeView(this).BindTarget(HomeFragment?.NotificationIcon).Hide(true);
                                UserDetails.CountNotificationsStatic = 0;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetupBottomNavigationView()
        {
            try
            {
                NavigationTabBar = FindViewById<LinearLayout>(Resource.Id.buttomnavigationBar);
                FragmentBottomNavigator = new CustomNavigationController(this);

                HomeFragment = new HomeFragment();
                TrendingFragment = new TrendingFragment();
                FavoritesFragment = new FavoritesFragment();

                FragmentBottomNavigator.FragmentListTab0.Add(HomeFragment);
                FragmentBottomNavigator.FragmentListTab1.Add(TrendingFragment);
                FragmentBottomNavigator.FragmentListTab2.Add(FavoritesFragment);

                if (UserDetails.IsLogin)
                {
                    LibraryFragment = new LibraryFragment();
                    ProfileFragment = new ProfileFragment();
                    FragmentBottomNavigator.FragmentListTab3.Add(LibraryFragment);
                    FragmentBottomNavigator.FragmentListTab4.Add(ProfileFragment);
                }

                FragmentBottomNavigator.ShowFragment0();

                //if (UserDetails.IsLogin && PlaylistFragment?.PlaylistAdapter == null)
                //    PlaylistFragment.PlaylistAdapter = new PlaylistAdapter(this, true) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
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
                    FragmentNavigatorBack();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Event Back

        public void BackPressed()
        {
            try
            {
                if (SlidingUpPanel != null && (SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else if (FragmentBottomNavigator.GetCountFragment() > 0)
                {
                    FragmentNavigatorBack();
                }
                else
                {
                    if (RecentlyBackPressed)
                    {
                        ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                        RecentlyBackPressed = false;
                        MoveTaskToBack(true);
                        //Finish();
                    }
                    else
                    {
                        RecentlyBackPressed = true;
                        Toast.MakeText(this, GetString(Resource.String.press_again_exit), ToastLength.Long)?.Show();
                        ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                //Finish();
            }
        }

        public void FragmentNavigatorBack()
        {
            try
            {
                FragmentBottomNavigator.OnBackStackClickFragment();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Upload An Album
        public void BtnUploadAnAlbumOnClick()
        {
            try
            {
                StartActivity(new Intent(this, typeof(UploadAlbumActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Upload Single Song
        public void BtnUploadSingleSongOnClick()
        {
            try
            {
                PermissionsType = "";

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                    new IntentController(this).OpenIntentAudio(); //505
                else
                {
                    if (PermissionsController.CheckPermissionStorage(this, "audio"))
                        new IntentController(this).OpenIntentAudio(); //505
                    else
                        new PermissionsController(this).RequestPermission(100, "audio");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void BtnImportSongOnClick()
        {
            try
            {
                StartActivity(new Intent(this, typeof(ImportSongActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 505 && resultCode == Result.Ok) //==> Audio
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Audio")
                        {
                            Intent intent = new Intent(this, typeof(UploadSongActivity));
                            intent.PutExtra("SongLocation", filepath);
                            StartActivity(intent);
                        }
                    }
                }
                else if (requestCode == 4711)
                {
                    switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                    {
                        case Result.Ok:
                            // In app update success
                            if (UpdateManagerApp.AppUpdateTypeSupported == AppUpdateType.Immediate)
                                Toast.MakeText(this, "App updated", ToastLength.Short)?.Show();
                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, "In app update cancelled", ToastLength.Short)?.Show();
                            break;
                        case (Result)ActivityResult.ResultInAppUpdateFailed:
                            Toast.MakeText(this, "In app update failed", ToastLength.Short)?.Show();
                            break;
                    }
                }
                else if (requestCode == 200 && resultCode == Result.Ok)
                {
                    var name = ListUtils.MyUserInfoList?.FirstOrDefault()?.Name;
                    if (!string.IsNullOrEmpty(name) && ProfileFragment != null && !name.Equals(ProfileFragment.TxtFullName.Text))
                        ProfileFragment.TxtFullName.Text = name;
                }
                else if (requestCode == 3500 && resultCode == Result.Ok) //Add Product
                {
                    if (string.IsNullOrEmpty(data?.GetStringExtra("itemData"))) return;

                    var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData") ?? "");
                    if (item != null)
                    {
                        var productsList = TrendingFragment?.ProductFragment?.MAdapter?.ProductsList;
                        if (productsList != null)
                        {
                            productsList.Add(item);

                            TrendingFragment.ProductFragment?.MAdapter.NotifyDataSetChanged();
                        }
                    }
                }
                else if (requestCode == 4500 && resultCode == Result.Ok) //Add Event
                {
                    if (string.IsNullOrEmpty(data?.GetStringExtra("itemData"))) return;

                    var item = JsonConvert.DeserializeObject<EventDataObject>(data.GetStringExtra("itemData") ?? "");
                    if (item != null)
                    {
                        var eventList = TrendingFragment?.EventFragment?.MAdapter?.EventsList;
                        if (eventList != null)
                        {
                            eventList.Add(item);

                            TrendingFragment?.EventFragment?.MAdapter.NotifyDataSetChanged();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public string PermissionsType = "";
        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        GalleryController?.OpenDialogGallery(TypeImage);
                        break;
                    case 108:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        if (PermissionsType == "DownloadAlbum")
                            OptionsAlbumBottomSheet.Instance?.SoundDownloadAlbum();
                        else
                            new IntentController(this).OpenIntentAudio(); //505
                        break;
                    case 100:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 1325 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        SongOptionBottomDialogFragment.Instance?.SetRingtone();
                        break;
                    case 1325:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 1005 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        SoundController?.SetDownload();
                        break;
                    case 1005:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 16248 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                        break;
                    case 16248:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region WakeLock System

        private void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(this).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetWakeLock()
        {
            try
            {
                if (Wl == null)
                {
                    PowerManager pm = (PowerManager)GetSystemService(PowerService);
                    Wl = pm?.NewWakeLock(WakeLockFlags.Partial, "My Tag");
                    Wl?.Acquire();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Listener Panel Layout

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            var percentage = (float)Math.Abs(verticalOffset) / appBarLayout.TotalScrollRange;
            Console.WriteLine(percentage);
        }

        public void OnPanelClosed(View panel)
        {

        }

        public void OnPanelOpened(View panel)
        {

        }

        public void OnPanelSlide(View panel, float slideOffset)
        {
            try
            {
                NavigationTabBar.Alpha = 1 - slideOffset;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnPanelStateChanged(View p0, SlidingUpPanelLayout.PanelState p1, SlidingUpPanelLayout.PanelState p2)
        {
            try
            {
                if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon?.Tag?.ToString() == "Close")
                    {
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);

                        SoundController.BackIcon.Tag = "Open";
                        SoundController?.SetUiSliding(false);
                        //NavigationTabBar.Hide();
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Hidden && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon?.Tag?.ToString() == "Open")
                    {
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            SoundController?.BackIcon.SetImageResource(Resource.Drawable.icon_close_vector);

                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Anchored)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Expanded)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Hidden)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Expanded)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon?.Tag?.ToString() == "Close")
                    {
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            SoundController?.BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);

                        SoundController.BackIcon.Tag = "Open";
                        SoundController?.SetUiSliding(false);
                        NavigationTabBar.Visibility = ViewStates.Gone;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Hidden)
                {
                    // Toast.MakeText(this, "p1 Anchored + Anchored ", ToastLength.Short)?.Show();
                }
                if (p1 == SlidingUpPanelLayout.PanelState.Collapsed && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon?.Tag?.ToString() == "Open")
                    {
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            SoundController?.BackIcon.SetImageResource(Resource.Drawable.icon_close_vector);

                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);

                        NavigationTabBar.Visibility = ViewStates.Visible;

                    }
                }

                if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    if (SoundController?.BackIcon != null && SoundController?.BackIcon?.Tag?.ToString() == "Open")
                    {
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            SoundController?.BackIcon.SetImageResource(Resource.Drawable.icon_close_vector);

                        SoundController.BackIcon.Tag = "Close";
                        SoundController?.SetUiSliding(true);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                        {
                            SoundController?.BtPlay.SetImageResource(Resource.Drawable.icon_player_pause);
                            SoundController?.BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_pause);
                        }
                        else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                        {
                            SoundController?.BtPlay.SetImageResource(Resource.Drawable.icon_player2_pause);
                            SoundController?.BtnPlayImage.SetImageResource(Resource.Drawable.icon_player2_pause);
                        }
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Purchase the song Or album Or story

        public void OpenDialogPurchaseSound(SoundDataObject soundObject)
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
                dialog.SetMessage(GetText(Resource.String.Lbl_PurchaseRequiredContent));
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
                {
                    try
                    {
                        if (soundObject.Price != null && DeepSoundTools.CheckWallet(soundObject.Price.Value))
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var (apiStatus, respond) = await RequestsAsync.Payments.PurchaseAsync("buy_song", soundObject.AudioId);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageObject result)
                                    {
                                        Console.WriteLine(result.Message);
                                        soundObject.IsPurchased = true;
                                        Constant.PlayPos = Constant.ArrayListPlay.IndexOf(soundObject);
                                        SoundController?.StartPlaySound(soundObject, Constant.ArrayListPlay);

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
                                    }
                                }
                                else Methods.DisplayReportResult(this, respond);
                            }
                            else
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                        else
                        {
                            var dialogBuilder = new MaterialAlertDialogBuilder(this);
                            dialogBuilder.SetTitle(GetText(Resource.String.Lbl_Wallet));
                            dialogBuilder.SetMessage(GetText(Resource.String.Lbl_Error_NoWallet));
                            dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
                            {
                                try
                                {
                                    StartActivity(new Intent(this, typeof(WalletActivity)));
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                            dialogBuilder.Show();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), (sender, args) =>
                {
                    try
                    {
                        SoundController?.ChangePlayPauseIcon();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenDialogPurchaseAlbum(DataAlbumsObject albumsObject)
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
                dialog.SetMessage(GetText(Resource.String.Lbl_PurchaseRequiredContent));
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
                {
                    try
                    {
                        if (DeepSoundTools.CheckWallet(albumsObject.Price))
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var (apiStatus, respond) = await RequestsAsync.Payments.PurchaseAsync("buy_album", albumsObject.AlbumId);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageObject result)
                                    {
                                        Console.WriteLine(result.Message);
                                        albumsObject.IsPurchased = 1;

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
                                    }
                                }
                                else Methods.DisplayReportResult(this, respond);
                            }
                            else
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                        else
                        {
                            var dialogBuilder = new MaterialAlertDialogBuilder(this);
                            dialogBuilder.SetTitle(GetText(Resource.String.Lbl_Wallet));
                            dialogBuilder.SetMessage(GetText(Resource.String.Lbl_Error_NoWallet));
                            dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
                            {
                                try
                                {
                                    StartActivity(new Intent(this, typeof(WalletActivity)));
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                            dialogBuilder.Show();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenDialogPurchaseStory(StoryDataObject storyObject)
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
                dialog.SetMessage(GetText(Resource.String.Lbl_PurchaseRequiredContent));
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
                {
                    try
                    {
                        if (DeepSoundTools.CheckWallet(storyObject.Paid))
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var (apiStatus, respond) = await RequestsAsync.Story.PurchaseAsync(storyObject.Id);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageObject result)
                                    {
                                        Console.WriteLine(result.Message);
                                        storyObject.Active = 1;

                                        var story = HomeFragment?.LatestHomeTab?.StoryAdapter?.StoryList?.FirstOrDefault(a => a.Id == storyObject.Id);
                                        if (story != null)
                                        {
                                            story.Active = 1;
                                            HomeFragment.LatestHomeTab.StoryAdapter.NotifyDataSetChanged();
                                        }

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
                                    }
                                }
                                else Methods.DisplayReportResult(this, respond);
                            }
                            else
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                        else
                        {
                            var dialogBuilder = new MaterialAlertDialogBuilder(this);
                            dialogBuilder.SetTitle(GetText(Resource.String.Lbl_Wallet));
                            dialogBuilder.SetMessage(GetText(Resource.String.Lbl_Error_NoWallet));
                            dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
                            {
                                try
                                {
                                    StartActivity(new Intent(this, typeof(WalletActivity)));
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                            dialogBuilder.Show();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Service Chat

        public void SetService(bool run = true)
        {
            try
            {
                if (!UserDetails.IsLogin)
                    return;

                if (run)
                {
                    // reschedule the job
                    ChatJobInfo.ScheduleJob(this);
                }
                else
                {
                    // Cancel all jobs
                    ChatJobInfo.StopJob(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnReceiveResult(string resultData)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<GetConversationListObject>(resultData);
                if (result != null)
                {
                    LastChatActivity.GetInstance()?.LoadDataJsonLastChat(result);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Result Gallery

        public void OpenDialogGallery(string type)
        {
            try
            {
                TypeImage = type;
                GalleryController?.OpenDialogGallery(type);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnActivityResult(Java.Lang.Object p0)
        {
            try
            {
                if (p0 is CropImageView.CropResult result)
                {
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.UriContent;
                        string filepath = Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu ? result.GetUriFilePath(this, true) : Methods.AttachmentFiles.GetActualPathFromFile(this, resultUri);
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            if (TypeImage == "Avatar")
                            {
                                if (ProfileFragment?.ImageAvatar == null)
                                    return;

                                File file2 = new File(filepath);
                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ProfileFragment?.ImageAvatar);

                                await Task.Run(async () =>
                                {
                                    try
                                    {
                                        var (apiStatus, respond) = await RequestsAsync.User.UpdateAvatarAsync(filepath);
                                        if (apiStatus.Equals(200))
                                        {
                                            if (respond is UpdateImageUserObject image)
                                            {
                                                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                                                if (dataUser != null)
                                                {
                                                    dataUser.Avatar = image.Img;

                                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                    dbDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                                                }
                                            }
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                            else if (TypeImage == "Cover")
                            {
                                if (ProfileFragment?.ImageCover == null)
                                    return;

                                File file2 = new File(filepath);
                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ProfileFragment?.ImageCover);

                                await Task.Run(async () =>
                                {
                                    try
                                    {
                                        var (apiStatus, respond) = await RequestsAsync.User.UpdateCoverAsync(filepath);
                                        if (apiStatus.Equals(200))
                                        {
                                            if (respond is UpdateImageUserObject image)
                                            {
                                                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                                                if (dataUser != null)
                                                {
                                                    dataUser.Cover = image.Img;

                                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                    dbDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                                                }
                                            }
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                            else if (TypeImage == "CreatePlaylist")
                            {
                                var page = CreatePlaylistBottomSheet.Instance;
                                if (page != null)
                                {
                                    page.PathImage = filepath;
                                    File file2 = new File(filepath);
                                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                    Glide.With(this).Load(photoUri).Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(page.Image);
                                }
                            }
                            else if (TypeImage == "EditPlaylist")
                            {
                                var page = EditPlaylistBottomSheet.Instance;
                                if (page != null)
                                {
                                    page.PathImage = filepath;
                                    File file2 = new File(filepath);
                                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                    Glide.With(this).Load(photoUri).Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(page.Image);
                                }
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void OpenProfile(long userId, UserDataObject item)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("UserId", userId.ToString());
                    if (item.Artist == 0)
                    {
                        UserProfileFragment userProfileFragment = new UserProfileFragment
                        {
                            Arguments = bundle
                        };
                        FragmentBottomNavigator.DisplayFragment(userProfileFragment);
                    }
                    else
                    {
                        //open profile Artist
                        ArtistsProfileFragment artistsProfileFragment = new ArtistsProfileFragment
                        {
                            Arguments = bundle
                        };
                        FragmentBottomNavigator.DisplayFragment(artistsProfileFragment);
                    }
                }
                else
                {
                    if (UserDetails.IsLogin)
                        FragmentBottomNavigator.ShowFragment4();
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Login), GetText(Resource.String.Lbl_Message_Sorry_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }

                if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded)
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Load General Data

        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();
                sqlEntity.Get_data_Login_Credentials();

                sqlEntity.GetSettings();
                if (UserDetails.IsLogin)
                {
                    sqlEntity.GetDataMyInfo();

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this), () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()), ApiRequest.GetMyPlaylist_Api, ApiRequest.LoadFavorites, ApiRequest.LoadLiked, GetNotInterestedSounds });
                }
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
                }

                ListUtils.GenresList = sqlEntity.Get_GenresList();
                ListUtils.PriceList = sqlEntity.Get_PriceList();

                if (ListUtils.GenresList?.Count == 0)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetGenres_Api });

                if (ListUtils.PriceList?.Count == 0 && AppSettings.ShowPrice)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPrices_Api });

                RunOnUiThread(() =>
                {
                    try
                    {
                        InAppUpdate();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InAppUpdate()
        {
            try
            {
                if (AppSettings.ShowSettingsUpdateManagerApp)
                    UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static int CountRateApp;
        public void InAppReview()
        {
            try
            {
                bool inAppReview = SharedPref.InAppReview.GetBoolean(SharedPref.PrefKeyInAppReview, false);
                if (!inAppReview && AppSettings.ShowSettingsRateApp)
                {
                    if (CountRateApp == AppSettings.ShowRateAppCount)
                    {
                        var dialog = new MaterialAlertDialogBuilder(this);
                        dialog.SetTitle(GetText(Resource.String.Lbl_RateOurApp));
                        dialog.SetMessage(GetText(Resource.String.Lbl_RateOurAppContent));
                        dialog.SetPositiveButton(GetText(Resource.String.Lbl_Rate), (materialDialog, action) =>
                        {
                            try
                            {
                                StoreReviewApp store = new StoreReviewApp();
                                store.OpenStoreReviewPage(this, PackageName);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                        dialog.Show();

                        SharedPref.InAppReview?.Edit()?.PutBoolean(SharedPref.PrefKeyInAppReview, true)?.Commit();
                    }
                    else
                    {
                        CountRateApp++;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        private async Task GetNotInterestedSounds()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Tracks.GetNotInterestedAsync("25");
                    if (apiStatus == 200)
                    {
                        if (respond is GetSoundDataObject result)
                        {
                            if (result.Data.Count > 0)
                            {
                                ListUtils.GlobalNotInterestedList = new ObservableCollection<SoundDataObject>(result.Data);
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private bool ShouldReturn()
        {
            try
            {
                if (!SplashScreenActivity.SplashWasShown)
                {
                    NavigateToSplashAndFinish();
                }

                return !SplashScreenActivity.SplashWasShown;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return !SplashScreenActivity.SplashWasShown;
            }
        }

        private void NavigateToSplashAndFinish()
        {
            try
            {
                base.OnCreate(null);

                var intents = new Intent(this, typeof(SplashScreenActivity));
                intents.AddFlags(ActivityFlags.NewTask);
                intents.AddFlags(ActivityFlags.ClearTop);

                StartActivity(intents);

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}