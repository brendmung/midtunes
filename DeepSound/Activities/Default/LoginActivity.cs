using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Auth.Api.Identity;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Extensions;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Com.Facebook;
using Com.Facebook.Login;
using Com.Facebook.Login.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.SocialLogins;
using DeepSound.Helpers.Utils;
using DeepSound.Library.OneSignalNotif;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Auth;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;

namespace DeepSound.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class LoginActivity : AppCompatActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, IOnSuccessListener, IOnFailureListener
    {
        #region Variables Basic

        private EditText EmailEditText, PasswordEditText;
        private ProgressBar ProgressBar;
        private AppCompatButton BtnSignIn;
        private TextView ForgotPassTextView, RegisterTextView;
        private FrameLayout WoWonderSignInButton, FbLoginButton, GoogleSignInButton;

        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker ProfileTracker;
        public static LoginActivity Instance;
        public static GoogleSignInClient MGoogleSignInClient;

        private ImageView BackIcon, EmailIcon, PasswordIcon, EyesIcon;
        private RelativeLayout EmailFrameLayout, PassFrameLayout;
        private CheckBox ChkRemember;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                InitializeDeepSound.Initialize(AppSettings.Cert, PackageName, AppSettings.TurnTrustFailureOnWebException, new MyReportModeApp());

                Methods.App.FullScreenApp(this, true);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.LoginLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitSocialLogins();

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

                if (AppSettings.EnableSmartLockForPasswords)
                    BuildClients(null);
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
                ProfileTracker?.StopTracking();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

                EmailFrameLayout = FindViewById<RelativeLayout>(Resource.Id.emailframelayout);
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                EmailIcon = FindViewById<ImageView>(Resource.Id.emailicon);

                PassFrameLayout = FindViewById<RelativeLayout>(Resource.Id.passframelayout);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                PasswordIcon = FindViewById<ImageView>(Resource.Id.passicon);

                EyesIcon = FindViewById<ImageView>(Resource.Id.eyesicon);
                EyesIcon.Click += EyesIcon_Click;
                EyesIcon.Tag = "hide";

                BtnSignIn = FindViewById<AppCompatButton>(Resource.Id.btn_login);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar);
                ProgressBar.Visibility = ViewStates.Gone;

                ChkRemember = FindViewById<CheckBox>(Resource.Id.ChkRemember);
                ChkRemember.Checked = true;

                ForgotPassTextView = FindViewById<TextView>(Resource.Id.txt_forgot_pass);
                RegisterTextView = FindViewById<TextView>(Resource.Id.txt_Regsiter);

                Methods.SetColorEditText(EmailEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(PasswordEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
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
                    BtnSignIn.Click += BtnSignInOnClick;
                    ForgotPassTextView.Click += ForgotPassTextViewOnClick;
                    RegisterTextView.Click += LinearRegisterOnClick;
                    EmailEditText.FocusChange += EmailFrameLayout_FocusChange;
                    PasswordEditText.FocusChange += PassFrameLayout_FocusChange;

                }
                else
                {
                    BackIcon.Click -= BackIconOnClick;
                    BtnSignIn.Click -= BtnSignInOnClick;
                    ForgotPassTextView.Click -= ForgotPassTextViewOnClick;
                    RegisterTextView.Click -= LinearRegisterOnClick;
                    EmailEditText.FocusChange -= EmailFrameLayout_FocusChange;
                    PasswordEditText.FocusChange -= PassFrameLayout_FocusChange;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);
                    LoginButton loginButton = new LoginButton(this);
                    ProfileTracker = new FbMyProfileTracker();
                    ProfileTracker.StartTracking();

                    FbLoginButton = FindViewById<FrameLayout>(Resource.Id.facebook);
                    FbLoginButton.Visibility = ViewStates.Visible;
                    FbLoginButton.Click += FbLoginButtonOnClick;

                    ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;
                    loginButton.SetPermissions(new string[]
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = ICallbackManager.Factory.Create();
                    LoginManager.Instance.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    FbLoginButton = FindViewById<FrameLayout>(Resource.Id.facebook);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient = GoogleSignIn.GetClient(this, gso);

                    GoogleSignInButton = FindViewById<FrameLayout>(Resource.Id.google);
                    GoogleSignInButton.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    GoogleSignInButton = FindViewById<FrameLayout>(Resource.Id.google);
                    GoogleSignInButton.Visibility = ViewStates.Gone;
                }

                //#WoWonder 
                if (AppSettings.ShowWoWonderLogin)
                {
                    WoWonderSignInButton = FindViewById<FrameLayout>(Resource.Id.Wowonder);
                    WoWonderSignInButton.Click += WoWonderSignInButtonOnClick;

                    //WoWonderSignInButton.Text = GetString(Resource.String.Lbl_LoginWith) + " " + AppSettings.AppNameWoWonder;
                    WoWonderSignInButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    WoWonderSignInButton = FindViewById<FrameLayout>(Resource.Id.Wowonder);
                    WoWonderSignInButton.Visibility = ViewStates.Gone;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void FbLoginButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                LoginManager.Instance.LogInWithReadPermissions(this, new List<string>
                {
                    "email",
                    "public_profile"
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Login With Facebook
        private void ProfileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                {
                    //FbFirstName = e.MProfile.FirstName;
                    //FbLastName = e.MProfile.LastName;
                    //FbName = e.MProfile.Name;
                    //FbProfileId = e.MProfile.Id;

                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAndWait();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Login With Google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MGoogleSignInClient == null)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient ??= GoogleSignIn.GetClient(this, gso);
                }

                var signInIntent = MGoogleSignInClient.SignInIntent;
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
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

        private void EmailFrameLayout_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    EmailFrameLayout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
                    EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));
                }
                else
                {
                    EmailFrameLayout.SetBackgroundResource(Resource.Drawable.new_login_status);
                    EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
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
                    if (EmailEditText.Text != "")
                        EmailIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, DeepSoundTools.IsTabDark() ? Color.White : Resource.Color.textDark_color)));

                    PassFrameLayout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
                    PasswordIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.accent)));
                    EyesIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.accent)));

                }
                else
                {
                    if (EmailEditText.Text != "")
                        EmailIcon.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : new Color(ContextCompat.GetColor(this, Resource.Color.textDark_color)));

                    PassFrameLayout.SetBackgroundResource(Resource.Drawable.new_login_status);
                    PasswordIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
                    EyesIcon.SetColorFilter(new Color(ContextCompat.GetColor(this, Resource.Color.text_color_in_between)));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EyesIcon_Click(object sender, EventArgs e)
        {
            try
            {
                if (EyesIcon.Tag?.ToString() == "hide")
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

        //Login DeepSound
        private async void BtnSignInOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (!string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(PasswordEditText.Text))
                    {
                        ProgressBar.Visibility = ViewStates.Visible;
                        BtnSignIn.Visibility = ViewStates.Gone;

                        await AuthApi(EmailEditText.Text.Replace(" ", ""), PasswordEditText.Text);
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    BtnSignIn.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), ex.Message, GetText(Resource.String.Lbl_Ok));
            }
        }

        private async Task AuthApi(string email, string password)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Auth.LoginAsync(email, password, UserDetails.DeviceId);
                if (apiStatus == 200)
                {
                    if (respond is LoginObject auth)
                    {
                        SetDataLogin(auth);

                        StartActivity(new Intent(this, typeof(HomeActivity)));
                        Finish();
                    }
                    else if (respond is LoginTwoFactorObject factorObject)
                    {
                        UserDetails.UserId = factorObject.UserId;
                        var intent = new Intent(this, typeof(VerificationAccountActivity));
                        intent.PutExtra("Type", "TwoFactor");
                        StartActivity(intent);

                        Finish();
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
                            case "Incorrect username or password":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "Your account is not activated yet, please check your inbox for the activation link":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                break;
                            default:
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                break;
                        }
                    }

                    ProgressBar.Visibility = ViewStates.Gone;
                    BtnSignIn.Visibility = ViewStates.Visible;
                }
                else if (apiStatus == 404)
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    BtnSignIn.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Open Register
        private void LinearRegisterOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Forgot Password
        private void ForgotPassTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(ForgotPasswordActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetDataLogin(LoginObject auth)
        {
            try
            {
                UserDetails.Username = EmailEditText.Text;
                UserDetails.FullName = EmailEditText.Text;
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
                    Username = EmailEditText.Text,
                    Password = PasswordEditText.Text,
                    Status = "Active",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId
                };
                ListUtils.DataUserLoginList = new ObservableCollection<DataTables.LoginTb> { user };

                UserDetails.IsLogin = true;

                if (ChkRemember.Checked)
                {
                    var dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                }

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

        #endregion

        #region Social Logins

        private string FbAccessToken, GAccessToken, GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;

                //SetResult(Result.Canceled);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;

                // Handle e
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                //SetResult(Result.Canceled);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;

                //SetResult(Result.Ok);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    //Login Api 
                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is LoginObject auth)
                        {
                            if (!string.IsNullOrEmpty(json?.ToString()))
                            {
                                var data = json.ToString();
                                var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                                //FbEmail = result.Email; 
                            }

                            SetDataLogin(auth);

                            StartActivity(new Intent(this, typeof(HomeActivity)));
                            Finish();
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
                                case "Incorrect username or password":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Your account is not activated yet, please check your inbox for the activation link":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception e)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), e.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //======================================================

        #region Google

        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    BtnSignIn.Visibility = ViewStates.Gone;

                    //var GAccountName = acct.Account.Name;
                    //var GAccountType = acct.Account.Type;
                    //var GDisplayName = acct.DisplayName;
                    //var GFirstName = acct.GivenName;
                    //var GLastName = acct.FamilyName;
                    //var GProfileId = acct.Id;
                    //var GEmail = acct.Email;
                    //var GImg = acct.PhotoUrl.Path;
                    GAccessToken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;
                    Console.WriteLine(GServerCode);

                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        //Login Api 
                        //string key = Methods.App.GetValueFromManifest(this, "com.google.android.geo.API_KEY");
                        var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(GAccessToken, "google", UserDetails.DeviceId);
                        if (apiStatus == 200)
                        {
                            if (respond is LoginObject auth)
                            {
                                SetDataLogin(auth);

                                StartActivity(new Intent(this, typeof(HomeActivity)));
                                Finish();
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
                                    case "Incorrect username or password":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case "Your account is not activated yet, please check your inbox for the activation link":
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    default:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                        break;
                                }
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            BtnSignIn.Visibility = ViewStates.Visible;
                        }
                        else if (apiStatus == 404)
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            BtnSignIn.Visibility = ViewStates.Visible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                BtnSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), e.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //======================================================

        #region WoWonder

        //Event Click login using WoWonder
        private void WoWonderSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(WoWonderLoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void LoginWoWonder(string woWonderAccessToken)
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Visible;
                BtnSignIn.Visibility = ViewStates.Gone;
                ForgotPassTextView.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(woWonderAccessToken))
                {
                    //Login Api 
                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(woWonderAccessToken, "wowonder", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is LoginObject auth)
                        {
                            SetDataLogin(auth);

                            StartActivity(new Intent(this, typeof(HomeActivity)));
                            FinishAffinity();
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
                                case "Incorrect username or password":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin2), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "Your account is not activated yet, please check your inbox for the activation link":
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        ForgotPassTextView.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        BtnSignIn.Visibility = ViewStates.Visible;
                        ForgotPassTextView.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                // Logins Facebook
                MFbCallManager?.OnActivityResult(requestCode, (int)resultCode, data);
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 0)
                {
                    var task = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);
                    SetContentGoogle(task);
                }
                else if (requestCode == RcCredentialsHint)
                {
                    if (resultCode == Result.Ok)
                    {
                        SignInCredential credential = OneTapClient.GetSignInCredentialFromIntent(data);
                        string idToken = credential.GoogleIdToken;
                        string username = credential.Id;
                        string password = credential.Password;

                        if (!string.IsNullOrEmpty(credential?.Id) && !string.IsNullOrEmpty(credential?.Password))
                        {
                            // Email/password account
                            Console.WriteLine("Signed in as {0}", credential.Id);

                            //send api auth  
                            ProgressBar.Visibility = ViewStates.Visible;
                            BtnSignIn.Visibility = ViewStates.Gone;

                            await AuthApi(credential.Id, credential.Password);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


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

        #region Cross App Authentication

        private static readonly int RcCredentialsHint = 10;

        private ISignInClient OneTapClient;
        private BeginSignInRequest SignInRequest;

        private async void BuildClients(string accountName)
        {
            try
            {
                OneTapClient = Identity.GetSignInClient(this);
                SignInRequest = new BeginSignInRequest.Builder()
                    .SetPasswordRequestOptions(new BeginSignInRequest.PasswordRequestOptions.Builder().SetSupported(true).Build())
                    .SetGoogleIdTokenRequestOptions(new BeginSignInRequest.GoogleIdTokenRequestOptions.Builder()
                        .SetSupported(true)
                        // Your server's client ID, not your Android client ID.
                        .SetServerClientId(AppSettings.ClientId)
                        // true : Only show accounts previously used to sign in.
                        // false : Show all accounts on the device.
                        .SetFilterByAuthorizedAccounts(false)
                        .Build())
                    // true : Automatically sign in when exactly one credential is retrieved.
                    //.SetAutoSelectEnabled(true)
                    .Build();

                await OneTapClient.BeginSignIn(SignInRequest).AddOnSuccessListener(this).AddOnFailureListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        void IOnSuccessListener.OnSuccess(Object result)
        {
            try
            {
                if (result is BeginSignInResult results)
                {
                    StartIntentSenderForResult(results.PendingIntent.IntentSender, RcCredentialsHint, null, 0, 0, 0);
                }
            }
            catch (IntentSender.SendIntentException e)
            {
                Console.WriteLine("Couldn't start One Tap UI: " + e.LocalizedMessage);
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {

        }

        #endregion

    }
}