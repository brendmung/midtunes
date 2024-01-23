using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using DeepSound.Activities.Base;
using DeepSound.Activities.SettingsUser.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class WithdrawalsActivity : BaseActivity
    {
        #region Variables Basic

        private TextView CountBalanceText, SendText, IconAmount, IconPayPalEmail;
        private EditText AmountEditText, PayPalEmailEditText;
        private double CountBalance;
        private LinearLayout PaymentHistoryLinear;
        private TextView IconPaymentHistory;
        private RecyclerView MRecycler;
        private PaymentHistoryAdapter MAdapter;
        private NestedScrollViewOnScroll MainScrollEvent;

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
                SetContentView(Resource.Layout.WithdrawalsLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
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
                SendText = FindViewById<TextView>(Resource.Id.toolbar_title);
                CountBalanceText = FindViewById<TextView>(Resource.Id.countBalanceText);

                IconAmount = FindViewById<TextView>(Resource.Id.IconAmount);
                AmountEditText = FindViewById<EditText>(Resource.Id.AmountEditText);

                IconPayPalEmail = FindViewById<TextView>(Resource.Id.IconPayPalEmail);
                PayPalEmailEditText = FindViewById<EditText>(Resource.Id.PayPalEmailEditText);

                PaymentHistoryLinear = (LinearLayout)FindViewById(Resource.Id.PaymentHistoryLinear);
                IconPaymentHistory = (TextView)FindViewById(Resource.Id.iconPaymentHistory);
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPaymentHistory, FontAwesomeIcon.ListUl);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAmount, FontAwesomeIcon.HandHoldingUsd);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, IconPayPalEmail, FontAwesomeIcon.Paypal);

                Methods.SetColorEditText(AmountEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(PayPalEmailEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
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
                    toolbar.Title = GetText(Resource.String.Lbl_Withdrawals);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new PaymentHistoryAdapter(this)
                {
                    WithdrawalsList = new ObservableCollection<WithdrawalsDataObject>()
                };
                MRecycler.SetLayoutManager(new LinearLayoutManager(this));
                MRecycler.SetItemViewCacheSize(20);
                MRecycler.HasFixedSize = true;
                MRecycler.NestedScrollingEnabled = false;
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);

                var NestedScrollView = FindViewById<NestedScrollView>(Resource.Id.nested_scroll_view);

                NestedScrollViewOnScroll recyclerViewOnScrollListener = new NestedScrollViewOnScroll();
                MainScrollEvent = recyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                NestedScrollView.SetOnScrollChangeListener(recyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

                MAdapter.WithdrawalsList.Insert(0, new WithdrawalsDataObject
                {
                    Id = "000",
                    Amount = GetString(Resource.String.Lbl_Amount),
                    Requested = GetString(Resource.String.Lbl_Requested),
                    Status = GetString(Resource.String.Lbl_Status)
                });

                MAdapter.NotifyDataSetChanged();

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
                    SendText.Click += SendTextOnClick;
                }
                else
                {
                    SendText.Click -= SendTextOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.WithdrawalsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void SendTextOnClick(object sender, EventArgs e)
        {
            try
            {
                if (CountBalance < Convert.ToDouble(AmountEditText.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CantRequestWithdrawals), ToastLength.Long)?.Show();
                }
                else if (string.IsNullOrEmpty(PayPalEmailEditText.Text.Replace(" ", "")) || string.IsNullOrEmpty(AmountEditText.Text.Replace(" ", "")))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
                }
                else
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        var (apiStatus, respond) = await RequestsAsync.Common.WithdrawAsync(AmountEditText.Text, PayPalEmailEditText.Text);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                Console.WriteLine(result.Message);
                                Toast.MakeText(this, GetText(Resource.String.Lbl_RequestSentWithdrawals), ToastLength.Long)?.Show();
                            }
                        }
                        else
                        {
                            Methods.DisplayAndHudErrorResult(this, respond);
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
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
                    CountBalance = Convert.ToDouble(local.Balance);
                    CountBalanceText.Text = "$" + CountBalance.ToString(CultureInfo.InvariantCulture);

                    StartApiService();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Load Blocks 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadData(offset) });
        }

        private async Task LoadData(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.WithdrawalsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Common.GetWithdrawalsAsync("15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetWithdrawalsList result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            foreach (var item in from item in result.Data let check = MAdapter.WithdrawalsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                MAdapter.WithdrawalsList.Add(item);
                            }

                            if (countList > 0)
                            {
                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.WithdrawalsList.Count - countList); });
                            }
                            else
                            {
                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (MAdapter.WithdrawalsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    PaymentHistoryLinear.Visibility = ViewStates.Visible;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;
                    PaymentHistoryLinear.Visibility = ViewStates.Gone;
                }

                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion


    }
}