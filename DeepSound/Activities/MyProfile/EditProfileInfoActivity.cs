using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditProfileInfoActivity : BaseActivity
    {
        #region Variables Basic

        private TextView NameIcon, AboutIcon, FacebookIcon, WebsiteIcon;
        private EditText EdtFullName, EdtAbout, EdtFacebook, EdtWebsite;
        private AppCompatButton BtnSave;
        private AdManagerAdView AdManagerAdView;


        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.EditMyProfileLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                GetMyInfoData();

                AdsGoogle.Ad_RewardedVideo(this);
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");

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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");

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

                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Intent resultIntent = new Intent();
                    SetResult(Result.Canceled, resultIntent);
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
                NameIcon = FindViewById<TextView>(Resource.Id.IconFullName);
                EdtFullName = FindViewById<EditText>(Resource.Id.FullNameEditText);

                AboutIcon = FindViewById<TextView>(Resource.Id.IconAbout);
                EdtAbout = FindViewById<EditText>(Resource.Id.AboutEditText);

                FacebookIcon = FindViewById<TextView>(Resource.Id.IconFacebook);
                EdtFacebook = FindViewById<EditText>(Resource.Id.FacebookEditText);

                WebsiteIcon = FindViewById<TextView>(Resource.Id.IconWebsite);
                EdtWebsite = FindViewById<EditText>(Resource.Id.WebsiteEditText);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                Methods.SetColorEditText(EdtFullName, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtAbout, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtFacebook, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtWebsite, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, NameIcon, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, AboutIcon, FontAwesomeIcon.InfoCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FacebookIcon, IonIconsFonts.LogoFacebook);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, WebsiteIcon, FontAwesomeIcon.GlobeAmericas);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_EditProfileInfo);
                    toolbar.SetTitleTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
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


        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    BtnSave.Click += BtnSaveOnClick;
                }
                else
                {
                    BtnSave.Click -= BtnSaveOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Events

        //Click save data and sent api
        private async void BtnSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {
                        {"name", EdtFullName.Text},
                        {"about_me", EdtAbout.Text},
                        {"facebook", EdtFacebook.Text},
                        {"website", EdtWebsite.Text},
                    };

                    if (string.IsNullOrEmpty(dictionary["website"]))
                        dictionary["website"] = "https://www.example.com/";

                    var (apiStatus, respond) = await RequestsAsync.User.UpdateProfileAsync(UserDetails.UserId.ToString(), dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            var local = ListUtils.MyUserInfoList?.FirstOrDefault();
                            if (local != null)
                            {
                                local.Name = EdtFullName.Text;
                                local.About = EdtAbout.Text;
                                local.Facebook = EdtFacebook.Text;
                                local.Website = EdtWebsite.Text;

                                //TextSanitizer aboutSanitizer = new TextSanitizer(HomeActivity.GetInstance()?.ProfileFragment.TxtAbout, this);
                                //aboutSanitizer.Load(Methods.FunString.DecodeString(EdtAbout.Text));

                                SqLiteDatabase database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyInfo(local);
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_UpdatedSuccessfully), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss();
                             
                            Intent returnIntent = new Intent(); 
                            SetResult(Result.Ok, returnIntent); 
                            Finish();
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        #endregion

        private void GetMyInfoData()
        {
            try
            {
                if (ListUtils.MyUserInfoList?.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                }

                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (dataUser != null)
                {
                    EdtFullName.Text = dataUser.Name;
                    EdtAbout.Text = Methods.FunString.DecodeString(dataUser.About);
                    EdtFacebook.Text = dataUser.Facebook;
                    EdtWebsite.Text = dataUser.Website;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}