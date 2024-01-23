using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Albums.Adapters;
using DeepSound.Activities.Artists.Adapters;
using DeepSound.Activities.Default;
using DeepSound.Activities.Genres.Adapters;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Story;
using DeepSound.Activities.Story.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Story;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeepSound.Activities.Tabbes.HomePages
{
    public class LatestHomeFragment : Fragment
    {
        #region Variables Basic

        public ArtistsAdapter ArtistsAdapter;
        public StoryAdapter StoryAdapter;
        private GenresAdapter GenresAdapter;
        public HSoundAdapter NewReleasesSoundAdapter, RecentlyPlayedSoundAdapter, PopularSoundAdapter, TopSongsSoundAdapter;
        public AlbumsAdapter AlbumsAdapter;

        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private ViewStub EmptyStateLayout, GenresViewStub, NewReleasesViewStub, RecentlyPlayedViewStub, PopularViewStub, ArtistsViewStub, TopSongsViewStub, TopAlbumsViewStub, StoryViewStub;
        private View Inflated, GenresInflated, NewReleasesInflated, RecentlyPlayedInflated, PopularInflated, ArtistsInflated, TopSongsInflated, TopAlbumsInflated, StoryInflated;

        private ObservableCollection<SoundDataObject> RecommendedList;
        public SongsByGenresFragment SongsByGenresFragment;
        private RelativeLayout MainAlert;
        private AlbumsFragment AlbumsFragment;

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
                View view = inflater.Inflate(Resource.Layout.TMainFeedLayout, container, false);

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

                AdsGoogle.Ad_Interstitial(Activity, true);

                Task.Factory.StartNew(StartApiService);
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
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                GenresViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubGenres);
                NewReleasesViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubNewReleases);
                RecentlyPlayedViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubRecentlyPlayed);
                PopularViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubPopular);
                ArtistsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubArtists);
                TopSongsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubTopSongs);
                TopAlbumsViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubTopAlbums);
                StoryViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubStory);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                MainAlert = (RelativeLayout)view.FindViewById(Resource.Id.mainAlert);
                MainAlert.Visibility = !UserDetails.IsLogin ? ViewStates.Visible : ViewStates.Gone;
                MainAlert.Click += MainAlertOnClick;
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
                RecommendedList = new ObservableCollection<SoundDataObject>();

                //Browse RecyclerView >> LinearLayoutManager.Horizontal
                GenresAdapter = new GenresAdapter(Activity) { GenresList = new ObservableCollection<GenresObject.DataGenres>() };
                GenresAdapter.GenresList = ListUtils.GenresList;
                GenresAdapter.ItemClick += GenresAdapterOnItemClick;

                //New Releases RecyclerView >> LinearLayoutManager.Horizontal 
                NewReleasesSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                NewReleasesSoundAdapter.ItemClick += NewReleasesSoundAdapterOnItemClick;

                //Recently Played RecyclerView >> LinearLayoutManager.Horizontal
                RecentlyPlayedSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                RecentlyPlayedSoundAdapter.ItemClick += RecentlyPlayedSoundAdapterOnItemClick;

                //Popular RecyclerView >> LinearLayoutManager.Horizontal
                PopularSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                PopularSoundAdapter.ItemClick += PopularSoundAdapterOnItemClick;

                //Artists RecyclerView >> LinearLayoutManager.Horizontal
                ArtistsAdapter = new ArtistsAdapter(Activity);
                ArtistsAdapter.ItemClick += ArtistsAdapterOnItemClick;

                //Top Songs RecyclerView >> LinearLayoutManager.Horizontal 
                TopSongsSoundAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                TopSongsSoundAdapter.ItemClick += TopSongsSoundAdapterOnItemClick;

                //Top Albums RecyclerView >> LinearLayoutManager.Horizontal
                AlbumsAdapter = new AlbumsAdapter(Activity);
                AlbumsAdapter.ItemClick += AlbumsAdapterItemClick;

                //Story RecyclerView >> LinearLayoutManager.Horizontal
                StoryAdapter = new StoryAdapter(Activity);
                StoryAdapter.ItemClick += StoryAdapterOnItemClick;

                Activity?.RunOnUiThread(() =>
                {
                    StoryInflated ??= StoryViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<UserDataObject>(Activity, StoryInflated, StoryAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, false, Context.GetText(Resource.String.Lbl_Story), false);
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MainAlertOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Activity, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open sound from NewRelease
        private void NewReleasesSoundAdapterOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = NewReleasesSoundAdapter.GetItem(e.Position);
                    if (item == null)
                        return;
                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, NewReleasesSoundAdapter.SoundsList);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open sound from Popular
        private void PopularSoundAdapterOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = PopularSoundAdapter.GetItem(e.Position);
                    if (item == null)
                        return;
                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, PopularSoundAdapter.SoundsList);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open sound from RecentlyPlayed
        private void RecentlyPlayedSoundAdapterOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = RecentlyPlayedSoundAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, RecentlyPlayedSoundAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open Songs By Genres
        private void GenresAdapterOnItemClick(object sender, GenresAdapterClickEventArgs e)
        {
            try
            {
                var item = GenresAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("GenresId", item.Id.ToString());
                    bundle.PutString("GenresText", item.CateogryName);

                    SongsByGenresFragment = new SongsByGenresFragment
                    {
                        Arguments = bundle
                    };
                    GlobalContext.FragmentBottomNavigator.DisplayFragment(SongsByGenresFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile  
        private void ArtistsAdapterOnItemClick(object sender, ArtistsAdapterClickEventArgs e)
        {
            try
            {
                var item = ArtistsAdapter.GetItem(e.Position);
                if (item?.Id != null) GlobalContext.OpenProfile(item.Id, item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StoryAdapterOnItemClick(object sender, StoryAdapterClickEventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Login), Activity.GetText(Resource.String.Lbl_Message_Sorry_signin), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                    return;
                }

                var item = StoryAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (item.Type == "Your")
                    {
                        StartActivity(new Intent(Activity, typeof(CreateStoryActivity)));
                    }
                    else
                    {
                        if (item.Active == 1)
                        {
                            if (Constant.Player != null && Constant.Player.PlayWhenReady)
                                GlobalContext?.SoundController.StartOrPausePlayer();

                            List<StoryDataObject> storyList = new List<StoryDataObject>(StoryAdapter.StoryList);
                            storyList.RemoveAll(o => o.Type == "Your");

                            var indexItem = storyList.IndexOf(item);

                            Intent intent = new Intent(Activity, typeof(StoryDetailsActivity));
                            intent.PutExtra("UserId", item.UserId);
                            intent.PutExtra("IndexItem", indexItem);
                            intent.PutExtra("StoriesCount", storyList.Count);
                            intent.PutExtra("DataItem", JsonConvert.SerializeObject(new ObservableCollection<StoryDataObject>(storyList)));
                            StartActivity(intent);
                        }
                        else
                        {
                            GlobalContext?.OpenDialogPurchaseStory(item);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BrowseMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //Show all Genres
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PopularMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //Popular >> index 5
                GlobalContext.HomeFragment.ViewPager.SetCurrentItem(5, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RecentlyPlayedMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //RecentlyPlayed >> index 3
                GlobalContext.HomeFragment.ViewPager.SetCurrentItem(3, false);

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void NewReleasesMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //LatestSongsTab >> index 2
                GlobalContext.HomeFragment.ViewPager.SetCurrentItem(2, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ArtistsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //ArtistsTab >> index 6
                GlobalContext.HomeFragment.ViewPager.SetCurrentItem(6, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void TopAlbumsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //TopAlbums >> index 4 
                GlobalContext.HomeFragment.ViewPager.SetCurrentItem(4, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TopSongsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                //TopSongs >> index 1
                GlobalContext.HomeFragment.ViewPager.SetCurrentItem(1, false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile Albums
        private void AlbumsAdapterItemClick(object sender, AlbumsAdapterClickEventArgs e)
        {
            try
            {
                var item = AlbumsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                    bundle.PutString("AlbumsId", item.Id.ToString());
                    AlbumsFragment = new AlbumsFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(AlbumsFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TopSongsSoundAdapterOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = TopSongsSoundAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, TopSongsSoundAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Refresh

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                NewReleasesSoundAdapter.SoundsList.Clear();
                NewReleasesSoundAdapter.NotifyDataSetChanged();

                RecentlyPlayedSoundAdapter.SoundsList.Clear();
                RecentlyPlayedSoundAdapter.NotifyDataSetChanged();

                PopularSoundAdapter.SoundsList.Clear();
                PopularSoundAdapter.NotifyDataSetChanged();

                GenresAdapter.GenresList.Clear();
                GenresAdapter.NotifyDataSetChanged();

                ArtistsAdapter.ArtistsList.Clear();
                ArtistsAdapter.NotifyDataSetChanged();

                TopSongsSoundAdapter.SoundsList.Clear();
                TopSongsSoundAdapter.NotifyDataSetChanged();

                AlbumsAdapter.AlbumsList.Clear();
                AlbumsAdapter.NotifyDataSetChanged();

                StoryAdapter.StoryList.Clear();
                StoryAdapter.NotifyDataSetChanged();

                RecommendedList.Clear();

                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Discover Api

        private void StartApiService()
        {
            if (Methods.CheckConnectivity())
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadDiscover, LoadBrowse, ApiRequest.GetGenres_Api, LoadArtists, LoadStory });
            }
            else
            {
                Activity?.RunOnUiThread(() =>
                {
                    try
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
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
        }

        private async Task LoadDiscover()
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Common.GetDiscoverAsync();
                if (apiStatus == 200)
                {
                    if (respond is DiscoverObject result)
                    {
                        if (result.Randoms?.Recommended?.Count > 0)
                        {
                            result.Randoms.Recommended = DeepSoundTools.ListFilter(result.Randoms?.Recommended);
                            RecommendedList = new ObservableCollection<SoundDataObject>();
                            foreach (var item in result.Randoms?.Recommended)
                            {
                                RecommendedList.Add(item);
                            }
                        }

                        if (result.NewReleases?.NewReleasesClass?.Data?.AnythingArray?.Count > 0)
                        {
                            var soundsList = new ObservableCollection<SoundDataObject>(result.NewReleases?.NewReleasesClass?.Data?.AnythingArray);
                            NewReleasesSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(DeepSoundTools.ListFilter(soundsList.ToList()));
                        }
                        else if (result.NewReleases?.NewReleasesClass?.Data?.NewReleasesDataList?.Count > 0)
                        {
                            var soundsList = new ObservableCollection<SoundDataObject>(result.NewReleases?.NewReleasesClass?.Data?.NewReleasesDataList?.Values);
                            NewReleasesSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(DeepSoundTools.ListFilter(soundsList.ToList()));
                        }

                        if (result.RecentlyPlayed?.RecentlyPlayedClass?.Data?.Count > 0)
                        {
                            if (RecentlyPlayedSoundAdapter.SoundsList.Count > 0)
                            {
                                var newItemList = result.RecentlyPlayed?.RecentlyPlayedClass?.Data.Where(c => !RecentlyPlayedSoundAdapter.SoundsList.Select(fc => fc.Id).Contains(c.Id)).ToList();
                                if (newItemList.Count > 0)
                                {
                                    ListUtils.AddRange(RecentlyPlayedSoundAdapter.SoundsList, newItemList);
                                }
                            }
                            else
                            {
                                RecentlyPlayedSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.RecentlyPlayed?.RecentlyPlayedClass?.Data);
                            }

                            var soundDataObjects = RecentlyPlayedSoundAdapter.SoundsList?.Reverse();
                            Console.WriteLine(soundDataObjects);

                            var list = RecentlyPlayedSoundAdapter.SoundsList.OrderBy(o => o.Views);
                            RecentlyPlayedSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(DeepSoundTools.ListFilter(list.ToList()));
                        }

                        if (result.MostPopularWeek != null && result.MostPopularWeek?.MostPopularWeekClass?.Data?.Count > 0)
                        {
                            PopularSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(DeepSoundTools.ListFilter(result.MostPopularWeek?.MostPopularWeekClass?.Data));
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task LoadStory()
        {
            if (!UserDetails.IsLogin || !AppSettings.ShowStory)
                return;
              
            var (apiStatus, respond) = await RequestsAsync.Story.GetStoryAsync();
            if (apiStatus == 200)
            {
                if (respond is GetStoryObject result)
                {
                    try
                    {
                        bool add = false;
                         
                        var dataOwner = StoryAdapter.StoryList.FirstOrDefault(a => a.Type == "Your");
                        if (dataOwner == null)
                        {
                            add = true;
                            var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                            StoryAdapter.StoryList.Insert(0, new StoryDataObject
                            {
                                Type = "Your",
                                UserData = dataUser
                            });
                        }

                        await Task.Factory.StartNew(() =>
                        {
                            var respondList = result.Data?.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in from item in result.Data let check = StoryAdapter.StoryList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    try
                                    {
                                        add = true;

                                        item.Audio = DeepSoundClient.InitializeDeepSound.WebsiteUrl + "/" + item.OrgAudio;

                                        var fileName = item.Audio.Split('/').Last();
                                        var mediaFile = DeepSoundTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, item.Audio);
                                        item.PathAudio = mediaFile;

                                        item.DurationsList ??= new List<long>();
                                        item.IsOwner = item.UserId == UserDetails.UserId;

                                        Glide.With(Context).Load(item.Image).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();

                                        var duration = DeepSoundTools.GetDuration(mediaFile);
                                        item.DurationsList.Add(duration);

                                        StoryAdapter.StoryList.Add(item);
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                }
                            }
                          
                            if (add)
                                Activity?.RunOnUiThread(() => { StoryAdapter.NotifyDataSetChanged(); });

                        }).ConfigureAwait(false);

                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            //Activity?.RunOnUiThread(ShowEmptyPage);
        }

        private async Task LoadArtists()
        {
            int countList = ArtistsAdapter.ArtistsList.Count;
            var (apiStatus, respond) = await RequestsAsync.User.GetArtistsAsync();
            if (apiStatus == 200)
            {
                if (respond is GetUserObject result)
                {
                    var respondList = result.Data?.UserList?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data?.UserList let check = ArtistsAdapter.ArtistsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                ArtistsAdapter.ArtistsList.Add(item);
                            }

                            Activity?.RunOnUiThread(() =>
                            {
                                ArtistsAdapter.NotifyItemRangeInserted(countList, ArtistsAdapter.ArtistsList.Count - countList);
                            });
                        }
                        else
                        {
                            ArtistsAdapter.ArtistsList = new ObservableCollection<UserDataObject>(result.Data.UserList);

                            Activity?.RunOnUiThread(() =>
                            {
                                ArtistsInflated ??= ArtistsViewStub.Inflate();

                                TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                                recyclerInflater.InflateLayout<UserDataObject>(Activity, ArtistsInflated, ArtistsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Artists));
                                if (!recyclerInflater.MainLinear.HasOnClickListeners)
                                {
                                    recyclerInflater.MainLinear.Click += null;
                                    recyclerInflater.MainLinear.Click += ArtistsMoreOnClick;
                                }
                            });
                        }
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArtists), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            //Activity?.RunOnUiThread(ShowEmptyPage);
        }

        private async Task LoadBrowse()
        {
            if (Methods.CheckConnectivity())
            {
                int countSongsList = TopSongsSoundAdapter.SoundsList.Count;
                int countAlbumsList = AlbumsAdapter.AlbumsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Common.GetTrendingAsync();
                if (apiStatus == 200)
                {
                    if (respond is GetTrendingObject result)
                    {
                        var respondSongsList = result.TopSongs?.Count;
                        if (respondSongsList > 0)
                        {
                            result.TopSongs = DeepSoundTools.ListFilter(result.TopSongs);

                            if (countSongsList > 0)
                            {
                                foreach (var item in from item in result.TopSongs let check = TopSongsSoundAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    TopSongsSoundAdapter.SoundsList.Add(item);
                                }
                            }
                            else
                            {
                                TopSongsSoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.TopSongs);
                            }
                        }
                        else
                        {
                            if (TopSongsSoundAdapter.SoundsList.Count > 10)
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreSongs), ToastLength.Short)?.Show();
                        }

                        var respondList = result.TopAlbums?.Count;
                        if (respondList > 0)
                        {
                            AlbumsAdapter.AlbumsList = new ObservableCollection<DataAlbumsObject>(result.TopAlbums.Take(12));
                        }
                        else
                        {
                            if (AlbumsAdapter.AlbumsList.Count > 10)
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreAlbums), ToastLength.Short)?.Show();
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

                if (NewReleasesSoundAdapter.SoundsList?.Count > 0)
                {
                    NewReleasesInflated ??= NewReleasesViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, NewReleasesInflated, NewReleasesSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_LatestSongs_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += NewReleasesMoreOnClick;
                    }
                }

                if (RecentlyPlayedSoundAdapter.SoundsList?.Count > 0)
                {
                    RecentlyPlayedInflated ??= RecentlyPlayedViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, RecentlyPlayedInflated, RecentlyPlayedSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_RecentlyPlayed));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += RecentlyPlayedMoreOnClick;
                    }
                }

                if (PopularSoundAdapter.SoundsList?.Count > 0)
                {
                    PopularInflated ??= PopularViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, PopularInflated, PopularSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Popular_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += PopularMoreOnClick;
                    }
                }

                if (GenresAdapter.GenresList.Count == 0)
                    GenresAdapter.GenresList = new ObservableCollection<GenresObject.DataGenres>(ListUtils.GenresList);

                if (GenresAdapter.GenresList.Count > 0)
                {
                    GenresInflated ??= GenresViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<GenresObject.DataGenres>(Activity, GenresInflated, GenresAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Genres), false);
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += BrowseMoreOnClick;
                    }
                }

                if (TopSongsSoundAdapter.SoundsList.Count > 0)
                {
                    TopSongsInflated ??= TopSongsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, TopSongsInflated, TopSongsSoundAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_TopSongs_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += TopSongsMoreOnClick;
                    }
                }

                if (AlbumsAdapter.AlbumsList?.Count > 0)
                {
                    if (TopAlbumsInflated == null)
                        TopAlbumsInflated = TopAlbumsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<DataAlbumsObject>(Activity, TopAlbumsInflated, AlbumsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_TopAlbums_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += TopAlbumsMoreOnClick;
                    }
                }

                if (RecommendedList?.Count == 0 && NewReleasesSoundAdapter?.SoundsList?.Count == 0 && RecentlyPlayedSoundAdapter?.SoundsList?.Count == 0 &&
                    PopularSoundAdapter?.SoundsList?.Count == 0 && GenresAdapter?.GenresList?.Count == 0 && ArtistsAdapter.ArtistsList?.Count == 0 &&
                    TopSongsSoundAdapter.SoundsList?.Count == 0 && AlbumsAdapter.AlbumsList?.Count == 0)
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
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    StartApiService();
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