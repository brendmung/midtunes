using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using DeepSound.Activities.Notification;
using DeepSound.Activities.Search;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Tabbes.HomePages;
using DeepSound.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using Google.Android.Material.Tabs;
using System;

namespace DeepSound.Activities.Tabbes.Fragments
{
    public class HomeFragment : AndroidX.Fragment.App.Fragment, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region  Variables Basic

        private TextView AppName;
        public ImageView AddIcon, NotificationIcon;
        private ImageView SearchIcon;

        private HomeActivity ContextGlobal;

        public ViewPager2 ViewPager;
        private TabLayout TabLayout;

        private MainTabAdapter Adapter;
        public LatestHomeFragment LatestHomeTab;
        private SongsByTypeFragment TopSongsTab;
        private SongsByTypeFragment LatestSongsTab;
        private SongsByTypeFragment RecentlyPlayedTab;
        private SongsByTypeFragment PopularSongsTab;
        private TabArtistsFragment ArtistsTab;
        private TopAlbumsFragment TopAlbumsTab;
        public SearchFragment SearchFragment;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                ContextGlobal = (HomeActivity)Activity;
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
                View view = inflater.Inflate(Resource.Layout.THomeLayout, container, false);
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
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                AppName = (TextView)view.FindViewById(Resource.Id.toolbartitle);
                AppName.Text = AppSettings.ApplicationName;

                SearchIcon = (ImageView)view.FindViewById(Resource.Id.searchIcon);
                SearchIcon.Click += SearchButtonOnClick;

                AddIcon = (ImageView)view.FindViewById(Resource.Id.addIcon);
                AddIcon.Click += AddIconOnClick;

                NotificationIcon = (ImageView)view.FindViewById(Resource.Id.notificationIcon);
                NotificationIcon.Click += NotificationButtonOnClick;

                ViewPager = view.FindViewById<ViewPager2>(Resource.Id.ViewPager);
                TabLayout = view.FindViewById<TabLayout>(Resource.Id.tab_home);

                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();

                //TabLayout.SetTabTextColors(DeepSoundTools.IsTabDark() ? Color.White : Color.Black, Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                LatestHomeTab = new LatestHomeFragment();

                Bundle bundleTopSongsTab = new Bundle();
                bundleTopSongsTab.PutString("SongsType", "BrowseTopSongs");
                TopSongsTab = new SongsByTypeFragment() { Arguments = bundleTopSongsTab };

                Bundle bundleLatestSongsTab = new Bundle();
                bundleLatestSongsTab.PutString("SongsType", "NewReleases");
                LatestSongsTab = new SongsByTypeFragment() { Arguments = bundleLatestSongsTab };

                if (UserDetails.IsLogin)
                {
                    Bundle bundleRecentlyPlayedTab = new Bundle();
                    bundleRecentlyPlayedTab.PutString("SongsType", "RecentlyPlayed");
                    RecentlyPlayedTab = new SongsByTypeFragment() { Arguments = bundleRecentlyPlayedTab };
                }

                Bundle bundlePopularSongsTab = new Bundle();
                bundlePopularSongsTab.PutString("SongsType", "Popular");
                PopularSongsTab = new SongsByTypeFragment() { Arguments = bundlePopularSongsTab };

                TopAlbumsTab = new TopAlbumsFragment();

                ArtistsTab = new TabArtistsFragment();

                Adapter = new MainTabAdapter(this);

                Adapter.AddFragment(LatestHomeTab, GetText(Resource.String.Lbl_Suggestion));

                Adapter.AddFragment(TopSongsTab, GetText(Resource.String.Lbl_TopSongs_Title));
                Adapter.AddFragment(LatestSongsTab, GetText(Resource.String.Lbl_LatestSongs_Title));

                if (UserDetails.IsLogin)
                    Adapter.AddFragment(RecentlyPlayedTab, GetText(Resource.String.Lbl_RecentlyPlayed));

                Adapter.AddFragment(TopAlbumsTab, GetText(Resource.String.Lbl_TopAlbums_Title));
                Adapter.AddFragment(PopularSongsTab, GetText(Resource.String.Lbl_Popular_Title));
                Adapter.AddFragment(ArtistsTab, GetText(Resource.String.Lbl_Artists));

                viewPager.UserInputEnabled = false;
                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;
                viewPager.ClearAnimation();

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly HomeFragment Fragment;

            public MyOnPageChangeCallback(HomeFragment fragment)
            {
                try
                {
                    Fragment = fragment;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);
                    if (position % 3 == 0)
                    {
                        AdsGoogle.Ad_Interstitial(Fragment.Activity);
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        #endregion

        #region Event

        private void AddIconOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Login), Activity.GetText(Resource.String.Lbl_Message_Sorry_signin), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                    return;
                }

                AddBottomSheetFragment addBottomSheetFragment = new AddBottomSheetFragment();
                addBottomSheetFragment.Show(ChildFragmentManager, addBottomSheetFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void NotificationButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Login), Activity.GetText(Resource.String.Lbl_Message_Sorry_signin), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                    return;
                }

                NotificationFragment notificationFragment = new NotificationFragment();
                ContextGlobal.FragmentBottomNavigator.DisplayFragment(notificationFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchFragment = new SearchFragment();
                ContextGlobal?.FragmentBottomNavigator.DisplayFragment(SearchFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}