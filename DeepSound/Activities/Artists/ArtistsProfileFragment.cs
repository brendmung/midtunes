using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.SwipeRefreshLayout.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Albums.Adapters;
using DeepSound.Activities.Chat;
using DeepSound.Activities.Event;
using DeepSound.Activities.Event.Adapters;
using DeepSound.Activities.MyContacts;
using DeepSound.Activities.MyProfile;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Stations.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.SuperTextLibrary;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Artists
{
    public class ArtistsProfileFragment : Fragment, IDialogListCallBack
    {
        #region Variables Basic

        private HomeActivity GlobalContext;

        private ImageView MenuIcon;
        private ImageView Image;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private TextView TxtName;
        private SuperTextView TxtAbout;

        private Details DetailsCounter;
        private UserDataObject DataUser;
        private string UserId = "";
        private static ArtistsProfileFragment Instance;
        private AppCompatButton BtnFollow, BtnChat;

        private ViewStub EmptyStateLayout, LatestSongsViewStub, TopSongsViewStub, AlbumsViewStub, PlaylistViewStub, StoreViewStub, StationsViewStub, EventViewStub;
        private View Inflated, LatestSongsInflated, TopSongsInflated, AlbumsInflated, PlaylistInflated, StoreInflated, StationsInflated, EventInflated;

        public HSoundAdapter LatestSongsAdapter, TopSongsAdapter, StoreAdapter;
        public HAlbumsAdapter AlbumsAdapter;
        public PlaylistAdapter PlaylistAdapter;
        public EventAdapter EventAdapter;
        public StationsAdapter StationsAdapter;

        private AdManagerAdView AdManagerAdView;

        public AlbumsFragment AlbumsFragment;

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
                View view = inflater.Inflate(Resource.Layout.ArtistsProfileLayout, container, false);
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

                Instance = this;

                UserId = Arguments?.GetString("UserId") ?? "";
                DataUser = JsonConvert.DeserializeObject<UserDataObject>(Arguments?.GetString("ItemData") ?? "");

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                GetMyInfoData();

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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                MenuIcon = view.FindViewById<ImageView>(Resource.Id.MenuIcon);
                MenuIcon.Click += MenuIconOnClick;

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                Image = view.FindViewById<ImageView>(Resource.Id.image);
                TxtName = view.FindViewById<TextView>(Resource.Id.txtName);
                TxtAbout = view.FindViewById<SuperTextView>(Resource.Id.AboutTextview);

                BtnFollow = view.FindViewById<AppCompatButton>(Resource.Id.btnFollow);
                BtnFollow.Click += BtnFollowOnClick;

                BtnChat = view.FindViewById<AppCompatButton>(Resource.Id.btnChat);
                BtnChat.Click += BtnChatOnClick;

                LatestSongsViewStub = view.FindViewById<ViewStub>(Resource.Id.viewStubLatestSongs);
                TopSongsViewStub = view.FindViewById<ViewStub>(Resource.Id.viewStubTopSongs);
                AlbumsViewStub = view.FindViewById<ViewStub>(Resource.Id.viewStubAlbums);
                PlaylistViewStub = view.FindViewById<ViewStub>(Resource.Id.viewStubPlaylist);
                StoreViewStub = view.FindViewById<ViewStub>(Resource.Id.viewStubStore);
                StationsViewStub = view.FindViewById<ViewStub>(Resource.Id.viewStubStations);
                EventViewStub = view.FindViewById<ViewStub>(Resource.Id.viewStubEvent);

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
                //Latest Songs RecyclerView >> LinearLayoutManager.Horizontal 
                LatestSongsAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                LatestSongsAdapter.ItemClick += LatestSongsAdapterOnItemClick;

                //Top Songs RecyclerView >> LinearLayoutManager.Horizontal 
                TopSongsAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                TopSongsAdapter.ItemClick += TopSongsAdapterOnItemClick;

                //Albums RecyclerView >> LinearLayoutManager.Horizontal 
                AlbumsAdapter = new HAlbumsAdapter(Activity);
                AlbumsAdapter.ItemClick += AlbumsAdapterItemClick;

                //Playlist RecyclerView
                PlaylistAdapter = new PlaylistAdapter(Activity) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
                PlaylistAdapter.ItemClick += PlaylistAdapterOnItemClick;

                //Store RecyclerView >> LinearLayoutManager.Horizontal 
                StoreAdapter = new HSoundAdapter(Activity) { SoundsList = new ObservableCollection<SoundDataObject>() };
                StoreAdapter.ItemClick += StoreAdapterOnItemClick;

                //Event RecyclerView
                EventAdapter = new EventAdapter(Activity) { EventsList = new ObservableCollection<EventDataObject>() };
                EventAdapter.ItemClick += EventAdapterItemClick;

                //Stations RecyclerView
                StationsAdapter = new StationsAdapter(Activity) { StationsList = new ObservableCollection<SoundDataObject>() };
                StationsAdapter.ItemClick += StationsAdapterOnItemClick;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ArtistsProfileFragment GetInstance()
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

        #region Event

        private void StationsAdapterOnItemClick(object sender, StationsAdapterClickEventArgs e)
        {
            try
            {
                var item = StationsAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, StationsAdapter.StationsList);
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

                    PlaylistProfileFragment playlistProfileFragment = new PlaylistProfileFragment
                    {
                        Arguments = bundle
                    };

                    GlobalContext.FragmentBottomNavigator.DisplayFragment(playlistProfileFragment);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StoreAdapterOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = StoreAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, StoreAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Play Sound 
        private void TopSongsAdapterOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = TopSongsAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, TopSongsAdapter.SoundsList);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Play Sound 
        private void LatestSongsAdapterOnItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = LatestSongsAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Constant.PlayPos = e.Position;
                        GlobalContext?.SoundController?.StartPlaySound(item, LatestSongsAdapter.SoundsList);
                    }
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
                if (e.Position > -1)
                {
                    var item = AlbumsAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Bundle bundle = new Bundle();
                        bundle.PutString("ItemData", JsonConvert.SerializeObject(item));
                        bundle.PutString("AlbumsId", item.Id.ToString());
                        AlbumsFragment albumsFragment = new AlbumsFragment
                        {
                            Arguments = bundle
                        };
                        GlobalContext.FragmentBottomNavigator.DisplayFragment(albumsFragment);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open chat page 
        private void BtnChatOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(Context, typeof(MessagesBoxActivity));
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("UserItem", JsonConvert.SerializeObject(DataUser));
                Context.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Follow  
        private void BtnFollowOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Login), Activity.GetText(Resource.String.Lbl_Message_Sorry_signin), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (BtnFollow.Tag?.ToString() == "true")
                {
                    BtnFollow.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    BtnFollow.SetTextColor(Color.ParseColor("#ffffff"));
                    BtnFollow.Text = Activity.GetText(Resource.String.Lbl_Follow);
                    BtnFollow.Tag = "false";

                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Sent_successfully_Unfollowed), ToastLength.Short)?.Show();

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(UserId, false) });
                }
                else //Not Friend
                {
                    BtnFollow.SetBackgroundResource(Resource.Drawable.round_button_normal);
                    BtnFollow.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    BtnFollow.Text = Activity.GetText(Resource.String.Lbl_Following);
                    BtnFollow.Tag = "true";

                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Sent_successfully_followed), ToastLength.Short)?.Show();
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(UserId, true) });
                }

                var dataUser = GlobalContext?.HomeFragment?.LatestHomeTab?.ArtistsAdapter?.ArtistsList?.FirstOrDefault(a => a.Id == DataUser.Id);
                if (dataUser != null)
                {
                    dataUser.IsFollowing = DataUser.IsFollowing;
                    GlobalContext.HomeFragment?.LatestHomeTab?.ArtistsAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //More 
        private void MenuIconOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(Context);

                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_ProfileInfo));

                if (UserDetails.IsLogin)
                    arrayAdapter.Add(Context.GetText(Resource.String.Lbl_Block));

                arrayAdapter.Add(Context.GetText(Resource.String.Lbl_CopyLinkToProfile));

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetPositiveButton(Context.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StoreMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("Type", "UserProfileStore");
                bundle.PutString("UserId", UserId);

                InfoArtistByTypeFragment infoArtistByTypeFragment = new InfoArtistByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(infoArtistByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AlbumsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("Type", "UserProfileAlbums");
                bundle.PutString("UserId", UserId);

                InfoArtistByTypeFragment infoArtistByTypeFragment = new InfoArtistByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(infoArtistByTypeFragment);
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
                Bundle bundle = new Bundle();
                bundle.PutString("Type", "UserProfileTopSongs");
                bundle.PutString("UserId", UserId);

                InfoArtistByTypeFragment infoArtistByTypeFragment = new InfoArtistByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(infoArtistByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LatestSongsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("Type", "UserProfileLatestSongs");
                bundle.PutString("UserId", UserId);

                InfoArtistByTypeFragment infoArtistByTypeFragment = new InfoArtistByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(infoArtistByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void StationsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("Type", "UserProfileStations");
                bundle.PutString("UserId", UserId);

                InfoArtistByTypeFragment infoArtistByTypeFragment = new InfoArtistByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(infoArtistByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EventsMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("Type", "UserProfileEvents");
                bundle.PutString("UserId", UserId);

                InfoArtistByTypeFragment infoArtistByTypeFragment = new InfoArtistByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(infoArtistByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PlaylistMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("Type", "UserProfilePlaylist");
                bundle.PutString("UserId", UserId);

                InfoArtistByTypeFragment infoArtistByTypeFragment = new InfoArtistByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(infoArtistByTypeFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Open User Contact >> Following
        private void LayoutFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", UserId);
                bundle.PutString("UserType", "Following");

                ContactsFragment contactsFragment = new ContactsFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(contactsFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open User Contact >> Followers
        private void LayoutFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", UserId);
                bundle.PutString("UserType", "Followers");

                ContactsFragment contactsFragment = new ContactsFragment
                {
                    Arguments = bundle
                };
                GlobalContext.FragmentBottomNavigator.DisplayFragment(contactsFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data User

        private void GetMyInfoData()
        {
            try
            {
                LoadDataUser();

                StartApiService();
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadProfile });
        }

        private async Task LoadProfile()
        {
            if (Methods.CheckConnectivity())
            {
                string fetch = "albums,playlists,liked,latest_songs,top_songs";
                if (AppSettings.ShowStations)
                    fetch += ",stations";

                if (AppSettings.ShowStore)
                    fetch += ",store";

                if (AppSettings.ShowEvent)
                    fetch += ",events";

                var (apiStatus, respond) = await RequestsAsync.User.ProfileAsync(UserId, fetch);
                if (apiStatus.Equals(200))
                {
                    if (respond is ProfileObject result)
                    {
                        if (result.Data != null)
                        {
                            DataUser = result.Data;

                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    LoadDataUser();

                                    if (result.Details != null)
                                    {
                                        DetailsCounter = result.Details;

                                        //TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(result.Details.Followers);
                                        //TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(result.Details.Following);

                                        //var trackCount = CalculateTotalTracks(result);

                                        //TxtCountTracks.Text = trackCount;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });

                            //Add Latest Songs
                            var Latestsongs = result?.Data?.Latestsongs?.FirstOrDefault();
                            if (Latestsongs?.Count > 0)
                            {
                                LatestSongsAdapter.SoundsList = new ObservableCollection<SoundDataObject>(Latestsongs?.Take(7));
                            }

                            //Add Latest Songs
                            var TopSongs = result?.Data?.TopSongs?.FirstOrDefault();
                            if (TopSongs?.Count > 0)
                            {
                                TopSongsAdapter.SoundsList = new ObservableCollection<SoundDataObject>(TopSongs?.Take(7));
                            }

                            //Add Albums
                            var Albums = result?.Data?.Albums?.FirstOrDefault();
                            if (Albums?.Count > 0)
                            {
                                AlbumsAdapter.AlbumsList = new ObservableCollection<DataAlbumsObject>(Albums?.Take(4));
                            }

                            //Add Playlists
                            var Playlists = result?.Data?.Playlists?.FirstOrDefault();
                            if (Playlists?.Count > 0)
                            {
                                PlaylistAdapter.PlaylistList = new ObservableCollection<PlaylistDataObject>(Playlists.Take(7));
                            }

                            //Add Store
                            var Store = result?.Data?.Store?.FirstOrDefault();
                            if (Store?.Count > 0)
                            {
                                StoreAdapter.SoundsList = new ObservableCollection<SoundDataObject>(Store.Take(7));
                            }

                            //Add Event
                            var Events = result?.Data?.Events;
                            if (Events?.Count > 0)
                            {
                                EventAdapter.EventsList = new ObservableCollection<EventDataObject>(Events.Take(7));
                            }

                            //Add Stations
                            var Stations = result?.Data?.Stations;
                            if (Stations?.Count > 0)
                            {
                                StationsAdapter.StationsList = new ObservableCollection<SoundDataObject>(Stations.Take(5));
                            }

                        }
                    }
                }
                else
                {
                    if (Activity != null)
                    {
                        Methods.DisplayReportResult(Activity, respond);
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Activity?.RunOnUiThread(() =>
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
                });
            }
        }

        //private string CalculateTotalTracks(ProfileObject result)
        //{
        //    var totalTracks = new List<SoundDataObject>();

        //    if (result.Data?.Latestsongs != null)
        //        totalTracks.AddRange(result.Data?.Latestsongs);

        //    if (result.Data?.TopSongs != null)
        //        totalTracks.AddRange(result.Data?.TopSongs);

        //    if (result.Data?.Store != null)
        //        totalTracks.AddRange(result.Data?.Store);

        //    if (result.Data?.Albums != null)
        //        foreach (var album in (result.Data?.Albums).Where(album => album.Songs != null))
        //        {
        //            totalTracks.AddRange(album.Songs);
        //        }

        //    var trackCount = totalTracks.GroupBy(x => x.Id).Select(x => x);
        //    return trackCount.Count().ToString();
        //}

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

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (LatestSongsAdapter.SoundsList?.Count > 0)
                {
                    LatestSongsInflated ??= LatestSongsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, LatestSongsInflated, LatestSongsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_LatestSongs_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += LatestSongsMoreOnClick;
                    }
                }

                if (TopSongsAdapter.SoundsList?.Count > 0)
                {
                    TopSongsInflated ??= TopSongsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, TopSongsInflated, TopSongsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_TopSongs_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += TopSongsMoreOnClick;
                    }
                }

                if (AlbumsAdapter.AlbumsList?.Count > 0)
                {
                    AlbumsInflated ??= AlbumsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<DataAlbumsObject>(Activity, AlbumsInflated, AlbumsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, Context.GetText(Resource.String.Lbl_Albums));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += AlbumsMoreOnClick;
                    }
                }

                if (PlaylistAdapter.PlaylistList?.Count > 0)
                {
                    PlaylistInflated ??= PlaylistViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<PlaylistDataObject>(Activity, PlaylistInflated, PlaylistAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Playlist));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += PlaylistMoreOnClick;
                    }
                }

                if (EventAdapter.EventsList?.Count > 0)
                {
                    EventInflated ??= EventViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<EventDataObject>(Activity, EventInflated, EventAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Event));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += EventsMoreOnClick;
                    }
                }

                if (StoreAdapter.SoundsList?.Count > 0)
                {
                    StoreInflated ??= StoreViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, StoreInflated, StoreAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_Store_Title));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += StoreMoreOnClick;
                    }
                }

                if (StationsAdapter.StationsList?.Count > 0)
                {
                    StationsInflated ??= StationsViewStub.Inflate();

                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                    recyclerInflater.InflateLayout<SoundDataObject>(Activity, StationsInflated, StationsAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, Context.GetText(Resource.String.Lbl_Stations));
                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                    {
                        recyclerInflater.MainLinear.Click += null;
                        recyclerInflater.MainLinear.Click += StationsMoreOnClick;
                    }
                }

                if (LatestSongsAdapter.SoundsList?.Count == 0 && TopSongsAdapter.SoundsList?.Count == 0 && AlbumsAdapter.AlbumsList?.Count == 0
                    && PlaylistAdapter.PlaylistList?.Count == 0 && StoreAdapter.SoundsList?.Count == 0 && EventAdapter.EventsList?.Count == 0 && StationsAdapter.StationsList?.Count == 0)
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
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadDataUser()
        {
            try
            {
                if (Activity == null)
                {
                    return;
                }

                GlideImageLoader.LoadImage(Activity, DataUser.Avatar, Image, ImageStyle.RoundedCrop, ImagePlaceholders.DrawableUser);

                TxtName.Text = DeepSoundTools.GetNameFinal(DataUser);
                TxtName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, DataUser.Verified == 1 ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                TextSanitizer aboutSanitizer = new TextSanitizer(TxtAbout, GlobalContext);
                aboutSanitizer.Load(DeepSoundTools.GetAboutFinal(DataUser));

                if (DataUser.IsFollowing == "1" || DataUser.IsFollowing == "true") // My Friend
                {
                    BtnFollow.SetBackgroundResource(Resource.Drawable.round_button_normal);
                    BtnFollow.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    BtnFollow.Text = Activity.GetText(Resource.String.Lbl_Following);
                    BtnFollow.Tag = "true";
                }
                else //Not Friend
                {
                    BtnFollow.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                    BtnFollow.SetTextColor(Color.ParseColor("#ffffff"));
                    BtnFollow.Text = Activity.GetText(Resource.String.Lbl_Follow);
                    BtnFollow.Tag = "false";
                }
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
                LatestSongsAdapter.SoundsList.Clear();
                LatestSongsAdapter.NotifyDataSetChanged();

                TopSongsAdapter.SoundsList.Clear();
                TopSongsAdapter.NotifyDataSetChanged();

                AlbumsAdapter.AlbumsList.Clear();
                AlbumsAdapter.NotifyDataSetChanged();

                PlaylistAdapter.PlaylistList.Clear();
                PlaylistAdapter.NotifyDataSetChanged();

                StoreAdapter.SoundsList.Clear();
                StoreAdapter.NotifyDataSetChanged();

                EventAdapter.EventsList.Clear();
                EventAdapter.NotifyDataSetChanged();

                StationsAdapter.StationsList.Clear();
                StationsAdapter.NotifyDataSetChanged();

                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == Context.GetText(Resource.String.Lbl_Block))
                {
                    if (Methods.CheckConnectivity())
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Long)?.Show();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.BlockUnBlockUserAsync(DataUser.Id.ToString(), true) });

                        GlobalContext.FragmentNavigatorBack();
                    }
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                else if (text == Context.GetText(Resource.String.Lbl_CopyLinkToProfile))
                {
                    string url = DataUser?.Url;
                    GlobalContext.SoundController.ClickListeners.OnMenuCopyOnClick(url);
                }
                else if (text == Context.GetText(Resource.String.Lbl_ProfileInfo))
                {
                    Intent intent = new Intent(Activity, typeof(DialogInfoUserActivity));
                    intent.PutExtra("ItemDataUser", JsonConvert.SerializeObject(DataUser));
                    intent.PutExtra("ItemDataDetails", JsonConvert.SerializeObject(DetailsCounter));
                    StartActivity(intent);
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