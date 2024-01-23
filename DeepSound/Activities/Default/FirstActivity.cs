using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Resource.Drawable;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignalNotif;
using DeepSound.SQLite;
using DeepSoundClient;
using Me.Relex.CircleIndicatorLib;
using System;
using System.Collections.Generic;
using static DeepSound.Helpers.Controller.ViewPagerStringAdapter;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class FirstActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView ImageBackground;
        private AppCompatButton BtnRegister, BtnSkip;

        private ViewPager ViewPagerView;
        private ViewPagerStringAdapter ViewPagerStringAdapter;
        private CircleIndicator CircleIndicatorView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                InitializeDeepSound.Initialize(AppSettings.Cert, PackageName, AppSettings.TurnTrustFailureOnWebException, new MyReportModeApp());

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.FirstLayout);

                //Get Value And Set Toolbar
                InitComponent();
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
                AddOrRemoveEvent(true);
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
                AddOrRemoveEvent(false);
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

        #region Functions

        private void InitComponent()
        {
            try
            {
                ImageBackground = FindViewById<ImageView>(Resource.Id.Logoplace);

                BtnRegister = FindViewById<AppCompatButton>(Resource.Id.RegisterButton);

                BtnSkip = FindViewById<AppCompatButton>(Resource.Id.SkipButton);

                ViewPagerView = FindViewById<ViewPager>(Resource.Id.viewPager);
                CircleIndicatorView = FindViewById<CircleIndicator>(Resource.Id.indicator);

                BtnRegister.Click += BtnRegisterOnClick;

                if (!AppSettings.ShowSkipButton)
                    BtnSkip.Visibility = ViewStates.Gone;

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

                var stringsList = new List<ViewPagerStrings>
                {
                    new ViewPagerStrings { Description = GetString(Resource.String.Lbl_FirstTitle) },
                    new ViewPagerStrings { Description = GetString(Resource.String.Lbl_FirstSubTitle) }
                };

                ViewPagerStringAdapter = new ViewPagerStringAdapter(this, stringsList);
                ViewPagerView.Adapter = ViewPagerStringAdapter;
                ViewPagerView.PageScrollStateChanged += ViewPagerView_PageScrollStateChanged;
                CircleIndicatorView.SetViewPager(ViewPagerView);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ViewPagerView_PageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs e)
        {
            if (ViewPagerView.CurrentItem == 1)
            {
                int drawableResourceId = this.Resources.GetIdentifier("new_person_image2", "drawable", PackageName);
                Glide.With(this).Load(drawableResourceId).Transition(DrawableTransitionOptions.WithCrossFade(400)).Into(ImageBackground);
                BtnRegister.Text = GetString(Resource.String.Btn_GetStarted);
            }
            else
            {
                int drawableResourceId = this.Resources.GetIdentifier("new_person_image1", "drawable", PackageName);
                Glide.With(this).Load(drawableResourceId).Transition(DrawableTransitionOptions.WithCrossFade(400)).Into(ImageBackground);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    BtnRegister.Click += BtnRegisterOnClick;
                    BtnSkip.Click += SkipButtonOnClick;
                }
                else
                {
                    BtnRegister.Click -= BtnRegisterOnClick;
                    BtnSkip.Click -= SkipButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void SkipButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                UserDetails.Username = "";
                UserDetails.FullName = "";
                UserDetails.Password = "";
                UserDetails.AccessToken = "";
                UserDetails.UserId = 0;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = "";
                UserDetails.Email = "";
                 
                UserDetails.IsLogin = false;
                  
                StartActivity(new Intent(this, typeof(HomeActivity)));
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void OnMoveSliderEffect()
        {
            int drawableResourceId = this.Resources.GetIdentifier("new_person_image2", "drawable", PackageName);
            Glide.With(this).Load(drawableResourceId).Transition(DrawableTransitionOptions.WithCrossFade(400)).Into(ImageBackground);

            ViewPagerView.SetCurrentItem(ViewPagerView.CurrentItem + 1, true);

        }
        private void BtnRegisterOnClick(object sender, EventArgs e)
        {
            try
            {
                if (BtnRegister.Text == GetString(Resource.String.Lbl_Next))
                {
                    OnMoveSliderEffect();
                    BtnRegister.Text = GetString(Resource.String.Btn_GetStarted);
                }
                else
                {
                    StartActivity(new Intent(this, typeof(LoginActivity)));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions 

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
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
    }
}