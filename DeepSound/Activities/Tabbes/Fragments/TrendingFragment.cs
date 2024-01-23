using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Blog;
using DeepSound.Activities.Blog.Adapters;
using DeepSound.Activities.Event;
using DeepSound.Activities.Event.Adapters;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Activities.Product;
using DeepSound.Activities.Product.Adapters;
using DeepSound.Activities.Search;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Blog;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using Q.Rorbin.Badgeview;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class TrendingFragment : Fragment
    {
        #region Variables Basic

        private ImageView CartIcon;
        private PlaylistAdapter PublicPlaylistAdapter;
        private EventAdapter EventAdapter;
        private BlogAdapter BlogAdapter;
        private ProductAdapter ProductAdapter;
        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private ViewStub EmptyStateLayout, EventViewStub, BlogViewStub, PublicPlaylistViewStub, ProductViewStub;
        private View Inflated, EventInflated, BlogInflated, PublicPlaylistInflated, ProductInflated;
        private RecyclerViewOnScrollListener EventScrollEvent, PublicPlaylistScrollEvent;
        public PlaylistProfileFragment PlaylistProfileFragment;
        private TemplateRecyclerInflater RecyclerInflaterEvent, RecyclerInflaterBlog, RecyclerInflaterPublicPlaylist, RecyclerInflaterProduct;
        private EventProfileFragment EventProfileFragment;
        public EventFragment EventFragment;
        private AutoCompleteTextView SearchBox;
        public SearchFragment SearchFragment;
        public ProductFragment ProductFragment;
        private ProductProfileFragment ProductProfileFragment;
        private static int CountCartsStatic;

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
                View view = inflater.Inflate(Resource.Layout.TTrendingLayout, container, false);
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                CartIcon = (ImageView)view.FindViewById(Resource.Id.CartIcon);
                CartIcon.Click += CartIconOnClick;

                SearchBox = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchViewBox);
                SearchBox.SetHintTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Gray);
                SearchBox.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Gray);
                SearchBox.Click += SearchBoxOnClick;

                EventViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubEvent);
                BlogViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubBlog);
                PublicPlaylistViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubPublicePlaylist);
                ProductViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubProduct);
                EmptyStateLayout = (ViewStub)view.FindViewById(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                var AdManagerAdView = view.FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                if (!AppSettings.ShowProduct || !UserDetails.IsLogin)
                    CartIcon.Visibility = ViewStates.Gone;
                else
                    CartIcon.Visibility = ViewStates.Visible;
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
                //Public Playlist RecyclerView
                PublicPlaylistAdapter = new PlaylistAdapter(Activity) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
                PublicPlaylistAdapter.ItemClick += PublicPlaylistAdapterOnItemClick;

                //Event RecyclerView
                EventAdapter = new EventAdapter(Activity) { EventsList = new ObservableCollection<EventDataObject>() };
                EventAdapter.ItemClick += EventAdapterItemClick;

                //Blog RecyclerView
                BlogAdapter = new BlogAdapter(Activity) { BlogList = new ObservableCollection<ArticleDataObject>() };
                BlogAdapter.ItemClick += BlogAdapterOnItemClick;

                // Product RecyclerView >> LinearLayoutManager.Horizontal
                ProductAdapter = new ProductAdapter(Activity) { ProductsList = new ObservableCollection<ProductDataObject>() };
                ProductAdapter.ItemClick += ProductAdapterOnItemClick;
                ProductAdapter.AddButtonItemClick += ProductAdapterOnAddButtonItemClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Refresh

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                EventAdapter.EventsList.Clear();
                EventAdapter.NotifyDataSetChanged();

                BlogAdapter.BlogList.Clear();
                BlogAdapter.NotifyDataSetChanged();

                PublicPlaylistAdapter.PlaylistList.Clear();
                PublicPlaylistAdapter.NotifyDataSetChanged();

                ProductAdapter.ProductsList.Clear();
                ProductAdapter.NotifyDataSetChanged();

                if (EventScrollEvent != null) EventScrollEvent.IsLoading = false;
                if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = false;

                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Public Playlist , Event =>> Api 

        private void StartApiService()
        {
            if (Methods.CheckConnectivity())
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPublicPlaylist(), () => GetEvent(), () => LoadProduct(), GetBlog });
            }
            else
            {
                SwipeRefreshLayout.Refreshing = false;

                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
        }

        private async Task GetPublicPlaylist(string offset = "0")
        {
            if (PublicPlaylistScrollEvent != null && PublicPlaylistScrollEvent.IsLoading)
                return;

            if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = true;

            int countList = PublicPlaylistAdapter.PlaylistList.Count;
            var (apiStatus, respond) = await RequestsAsync.Playlist.GetPublicPlaylistAsync("15", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is PlaylistObject result)
                {
                    var respondList = result.Playlist.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Playlist let check = PublicPlaylistAdapter.PlaylistList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            PublicPlaylistAdapter.PlaylistList.Add(item);
                        }

                        if (countList > 0)
                        {
                            Activity?.RunOnUiThread(() => { PublicPlaylistAdapter.NotifyItemRangeInserted(countList, PublicPlaylistAdapter.PlaylistList.Count - countList); });
                        }
                        else
                        {
                            Activity?.RunOnUiThread(() =>
                            {
                                PublicPlaylistInflated ??= PublicPlaylistViewStub.Inflate();

                                RecyclerInflaterPublicPlaylist = new TemplateRecyclerInflater();
                                RecyclerInflaterPublicPlaylist.InflateLayout<PlaylistDataObject>(Activity, PublicPlaylistInflated, PublicPlaylistAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Activity.GetString(Resource.String.Lbl_Hot_Playlist), false);

                                if (PublicPlaylistScrollEvent == null)
                                {
                                    RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterPublicPlaylist.LayoutManager);
                                    PublicPlaylistScrollEvent = playlistRecyclerViewOnScrollListener;
                                    PublicPlaylistScrollEvent.LoadMoreEvent += PublicPlaylistScrollEventOnLoadMoreEvent;
                                    RecyclerInflaterPublicPlaylist.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                    PublicPlaylistScrollEvent.IsLoading = false;
                                }
                            });
                        }
                    }
                    else
                    {
                        if (RecyclerInflaterPublicPlaylist.Recyler != null)
                            if (PublicPlaylistAdapter.PlaylistList.Count > 10 && !RecyclerInflaterPublicPlaylist.Recyler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePlaylist), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity?.RunOnUiThread(() => { ShowEmptyPage("PublicPlaylist"); });
        }

        private async Task GetEvent(string offset = "0")
        {
            if (!AppSettings.ShowEvent)
                return;

            if (EventScrollEvent != null && EventScrollEvent.IsLoading)
                return;

            if (EventScrollEvent != null) EventScrollEvent.IsLoading = true;

            int countList = EventAdapter.EventsList.Count;
            var (apiStatus, respond) = await RequestsAsync.Event.GetEventsAsync("15", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is GetEventObject result)
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = EventAdapter.EventsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            EventAdapter.EventsList.Add(item);
                        }

                        if (countList > 0)
                        {
                            Activity?.RunOnUiThread(() => { EventAdapter.NotifyItemRangeInserted(countList, EventAdapter.EventsList.Count - countList); });
                        }
                        else
                        {
                            Activity?.RunOnUiThread(() =>
                            {
                                EventInflated ??= EventViewStub.Inflate();

                                RecyclerInflaterEvent = new TemplateRecyclerInflater();
                                RecyclerInflaterEvent.InflateLayout<EventDataObject>(Activity, EventInflated, EventAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Activity.GetString(Resource.String.Lbl_Event));
                                if (!RecyclerInflaterEvent.MainLinear.HasOnClickListeners)
                                {
                                    RecyclerInflaterEvent.MainLinear.Click += null;
                                    RecyclerInflaterEvent.MainLinear.Click += EventMoreOnClick;
                                }

                                if (EventScrollEvent == null)
                                {
                                    RecyclerViewOnScrollListener eventRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterEvent.LayoutManager);
                                    EventScrollEvent = eventRecyclerViewOnScrollListener;
                                    EventScrollEvent.LoadMoreEvent += EventScrollEventOnLoadMoreEvent;
                                    RecyclerInflaterEvent.Recyler.AddOnScrollListener(eventRecyclerViewOnScrollListener);
                                    EventScrollEvent.IsLoading = false;
                                }
                            });
                        }
                    }
                    else
                    {
                        if (RecyclerInflaterEvent.Recyler != null)
                            if (EventAdapter.EventsList.Count > 10 && !RecyclerInflaterEvent.Recyler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreEvents), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity?.RunOnUiThread(() => { ShowEmptyPage("Event"); });
        }

        private async Task GetBlog()
        {
            if (!AppSettings.ShowBlog)
                return;

            int countList = BlogAdapter.BlogList.Count;
            var (apiStatus, respond) = await RequestsAsync.Articles.GetArticlesAsync("15", "0");
            if (apiStatus.Equals(200))
            {
                if (respond is GetArticlesObject result)
                {
                    var respondList = result.ArticleList.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.ArticleList let check = BlogAdapter.BlogList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            BlogAdapter.BlogList.Add(item);
                        }

                        if (countList > 0)
                        {
                            Activity?.RunOnUiThread(() => { BlogAdapter.NotifyItemRangeInserted(countList, BlogAdapter.BlogList.Count - countList); });
                        }
                        else
                        {
                            Activity?.RunOnUiThread(() =>
                            {
                                BlogInflated ??= BlogViewStub.Inflate();

                                RecyclerInflaterBlog = new TemplateRecyclerInflater();
                                RecyclerInflaterBlog.InflateLayout<EventDataObject>(Activity, BlogInflated, BlogAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Activity.GetString(Resource.String.Lbl_Article));
                                if (!RecyclerInflaterBlog.MainLinear.HasOnClickListeners)
                                {
                                    RecyclerInflaterBlog.MainLinear.Click += null;
                                    RecyclerInflaterBlog.MainLinear.Click += BlogMoreOnClick;
                                }
                            });
                        }
                    }
                    else
                    {
                        if (RecyclerInflaterBlog.Recyler != null)
                            if (BlogAdapter.BlogList.Count > 10 && !RecyclerInflaterBlog.Recyler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArtists), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity?.RunOnUiThread(() => { ShowEmptyPage("Blog"); });
        }

        private async Task LoadProduct(string offsetProduct = "0")
        {
            if (!AppSettings.ShowProduct)
                return;

            int countList = ProductAdapter.ProductsList.Count;
            var (apiStatus, respond) = await RequestsAsync.Product.GetProductsAsync("20", offsetProduct);
            if (apiStatus == 200)
            {
                if (respond is GetProductObject result)
                {
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = ProductAdapter.ProductsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                ProductAdapter.ProductsList.Add(item);
                            }

                            Activity?.RunOnUiThread(() => { ProductAdapter.NotifyItemRangeInserted(countList, ProductAdapter.ProductsList.Count - countList); });
                        }
                        else
                        {
                            ProductAdapter.ProductsList = new ObservableCollection<ProductDataObject>(result.Data);

                            Activity?.RunOnUiThread(() =>
                            {
                                ProductInflated ??= ProductViewStub.Inflate();

                                RecyclerInflaterProduct = new TemplateRecyclerInflater();
                                RecyclerInflaterProduct.InflateLayout<ProductDataObject>(Activity, ProductInflated, ProductAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Products));
                                if (!RecyclerInflaterProduct.MainLinear.HasOnClickListeners)
                                {
                                    RecyclerInflaterProduct.MainLinear.Click += null;
                                    RecyclerInflaterProduct.MainLinear.Click += ProductMoreOnClick;
                                }
                            });
                        }
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            await LoadCarts();
            Activity?.RunOnUiThread(() => { ShowEmptyPage("Product"); });
        }

        private void ShowEmptyPage(string type)
        {
            try
            {
                if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = false;
                if (EventScrollEvent != null) EventScrollEvent.IsLoading = false;

                SwipeRefreshLayout.Refreshing = false;

                if (type == "PublicPlaylist")
                {
                    if (PublicPlaylistAdapter?.PlaylistList?.Count > 0)
                    {
                        if (RecyclerInflaterPublicPlaylist.Recyler != null)
                            RecyclerInflaterPublicPlaylist.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (type == "Event")
                {
                    if (EventAdapter?.EventsList?.Count > 0)
                    {
                        if (RecyclerInflaterEvent.Recyler != null)
                            RecyclerInflaterEvent.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (type == "Blog")
                {
                    if (BlogAdapter?.BlogList?.Count > 0)
                    {
                        if (RecyclerInflaterBlog.Recyler != null)
                            RecyclerInflaterBlog.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (type == "Product")
                {
                    if (ProductAdapter?.ProductsList?.Count > 0)
                    {
                        if (RecyclerInflaterProduct.Recyler != null)
                            RecyclerInflaterProduct.Recyler.Visibility = ViewStates.Visible;

                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                }

                if (PublicPlaylistAdapter?.PlaylistList?.Count == 0 && EventAdapter?.EventsList?.Count == 0 && BlogAdapter?.BlogList?.Count == 0 && ProductAdapter?.ProductsList?.Count == 0)
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSound);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }

            }
            catch (Exception e)
            {
                if (PublicPlaylistScrollEvent != null) PublicPlaylistScrollEvent.IsLoading = false;
                if (EventScrollEvent != null) EventScrollEvent.IsLoading = false;

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

        #region Scroll

        private void EventScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = EventAdapter.EventsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !EventScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetEvent(item.Id.ToString()) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PublicPlaylistScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = PublicPlaylistAdapter.PlaylistList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !PublicPlaylistScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPublicPlaylist(item.Id.ToString()) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Event 

        private void ProductAdapterOnAddButtonItemClick(object sender, ProductAdapterClickEventArgs e)
        {
            try
            {
                var item = ProductAdapter.GetItem(e.Position);
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

                            UpdateBadgeViewIcon(true);

                            break;
                        default:
                            e.AddButton.Text = GetText(Resource.String.Lbl_AddToCart);
                            e.AddButton.Tag = "false";
                            item.AddedToCart = 0;

                            UpdateBadgeViewIcon(false);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Product.AddToCartAsync(item.Id?.ToString(), "Remove") });
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ProductAdapterOnItemClick(object sender, ProductAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = ProductAdapter.GetItem(e.Position);
                    if (item == null)
                        return;

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

        private void ProductMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                ProductFragment = new ProductFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(ProductFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BlogMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Activity, typeof(BlogActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EventMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                EventFragment = new EventFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(EventFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BlogAdapterOnItemClick(object sender, BlogAdapterClickEventArgs e)
        {
            try
            {
                var item = BlogAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Intent intent = new Intent(Activity, typeof(ShowArticleActivity));
                    intent.PutExtra("itemObject", JsonConvert.SerializeObject(item));
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void EventAdapterItemClick(object sender, EventAdapterClickEventArgs e)
        {
            try
            {
                var item = EventAdapter.EventsList[e.Position];
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("EventId", item.Id.ToString());

                    EventProfileFragment = new EventProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(EventProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PublicPlaylistAdapterOnItemClick(object sender, PlaylistAdapterClickEventArgs e)
        {
            try
            {
                var item = PublicPlaylistAdapter.PlaylistList[e.Position];
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("PlaylistId", item.Id.ToString());

                    PlaylistProfileFragment = new PlaylistProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(PlaylistProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Cart List
        private void CartIconOnClick(object sender, EventArgs e)
        {
            try
            {
                CartFragment cartFragment = new CartFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(cartFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region SearchView

        private void SearchBoxOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("Key", "");
                SearchFragment = new SearchFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SearchFragment);
                SearchBox.ClearFocus();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Carts & Badge

        private async Task LoadCarts()
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Product.GetCartsAsync("8").ConfigureAwait(false);
                if (apiStatus == 200)
                {
                    if (respond is GetCartsObject result)
                    {
                        Activity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var count = result.Data?.Count ?? 0;
                                if (count > 0)
                                {
                                    ShowOrHideBadgeViewIcon(count, true);
                                }
                                else
                                {
                                    ShowOrHideBadgeViewIcon();
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private QBadgeView BadgeCart;
        private void ShowOrHideBadgeViewIcon(int count = 0, bool show = false)
        {
            try
            {
                CountCartsStatic = count;
                if (show)
                {
                    BadgeCart ??= new QBadgeView(Activity);
                    int gravity = (int)(GravityFlags.End | GravityFlags.Top);
                    BadgeCart.BindTarget(CartIcon);
                    BadgeCart.SetBadgeNumber(count);
                    BadgeCart.SetBadgeGravity(gravity);
                    BadgeCart.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                    BadgeCart.SetGravityOffset(10, true);
                }
                else
                {
                    BadgeCart?.BindTarget(CartIcon).Hide(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateBadgeViewIcon(bool add)
        {
            try
            {
                if (add)
                {
                    ShowOrHideBadgeViewIcon(CountCartsStatic++, true);
                }
                else
                {
                    if (CountCartsStatic > 1)
                    {
                        ShowOrHideBadgeViewIcon(CountCartsStatic--, true);
                    }
                    else
                    {
                        ShowOrHideBadgeViewIcon();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }
}