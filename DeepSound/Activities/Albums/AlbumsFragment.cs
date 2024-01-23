using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Library.Listeners;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Albums
{
    public class AlbumsFragment : Fragment
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        public static AlbumsFragment Instance;

        private ImageView MoreIcon;
        private ImageView AlbumImage;
        private TextView TxtName, TxtCount;

        private LinearLayout TopButton;
        private LinearLayout BtnShuffle, BtnPlay;
        private AppCompatButton BtnBuy;

        private SwipeRefreshLayout SwipeRefreshLayout;
        public RowSoundAdapter MAdapter;
        private LinearLayoutManager LayoutManager;
        private RecyclerView MRecycler;
        private ViewStub EmptyStateLayout;
        private View Inflated;

        private AdManagerAdView AdManagerAdView;

        private DataAlbumsObject AlbumsObject;
        private string AlbumsId;

        private LibrarySynchronizer LibrarySynchronizer;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
            Instance = this;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.AlbumsLayout, container, false);
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

                LibrarySynchronizer = new LibrarySynchronizer(Activity);

                SetDataAlbums();

                AdsGoogle.Ad_Interstitial(Activity);
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
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
        public override void OnDestroy()
        {
            try
            {
                Instance = null;
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MoreIcon = view.FindViewById<ImageView>(Resource.Id.MenuIcon);
                MoreIcon.Click += MoreIconOnClick;

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                AlbumImage = view.FindViewById<ImageView>(Resource.Id.image);
                TxtName = view.FindViewById<TextView>(Resource.Id.txtName);
                TxtCount = view.FindViewById<TextView>(Resource.Id.txtCount);

                TopButton = view.FindViewById<LinearLayout>(Resource.Id.TopButton);

                BtnShuffle = view.FindViewById<LinearLayout>(Resource.Id.btnShuffle);
                BtnShuffle.Click += BtnShuffleOnClick;

                BtnPlay = view.FindViewById<LinearLayout>(Resource.Id.btnPlay);
                BtnPlay.Click += BtnPlayOnClick;

                BtnBuy = view.FindViewById<AppCompatButton>(Resource.Id.btnBuy);
                BtnBuy.Click += BtnBuyOnClick;

                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.songRecycler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                AdManagerAdView = view.FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);
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
                GlobalContext.SetToolBar(toolbar, " ");
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
                MAdapter = new RowSoundAdapter(Activity, "AlbumsFragment") { SoundsList = new ObservableCollection<SoundDataObject>() };
                MAdapter.ItemClick += MAdapterItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SoundDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Start Play all Sound 
        private void MAdapterItemClick(object sender, RowSoundAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (item.IsPlay)
                    {
                        item.IsPlay = false;

                        var index = MAdapter.SoundsList.IndexOf(item);
                        MAdapter.NotifyItemChanged(index, "playerAction");

                        GlobalContext?.SoundController.StartOrPausePlayer();
                    }
                    else
                    {
                        var list = MAdapter.SoundsList.Where(sound => sound.IsPlay).ToList();
                        if (list.Count > 0)
                        {
                            foreach (var all in list)
                            {
                                all.IsPlay = false;

                                var index = MAdapter.SoundsList.IndexOf(all);
                                MAdapter.NotifyItemChanged(index, "playerAction");
                            }
                        }

                        item.IsPlay = true;
                        MAdapter.NotifyItemChanged(e.Position, "playerAction");

                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.SoundsList, MAdapter);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnPlayOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = MAdapter.SoundsList.FirstOrDefault();
                if (item != null)
                {
                    if (item.IsPlay)
                    {
                        item.IsPlay = false;

                        var index = MAdapter.SoundsList.IndexOf(item);
                        MAdapter.NotifyItemChanged(index, "playerAction");

                        GlobalContext?.SoundController.StartOrPausePlayer();
                    }
                    else
                    {
                        var list = MAdapter.SoundsList.Where(sound => sound.IsPlay).ToList();
                        if (list.Count > 0)
                        {
                            foreach (var all in list)
                            {
                                all.IsPlay = false;

                                var index = MAdapter.SoundsList.IndexOf(all);
                                MAdapter.NotifyItemChanged(index, "playerAction");
                            }
                        }

                        item.IsPlay = true;
                        MAdapter.NotifyItemChanged(0, "playerAction");

                        Constant.PlayPos = 0;
                        GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.SoundsList, MAdapter);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnShuffleOnClick(object sender, EventArgs e)
        {
            try
            {
                Random rand = new Random();
                Constant.PlayPos = rand.Next(MAdapter.SoundsList.Count - 1);

                var item = MAdapter.SoundsList[Constant.PlayPos];
                if (item != null)
                {
                    if (item.IsPlay)
                    {
                        item.IsPlay = false;

                        var index = MAdapter.SoundsList.IndexOf(item);
                        MAdapter.NotifyItemChanged(index, "playerAction");

                        GlobalContext?.SoundController.StartOrPausePlayer();
                    }
                    else
                    {
                        var list = MAdapter.SoundsList.Where(sound => sound.IsPlay).ToList();
                        if (list.Count > 0)
                        {
                            foreach (var all in list)
                            {
                                all.IsPlay = false;

                                var index = MAdapter.SoundsList.IndexOf(all);
                                MAdapter.NotifyItemChanged(index, "playerAction");
                            }
                        }

                        item.IsPlay = true;
                        MAdapter.NotifyItemChanged(Constant.PlayPos, "playerAction");

                        GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.SoundsList, MAdapter, true);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Buy Album
        private void BtnBuyOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext?.OpenDialogPurchaseAlbum(AlbumsObject);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.SoundsList.Clear();
                MAdapter.NotifyDataSetChanged();

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Icon More
        private void MoreIconOnClick(object sender, EventArgs e)
        {
            try
            {
                LibrarySynchronizer?.AlbumsOnMoreClick(AlbumsObject, MAdapter.SoundsList);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Albums Songs And Data 

        private void SetDataAlbums()
        {
            try
            {
                AlbumsId = Arguments?.GetString("AlbumsId") ?? "";
                if (!string.IsNullOrEmpty(AlbumsId))
                {
                    AlbumsObject = JsonConvert.DeserializeObject<DataAlbumsObject>(Arguments?.GetString("ItemData") ?? "");
                    if (AlbumsObject != null)
                    {
                        GlideImageLoader.LoadImage(Activity, AlbumsObject.Thumbnail, AlbumImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        TxtName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(AlbumsObject.Title), 80);

                        var count = !string.IsNullOrEmpty(AlbumsObject.CountSongs) ? AlbumsObject.CountSongs : AlbumsObject.SongsCount ?? "0";
                        var text = count + " " + Context.GetText(Resource.String.Lbl_Songs);
                        if (AppSettings.ShowCountPurchases)
                            text = text + " - " + AlbumsObject.Purchases + " " + Context.GetText(Resource.String.Lbl_Purchases);

                        TxtCount.Text = text;

                        if (AlbumsObject.IsOwner != null && Math.Abs(AlbumsObject.Price) > 0 && !AlbumsObject.IsOwner.Value && AlbumsObject.IsPurchased == 0)
                        {
                            SwipeRefreshLayout.Refreshing = false;

                            BtnBuy.Visibility = ViewStates.Visible;

                            TopButton.Visibility = ViewStates.Gone;
                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSoundWithPaid);

                            EmptyStateLayout.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            BtnBuy.Visibility = ViewStates.Gone;
                            StartApiService();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadAlbumsSongs });
        }

        private async Task LoadAlbumsSongs()
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Albums.GetAlbumSongsAsync(AlbumsId);
                if (apiStatus == 200)
                {
                    if (respond is GetAlbumSongsObject result)
                    {
                        var respondList = result.Songs?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                result.Songs = DeepSoundTools.ListFilter(result.Songs);

                                foreach (var item in from item in result.Songs let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() =>
                                {
                                    MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList);
                                });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Songs);
                                Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreSongs), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

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
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.SoundsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    TopButton.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;
                    TopButton.Visibility = ViewStates.Gone;

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