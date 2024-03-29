﻿using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using DeepSound.Activities.Chat;
using DeepSound.Activities.MyContacts;
using DeepSound.Activities.MyProfile;
using DeepSound.Activities.Tabbes;
using DeepSound.Activities.UserProfile.Fragments;
using DeepSound.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Google.Android.Material.AppBar;
using Google.Android.Material.Dialog;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace DeepSound.Activities.UserProfile
{
    public class UserProfileFragment : Fragment, IDialogListCallBack, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        private CollapsingToolbarLayout CollapsingToolbar;
        private FrameLayout IconBack;
        private ImageView IconPro, IconMore, IconInfo, ImageBack;
        private ImageView ImageCover;
        private CircleImageView ImageAvatar;
        private TextView TxtFullName, TxtCountFollowers, TxtFollowers, TxtCountFollowing, TxtFollowing;
        private AppCompatButton BtnFollow, ChatButton;
        private ViewPager2 ViewPagerView;
        private TabLayout Tabs;
        private UserActivitiesFragment ActivitiesFragment;
        private UserAlbumsFragment AlbumsFragment;
        private UserLikedFragment LikedFragment;
        private UserPlaylistFragment PlaylistFragment;
        private UserSongsFragment SongsFragment;
        private UserStationsFragment StationsFragment;
        private UserStoreFragment StoreFragment;
        private UserEventsFragment EventFragment;
        private Details DetailsCounter;

        private UserDataObject DataUser;
        private string UserId;
        private MainTabAdapter Adapter;

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
                View view = inflater.Inflate(Resource.Layout.UserProfileLayout, container, false);
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

                UserId = Arguments?.GetString("UserId") ?? "";

                InitComponent(view);
                SetUpViewPager();

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Activity.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                    Activity.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                }

                GetMyInfoData();

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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                CollapsingToolbar = (CollapsingToolbarLayout)view.FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = "";

                ImageBack = view.FindViewById<ImageView>(Resource.Id.ImageBack);
                IconBack = view.FindViewById<FrameLayout>(Resource.Id.back);
                IconBack.Click += IconBackOnClick;

                ImageCover = (ImageView)view.FindViewById(Resource.Id.imageCover);

                IconPro = (ImageView)view.FindViewById(Resource.Id.pro);
                IconPro.Visibility = ViewStates.Invisible;

                IconMore = (ImageView)view.FindViewById(Resource.Id.more);
                IconMore.Click += ButtonMoreOnClick;

                IconInfo = (ImageView)view.FindViewById(Resource.Id.info);
                IconInfo.Click += IconInfoOnClick;

                ImageAvatar = (CircleImageView)view.FindViewById(Resource.Id.imageAvatar);

                TxtFullName = (TextView)view.FindViewById(Resource.Id.fullNameTextView);

                TxtCountFollowers = (TextView)view.FindViewById(Resource.Id.countFollowersTextView);
                TxtFollowers = (TextView)view.FindViewById(Resource.Id.FollowersTextView);
                TxtFollowers.Click += TxtFollowersOnClick;
                TxtCountFollowers.Click += TxtFollowersOnClick;

                TxtCountFollowing = (TextView)view.FindViewById(Resource.Id.countFollowingTextView);
                TxtFollowing = (TextView)view.FindViewById(Resource.Id.FollowingTextView);
                TxtFollowing.Click += TxtFollowingOnClick;
                TxtCountFollowing.Click += TxtFollowingOnClick;

                BtnFollow = (AppCompatButton)view.FindViewById(Resource.Id.FollowButton);
                BtnFollow.Click += BtnFollowOnClick;

                ChatButton = (AppCompatButton)view.FindViewById(Resource.Id.ChatButton);
                ChatButton.Click += ChatButtonOnClick;

                ViewPagerView = (ViewPager2)view.FindViewById(Resource.Id.profilePager);
                Tabs = (TabLayout)view.FindViewById(Resource.Id.sectionTab);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager()
        {
            try
            {
                ActivitiesFragment = new UserActivitiesFragment();
                AlbumsFragment = new UserAlbumsFragment();
                LikedFragment = new UserLikedFragment();
                PlaylistFragment = new UserPlaylistFragment();
                SongsFragment = new UserSongsFragment();
                StationsFragment = new UserStationsFragment();
                StoreFragment = new UserStoreFragment();
                EventFragment = new UserEventsFragment();

                Bundle bundle = new Bundle();
                bundle.PutString("UserId", UserId);

                ActivitiesFragment.Arguments = bundle;
                AlbumsFragment.Arguments = bundle;
                LikedFragment.Arguments = bundle;
                PlaylistFragment.Arguments = bundle;
                SongsFragment.Arguments = bundle;
                StationsFragment.Arguments = bundle;
                StoreFragment.Arguments = bundle;
                EventFragment.Arguments = bundle;

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(SongsFragment, GetText(Resource.String.Lbl_Songs));
                Adapter.AddFragment(AlbumsFragment, GetText(Resource.String.Lbl_Albums));
                Adapter.AddFragment(PlaylistFragment, GetText(Resource.String.Lbl_Playlist));
                Adapter.AddFragment(LikedFragment, GetText(Resource.String.Lbl_Liked));
                Adapter.AddFragment(ActivitiesFragment, GetText(Resource.String.Lbl_Activities_Title));

                if (AppSettings.ShowStore)
                    Adapter.AddFragment(StoreFragment, GetText(Resource.String.Lbl_Store_Title));

                if (AppSettings.ShowStations)
                    Adapter.AddFragment(StationsFragment, GetText(Resource.String.Lbl_Stations));

                if (AppSettings.ShowEvent)
                    Adapter.AddFragment(EventFragment, GetText(Resource.String.Lbl_Event));

                ViewPagerView.CurrentItem = Adapter.ItemCount;
                ViewPagerView.OffscreenPageLimit = Adapter.ItemCount;

                ViewPagerView.Orientation = ViewPager2.OrientationHorizontal;
                ViewPagerView.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                ViewPagerView.Adapter = Adapter;
                ViewPagerView.Adapter.NotifyDataSetChanged();

                Tabs.SetTabTextColors(Color.White, Color.ParseColor(AppSettings.MainColor));
                new TabLayoutMediator(Tabs, ViewPagerView, this).Attach();
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
            private readonly UserProfileFragment Activity;

            public MyOnPageChangeCallback(UserProfileFragment activity)
            {
                try
                {
                    Activity = activity;
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
                    var number = Activity.GetSelectedTab(position);
                    switch (number)
                    {
                        //Songs
                        case 1:
                            Activity.SongsFragment?.StartApiServiceWithOffset();
                            break;
                        //Albums
                        case 2:
                            Activity.AlbumsFragment?.StartApiServiceWithOffset();
                            break;
                        //Playlist
                        case 3:
                            Activity.PlaylistFragment?.StartApiServiceWithOffset();
                            break;
                        //Liked
                        case 4:
                            Activity.LikedFragment?.StartApiServiceWithOffset();
                            break;
                        //Activities
                        case 5:
                            Activity.ActivitiesFragment?.StartApiServiceWithOffset();
                            break;
                        //Store
                        case 6:
                            Activity.StoreFragment?.StartApiServiceWithOffset();
                            break;
                        //Stations
                        case 7:
                            Activity.StationsFragment?.StartApiServiceWithOffset();
                            break;
                        //Event
                        case 8:
                            Activity.EventFragment?.StartApiServiceWithOffset();
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        private int GetSelectedTab(int number)
        {
            try
            {
                var tabName = Tabs.GetTabAt(number)?.Text;
                if (tabName == Resources.GetText(Resource.String.Lbl_Songs))
                {
                    return 1;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Albums))
                {
                    return 2;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Playlist))
                {
                    return 3;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Liked))
                {
                    return 4;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Activities_Title))
                {
                    return 5;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Store_Title))
                {
                    return 6;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Stations))
                {
                    return 7;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_Event))
                {
                    return 8;
                }
                return 0;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        #endregion

        #region Event

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.FragmentNavigatorBack();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open chat 
        private void ChatButtonOnClick(object sender, EventArgs e)
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

        //Open Edit page profile
        private void BtnFollowOnClick(object sender, EventArgs e)
        {
            if (!UserDetails.IsLogin)
            {
                PopupDialogController dialog = new PopupDialogController(GlobalContext, null, "Login");
                dialog.ShowNormalDialog(GlobalContext.GetText(Resource.String.Lbl_Login), GlobalContext.GetText(Resource.String.Lbl_Message_Sorry_signin), GlobalContext.GetText(Resource.String.Lbl_Yes), GlobalContext.GetText(Resource.String.Lbl_No));
                return;
            }

            if (Methods.CheckConnectivity())
            {
                switch (BtnFollow?.Tag?.ToString())
                {
                    case "Add": //Sent follow 

                        //BtnFollow.SetBackgroundResource(Resource.Drawable.SubcribeButton);
                        //BtnFollow.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));

                        //icon
                        var iconTick = Activity.GetDrawable(Resource.Drawable.icon_tick_vector);
                        //iconTick.Bounds = new Rect(10, 10, 10, 7);
                        iconTick.SetTint(Color.White);
                        BtnFollow.SetCompoundDrawablesWithIntrinsicBounds(iconTick, null, null, null);
                        BtnFollow.Text = GetText(Resource.String.Lbl_Following);
                        BtnFollow.Tag = "friends";

                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Sent_successfully_followed), ToastLength.Short)?.Show();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(UserId, true) });
                        break;
                    case "friends": //Sent un follow 

                        //BtnFollow.SetBackgroundResource(Resource.Drawable.SubcribeButton);
                        //BtnFollow.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#444444"));

                        //icon
                        var iconAdd = Activity.GetDrawable(Resource.Drawable.icon_add_fill_vector);
                        //iconAdd.Bounds = new Rect(10, 10, 10, 7);
                        iconAdd.SetTint(Color.White);
                        BtnFollow.SetCompoundDrawablesWithIntrinsicBounds(iconAdd, null, null, null);
                        BtnFollow.Text = GetText(Resource.String.Lbl_Follow);
                        BtnFollow.Tag = "Add";

                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Sent_successfully_Unfollowed), ToastLength.Short)?.Show();
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollowUserAsync(UserId, false) });
                        break;
                }
            }
            else
            {
                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }

        }

        //More 
        private void ButtonMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(Context);

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

        //Show Info User
        private void IconInfoOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(DialogInfoUserActivity));
                intent.PutExtra("ItemDataUser", JsonConvert.SerializeObject(DataUser));
                intent.PutExtra("ItemDataDetails", JsonConvert.SerializeObject(DetailsCounter));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open User Contact >> Following
        private void TxtFollowingOnClick(object sender, EventArgs e)
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
        private void TxtFollowersOnClick(object sender, EventArgs e)
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
                DataUser = JsonConvert.DeserializeObject<UserDataObject>(Arguments?.GetString("ItemData") ?? "");
                if (DataUser != null) LoadDataUser(DataUser);

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadDataUser(UserDataObject dataUser)
        {
            try
            {
                if (dataUser != null)
                {
                    CollapsingToolbar.Title = DeepSoundTools.GetNameFinal(dataUser);

                    GlideImageLoader.LoadImage(Activity, dataUser.Cover, ImageCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                    GlideImageLoader.LoadImage(Activity, dataUser.Avatar, ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                    TxtFullName.Text = DeepSoundTools.GetNameFinal(dataUser);

                    IconPro.Visibility = dataUser.IsPro == 1 ? ViewStates.Visible : ViewStates.Gone;

                    if (dataUser.Verified == 1)
                        TxtFullName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);

                    if (dataUser.IsFollowing == "1" || dataUser.IsFollowing == "true") // My Friend
                    {
                        //BtnFollow.SetBackgroundResource(Resource.Drawable.SubcribeButton);
                        //BtnFollow.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));

                        //icon
                        var iconTick = Activity.GetDrawable(Resource.Drawable.icon_tick_vector);
                        //iconTick.Bounds = new Rect(10, 10, 10, 7);
                        iconTick.SetTint(Color.White);

                        BtnFollow.SetCompoundDrawablesWithIntrinsicBounds(iconTick, null, null, null);
                        BtnFollow.Text = GetText(Resource.String.Lbl_Following);
                        BtnFollow.Tag = "friends";

                    }
                    else  //Not Friend
                    {
                        //BtnFollow.SetBackgroundResource(Resource.Drawable.SubcribeButton);
                        //BtnFollow.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#444444"));

                        //icon
                        var iconAdd = Activity.GetDrawable(Resource.Drawable.icon_add_fill_vector);
                        //iconAdd.Bounds = new Rect(10, 10, 10, 7);
                        iconAdd.SetTint(Color.White);
                        BtnFollow.SetCompoundDrawablesWithIntrinsicBounds(iconAdd, null, null, null);
                        BtnFollow.Text = GetText(Resource.String.Lbl_Follow);
                        BtnFollow.Tag = "Add";
                    }

                    if (SongsFragment?.IsCreated == true)
                        SongsFragment.PopulateData(dataUser.Latestsongs?.FirstOrDefault());

                    if (AlbumsFragment?.IsCreated == true)
                        AlbumsFragment.PopulateData(dataUser.Albums?.FirstOrDefault());

                    if (PlaylistFragment?.IsCreated == true)
                        PlaylistFragment.PopulateData(dataUser.Playlists?.FirstOrDefault());

                    if (LikedFragment?.IsCreated == true)
                        LikedFragment.PopulateData(dataUser.Liked?.FirstOrDefault());

                    if (ActivitiesFragment?.IsCreated == true)
                        ActivitiesFragment.PopulateData(dataUser.Activities?.FirstOrDefault());

                    if (StationsFragment?.IsCreated == true)
                        StationsFragment.PopulateData(dataUser.Stations);

                    if (StoreFragment?.IsCreated == true)
                        StoreFragment.PopulateData(dataUser.Store?.FirstOrDefault());

                    if (EventFragment?.IsCreated == true)
                        EventFragment.PopulateData(dataUser.Events);
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
                Toast.MakeText(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartApiFetch });
        }

        private async Task StartApiFetch()
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
                        Activity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                DataUser = result.Data;

                                LoadDataUser(result.Data);

                                if (result.Details != null)
                                {
                                    DetailsCounter = result.Details;

                                    TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(result.Details.Followers);
                                    TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(result.Details.Following);
                                    //TxtCountTracks.Text = Methods.FunString.FormatPriceValue(result.Details.LatestSongs);
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            else
            {
                Methods.DisplayReportResult(Activity, respond);
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }
}