using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads;
using DeepSound.Activities.Address;
using DeepSound.Activities.Address.Adapters;
using DeepSound.Activities.Product.Adapters;
using DeepSound.Activities.SettingsUser.General;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Address;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Product
{
    public class CartFragment : Fragment
    {
        #region Variables Basic

        private HomeActivity GlobalContext;

        private LinearLayout TotalLinear, AddNewAddressLinear;
        private TextView TotalNumber;
        private AddressAdapter AddressAdapter;
        private CartAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler, AddressRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private AdView MAdView;
        private TextView BuyButton;
        private ProductProfileFragment ProductProfileFragment;
        private string AddressId;
        private long CountTotal;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.CheckoutCartLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                StartApiService();
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

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                AdsGoogle.LifecycleAdView(MAdView, "Resume");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                AdsGoogle.LifecycleAdView(MAdView, "Pause");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                AdsGoogle.LifecycleAdView(MAdView, "Destroy");
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.CartRecycler);
                AddressRecycler = (RecyclerView)view.FindViewById(Resource.Id.AddressRecycler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                TotalLinear = view.FindViewById<LinearLayout>(Resource.Id.TotalLinear);
                TotalNumber = view.FindViewById<TextView>(Resource.Id.TotalNumber);

                AddNewAddressLinear = view.FindViewById<LinearLayout>(Resource.Id.AddNewAddressLinear);
                AddNewAddressLinear.Click += AddNewAddressLinearOnClick;

                BuyButton = view.FindViewById<TextView>(Resource.Id.toolbar_title);
                BuyButton.Click += BuyButtonOnClick;

                MAdView = view.FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                GlobalContext.SetToolBar(toolbar, GetString(Resource.String.Lbl_Carts));
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
                MAdapter = new CartAdapter(Activity) { CartsList = new ObservableCollection<CartDataObject>() };
                MAdapter.ItemClick += MAdapterItemClick;
                MAdapter.OnRemoveButtItemClick += MAdapterOnOnRemoveButtItemClick;
                MAdapter.OnSelectQtyItemClick += MAdapterOnOnSelectQtyItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CartDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                //=====

                AddressAdapter = new AddressAdapter(Activity, "Select") { AddressList = new ObservableCollection<AddressDataObject>() };
                AddressAdapter.ItemClick += AddressAdapterItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                AddressRecycler.SetLayoutManager(LayoutManager);
                AddressRecycler.HasFixedSize = true;
                AddressRecycler.SetItemViewCacheSize(10);
                AddressRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                AddressRecycler.SetAdapter(AddressAdapter);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void AddNewAddressLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(AddressActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //select address
        private void AddressAdapterItemClick(object sender, AddressAdapterClickEventArgs e)
        {
            try
            {
                var item = AddressAdapter.GetItem(e.Position);
                if (item != null)
                {
                    AddressId = item.Id;

                    var list = AddressAdapter.AddressList.Where(a => a.Selected).ToList();
                    foreach (var data in list)
                    {
                        data.Selected = false;
                    }

                    item.Selected = true;
                    AddressAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Checkout all cart
        private async void BuyButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(AddressId))
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_PleaseSelectAddress), ToastLength.Long)?.Show();
                    return;
                }

                if (DeepSoundTools.CheckWallet(CountTotal))
                {
                    if (Methods.CheckConnectivity())
                    {
                        var (apiStatus, respond) = await RequestsAsync.Product.BuyProductAsync(AddressId);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                Console.WriteLine(result.Message);

                                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_OrderPlacedSuccessfully), ToastLength.Long)?.Show();
                            }
                        }
                        else Methods.DisplayReportResult(Activity, respond);
                    }
                    else
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
                else
                {
                    var dialogBuilder = new MaterialAlertDialogBuilder(Activity);
                    dialogBuilder.SetTitle(GetText(Resource.String.Lbl_Wallet));
                    dialogBuilder.SetMessage(GetText(Resource.String.Lbl_Error_NoWallet));
                    dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
                    {
                        try
                        {
                            StartActivity(new Intent(Activity, typeof(WalletActivity)));
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //change qty
        private void MAdapterOnOnSelectQtyItemClick(object sender, CartAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item?.Product != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var dialogList = new MaterialAlertDialogBuilder(Context);

                        var arrayAdapter = new List<string>();
                        for (int i = 1; i <= 100; i++)
                        {
                            arrayAdapter.Add(i.ToString());
                        }

                        dialogList.SetTitle(GetText(Resource.String.Lbl_Qty));
                        dialogList.SetItems(arrayAdapter.ToArray(), (o, args) =>
                        {
                            try
                            {
                                var text = arrayAdapter[args.Which];

                                item.Units = args.Which + 1;
                                e.CountQty.Text = Context.GetText(Resource.String.Lbl_Qty) + " : " + text;

                                MAdapter.NotifyItemChanged(e.Position);

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Product.ChangeQtyProductAsync(item.Product.Id?.ToString(), text) });

                                CheckCountTotalProduct();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                        dialogList.Show();
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //remove Cart
        private void MAdapterOnOnRemoveButtItemClick(object sender, CartAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item?.Product != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Product.AddToCartAsync(item.Product.Id?.ToString(), "Remove") });

                        MAdapter.CartsList.Remove(item);
                        MAdapter.NotifyDataSetChanged();

                        Activity?.RunOnUiThread(() => ShowEmptyPage("Carts"));
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open Profile Cart
        private void MAdapterItemClick(object sender, CartAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item.Product));
                    bundle.PutString("ProductId", item.ProductId.ToString());

                    ProductProfileFragment = new ProductProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(ProductProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.CartsList.Clear();
                MAdapter.NotifyDataSetChanged();

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Carts

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadCarts(), () => LoadAddress() });
        }

        private async Task LoadCarts(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.CartsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Product.GetCartsAsync("20", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetCartsObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.CartsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.CartsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CartsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.CartsList = new ObservableCollection<CartDataObject>(result.Data);
                                Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.CartsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreCarts), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity?.RunOnUiThread(() => ShowEmptyPage("Carts"));
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private async Task LoadAddress(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                int countList = AddressAdapter.AddressList.Count;
                var (apiStatus, respond) = await RequestsAsync.Address.GetAddressListAsync("15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetAddressListObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = AddressAdapter.AddressList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    AddressAdapter.AddressList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { AddressAdapter.NotifyItemRangeInserted(countList, AddressAdapter.AddressList.Count - countList); });
                            }
                            else
                            {
                                AddressAdapter.AddressList = new ObservableCollection<AddressDataObject>(result.Data);
                                Activity?.RunOnUiThread(() => { AddressAdapter.NotifyDataSetChanged(); });
                            }
                        }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity?.RunOnUiThread(() => ShowEmptyPage("Address"));
            }
            else
            {
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void ShowEmptyPage(string type)
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (type == "Cart")
                {
                    if (MAdapter.CartsList.Count > 0)
                    {
                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        MRecycler.Visibility = ViewStates.Gone;

                        Inflated ??= EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoCarts);
                        if (x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                        }
                        EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else
                {

                }

                CheckCountTotalProduct();
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void CheckCountTotalProduct()
        {
            try
            {
                CountTotal = 0;
                foreach (var data in MAdapter.CartsList)
                {
                    if (data.Units != null && data.Units.Value > 1)
                    {
                        if (data.Product.Price != null)
                            CountTotal += (data.Product.Price.Value * data.Units.Value);
                    }
                    else
                    {
                        if (data.Product.Price != null)
                            CountTotal += data.Product.Price.Value;
                    }
                }

                TotalNumber.Text = "$" + CountTotal;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


    }
}