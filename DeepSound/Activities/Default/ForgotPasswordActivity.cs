using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using System;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ForgotPasswordActivity : AppCompatActivity
    {
        #region Variables Basic

        private EditText EmailEditText;
        private AppCompatButton BtnSend;
        private ProgressBar ProgressBar;

        private ImageView BackIcon, EmailIcon;
        private RelativeLayout EmailFrameLayout;

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
                SetContentView(Resource.Layout.ForgotPasswordLayout);

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
                BackIcon = FindViewById<ImageView>(Resource.Id.backArrow);
                BackIcon.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                BackIcon.ImageTintList = ColorStateList.ValueOf(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                EmailFrameLayout = FindViewById<RelativeLayout>(Resource.Id.emailframelayout);
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                EmailIcon = FindViewById<ImageView>(Resource.Id.emailicon);

                BtnSend = FindViewById<AppCompatButton>(Resource.Id.Lbl_Send);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                ProgressBar.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(EmailEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
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
                    BackIcon.Click += BackIconOnClick;
                    EmailEditText.FocusChange += EmailFrameLayout_FocusChange;
                    BtnSend.Click += BtnSendOnClick;
                }
                else
                {
                    BackIcon.Click -= BackIconOnClick;
                    EmailEditText.FocusChange -= EmailFrameLayout_FocusChange;
                    BtnSend.Click -= BtnSendOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Send email
        private async void BtnSendOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(EmailEditText.Text))
                {
                    if (Methods.CheckConnectivity())
                    {
                        var check = Methods.FunString.IsEmailValid(EmailEditText.Text);
                        if (!check)
                        {
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Visible;
                            BtnSend.Visibility = ViewStates.Gone;
                            var (apiStatus, respond) = await RequestsAsync.Auth.ForgotPasswordAsync(EmailEditText.Text);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    ProgressBar.Visibility = ViewStates.Gone;
                                    BtnSend.Visibility = ViewStates.Visible;

                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), result.Message, GetText(Resource.String.Lbl_Ok));
                                }
                            }
                            else if (apiStatus == 400)
                            {
                                if (respond is ErrorObject error)
                                {
                                    string errorText = error.Error;
                                    switch (errorText)
                                    {
                                        case "Please check your details":
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorPleaseCheckYourDetails), GetText(Resource.String.Lbl_Ok));
                                            break;
                                        case "This e-mail is not found":
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorForgotPassword2), GetText(Resource.String.Lbl_Ok));
                                            break;
                                        case "Error found while sending the reset link, please try again later":
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorForgotPassword3), GetText(Resource.String.Lbl_Ok));
                                            break;
                                        default:
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                            break;
                                    }
                                }

                                ProgressBar.Visibility = ViewStates.Gone;
                                BtnSend.Visibility = ViewStates.Visible;
                            }
                            else if (apiStatus == 404)
                            {
                                ProgressBar.Visibility = ViewStates.Gone;
                                BtnSend.Visibility = ViewStates.Visible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_something_went_wrong), GetText(Resource.String.Lbl_Ok));
                            }
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSend.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_something_went_wrong), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSend.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), exception.ToString(), GetText(Resource.String.Lbl_Ok));
            }
        }

        private void EmailFrameLayout_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    if (EmailEditText.Text != "" && EmailEditText.Text.Contains("@"))
                        EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

                    EmailFrameLayout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
                    EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));
                }
                else
                {
                    if (EmailEditText.Text != "" && EmailEditText.Text.Contains("@"))
                        EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

                    if (!EmailEditText.Text.Contains("@")) // Email Validation
                    {
                        EmailFrameLayout.SetBackgroundResource(Resource.Drawable.new_edittextvalidation);
                        EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.validation_error_red)));
                    }
                    else
                    {
                        EmailFrameLayout.SetBackgroundResource(Resource.Drawable.new_login_status);
                        EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}