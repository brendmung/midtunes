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
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Requests;
using System;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ResetPasswordActivity : BaseActivity
    {
        #region Variables Basic

        private EditText TxtConfirmPassword, TxtNewPassword;
        private TextView IconConfirmPassword, IconNewPassword;
        private AppCompatButton BtnSave;
        private string Code;

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
                SetContentView(Resource.Layout.ResetPasswordLayout);

                Code = Intent?.GetStringExtra("EmailCode") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
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

                IconConfirmPassword = FindViewById<TextView>(Resource.Id.IconConfirmPassword);
                TxtConfirmPassword = FindViewById<EditText>(Resource.Id.ConfirmPasswordEditText);

                IconNewPassword = FindViewById<TextView>(Resource.Id.IconNewPassword);
                TxtNewPassword = FindViewById<EditText>(Resource.Id.NewPasswordEditText);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconConfirmPassword, FontAwesomeIcon.PaperPlane);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconNewPassword, FontAwesomeIcon.UnlockAlt);

                Methods.SetColorEditText(TxtConfirmPassword, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtNewPassword, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                AdsGoogle.Ad_AdMobNative(this);
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
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_ResetPassword);

                    toolBar.SetTitleTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        BtnSave.Click += SaveData_OnClick;
                        break;
                    default:
                        BtnSave.Click -= SaveData_OnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private void DestroyBasic()
        {
            try
            {
                BtnSave = null;
                IconConfirmPassword = null;
                TxtConfirmPassword = null;
                IconNewPassword = null;
                TxtNewPassword = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Save data 
        private async void SaveData_OnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtNewPassword.Text) || string.IsNullOrEmpty(TxtConfirmPassword.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
                    return;
                }

                if (TxtNewPassword.Text != TxtConfirmPassword.Text)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Your_password_dont_match), ToastLength.Long)?.Show();
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                var (apiStatus, respond) = await RequestsAsync.Auth.ResetPasswordAsync(TxtNewPassword.Text, TxtConfirmPassword.Text, Code);
                if (apiStatus == 200)
                {
                    UserDetails.Password = TxtNewPassword.Text;

                    Toast.MakeText(this, GetText(Resource.String.Lbl_YourDetailsWasUpdated), ToastLength.Long)?.Show();

                    AndHUD.Shared.Dismiss();

                    Finish();
                }
                else
                    Methods.DisplayAndHudErrorResult(this, respond);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                //Show a Error image with a message
                AndHUD.Shared.ShowError(this, e.Message, MaskType.Clear, TimeSpan.FromSeconds(1));
            }
        }

        #endregion

    }
}