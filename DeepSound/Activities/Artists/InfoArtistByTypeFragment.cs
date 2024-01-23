using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.Transitions;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Albums.Adapters;
using DeepSound.Activities.Event;
using DeepSound.Activities.Event.Adapters;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Artists
{
    public class InfoArtistByTypeFragment : Fragment
    {
        #region Variables Basic

        private RowSoundAdapter SoundAdapter;
        private HAlbumsAdapter AlbumsAdapter;
        private EventAdapter EventAdapter;
        private PlaylistAdapter PlaylistAdapter;

        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private string Type, UserId;
        private AdView MAdView;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private ArtistsProfileFragment ArtistsProfileFragment;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
            ArtistsProfileFragment = ArtistsProfileFragment.GetInstance();
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

                Type = Arguments?.GetString("Type") ?? "";
                UserId = Arguments?.GetString("UserId") ?? "";

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                GetSongsByType();

                AdsGoogle.Ad_Interstitial(Activity);
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
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

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

                switch (Type)
                {
                    case "UserProfileLatestSongs":
                        {
                            GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_LatestSongs_Title));
                            break;
                        }
                    case "UserProfileTopSongs":
                        {
                            GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_TopSongs_Title));
                            break;
                        }
                    case "UserProfileStore":
                        {
                            GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Store_Title));
                            break;
                        }
                    case "UserProfileStations":
                        {
                            GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Stations));
                            break;
                        }
                    case "UserProfileAlbums":
                        {
                            GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Albums));
                            break;
                        }
                    case "UserProfileEvents":
                        {
                            GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Event));
                            break;
                        }
                    case "UserProfilePlaylist":
                        {
                            GlobalContext.SetToolBar(toolbar, Context.GetString(Resource.String.Lbl_Playlist));
                            break;
                        }
                    default:
                        GlobalContext.SetToolBar(toolbar, Type);
                        break;
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
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);

                if (Type == "UserProfileAlbums")
                {
                    AlbumsAdapter = new HAlbumsAdapter(Activity) { AlbumsList = new ObservableCollection<DataAlbumsObject>() };
                    AlbumsAdapter.ItemClick += AlbumsAdapterItemClick;
                    var preLoader = new RecyclerViewPreloader<DataAlbumsObject>(Activity, AlbumsAdapter, sizeProvider, 10);
                    MRecycler.AddOnScrollListener(preLoader);
                    MRecycler.SetAdapter(AlbumsAdapter);
                }
                else if (Type == "UserProfileEvents")
                {
                    EventAdapter = new EventAdapter(Activity) { EventsList = new ObservableCollection<EventDataObject>() };
                    EventAdapter.ItemClick += EventAdapterItemClick;
                    var preLoader = new RecyclerViewPreloader<DataAlbumsObject>(Activity, AlbumsAdapter, sizeProvider, 10);
                    MRecycler.AddOnScrollListener(preLoader);
                    MRecycler.SetAdapter(AlbumsAdapter);
                }
                else if (Type == "UserProfilePlaylist")
                {
                    PlaylistAdapter = new PlaylistAdapter(Activity) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
                    PlaylistAdapter.ItemClick += PlaylistAdapterOnItemClick;
                    var preLoader = new RecyclerViewPreloader<DataAlbumsObject>(Activity, AlbumsAdapter, sizeProvider, 10);
                    MRecycler.AddOnScrollListener(preLoader);
                    MRecycler.SetAdapter(AlbumsAdapter);
                }
                else
                {
                    SoundAdapter = new RowSoundAdapter(Activity, "InfoArtistByTypeFragment") { SoundsList = new ObservableCollection<SoundDataObject>() };
                    SoundAdapter.ItemClick += SoundAdapterItemClick;
                    var preLoader = new RecyclerViewPreloader<SoundDataObject>(Activity, SoundAdapter, sizeProvider, 10);
                    MRecycler.AddOnScrollListener(preLoader);
                    MRecycler.SetAdapter(SoundAdapter);
                }

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

        //Open profile >> Playlist 
        private void PlaylistAdapterOnItemClick(object sender, PlaylistAdapterClickEventArgs e)
        {
            try
            {
                var item = PlaylistAdapter.PlaylistList[e.Position];
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("PlaylistId", item.Id.ToString());

                    PlaylistProfileFragment PlaylistProfileFragment = new PlaylistProfileFragment
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

        //Open profile >> Event 
        private void EventAdapterItemClick(object sender, EventAdapterClickEventArgs e)
        {
            try
            {
                var item = EventAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("EventId", item.Id.ToString());

                    EventProfileFragment eventProfileFragment = new EventProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(eventProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile >> Albums
        private void AlbumsAdapterItemClick(object sender, HAlbumsAdapterClickEventArgs e)
        {
            try
            {
                var item = AlbumsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("AlbumsId", item.Id.ToString());
                    var albumsFragment = new AlbumsFragment
                    {
                        Arguments = bundle
                    };

                    SharedElementReturnTransition = (TransitionInflater.From(Activity).InflateTransition(Resource.Transition.change_image_transform));
                    ExitTransition = (TransitionInflater.From(Activity).InflateTransition(Resource.Transition.change_image_transform));

                    albumsFragment.SharedElementEnterTransition = TransitionInflater.From(Activity).InflateTransition(Resource.Transition.change_image_transform);
                    albumsFragment.ExitTransition = TransitionInflater.From(Activity).InflateTransition(Resource.Transition.change_image_transform);

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(albumsFragment, e.Image);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Play Sound 
        private void SoundAdapterItemClick(object sender, RowSoundAdapterClickEventArgs e)
        {
            try
            {
                var item = SoundAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (item.IsPlay)
                    {
                        item.IsPlay = false;

                        var index = SoundAdapter.SoundsList.IndexOf(item);
                        SoundAdapter.NotifyItemChanged(index, "playerAction");

                        GlobalContext?.SoundController.StartOrPausePlayer();
                    }
                    else
                    {
                        var list = SoundAdapter.SoundsList.Where(sound => sound.IsPlay).ToList();
                        if (list.Count > 0)
                        {
                            foreach (var all in list)
                            {
                                all.IsPlay = false;

                                var index = SoundAdapter.SoundsList.IndexOf(all);
                                SoundAdapter.NotifyItemChanged(index, "playerAction");
                            }
                        }

                        item.IsPlay = true;
                        SoundAdapter.NotifyItemChanged(e.Position, "playerAction");

                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, SoundAdapter.SoundsList, SoundAdapter);
                    }
                }
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
                string id;
                switch (Type)
                {
                    case "UserProfileAlbums":
                        id = AlbumsAdapter.AlbumsList.LastOrDefault()?.Id.ToString();
                        break;
                    case "UserProfileEvents":
                        id = EventAdapter.EventsList.LastOrDefault()?.Id.ToString();
                        break;
                    case "UserProfilePlaylist":
                        id = PlaylistAdapter.PlaylistList.LastOrDefault()?.Id.ToString();
                        break;
                    default:
                        id = SoundAdapter.SoundsList.LastOrDefault()?.Id.ToString();
                        break;
                }

                if (!string.IsNullOrEmpty(id) && !MainScrollEvent.IsLoading)
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    else
                    {
                        switch (Type)
                        {
                            case "UserProfileLatestSongs":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadLatestSongs(id) });
                                break;
                            case "UserProfileTopSongs":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadTopSongsUser(id) });
                                break;
                            case "UserProfileStore":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadStoreUser(id) });
                                break;
                            case "UserProfileStations":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadStationUser(id) });
                                break;
                            case "UserProfileAlbums":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadAlbumsUser(id) });
                                break;
                            case "UserProfileEvents":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadEventUser(id) });
                                break;
                            case "UserProfilePlaylist":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadPlaylistUser(id) });
                                break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data

        private void GetSongsByType()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    switch (Type)
                    {
                        case "UserProfileLatestSongs":
                            {
                                if (ArtistsProfileFragment?.LatestSongsAdapter?.SoundsList?.Count > 0)
                                {
                                    SoundAdapter.SoundsList = ArtistsProfileFragment?.LatestSongsAdapter?.SoundsList;
                                    SoundAdapter.NotifyDataSetChanged();
                                }
                                break;
                            }
                        case "UserProfileTopSongs":
                            {
                                if (ArtistsProfileFragment?.TopSongsAdapter?.SoundsList?.Count > 0)
                                {
                                    SoundAdapter.SoundsList = ArtistsProfileFragment?.TopSongsAdapter?.SoundsList;
                                    SoundAdapter.NotifyDataSetChanged();
                                }
                                break;
                            }
                        case "UserProfileAlbums":
                            {
                                if (ArtistsProfileFragment?.AlbumsAdapter?.AlbumsList?.Count > 0)
                                {
                                    AlbumsAdapter.AlbumsList = ArtistsProfileFragment?.AlbumsAdapter?.AlbumsList;
                                    AlbumsAdapter.NotifyDataSetChanged();
                                }
                                break;
                            }
                        case "UserProfileStore":
                            {
                                if (ArtistsProfileFragment?.StoreAdapter?.SoundsList?.Count > 0)
                                {
                                    SoundAdapter.SoundsList = ArtistsProfileFragment?.StoreAdapter?.SoundsList;
                                    SoundAdapter.NotifyDataSetChanged();
                                }
                                break;
                            }
                        case "UserProfileStations":
                            {
                                if (ArtistsProfileFragment?.StationsAdapter?.StationsList?.Count > 0)
                                {
                                    SoundAdapter.SoundsList = ArtistsProfileFragment?.StationsAdapter?.StationsList;
                                    SoundAdapter.NotifyDataSetChanged();
                                }
                                break;
                            }
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
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task LoadPlaylistUser(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = PlaylistAdapter.PlaylistList.Count;
                var (apiStatus, respond) = await RequestsAsync.Playlist.GetPlaylistAsync(UserId, "15", offset);
                if (apiStatus.Equals(200))
                {
                    if (respond is PlaylistObject result)
                    {
                        var respondList = result.Playlist.Count;
                        if (respondList > 0)
                        {
                            foreach (var item in from item in result.Playlist let check = PlaylistAdapter.PlaylistList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                PlaylistAdapter.PlaylistList.Add(item);
                            }

                            if (countList > 0)
                            {
                                Activity?.RunOnUiThread(() => { PlaylistAdapter.NotifyItemRangeInserted(countList, PlaylistAdapter.PlaylistList.Count - countList); });
                            }
                            else
                            {
                                Activity?.RunOnUiThread(() => { PlaylistAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (PlaylistAdapter.PlaylistList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePlaylist), ToastLength.Short)?.Show();
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

        private async Task LoadEventUser(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = EventAdapter.EventsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Event.GetUserEventsAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetEventObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = EventAdapter.EventsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    EventAdapter.EventsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { AlbumsAdapter.NotifyItemRangeInserted(countList, EventAdapter.EventsList.Count - countList); });
                            }
                            else
                            {
                                EventAdapter.EventsList = new ObservableCollection<EventDataObject>(result.Data);
                                Activity?.RunOnUiThread(() => { AlbumsAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (EventAdapter.EventsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreEvents), ToastLength.Short)?.Show();
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

        private async Task LoadAlbumsUser(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = AlbumsAdapter.AlbumsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetUserAlbumsAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is AlbumsObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = AlbumsAdapter.AlbumsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    AlbumsAdapter.AlbumsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { AlbumsAdapter.NotifyItemRangeInserted(countList, AlbumsAdapter.AlbumsList.Count - countList); });
                            }
                            else
                            {
                                AlbumsAdapter.AlbumsList = new ObservableCollection<DataAlbumsObject>(result.Data);
                                Activity?.RunOnUiThread(() => { AlbumsAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (AlbumsAdapter.AlbumsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreAlbums), ToastLength.Short)?.Show();
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

        private async Task LoadStationUser(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = SoundAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetUserStationsAsync(UserId, "20", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundDataObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            result.Data = DeepSoundTools.ListFilter(result.Data);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = SoundAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    SoundAdapter.SoundsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { SoundAdapter.NotifyItemRangeInserted(countList, SoundAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                SoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data);
                                Activity?.RunOnUiThread(() => { SoundAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (SoundAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreStations), ToastLength.Short)?.Show();
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

        private async Task LoadStoreUser(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = SoundAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetUserStoreAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundDataObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            result.Data = DeepSoundTools.ListFilter(result.Data);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = SoundAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    SoundAdapter.SoundsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { SoundAdapter.NotifyItemRangeInserted(countList, SoundAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                SoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data);
                                Activity?.RunOnUiThread(() => { SoundAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (SoundAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreSongs), ToastLength.Short)?.Show();
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

        private async Task LoadTopSongsUser(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = SoundAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetUserTopSongAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundObject result)
                    {
                        var respondList = result.Data?.SoundList?.Count;
                        if (respondList > 0)
                        {
                            result.Data.SoundList = DeepSoundTools.ListFilter(result.Data.SoundList);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data.SoundList let check = SoundAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    SoundAdapter.SoundsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { SoundAdapter.NotifyItemRangeInserted(countList, SoundAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                SoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data.SoundList);
                                Activity?.RunOnUiThread(() => { SoundAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (SoundAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreRecentlyPlayed), ToastLength.Short)?.Show();
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

        private async Task LoadLatestSongs(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = SoundAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetUserLatestSongAsync(UserId, "15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetSoundDataObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            result.Data = DeepSoundTools.ListFilter(result.Data);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = SoundAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    SoundAdapter.SoundsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { SoundAdapter.NotifyItemRangeInserted(countList, SoundAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                SoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data);
                                Activity?.RunOnUiThread(() => { SoundAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (SoundAdapter.SoundsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreRecentlyPlayed), ToastLength.Short)?.Show();
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

                int count;
                if (Type == "UserProfileAlbums")
                {
                    count = AlbumsAdapter.AlbumsList.Count;
                }
                else if (Type == "UserProfileEvents")
                {
                    count = EventAdapter.EventsList.Count;
                }
                else if (Type == "UserProfilePlaylist")
                {
                    count = PlaylistAdapter.PlaylistList.Count;
                }
                else
                {
                    count = SoundAdapter.SoundsList.Count;
                }

                if (count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    if (Type == "UserProfileAlbums")
                    {
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoAlbums);
                    }
                    else if (Type == "UserProfileEvents")
                    {
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoEvents);
                    }
                    else if (Type == "UserProfilePlaylist")
                    {
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPlaylist);
                    }
                    else
                    {
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSound);
                    }

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

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                GetSongsByType();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion 
    }
}