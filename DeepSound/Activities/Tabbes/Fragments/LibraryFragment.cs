using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Chat;
using DeepSound.Activities.Library;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class LibraryFragment : Fragment
    {
        #region Variables Basic

        private HomeActivity GlobalContext;

        private ImageView IconMessages;

        public HSoundAdapter MAdapter;
        private ViewStub RecentlyViewStub;
        private View RecentlyInflated;
        private AdManagerAdView AdManagerAdView;

        private LinearLayout PlayListsLayout, LikedLayout, DownloadsLayout, SharedLayout, PurchasesLayout;
        public LikedFragment LikedFragment;
        public RecentlyPlayedFragment RecentlyPlayedFragment;
        public FavoritesFragment FavoritesFragment;
        public LatestDownloadsFragment LatestDownloadsFragment;
        public MyPlaylistFragment MyPlaylistFragment;
        public SharedFragment SharedFragment;
        public PurchasesFragment PurchasesFragment;


        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                GlobalContext = (HomeActivity)Activity;
                HasOptionsMenu = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                View view = inflater?.Inflate(Resource.Layout.TLibraryLayout, container, false);
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
                //Get Value And Set Toolbar
                InitComponent(view);
                SetRecyclerViewAdapters();

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                IconMessages = (ImageView)view.FindViewById(Resource.Id.MessagesIcon);
                IconMessages.Click += IconMessagesOnClick;

                RecentlyViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubRecently);

                PlayListsLayout = view.FindViewById<LinearLayout>(Resource.Id.PlayListsLayout);
                PlayListsLayout.Click += PlayListsLayoutOnClick;

                LikedLayout = view.FindViewById<LinearLayout>(Resource.Id.LikedLayout);
                LikedLayout.Click += LikedLayoutOnClick;

                DownloadsLayout = view.FindViewById<LinearLayout>(Resource.Id.DownloadsLayout);
                DownloadsLayout.Click += DownloadsLayoutOnClick;

                SharedLayout = view.FindViewById<LinearLayout>(Resource.Id.SharedLayout);
                SharedLayout.Click += SharedLayoutOnClick;

                PurchasesLayout = view.FindViewById<LinearLayout>(Resource.Id.PurchasesLayout);
                PurchasesLayout.Click += PurchasesLayoutOnClick;

                AdManagerAdView = view.FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);
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
                MAdapter = new HSoundAdapter(Activity)
                {
                    SoundsList = new ObservableCollection<SoundDataObject>()
                };
                MAdapter.ItemClick += MAdapterRecentlyItemClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //open last chat 
        private void IconMessagesOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(Context, typeof(LastChatActivity));
                Context.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterRecentlyItemClick(object sender, HSoundAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    item.IsPlay = true;
                    MAdapter.NotifyItemChanged(e.Position);

                    Constant.PlayPos = e.Position;
                    GlobalContext?.SoundController?.StartPlaySound(item, MAdapter.SoundsList);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void PurchasesLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                PurchasesFragment = new PurchasesFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(PurchasesFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SharedLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                SharedFragment = new SharedFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(SharedFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DownloadsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                LatestDownloadsFragment = new LatestDownloadsFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(LatestDownloadsFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikedLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                LikedFragment = new LikedFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(LikedFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PlayListsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                MyPlaylistFragment = new MyPlaylistFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(MyPlaylistFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadRecentlyAsync });
        }

        private async Task LoadRecentlyAsync()
        {
            //if (!AppSettings.AllowRecentlyPlayed)
            //    return;

            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.SoundsList.Count;
                var (apiStatus, respond) = await RequestsAsync.User.GetRecentlyPlayedAsync(UserDetails.UserId.ToString(), "15", "0");
                if (apiStatus == 200)
                {
                    if (respond is GetSoundObject result)
                    {
                        var respondList = result.Data?.SoundList.Count;
                        if (respondList > 0)
                        {
                            result.Data.SoundList = DeepSoundTools.ListFilter(result.Data?.SoundList);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data?.SoundList let check = MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.SoundsList.Add(item);
                                }

                                Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.SoundsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(result.Data?.SoundList);
                                Activity?.RunOnUiThread(() =>
                                {
                                    RecentlyInflated ??= RecentlyViewStub.Inflate();

                                    TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                                    recyclerInflater.InflateLayout<UserDataObject>(Activity, RecentlyInflated, MAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, Context.GetText(Resource.String.Lbl_RecentlyPlayed));
                                    if (!recyclerInflater.MainLinear.HasOnClickListeners)
                                    {
                                        recyclerInflater.MainLinear.Click += null;
                                        recyclerInflater.MainLinear.Click += RecentlyMoreOnClick;
                                    }
                                });
                            }
                        }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
            }
            else
            {
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void RecentlyMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                RecentlyPlayedFragment = new RecentlyPlayedFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(RecentlyPlayedFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}