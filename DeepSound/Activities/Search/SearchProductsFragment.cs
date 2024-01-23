using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Product;
using DeepSound.Activities.Product.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace DeepSound.Activities.Search
{
    public class SearchProductsFragment : Fragment
    {
        #region Variables Basic

        public ProductAdapter MAdapter;
        private HomeActivity GlobalContext;
        private SearchFragment ContextSearch;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        public ProgressBar ProgressBarLoader;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;
        private ProductProfileFragment ProductProfileFragment;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
            ContextSearch = (SearchFragment)ParentFragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.SearchSongsLayout, container, false);

                InitComponent(view);
                SetRecyclerViewAdapters();
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
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

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                ProgressBarLoader = (ProgressBar)view.FindViewById(Resource.Id.sectionProgress);
                ProgressBarLoader.Visibility = ViewStates.Gone;

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
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

                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += ContextSearch.TryAgainButton_Click;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.ProductsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                {
                    ContextSearch.OffsetProducts = item.Id.ToString();
                    ContextSearch.StartApiService();
                }
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

        //Open profile Product
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

        #endregion

    }
}