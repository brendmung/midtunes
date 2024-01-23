using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
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
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AndroidHUD;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Playlist
{
    public class PlaylistProfileFragment : Fragment
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        public static PlaylistProfileFragment Instance;

        private ImageView AddToIcon, DownloadsIcon, ShareIcon, MoreIcon;
        private ImageView PlaylistImage;
        private TextView TxtName, TxtCount;

        private LinearLayout TopButton;
        private LinearLayout BtnShuffle, BtnPlay;

        private SwipeRefreshLayout SwipeRefreshLayout;
        public RowSoundAdapter MAdapter;
        private LinearLayoutManager LayoutManager;
        private RecyclerView MRecycler;
        private ViewStub EmptyStateLayout;
        private View Inflated;

        private AdManagerAdView AdManagerAdView;

        private string PlaylistId;
        private PlaylistDataObject PlaylistObject;
        private LibrarySynchronizer LibrarySynchronizer;

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
                View view = inflater.Inflate(Resource.Layout.PlaylistProfileLayout, container, false);
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

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                LibrarySynchronizer = new LibrarySynchronizer(Activity);

                SetDataPlaylist();
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
                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                PlaylistImage = view.FindViewById<ImageView>(Resource.Id.image);
                TxtName = view.FindViewById<TextView>(Resource.Id.name);
                TxtCount = view.FindViewById<TextView>(Resource.Id.count);

                AddToIcon = view.FindViewById<ImageView>(Resource.Id.AddToIcon);
                AddToIcon.Click += AddToIconOnClick;
                AddToIcon.Visibility = ViewStates.Gone; //wael add after add new api

                DownloadsIcon = view.FindViewById<ImageView>(Resource.Id.DownloadsIcon);
                DownloadsIcon.Click += DownloadsIconOnClick;

                ShareIcon = view.FindViewById<ImageView>(Resource.Id.ShareIcon);
                ShareIcon.Click += ShareIconOnClick;

                MoreIcon = view.FindViewById<ImageView>(Resource.Id.MoreIcon);
                MoreIcon.Click += MoreIconOnClick;

                TopButton = view.FindViewById<LinearLayout>(Resource.Id.TopButton);

                BtnShuffle = view.FindViewById<LinearLayout>(Resource.Id.btnShuffle);
                BtnShuffle.Click += BtnShuffleOnClick;

                BtnPlay = view.FindViewById<LinearLayout>(Resource.Id.btnPlay);
                BtnPlay.Click += BtnPlayOnClick;

                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.recycler);
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
                MAdapter = new RowSoundAdapter(Activity, "PlaylistProfileFragment") { SoundsList = new ObservableCollection<SoundDataObject>() };
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

        private void MoreIconOnClick(object sender, EventArgs e)
        {
            try
            {
                LibrarySynchronizer?.PlaylistMoreOnClick(PlaylistObject);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void ShareIconOnClick(object sender, EventArgs e)
        {
            try
            {
                //Share Plugin same as Song
                if (!CrossShare.IsSupported)
                {
                    return;
                }

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = PlaylistObject?.Name,
                    Text = "",
                    Url = PlaylistObject?.Url
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DownloadsIconOnClick(object sender, EventArgs e)
        {
            try
            {
                var isPro = ListUtils.MyUserInfoList?.FirstOrDefault()?.IsPro ?? 0;
                if (isPro == 0)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "GoPro");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_ActivateWithUpgraded), GetText(Resource.String.Lbl_Ok), GetText(Resource.String.Lbl_Cancel));
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                AndHUD.Shared.Show(Context, GetText(Resource.String.Lbl_Downloading));

                Methods.Path.Chack_MyFolder();

                foreach (var item in MAdapter.SoundsList)
                {
                    var SoundDownload = new SoundDownloadAsyncController(item.AudioLocation, item.Title, Activity);
                    if (!SoundDownload.CheckDownloadLinkIfExits())
                    {
                        SoundDownload.StartDownloadManager(item.Title, item, "Playlist");
                    }
                }

                AndHUD.Shared.Dismiss(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AddToIconOnClick(object sender, EventArgs e)
        {
            try
            {
                //wael add  code after update api
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

        #endregion

        #region Load Playlist Songs And Data 

        private void SetDataPlaylist()
        {
            try
            {
                PlaylistId = Arguments?.GetString("PlaylistId") ?? "";
                if (!string.IsNullOrEmpty(PlaylistId))
                {
                    PlaylistObject = JsonConvert.DeserializeObject<PlaylistDataObject>(Arguments?.GetString("ItemData") ?? "");
                    if (PlaylistObject != null)
                    {
                        GlideImageLoader.LoadImage(Activity, PlaylistObject.ThumbnailReady, PlaylistImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        var d = PlaylistObject.Name.Replace("<br>", "");
                        TxtName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(d), 70);

                        if (!string.IsNullOrEmpty(PlaylistObject.Songs.ToString()))
                            TxtCount.Text = PlaylistObject.Songs + " " + GetText(Resource.String.Lbl_Songs);
                        else
                            TxtCount.Text = "0 " + GetText(Resource.String.Lbl_Songs);

                        if (PlaylistObject.Publisher?.PublisherClass.Id == UserDetails.UserId)
                        {
                            MoreIcon.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            MoreIcon.Visibility = ViewStates.Gone;
                        }
                    }

                    StartApiService();
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadPlaylistSongs });
        }

        private async Task LoadPlaylistSongs()
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Playlist.GetPlaylistSongsAsync(PlaylistId);
                if (apiStatus == 200)
                {
                    if (respond is PlaylistSongsObject result)
                    {
                        var respondList = result.Songs?.Count;
                        if (respondList > 0)
                        {
                            result.Songs = DeepSoundTools.ListFilter(result.Songs);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Songs let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
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
                    DownloadsIcon.Visibility = ViewStates.Visible;

                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;
                    TopButton.Visibility = ViewStates.Gone;
                    DownloadsIcon.Visibility = ViewStates.Gone;

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