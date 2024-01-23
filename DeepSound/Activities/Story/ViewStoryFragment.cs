using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using DeepSound.Activities.Story.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSound.Library.Anjo.Stories.DragView;
using DeepSound.Library.Anjo.Stories.StoriesProgressView;
using DeepSoundClient.Classes.Story;
using DeepSoundClient.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace DeepSound.Activities.Story
{
    public class ViewStoryFragment : Fragment, DragToClose.IDragListener, StoriesProgressView.IStoriesListener
    {
        #region  Variables Basic

        private StoryDetailsActivity GlobalContext;
        private TextView IconBack;
        private ImageView UserImageView;
        private string UserId = "", StoryId = "";
        public StoriesProgressView StoriesProgress;
        private StoryDataObject DataStories;
        private View MainView;
        private LinearLayout UserLayout;
        private TextView UsernameTextView, LastSeenTextView, DeleteIconView;
        public RecyclerView MRecycler;
        private StoryShowAdapter MAdapter;
        private ObservableCollection<StoryDataObject> StoryList;
        private int Counter;
        public StoriesProgressView.IStoryStateListener StoryStateListener;

        private static ViewStoryFragment Instance;

        private bool MIsVisibleToUser;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (StoryDetailsActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                MainView = inflater.Inflate(Resource.Layout.StorySwipeLayout, container, false);
                return MainView;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                MainView = view;

                Instance = this;

                InitComponent(view);
                SetRecyclerViewAdapters();

                var checkSection = HomeActivity.GetInstance()?.HomeFragment?.LatestHomeTab?.StoryAdapter?.StoryList;
                if (checkSection?.Count > 0)
                {
                    List<StoryDataObject> storiesList = new List<StoryDataObject>(checkSection);
                    storiesList.RemoveAll(o => o.Type == "Your");

                    StoryList = new ObservableCollection<StoryDataObject>(storiesList);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void SetMenuVisibility(bool menuVisible)
        {
            try
            {
                base.SetMenuVisibility(menuVisible);
                MIsVisibleToUser = menuVisible;
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
                AddOrRemoveEvent(true);
                //StoryStateListener?.OnResume();

                if (IsResumed && MIsVisibleToUser)
                {
                    //var position = Arguments?.GetInt("position", 0); 
                    DataStories = JsonConvert.DeserializeObject<StoryDataObject>(Arguments?.GetString("DataItem") ?? "");
                    if (DataStories != null)
                    {
                        LoadData(DataStories);
                    }
                }
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
                AddOrRemoveEvent(false);
                StoryStateListener?.OnPause();
                StoriesProgress?.Pause();
                base.OnPause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnStop()
        {
            try
            {
                base.OnStop();

                if (MIsVisibleToUser)
                    StoryStateListener?.OnPause();
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

        public override void OnDestroyView()
        {
            try
            {
                Destroy();
                StoriesProgress?.Destroy();
                StoriesProgress = null;
                Instance = null;

                base.OnDestroyView();
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
                Destroy();
                StoriesProgress?.Destroy();
                StoriesProgress = null;
                Instance = null;

                GlobalContext = null;
                IconBack = null;
                UserImageView = null;
                UserId = "";
                StoryId = "";
                StoriesProgress = null;
                DataStories = null;
                MainView = null;
                UserLayout = null;
                UsernameTextView = null;
                LastSeenTextView = null;
                DeleteIconView = null;
                MRecycler = null;
                MAdapter = null;
                StoryList = null;
                Counter = 0;
                StoryStateListener = null;

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.story_container);

                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack);

                StoriesProgress = view.FindViewById<StoriesProgressView>(Resource.Id.storyProgressView);

                UserLayout = view.FindViewById<LinearLayout>(Resource.Id.userLayout);
                UserImageView = view.FindViewById<ImageView>(Resource.Id.imageAvatar);
                UsernameTextView = view.FindViewById<TextView>(Resource.Id.username);
                LastSeenTextView = view.FindViewById<TextView>(Resource.Id.time);

                DeleteIconView = view.FindViewById<TextView>(Resource.Id.DeleteIcon);
                DeleteIconView.Visibility = ViewStates.Invisible;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, DeleteIconView, IonIconsFonts.Trash);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconBack, FontAwesomeIcon.LongArrowLeft);

                GlobalContext.DragToClose.SetDragListener(this);
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
                MAdapter = new StoryShowAdapter(GlobalContext, StoriesProgress, this)
                {
                    StoryList = new ObservableCollection<StoryDataObject>()
                };

                var layoutManager = new MyLinearLayoutManager(Context, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(layoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.AddOnChildAttachStateChangeListener(MAdapter);

                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<StoryDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    if (DeleteIconView != null) DeleteIconView.Click += DeleteIconViewOnClick;
                    if (IconBack != null) IconBack.Click += IconBackOnClick;
                }
                else
                {
                    if (DeleteIconView != null) DeleteIconView.Click -= DeleteIconViewOnClick;
                    if (IconBack != null) IconBack.Click -= IconBackOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ViewStoryFragment GetInstance()
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

        #region Events

        private void IconBackOnClick(object sender, EventArgs e)
        {
            GlobalContext.Finish();
        }

        //delete story
        private async void DeleteIconViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(StoryId))
                    return;

                StoriesProgress?.Pause();

                if (Methods.CheckConnectivity())
                {
                    (int respondCode, var respond) = await RequestsAsync.Story.DeleteStoryAsync(StoryId);
                    if (respondCode == 200)
                    {
                        var checkSection = HomeActivity.GetInstance()?.HomeFragment?.LatestHomeTab?.StoryAdapter;

                        var story = checkSection?.StoryList?.FirstOrDefault(a => a.Id == StoryId);
                        if (story != null)
                        {
                            checkSection.StoryList.Remove(story);
                            checkSection.NotifyDataSetChanged();
                        }
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_Deleted), ToastLength.Short)?.Show();

                        GlobalContext.Finish();
                    }
                    else Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Drag

        public void OnStartDraggingView()
        {

        }

        public void OnDraggingView(float offset)
        {
            try
            {
                if (StoriesProgress != null) StoriesProgress.Alpha = offset;
                if (UserLayout != null) UserLayout.Alpha = offset;
                if (MRecycler != null) MRecycler.Alpha = offset;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnViewClosed()
        {
            try
            {
                Destroy();
                StoriesProgress?.Destroy();
                StoriesProgress = null;
                Instance = null;

                GlobalContext = null;
                IconBack = null;
                UserImageView = null;
                UserId = "";
                StoryId = "";
                StoriesProgress = null;
                DataStories = null;
                MainView = null;
                UserLayout = null;
                UsernameTextView = null;
                LastSeenTextView = null;
                DeleteIconView = null;
                MRecycler = null;
                MAdapter = null;
                StoryList = null;
                Counter = 0;
                StoryStateListener = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Destroy()
        {
            try
            {
                StoryShowAdapter.MediaPlayer?.Stop();
                StoryShowAdapter.MediaPlayer?.Release();

                StoryShowAdapter.MediaPlayer = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region StoriesProgressView

        public void SetStoryStateListener(StoriesProgressView.IStoryStateListener storyStateListener)
        {
            StoryStateListener = storyStateListener;
        }

        public void OnNext()
        {
            try
            {
                //StoriesProgress.Pause();
                ++Counter;

                OnComplete();

                //if (Counter + 1 > DataStories.Count)
                //{
                //    OnComplete();
                //    return;
                //}

                //var dataStory = DataStories[Counter];
                //if (dataStory != null)
                //{
                //    StoryId = dataStory.Id;
                //    Destroy();
                //    MRecycler.ScrollToPosition(Counter);
                //    MAdapter.NotifyItemChanged(Counter);
                //}
                //else
                //{
                //    OnComplete();
                //}
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPrev()
        {
            try
            {
                if (Counter <= 0)
                {
                    //StoriesProgress?.Pause();
                    StoriesProgress?.Destroy();
                    StoriesProgress = null;

                    --Counter;

                    if (GlobalContext.Pager.CurrentItem > 0)
                        GlobalContext.Pager.SetCurrentItem(GlobalContext.Pager.CurrentItem - 1, true);
                    else
                        GlobalContext.Finish();
                }
                else
                {
                    //StoriesProgress?.Pause();
                    --Counter;

                    //var dataStory = DataStories.Stories[Counter];
                    //if (dataStory != null)
                    //{
                    //    StoryId = dataStory.Id;
                    //    Destroy();
                    //    MRecycler.ScrollToPosition(Counter);
                    //    MAdapter.NotifyItemChanged(Counter);
                    //}
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnComplete()
        {
            try
            {
                if (GlobalContext.Pager.CurrentItem < StoryList.Count - 1)
                {
                    StoriesProgress?.Destroy();
                    StoriesProgress = null;

                    if (GlobalContext.Pager.CurrentItem < StoryList.Count - 1)
                        GlobalContext.Pager.SetCurrentItem(GlobalContext.Pager.CurrentItem + 1, true);
                }
                else
                {
                    AdsGoogle.Ad_Interstitial(Activity);
                    GlobalContext.Finish();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Info story

        private void LoadData(StoryDataObject dataStories)
        {
            try
            {
                if (dataStories == null) return;
                UserId = dataStories.UserId.ToString();

                GlideImageLoader.LoadImage(Activity, dataStories.UserData.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                UsernameTextView.Text = DeepSoundTools.GetNameFinal(dataStories.UserData);

                var fistStory = DataStories;
                if (fistStory != null)
                {
                    StoryId = fistStory.Id.ToString();
                    DeleteIconView.Visibility = dataStories.UserId == UserDetails.UserId ? ViewStates.Visible : ViewStates.Invisible;

                    LastSeenTextView.Text = Methods.Time.TimeAgo(fistStory.Time, false);
                }

                StoriesProgress ??= MainView?.FindViewById<StoriesProgressView>(Resource.Id.storyProgressView);

                if (StoriesProgress != null)
                {
                    StoriesProgress.Visibility = ViewStates.Visible;

                    int count = 2;
                    StoriesProgress.Visibility = ViewStates.Visible;
                    StoriesProgress.SetStoriesCount(count); // <- set stories
                    StoriesProgress.SetStoriesListener(this); // <- set listener  
                    //StoriesProgress.SetStoryDuration(10000L); // <- set a story duration   

                    StoriesProgress?.SetStoriesCountWithDurations(dataStories.DurationsList.ToArray());

                    MAdapter.StoryList = new ObservableCollection<StoryDataObject>() { dataStories };
                    MAdapter.NotifyDataSetChanged();

                    //StoriesProgress?.StartStories(); // <- start progress 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private class MyLinearLayoutManager : LinearLayoutManager
        {

            protected MyLinearLayoutManager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }

            public MyLinearLayoutManager(Context context) : base(context)
            {
                Init();
            }

            public MyLinearLayoutManager(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
            {
                Init();
            }

            public MyLinearLayoutManager(Context context, int orientation, bool reverseLayout) : base(context, orientation, reverseLayout)
            {
                Init();
            }

            private void Init()
            {
                try
                {
                    Orientation = Horizontal;
                    ReverseLayout = false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override bool CanScrollVertically()
            {
                return false;
            }

            public override bool CanScrollHorizontally()
            {
                return false;
            }
        }

        public void SetLastSeenTextView(StoryDataObject story)
        {
            Activity?.RunOnUiThread(() =>
            {
                try
                {
                    StoryId = story.Id.ToString();
                    UserId = story.UserId.ToString();
                    DeleteIconView.Visibility = story.UserId == UserDetails.UserId ? ViewStates.Visible : ViewStates.Invisible;

                    LastSeenTextView.Text = Methods.Time.TimeAgo(story.Time, false);

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartStory });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        public void OnEventMainThread(bool show)
        {
            try
            {
                if (show)
                {
                    FadeInAnimation(UserLayout, 200);
                    FadeInAnimation(StoriesProgress, 200);
                }
                else
                {
                    FadeOutAnimation(UserLayout, 200);
                    FadeOutAnimation(StoriesProgress, 200);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FadeOutAnimation(View view, long animationDuration)
        {
            try
            {
                var fadeOut = new AlphaAnimation(1, 0)
                {
                    Interpolator = new AccelerateInterpolator(),
                    StartOffset = animationDuration,
                    Duration = animationDuration
                };
                fadeOut.AnimationEnd += (sender, args) =>
                {
                    try
                    {
                        if (view != null) view.Visibility = ViewStates.Invisible;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };

                view?.StartAnimation(fadeOut);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FadeInAnimation(View view, long animationDuration)
        {
            try
            {
                Animation fadeIn = new AlphaAnimation(0, 1)
                {
                    Interpolator = new DecelerateInterpolator(),
                    Duration = animationDuration
                };
                fadeIn.AnimationEnd += (sender, args) =>
                {
                    try
                    {
                        if (view != null) view.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };

                view?.StartAnimation(fadeIn);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task StartStory()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Story.GetStartStoryAsync(UserId, StoryId);
                if (apiStatus == 200)
                {
                    if (respond is GetStoryObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            foreach (var item in result.Data)
                            {
                                if (item.Next != null)
                                {
                                    StoryDataObject check = StoryList.FirstOrDefault(a => a.Id == item.Next.Id && a.Next.UserId.ToString() == UserId);
                                    if (check == null)
                                    {
                                        item.Audio = DeepSoundClient.InitializeDeepSound.WebsiteUrl + "/" + item.OrgAudio;

                                        var fileName = item.Audio.Split('/').Last();
                                        var mediaFile = DeepSoundTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, item.Audio);
                                        item.PathAudio = mediaFile;

                                        item.DurationsList ??= new List<long>();
                                        item.IsOwner = item.UserId == UserDetails.UserId;

                                        Glide.With(Context).Load(item.Image).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();

                                        var duration = DeepSoundTools.GetDuration(mediaFile);
                                        item.DurationsList.Add(duration);

                                        StoryList.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    Methods.DisplayReportResult(Activity, respond);
            }
            else
            {
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

    }
}