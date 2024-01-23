using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Global;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeepSound.Activities.Search
{
    public class SearchSongsFragment : Fragment
    {
        #region Variables Basic

        public RowSoundAdapter MAdapter;
        private HomeActivity GlobalContext;
        private SearchFragment ContextSearch;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        public ProgressBar ProgressBarLoader;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;

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
                ProgressBarLoader.Visibility = ViewStates.Visible;

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
                MAdapter = new RowSoundAdapter(Activity, "SearchSongsFragment") { SoundsList = new ObservableCollection<SoundDataObject>() };
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

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

                //if (Inflated == null)
                //    Inflated = EmptyStateLayout.Inflate();

                //EmptyStateInflater x = new EmptyStateInflater();
                //x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                //if (!x.EmptyStateButton.HasOnClickListeners)
                //{
                //    x.EmptyStateButton.Click += null;
                //    x.EmptyStateButton.Click += ContextSearch.TryAgainButton_Click;
                //}

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
                var item = MAdapter.SoundsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                {
                    ContextSearch.OffsetSongs = item.Id.ToString();
                    ContextSearch.StartApiService();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Play Sound 
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

        #endregion

    }
}