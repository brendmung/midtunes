using Android.App;
using Android.BillingClient.Api;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using DeepSound.Activities.SettingsUser;
using DeepSound.Activities.SettingsUser.General;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.PaymentGoogle;
using DeepSound.SQLite;
using DeepSoundClient;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using InAppBilling.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseActivity = DeepSound.Activities.Base.BaseActivity;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Upgrade
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class GoProActivity : BaseActivity, IBillingPaymentListener, IDialogListCallBack
    {
        #region Variables Basic

        private LinearLayout OptionLinerLayout;
        private AppCompatButton UpgradeButton;
        private TextView PriceText;

        private TextDecorator TextDecorator;
        private BillingSupport BillingSupport;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Go_Pro_Layout);

                TextDecorator = new TextDecorator();

                if (AppSettings.ShowInAppBilling && InitializeDeepSound.IsExtended)
                    BillingSupport = new BillingSupport(this, AppSettings.Cert, InAppBillingGoogle.ListProductSku, this);

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
                if (AppSettings.ShowInAppBilling && InitializeDeepSound.IsExtended)
                    BillingSupport?.Destroy();

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
                OptionLinerLayout = FindViewById<LinearLayout>(Resource.Id.OptionLinerLayout);
                UpgradeButton = FindViewById<AppCompatButton>(Resource.Id.btnUpgrade);

                PriceText = FindViewById<TextView>(Resource.Id.priceTextView);

                var list = ListUtils.SettingsSiteList;
                if (list != null)
                {
                    PriceText.Text = list.CurrencySymbol + list.ProPrice + " /" + GetText(Resource.String.Lbl_Month);
                }

                //Typeface font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                string name = "go_pro_array";
                int resourceId = Resources.GetIdentifier(name, "array", PackageName);
                if (resourceId == 0)
                {
                    return;
                }

                string[] planArray = Resources.GetStringArray(resourceId);
                if (planArray?.Length > 0)
                {
                    foreach (string options in planArray)
                    {
                        if (!string.IsNullOrEmpty(options))
                        {
                            AppCompatTextView text = new AppCompatTextView(this)
                            {
                                Text = options,
                                TextSize = 14
                            };
                            text.SetTextColor(Color.White);
                            text.Gravity = GravityFlags.CenterHorizontal;
                            //text.SetTypeface(font, TypefaceStyle.Normal);
                            TextDecorator.Content = options;
                            TextDecorator.DecoratedContent = new Android.Text.SpannableString(options);
                            //TextDecorator.SetTextColor(IonIconsFonts.Checkmark, "#43a735");
                            //TextDecorator.SetTextColor(IonIconsFonts.Close, "#e13c4c");

                            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);//height and width are inpixel
                            layoutParams.SetMargins(0, 30, 0, 0);

                            text.LayoutParameters = layoutParams;
                            OptionLinerLayout.AddView(text);
                            TextDecorator.Build(text, TextDecorator.DecoratedContent);
                        }
                    }
                }
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = " ";
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
                    UpgradeButton.Click += UpgradeButtonOnClick;
                }
                else
                {
                    UpgradeButton.Click -= UpgradeButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void UpgradeButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetString(Resource.String.Lbl_Wallet));
                if (AppSettings.ShowInAppBilling && InitializeDeepSound.IsExtended)
                    arrayAdapter.Add(GetString(Resource.String.Btn_GooglePlay));

                dialogList.SetTitle(GetText(Resource.String.Lbl_Go_Pro));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(IDialogInterface dialog, int position, string text)
        {
            try
            {
                if (text == GetString(Resource.String.Lbl_Wallet))
                {
                    var price = long.Parse(ListUtils.SettingsSiteList?.ProPrice ?? "0");
                    if (DeepSoundTools.CheckWallet(price))
                    {
                        if (Methods.CheckConnectivity())
                        {
                            var (apiStatus, respond) = await RequestsAsync.Payments.PurchaseAsync("go_pro", "");
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    Console.WriteLine(result.Message);

                                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dataUser.IsPro = 1;

                                        var sqlEntity = new SqLiteDatabase();
                                        sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);

                                        SettingsActivity.Instance.GoProLayout.Visibility = ViewStates.Gone;

                                        //var HomeFragmentProIcon = HomeActivity.GetInstance()?.HomeFragment?.ProIcon;
                                        //if (HomeFragmentProIcon != null)
                                        //    HomeFragmentProIcon.Visibility = ViewStates.Gone;
                                    }

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Upgraded), ToastLength.Long)?.Show();
                                    Finish();
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
                else if (text == GetString(Resource.String.Btn_GooglePlay))
                {
                    BillingSupport?.PurchaseNow(InAppBillingGoogle.Membership);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Billing

        public void OnPaymentError(string error)
        {
            Console.WriteLine(error);
        }

        public async void OnPaymentSuccess(IList<Purchase> result)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.User.UpgradeMembershipAsync(UserDetails.UserId.ToString());
                    if (apiStatus == 200)
                    {
                        var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                        if (dataUser != null)
                        {
                            dataUser.IsPro = 1;

                            var sqlEntity = new SqLiteDatabase();
                            sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);

                            //var HomeFragmentProIcon = HomeActivity.GetInstance()?.HomeFragment?.ProIcon;
                            //if (HomeFragmentProIcon != null)
                            //    HomeFragmentProIcon.Visibility = ViewStates.Gone;
                        }

                        Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
                        Finish();
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void GetPurchase(IList<Purchase> result)
        {

        }

        #endregion
    }

}