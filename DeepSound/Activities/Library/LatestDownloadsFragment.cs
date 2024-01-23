using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Library
{
    public class LatestDownloadsFragment : Fragment
    {
        #region Variables Basic

        public RowSoundAdapter MAdapter;
        private HomeActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;

        private AdView MAdView;

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

                LoadLatestDownloads();

                AdsGoogle.Ad_RewardedVideo(Activity);
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
                GlobalContext.SetToolBar(toolbar, GetString(Resource.String.Lbl_LatestDownloads));
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
                MAdapter = new RowSoundAdapter(Activity, "LatestDownloadsFragment") { SoundsList = new ObservableCollection<SoundDataObject>() };
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

        //Start Play Sound 
        private void MAdapterItemClick(object sender, RowSoundAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var sqlEntity = new SqLiteDatabase();
                    SoundDataObject dataSound = sqlEntity.Get_LatestDownloadsSound(item.Id);
                    if (dataSound != null && !string.IsNullOrEmpty(dataSound.AudioLocation) && (dataSound.AudioLocation.Contains("file://") || dataSound.AudioLocation.Contains("content://") || dataSound.AudioLocation.Contains("storage") || dataSound.AudioLocation.Contains("/data/user/0/")))
                    {
                        var filePath = SoundDownloadAsyncController.GetDownloadedDiskVideoUri(dataSound.Title);
                        if (!string.IsNullOrEmpty(filePath))
                            item.AudioLocation = filePath;
                        else
                        {
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_FileNotFoundInDownload), ToastLength.Long)?.Show();
                            return;
                        }

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
                    else
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_FileNotFoundInDownload), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Downloads Sounds

        //Get Latest Downloads Sounds from Database
        private void LoadLatestDownloads()
        {
            try
            {
                MAdapter.SoundsList.Clear();

                var sqlEntity = new SqLiteDatabase();
                var watchOffline = sqlEntity.Get_LatestDownloadsSound();

                if (watchOffline?.Count > 0)
                {
                    MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(watchOffline);
                    MAdapter.NotifyDataSetChanged();

                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }

                if (MAdapter.SoundsList.Count == 0)
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    //empty
                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSound);
                    EmptyStateLayout.Visibility = ViewStates.Visible;
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