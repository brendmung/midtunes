﻿using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Facebook.Ads;
using DeepSound.Activities.Stations.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeepSound.Activities.UserProfile.Fragments
{
    public class UserStationsFragment : Fragment
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        private View Inflated;
        private ViewStub EmptyStateLayout;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private StationsAdapter MAdapter;
        private LinearLayoutManager MLayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        public bool IsCreated;
        private string UserId;
        private AdView BannerAd;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var view = inflater.Inflate(Resource.Layout.MainFragmentLayout, container, false);
                IsCreated = true;
                UserId = Arguments?.GetString("UserId");

                InitComponent(view);
                SetRecyclerViewAdapters();

                var TopLayout = view.FindViewById<RelativeLayout>(Resource.Id.Toplayout);
                TopLayout.Visibility = ViewStates.Gone;
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override void OnDestroy()
        {
            try
            {
                BannerAd?.Destroy();
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
                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(DeepSoundTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                LinearLayout adContainer = view.FindViewById<LinearLayout>(Resource.Id.bannerContainer);
                if (AppSettings.ShowFbBannerAds)
                    BannerAd = AdsFacebook.InitAdView(Activity, adContainer, MRecycler);
                else if (AppSettings.ShowAppLovinBannerAds)
                    AdsAppLovin.InitBannerAd(Activity, adContainer, MRecycler);
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
                MAdapter = new StationsAdapter(Activity) { StationsList = new ObservableCollection<SoundDataObject>() };
                MAdapter.ItemClick += MAdapterItemClick;
                MLayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(MLayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SoundDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
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

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.StationsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id.ToString());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Start Play Sound 
        private void MAdapterItemClick(object sender, StationsAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.StationsList);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.StationsList.Clear();
                MAdapter.NotifyDataSetChanged();

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;
                MainScrollEvent.IsLoading = false;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data 

        public void PopulateData(List<SoundDataObject> list)
        {
            try
            {
                if (list?.Count > 0)
                {
                    list.Reverse();
                    MAdapter.StationsList = new ObservableCollection<SoundDataObject>(list);
                    MAdapter.NotifyDataSetChanged();
                    ShowEmptyPage();
                }
                else
                {
                    ShowEmptyPage();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartApiServiceWithOffset()
        {
            try
            {
                if (MAdapter.StationsList.Count > 0)
                {
                    var item = MAdapter.StationsList.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                        StartApiService(item.Id.ToString());
                }
                else
                {
                    StartApiService();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadData(offset) });
        }

        private async Task LoadData(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = MAdapter.StationsList.Count;
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
                                foreach (var item in from item in result.Data let check = MAdapter.StationsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.StationsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.StationsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.StationsList = new ObservableCollection<SoundDataObject>(result.Data);
                                Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.StationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
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

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.StationsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoStations);
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

    }
}