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
using DeepSound.Activities.Genres;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Auth;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RegisterActivity : AppCompatActivity
    {
        #region Variables Basic

        private EditText EmailEditText, UsernameEditText, PasswordEditText, ConfirmPasswordEditText;
        private AppCompatButton RegisterButton;
        private ProgressBar ProgressBar;
        private TextView TermsTextView, SignInTextView;
        private ImageView BackIcon, EmailIcon, PasswordIcon, EyesIcon, EyesIcon2, ConfirmPasswordIcon, UsernameIcon;
        private RelativeLayout EmailFrameLayout, PassFrameLayout, ConfirmPasswordLayout, UsernameFrameLayout;
        private string Referral;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Methods.App.FullScreenApp(this, true);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.RegisterLayout);

                Referral = Intent?.GetStringExtra("Referral") ?? "";

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

        #region Functions

        private void InitComponent()
        {
            try
            {
                BackIcon = FindViewById<ImageView>(Resource.Id.backArrow);
                BackIcon.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                BackIcon.ImageTintList = ColorStateList.ValueOf(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                UsernameFrameLayout = FindViewById<RelativeLayout>(Resource.Id.usernameframelayout);
                UsernameEditText = FindViewById<EditText>(Resource.Id.edt_username);
                UsernameIcon = FindViewById<ImageView>(Resource.Id.usernameicon);

                EmailFrameLayout = FindViewById<RelativeLayout>(Resource.Id.emailframelayout);
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                EmailIcon = FindViewById<ImageView>(Resource.Id.emailicon);

                PassFrameLayout = FindViewById<RelativeLayout>(Resource.Id.passframelayout);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                PasswordIcon = FindViewById<ImageView>(Resource.Id.passicon);

                ConfirmPasswordLayout = FindViewById<RelativeLayout>(Resource.Id.passconfirmframelayout);
                ConfirmPasswordEditText = FindViewById<EditText>(Resource.Id.edt_Confirmpassword);
                ConfirmPasswordIcon = FindViewById<ImageView>(Resource.Id.passconfirmicon);

                EyesIcon = FindViewById<ImageView>(Resource.Id.eyesicon);
                EyesIcon.Tag = "hide";

                EyesIcon2 = FindViewById<ImageView>(Resource.Id.eyes2icon);
                EyesIcon2.Tag = "hide";

                RegisterButton = FindViewById<AppCompatButton>(Resource.Id.SignInButton);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar);
                ProgressBar.Visibility = ViewStates.Gone;

                TermsTextView = FindViewById<TextView>(Resource.Id.txt_termsservices);
                SignInTextView = FindViewById<TextView>(Resource.Id.txt_signin);

                Methods.SetColorEditText(UsernameEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EmailEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(PasswordEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(ConfirmPasswordEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

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
                    UsernameEditText.FocusChange += UsernameFrameLayout_FocusChange;
                    EmailEditText.FocusChange += EmailFrameLayout_FocusChange;
                    PasswordEditText.FocusChange += PassFrameLayout_FocusChange;
                    ConfirmPasswordEditText.FocusChange += ConfirmPasswordFrameLayout_FocusChange;
                    RegisterButton.Click += RegisterButtonOnClick;
                    TermsTextView.Click += TermsLayoutOnClick;
                    SignInTextView.Click += SignInTextView_Click;
                    EyesIcon.Click += EyesIcon_Click;
                    EyesIcon2.Click += Eyes2Icon_Click;
                }
                else
                {
                    BackIcon.Click -= BackIconOnClick;
                    UsernameEditText.FocusChange -= UsernameFrameLayout_FocusChange;
                    EmailEditText.FocusChange -= EmailFrameLayout_FocusChange;
                    PasswordEditText.FocusChange -= PassFrameLayout_FocusChange;
                    ConfirmPasswordEditText.FocusChange -= ConfirmPasswordFrameLayout_FocusChange;
                    RegisterButton.Click -= RegisterButtonOnClick;
                    TermsTextView.Click -= TermsLayoutOnClick;
                    SignInTextView.Click -= SignInTextView_Click;
                    EyesIcon.Click -= EyesIcon_Click;
                    EyesIcon2.Click -= Eyes2Icon_Click;
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

        //Register QuickDate
        private async void RegisterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (!string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(UsernameEditText.Text.Replace(" ", "")) ||
                        !string.IsNullOrEmpty(PasswordEditText.Text) || !string.IsNullOrEmpty(ConfirmPasswordEditText.Text))
                    {
                        var check = Methods.FunString.IsEmailValid(EmailEditText.Text.Replace(" ", ""));
                        if (!check)
                        {
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        }
                        else
                        {
                            if (PasswordEditText.Text != ConfirmPasswordEditText.Text)
                            {
                                ProgressBar.Visibility = ViewStates.Gone;
                                RegisterButton.Visibility = ViewStates.Visible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_Register_password), GetText(Resource.String.Lbl_Ok));
                            }
                            else
                            {
                                ProgressBar.Visibility = ViewStates.Visible;
                                RegisterButton.Visibility = ViewStates.Gone;

                                var (apiStatus, respond) = await RequestsAsync.Auth.RegisterAsync(UsernameEditText.Text.Replace(" ", ""), EmailEditText.Text.Replace(" ", ""), UsernameEditText.Text.Replace(" ", ""), PasswordEditText.Text, ConfirmPasswordEditText.Text, UserDetails.DeviceId, Referral);
                                if (apiStatus == 200)
                                {
                                    if (respond is RegisterObject auth)
                                    {
                                        if (auth.WaitValidation == 0)
                                        {
                                            SetDataLogin(auth);

                                            Intent intent = new Intent(this, typeof(GenresActivity));
                                            intent.PutExtra("Event", "Continue");
                                            StartActivity(intent);

                                            FinishAffinity();
                                        }
                                        else
                                        {
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_VerifyRegistration), GetText(Resource.String.Lbl_Ok));
                                        }
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
                                            case "This username is already taken":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister2), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "Username length must be between 5 / 32":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister3), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "Invalid username characters":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister4), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "This e-mail is already taken":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister5), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "This e-mail is invalid":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister6), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "Passwords don't match":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister7), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            case "Password is too short":
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister8), GetText(Resource.String.Lbl_Ok));
                                                break;
                                            default:
                                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                                break;
                                        }
                                    }

                                    ProgressBar.Visibility = ViewStates.Gone;
                                    RegisterButton.Visibility = ViewStates.Visible;
                                }
                                else if (apiStatus == 404)
                                {
                                    ProgressBar.Visibility = ViewStates.Gone;
                                    RegisterButton.Visibility = ViewStates.Visible;
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                                }
                            }
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        RegisterButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    RegisterButton.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                ProgressBar.Visibility = ViewStates.Gone;
                RegisterButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), ex.Message, GetText(Resource.String.Lbl_Ok));

            }
        }

        private void SignInTextView_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TermsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var url = InitializeDeepSound.WebsiteUrl + "/terms/terms";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void EyesIcon_Click(object sender, EventArgs e)
        {

            try
            {
                if (EyesIcon.Tag.ToString() == "hide")
                {
                    EyesIcon.SetImageResource(Resource.Drawable.new_eyes_show);
                    EyesIcon.Tag = "show";
                    PasswordEditText.InputType = Android.Text.InputTypes.TextVariationNormal | Android.Text.InputTypes.ClassText;
                    PasswordEditText.SetSelection(PasswordEditText.Text.Length);
                }
                else
                {
                    EyesIcon.SetImageResource(Resource.Drawable.new_eyes_signup);
                    EyesIcon.Tag = "hide";
                    PasswordEditText.InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText;
                    PasswordEditText.SetSelection(PasswordEditText.Text.Length);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Eyes2Icon_Click(object sender, EventArgs e)
        {
            try
            {
                if (EyesIcon2.Tag.ToString() == "hide")
                {
                    EyesIcon2.SetImageResource(Resource.Drawable.new_eyes_show);
                    EyesIcon2.Tag = "show";
                    ConfirmPasswordEditText.InputType = Android.Text.InputTypes.TextVariationNormal | Android.Text.InputTypes.ClassText;
                    ConfirmPasswordEditText.SetSelection(ConfirmPasswordEditText.Text.Length);
                }
                else
                {
                    EyesIcon2.SetImageResource(Resource.Drawable.new_eyes_signup);
                    EyesIcon2.Tag = "hide";
                    ConfirmPasswordEditText.InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText;
                    ConfirmPasswordEditText.SetSelection(ConfirmPasswordEditText.Text.Length);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void UsernameFrameLayout_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    InitEditTextsIconsColor();
                    UsernameFrameLayout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
                    UsernameIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));
                }
                else
                {
                    InitEditTextsIconsColor();

                    UsernameFrameLayout.SetBackgroundResource(Resource.Drawable.new_login_status);
                    UsernameIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EmailFrameLayout_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    InitEditTextsIconsColor();

                    EmailFrameLayout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
                    EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));
                }
                else
                {
                    InitEditTextsIconsColor();

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

        private void InitEditTextsIconsColor()
        {
            try
            {
                if (EmailEditText.Text != "" && EmailEditText.Text.Contains("@"))
                    EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

                if (UsernameEditText.Text != "")
                    UsernameIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

                if (PasswordEditText.Text != "")
                    PasswordIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

                if (ConfirmPasswordEditText.Text != "")
                    ConfirmPasswordIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ConfirmPasswordFrameLayout_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {

                    InitEditTextsIconsColor();
                    ConfirmPasswordLayout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
                    ConfirmPasswordIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.accent)));
                    EyesIcon2.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.accent)));

                }
                else
                {
                    InitEditTextsIconsColor();

                    ConfirmPasswordLayout.SetBackgroundResource(Resource.Drawable.new_login_status);
                    ConfirmPasswordIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
                    EyesIcon2.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PassFrameLayout_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    InitEditTextsIconsColor();
                    PassFrameLayout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
                    PasswordIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.accent)));
                    EyesIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.accent)));

                }
                else
                {

                    InitEditTextsIconsColor();
                    PassFrameLayout.SetBackgroundResource(Resource.Drawable.new_login_status);
                    PasswordIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
                    EyesIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        private void SetDataLogin(RegisterObject auth)
        {
            try
            {
                UserDetails.Username = UsernameEditText.Text;
                UserDetails.FullName = UsernameEditText.Text;
                UserDetails.Password = PasswordEditText.Text;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.Data.Id;
                UserDetails.Status = "Active";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = EmailEditText.Text;

                Current.AccessToken = auth.AccessToken;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId.ToString(),
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UsernameEditText.Text,
                    Password = PasswordEditText.Text,
                    Status = "Active",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId
                };
                ListUtils.DataUserLoginList = new ObservableCollection<DataTables.LoginTb> { user };

                UserDetails.IsLogin = true;

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                if (auth.Data != null)
                {
                    ListUtils.MyUserInfoList = new ObservableCollection<UserDataObject> { auth.Data };
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()) });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}