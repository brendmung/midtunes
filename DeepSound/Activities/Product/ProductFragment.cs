using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads;
using DeepSound.Activities.Product.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Requests;
using Google.Android.Material.FloatingActionButton;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Product
{
    public class ProductFragment : Fragment
    {
        #region Variables Basic

        public ProductAdapter MAdapter;
        private HomeActivity GlobalContext;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        private View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;
        private AdView MAdView;
        private TextView FilterButton;
        private FloatingActionButton ActionButton;
        private ProductProfileFragment ProductProfileFragment;

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
                View view = inflater.Inflate(Resource.Layout.RecyclerDefaultLayout, container, false);
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                FilterButton = (TextView)view.FindViewById(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FilterButton, IonIconsFonts.Options);
                FilterButton.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                FilterButton.SetTextSize(ComplexUnitType.Sp, 20f);
                FilterButton.Visibility = ViewStates.Visible;
                FilterButton.Click += FilterButtonOnClick;

                ActionButton = view.FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                ActionButton.Visibility = ViewStates.Visible;
                ActionButton.Click += ActionButtonOnClick;

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
                GlobalContext.SetToolBar(toolbar, GetString(Resource.String.Lbl_Products));
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
                MAdapter = new ProductAdapter(Activity) { ProductsList = new ObservableCollection<ProductDataObject>() };
                MAdapter.ItemClick += MAdapterItemClick;
                MAdapter.AddButtonItemClick += MAdapterOnAddButtonItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<ProductDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Add new Product 
        private void ActionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivityForResult(new Intent(Activity, typeof(CreateProductActivity)), 3500);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Filter
        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                FilterProductDialogFragment dialogFragment = new FilterProductDialogFragment(this);
                dialogFragment.Show(ChildFragmentManager, dialogFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.ProductsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id.ToString());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnAddButtonItemClick(object sender, ProductAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        return;
                    }

                    switch (e.AddButton?.Tag?.ToString())
                    {
                        case "false":
                            e.AddButton.Text = GetText(Resource.String.Lbl_RemoveFromCart);
                            e.AddButton.Tag = "true";
                            item.AddedToCart = 1;

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Product.AddToCartAsync(item.Id?.ToString(), "Add") });

                            GlobalContext?.TrendingFragment?.UpdateBadgeViewIcon(true);

                            break;
                        default:
                            e.AddButton.Text = GetText(Resource.String.Lbl_AddToCart);
                            e.AddButton.Tag = "false";
                            item.AddedToCart = 0;

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Product.AddToCartAsync(item.Id?.ToString(), "Remove") });

                            GlobalContext?.TrendingFragment?.UpdateBadgeViewIcon(false);

                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // open Profile Product
        private void MAdapterItemClick(object sender, ProductAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("ProductId", item.Id.ToString());

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
                MAdapter.ProductsList.Clear();
                MAdapter.NotifyDataSetChanged();

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;
                MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Products 

        public void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadProducts(offset) });
        }

        private async Task LoadProducts(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.ProductsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Product.GetProductsAsync("15", offset, UserDetails.ProductPriceMin, UserDetails.ProductPriceMax, UserDetails.ProductCategory);
                if (apiStatus == 200)
                {
                    if (respond is GetProductObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.ProductsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.ProductsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.ProductsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.ProductsList = new ObservableCollection<ProductDataObject>(result.Data);
                                Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.ProductsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
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
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.ProductsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoProducts);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
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
    }
}