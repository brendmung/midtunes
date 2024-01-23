using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Library
{
    public class MyPlaylistFragment : Fragment
    {
        #region Variables Basic

        public PlaylistRowAdapter MAdapter;
        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private PlaylistProfileFragment PlaylistProfileFragment;
        private PopupFilterList PopupFilterList;

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
                View view = inflater.Inflate(Resource.Layout.PlaylistLayout, container, false);
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
                PopupFilterList = new PopupFilterList(view, Activity, MAdapter);

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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

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
                GlobalContext.SetToolBar(toolbar, GetString(Resource.String.Lbl_MyPlaylist));
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
                MAdapter = new PlaylistRowAdapter(Activity) { PlaylistList = new ObservableCollection<PlaylistDataObject>() };
                MAdapter.ItemClick += MAdapterItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PlaylistDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
                 
                if (ListUtils.PlaylistList.Count > 0)
                {
                    MAdapter.PlaylistList = new ObservableCollection<PlaylistDataObject>(ListUtils.PlaylistList);
                    MAdapter.NotifyDataSetChanged();
                    Activity?.RunOnUiThread(ShowEmptyPage);
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
                MAdapter.PlaylistList.Clear();
                MAdapter.NotifyDataSetChanged();
                MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Playlist Api 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPlaylist(offset) });
        }

        private async Task GetPlaylist(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.PlaylistList.Count;
                var (apiStatus, respond) = await RequestsAsync.Playlist.GetPlaylistAsync(UserDetails.UserId.ToString(), "15", offset);
                if (apiStatus.Equals(200))
                {
                    if (respond is PlaylistObject result)
                    {
                        var respondList = result.Playlist.Count;
                        if (respondList > 0)
                        {
                            foreach (var item in from item in result.Playlist let check = MAdapter.PlaylistList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                MAdapter.PlaylistList.Add(item);
                                ListUtils.PlaylistList.Add(item);
                            }

                            if (countList > 0)
                            {
                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.PlaylistList.Count - countList); });
                            }
                            else
                            {
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.PlaylistList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePlaylist), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity.RunOnUiThread(ShowEmptyPage);
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
                 
                var item = MAdapter.PlaylistList.FirstOrDefault(a => a.Name == Context.GetText(Resource.String.Lbl_AddNewPlaylist));
                if (item == null)
                {
                    var addNewPlaylist = new PlaylistDataObject()
                    {
                        Id = 0,
                        Name = GetText(Resource.String.Lbl_AddNewPlaylist),
                        UserId = 0
                    };

                    if (!MAdapter.PlaylistList.Contains(addNewPlaylist))
                        MAdapter.PlaylistList.Insert(0, addNewPlaylist);
                }
                 
                if (MAdapter.PlaylistList.Count > 1)
                {
                    PopupFilterList.TxtSongName.Text = MAdapter.PlaylistList.Count - 1 + " " + GetText(Resource.String.Lbl_Playlist);

                    PopupFilterList.TopLayout.Visibility = ViewStates.Visible;
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    PopupFilterList.TopLayout.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPlaylist);
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

        #region Scroll

        //My Playlist
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.PlaylistList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id.ToString());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Event 

        private void MAdapterItemClick(object sender, PlaylistRowAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.PlaylistList[e.Position];
                if (item != null)
                {
                    if (item?.Name == Context.GetText(Resource.String.Lbl_AddNewPlaylist))
                    {
                        CreatePlaylistBottomSheet createPlaylistBottomSheet = new CreatePlaylistBottomSheet();
                        createPlaylistBottomSheet.Show(GlobalContext.SupportFragmentManager, createPlaylistBottomSheet.Tag);
                    }
                    else
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
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}