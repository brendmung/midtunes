using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Com.Razorpay;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Payment;
using DeepSoundClient;
using DeepSoundClient.Classes.Payment;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using IyziPay;
using IyziPay.Lib.Model;
using SecurionPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseActivity = DeepSound.Activities.Base.BaseActivity;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class WalletActivity : BaseActivity, IDialogListCallBack, IPaymentResultWithDataListener, ISecurionPayPaymentListener, IIyziPayPaymentListener, IDialogInputCallBack
    {
        #region Variables Basic

        private ImageView Avatar;
        private TextView TxtProfileName, TxtUsername;

        private TextView TxtMyBalance;
        public EditText TxtAmount;
        private AppCompatButton BtnReplenish;
        private InitPayPalPayment InitPayPalPayment;
        private InitPayStackPayment PayStackPayment;
        private InitCashFreePayment CashFreePayment;
        private InitRazorPayPayment InitRazorPay;
        private InitPaySeraPayment PaySeraPayment;
        private InitSecurionPayPayment SecurionPayPayment;
        private InitIyziPayPayment IyziPayPayment;
        private InitAamarPayPayment AamarPayPayment;

        private string Price;
        private static WalletActivity Instance;

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
                SetContentView(Resource.Layout.WalletLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitBuy();
                InitComponent();
                InitToolbar();
                Get_Data_User();

                AdsGoogle.Ad_AdMobNative(this);
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
                if (AppSettings.ShowRazorPay) InitRazorPay?.StopRazorPay();
                if (AppSettings.ShowPayStack) PayStackPayment?.StopPayStack();
                if (AppSettings.ShowPaySera) PaySeraPayment?.StopPaySera();
                if (AppSettings.ShowSecurionPay) SecurionPayPayment?.StopSecurionPay();
                if (AppSettings.ShowIyziPay) IyziPayPayment?.StopIyziPay();

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
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitBuy()
        {
            try
            {
                if (AppSettings.ShowPaypal) InitPayPalPayment ??= new InitPayPalPayment(this);
                if (AppSettings.ShowRazorPay) InitRazorPay ??= new InitRazorPayPayment(this);
                if (AppSettings.ShowPayStack) PayStackPayment ??= new InitPayStackPayment(this);
                if (AppSettings.ShowCashFree) CashFreePayment ??= new InitCashFreePayment(this);
                if (AppSettings.ShowPaySera) PaySeraPayment ??= new InitPaySeraPayment(this);
                if (AppSettings.ShowSecurionPay) SecurionPayPayment ??= new InitSecurionPayPayment(this, this, ListUtils.SettingsSiteList?.SecurionpayPublicKey);
                if (AppSettings.ShowAamarPay) AamarPayPayment ??= new InitAamarPayPayment(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            try
            {
                Avatar = FindViewById<ImageView>(Resource.Id.avatar);
                TxtProfileName = FindViewById<TextView>(Resource.Id.name);
                TxtUsername = FindViewById<TextView>(Resource.Id.tv_subname);

                TxtMyBalance = FindViewById<TextView>(Resource.Id.myBalance);

                TxtAmount = FindViewById<EditText>(Resource.Id.AmountEditText);
                BtnReplenish = FindViewById<AppCompatButton>(Resource.Id.ReplenishButton);

                Methods.SetColorEditText(TxtAmount, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

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
                    toolbar.Title = GetText(Resource.String.Lbl_Wallet);
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
                    BtnReplenish.Click += BtnReplenishOnClick;
                }
                else
                {
                    BtnReplenish.Click -= BtnReplenishOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static WalletActivity GetInstance()
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

        #endregion

        #region Events

        private void BtnReplenishOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text) || Convert.ToInt32(TxtAmount.Text) == 0)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterAmount), ToastLength.Long)?.Show();
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    return;
                }

                Price = TxtAmount.Text;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                if (AppSettings.ShowPaypal) arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));
                if (AppSettings.ShowCreditCard) arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));
                if (AppSettings.ShowBankTransfer) arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));
                if (AppSettings.ShowRazorPay) arrayAdapter.Add(GetString(Resource.String.Lbl_RazorPay));
                if (AppSettings.ShowPayStack) arrayAdapter.Add(GetString(Resource.String.Lbl_PayStack));
                if (AppSettings.ShowCashFree) arrayAdapter.Add(GetString(Resource.String.Lbl_CashFree));
                if (AppSettings.ShowPaySera) arrayAdapter.Add(GetString(Resource.String.Lbl_PaySera));
                if (AppSettings.ShowSecurionPay) arrayAdapter.Add(GetString(Resource.String.Lbl_SecurionPay));
                if (AppSettings.ShowPayUmoney) arrayAdapter.Add(GetString(Resource.String.Lbl_PayUmoney));
                if (AppSettings.ShowAuthorizeNet) arrayAdapter.Add(GetString(Resource.String.Lbl_AuthorizeNet));
                if (AppSettings.ShowIyziPay) arrayAdapter.Add(GetString(Resource.String.Lbl_IyziPay));
                if (AppSettings.ShowAamarPay) arrayAdapter.Add(GetString(Resource.String.Lbl_AamarPay));
                if (AppSettings.ShowFlutterWave) arrayAdapter.Add(GetString(Resource.String.Lbl_FlutterWave));

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Btn_Paypal))
                {
                    InitPayPalPayment.BtnPaypalOnClick(Price);
                }
                else if (text == GetString(Resource.String.Lbl_CreditCard))
                {
                    OpenIntentCreditCard();
                }
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
                }
                else if (text == GetString(Resource.String.Lbl_RazorPay))
                {
                    InitRazorPay?.BtnRazorPayOnClick(Price);
                }
                else if (text == GetString(Resource.String.Lbl_PayStack))
                {
                    OpenPayStackDialog();
                }
                else if (text == GetString(Resource.String.Lbl_CashFree))
                {
                    OpenCashFreeDialog();
                }
                else if (text == GetString(Resource.String.Lbl_PaySera))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show();

                    await PaySera();
                }
                else if (text == GetString(Resource.String.Lbl_SecurionPay))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show();

                    await SecurionPay();
                }
                else if (text == GetString(Resource.String.Lbl_PayUmoney))
                {
                    OpenIntentPayUmoney();
                }
                else if (text == GetString(Resource.String.Lbl_AuthorizeNet))
                {
                    OpenIntentAuthorizeNet();
                }
                else if (text == GetString(Resource.String.Lbl_IyziPay))
                {
                    IyziPay();
                }
                else if (text == GetString(Resource.String.Lbl_AamarPay))
                {
                    AamarPayPayment.BtnAamarPayOnClick(Price);
                }
                else if (text == GetString(Resource.String.Lbl_FlutterWave))
                {
                    FlutterWave();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnInput(IDialogInterface dialog, string input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.Length <= 0) return;
                    var check = Methods.FunString.IsEmailValid(input.Replace(" ", ""));
                    if (!check)
                    {
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        return;
                    }

                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show();

                    await PayStack(input);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentCreditCard()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Price", Price);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", "");
                intent.PutExtra("Price", Price);
                intent.PutExtra("payType", "AddFunds");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentPayUmoney()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PayUmoneyPaymentActivity));
                intent.PutExtra("Price", Price);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentAuthorizeNet()
        {
            try
            {
                Intent intent = new Intent(this, typeof(AuthorizeNetPaymentActivity));
                intent.PutExtra("Price", Price);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void FlutterWave()
        {
            try
            {
                Intent intent = new Intent(this, typeof(FlutterWaveActivity));
                intent.PutExtra("Price", Price);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region RazorPay

        public void OnPaymentError(int code, string response, PaymentData p2)
        {
            try
            {
                Console.WriteLine("razorpay : Payment failed: " + code + " " + response);
                Toast.MakeText(this, "Payment failed: " + response, ToastLength.Long)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPaymentSuccess(string razorpayPaymentId, PaymentData p1)
        {
            try
            {
                Console.WriteLine("razorpay : Payment Successful:" + razorpayPaymentId);

                if (!string.IsNullOrEmpty(razorpayPaymentId) && Methods.CheckConnectivity())
                {
                    var priceInt = Convert.ToInt32(Price) * 100;

                    var (apiStatus, respond) = await RequestsAsync.Payments.TopWalletRazorpayAsync(razorpayPaymentId, priceInt.ToString());
                    if (apiStatus == 200)
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                TxtAmount.Text = string.Empty;
                                Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    else
                        Methods.DisplayReportResult(this, respond);
                }
                else if (!string.IsNullOrEmpty(razorpayPaymentId))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region CashFree

        private EditText TxtName, TxtEmail, TxtPhone;
        private void OpenCashFreeDialog()
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);

                dialog.SetTitle(Resource.String.Lbl_CashFree);

                View view = LayoutInflater.Inflate(Resource.Layout.CashFreePaymentLayout, null);
                dialog.SetView(view);
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_PayNow), async (o, args) =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterName), ToastLength.Short)?.Show();
                            return;
                        }

                        var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                        switch (check)
                        {
                            case false:
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                return;
                        }

                        if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short)?.Show();
                            return;
                        }

                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                        await CashFree(TxtName.Text, TxtEmail.Text, TxtPhone.Text);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                var iconName = view.FindViewById<TextView>(Resource.Id.IconName);
                TxtName = view.FindViewById<EditText>(Resource.Id.NameEditText);

                var iconEmail = view.FindViewById<TextView>(Resource.Id.IconEmail);
                TxtEmail = view.FindViewById<EditText>(Resource.Id.EmailEditText);

                var iconPhone = view.FindViewById<TextView>(Resource.Id.IconPhone);
                TxtPhone = view.FindViewById<EditText>(Resource.Id.PhoneEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconName, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconEmail, FontAwesomeIcon.PaperPlane);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconPhone, FontAwesomeIcon.Mobile);

                Methods.SetColorEditText(TxtName, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPhone, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                var local = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (local != null)
                {
                    TxtName.Text = DeepSoundTools.GetNameFinal(local);
                    TxtEmail.Text = local.Email;
                    TxtPhone.Text = local.PhoneNumber;
                }

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private async Task CashFree(string name, string email, string phone)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", name},
                        {"phone", phone},
                        {"email", email},
                        {"price", Price},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializeCashFreeAsync("initialize", AppSettings.CashFreeCurrency, UserDetails.UserId.ToString(), ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "", ListUtils.SettingsSiteList?.CashfreeMode, keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CashFreeObject result:
                                        CashFreePayment ??= new InitCashFreePayment(this);
                                        CashFreePayment.DisplayCashFreePayment(result, Price);
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region PayStack

        private void OpenPayStackDialog()
        {
            try
            {
                var dialogBuilder = new MaterialAlertDialogBuilder(this);
                dialogBuilder.SetTitle(Resource.String.Lbl_PayStack);

                EditText input = new EditText(this);
                input.SetHint(Resource.String.Lbl_Email);
                input.InputType = InputTypes.TextVariationEmailAddress;
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                input.LayoutParameters = lp;

                dialogBuilder.SetView(input);

                dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_PayNow), new MaterialDialogUtils(input, this));
                dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                dialogBuilder.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PayStack(string email)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //var priceInt = Convert.ToInt32(Price) * 100;

                    var keyValues = new Dictionary<string, string>
                    {
                        {"email", email},
                        {"price", Price},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePayStackAsync("initialize", keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case InitializePaymentObject result:
                                        PayStackPayment ??= new InitPayStackPayment(this);
                                        PayStackPayment.DisplayPayStackPayment(result.Url, Price);
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region PaySera

        private async Task PaySera()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"price", Price},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializePaySeraAsync("initialize", keyValues);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case InitializePaymentObject result:
                                        PaySeraPayment ??= new InitPaySeraPayment(this);
                                        PaySeraPayment.DisplayPaySeraPayment(result.Url, Price);
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region SecurionPay

        private async Task SecurionPay()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Payments.InitializeSecurionPayAsync("initialize", Price);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case InitializeSecurionPayObject result:
                                        SecurionPayPayment ??= new InitSecurionPayPayment(this, this, ListUtils.SettingsSiteList?.SecurionpayPublicKey);
                                        SecurionPayPayment.DisplaySecurionPayPayment(result.Token, Price, DeepSoundTools.IsTabDark() ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task SecurionPay(string request, string charge)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Payments.SecurionPayAsync(request, charge);
                    switch (apiStatus)
                    {
                        case 200:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();

                            break;
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPaymentError(string error)
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPaymentSuccess(SecurionPayResult result)
        {
            try
            {
                if (!string.IsNullOrEmpty(result?.Charge?.Id))
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SecurionPay("pay", result.Charge.Id) });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region IyziPay

        private void IyziPay()
        {
            try
            {
                IyziPayPaymentObject request = new IyziPayPaymentObject()
                {
                    Locale = Locale.TR,

                    Id = ListUtils.SettingsSiteList.IyzipayBuyerId,
                    Name = ListUtils.SettingsSiteList.IyzipayBuyerName,
                    Surname = ListUtils.SettingsSiteList.IyzipayBuyerSurname,
                    GsmNumber = ListUtils.SettingsSiteList.IyzipayBuyerGsmNumber,
                    Email = ListUtils.SettingsSiteList.IyzipayBuyerEmail,
                    IdentityNumber = ListUtils.SettingsSiteList.IyzipayIdentityNumber,
                    Address = ListUtils.SettingsSiteList.IyzipayAddress,
                    City = ListUtils.SettingsSiteList.IyzipayCity,
                    Country = ListUtils.SettingsSiteList.IyzipayCountry,
                    Zip = ListUtils.SettingsSiteList.IyzipayZip,

                    Price = Price,
                    Currency = Currency.TRY,
                    CallbackUrl = InitializeDeepSound.WebsiteUrl + "/requests.php?f=iyzipay&s=success&amount=" + Price,

                    ApiKey = ListUtils.SettingsSiteList.IyzipayKey,
                    SecretKey = ListUtils.SettingsSiteList.IyzipaySecretKey,
                    BaseUrl = ListUtils.SettingsSiteList.IyzipayMode == "0" ? "https://merchant.iyzipay.com/" : "https://sandbox-api.iyzipay.com/"
                };

                IyziPayPayment ??= new InitIyziPayPayment(this, this, request);
                IyziPayPayment.DisplayIyziPayPayment(Price, DeepSoundTools.IsTabDark() ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
                //string Token = IyziPayPayment.CheckoutFormInitialize?.Token; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnIyziPayPaymentSuccess(CheckoutFormInitialize result)
        {
            try
            {
                if (!string.IsNullOrEmpty(result?.Token))
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => IyziPay(result?.Token) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task IyziPay(string token)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Payments.IyziPayAsync("wallet", token, Price);
                    switch (apiStatus)
                    {
                        case 200:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();

                            IyziPayPayment.StopIyziPay();
                            break;
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void Get_Data_User()
        {
            try
            {
                if (ListUtils.MyUserInfoList?.Count == 0)
                    await ApiRequest.GetInfoData(this, UserDetails.UserId.ToString());

                var local = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (local != null)
                {
                    GlideImageLoader.LoadImage(this, local.Avatar, Avatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    TxtProfileName.Text = DeepSoundTools.GetNameFinal(local);
                    TxtUsername.Text = "@" + local.Username;

                    TxtMyBalance.Text = local.WalletFormat;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}