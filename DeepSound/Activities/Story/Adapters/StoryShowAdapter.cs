using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.Stories.StoriesProgressView;
using DeepSoundClient.Classes.Story;
using Java.IO;
using Java.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IList = System.Collections.IList;
using Uri = Android.Net.Uri;

namespace DeepSound.Activities.Story.Adapters
{
    public class StoryShowAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, RecyclerView.IOnChildAttachStateChangeListener
    {
        public readonly AppCompatActivity ActivityContext;
        public readonly StoriesProgressView StoriesProgress;
        public readonly ViewStoryFragment StoryFragment;
        public ObservableCollection<StoryDataObject> StoryList = new ObservableCollection<StoryDataObject>();
        private readonly RecyclerView RecyclerView;
        private View ViewHolderParent;

        public static MediaPlayer MediaPlayer { get; set; }

        public StoryShowAdapter(AppCompatActivity context, StoriesProgressView storyProgressView, ViewStoryFragment storyFragment)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                StoriesProgress = storyProgressView;
                StoryFragment = storyFragment;
                RecyclerView = storyFragment.MRecycler;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => StoryList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> ViewStoryLayout
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewStoryLayout, parent, false);
                var vh = new StoryShowAdapterViewHolder(itemView, this);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is StoryShowAdapterViewHolder holder)
                {
                    var item = StoryList[position];
                    if (item != null)
                    {
                        ViewHolderParent = holder.MainView;

                        Glide.With(ActivityContext?.BaseContext).Load(item.Image).Apply(new RequestOptions()).Into(holder.StoryImageView);

                        holder.MoreInfoButton.Visibility = string.IsNullOrEmpty(item.Url) ? ViewStates.Gone : ViewStates.Visible;

                        StoryFragment.SetLastSeenTextView(item);

                        if (item.UserId == UserDetails.UserId)
                        {
                            holder.OpenSeenListLayout.Visibility = ViewStates.Visible;
                            holder.SeenCounterTextView.Visibility = ViewStates.Visible;
                            holder.SeenCounterTextView.Text = item.ViewsCount;
                        }
                        else
                        {
                            holder.OpenSeenListLayout.Visibility = ViewStates.Gone;
                            holder.SeenCounterTextView.Visibility = ViewStates.Gone;
                        }

                        var fileName = item.Audio.Split('/').Last();
                        var mediaFile = DeepSoundTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, item.Audio);

                        if (MediaPlayer == null)
                        {
                            MediaPlayer = new MediaPlayer();
                            MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder()?.SetUsage(AudioUsageKind.Media)?.SetContentType(AudioContentType.Music)?.Build());
                            MediaPlayer.Completion += (sender, args) =>
                            {
                                try
                                {
                                    MediaPlayer.Stop();
                                    MediaPlayer.Reset();
                                    MediaPlayer = null;

                                    StoriesProgress?.Skip();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            };

                            MediaPlayer.Prepared += (o, eventArgs) =>
                            {
                                try
                                {
                                    MediaPlayer.Start();

                                    StoriesProgress?.StartStories(); // <- start progress
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            };

                            if (!string.IsNullOrEmpty(mediaFile) && (mediaFile.Contains("file://") || mediaFile.Contains("content://") || mediaFile.Contains("storage") || mediaFile.Contains("/data/user/0/")))
                            {
                                File file2 = new File(mediaFile);
                                var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);

                                MediaPlayer.SetDataSource(ActivityContext, photoUri);
                                MediaPlayer.Prepare();
                            }
                            else
                            {
                                MediaPlayer.SetDataSource(ActivityContext, Uri.Parse(mediaFile));
                                MediaPlayer.PrepareAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public StoryDataObject GetItem(int position)
        {
            return StoryList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StoryList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.Image))
                        d.Add(item.Image);

                    return d;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }

        #region ChildAttachState

        public void OnChildViewAttachedToWindow(View view)
        {
            try
            {
                //if (ViewHolderParent != null && ViewHolderParent.Equals(view))
                //{
                //    var mainHolder = RecyclerView.GetChildViewHolder(view);
                //    if (mainHolder is StoryShowAdapterViewHolder holder)
                //    {
                //        if (PlayerView != null)
                //        {
                //            holder.Play();
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnChildViewDetachedFromWindow(View view)
        {
            try
            {
                if (ViewHolderParent != null && ViewHolderParent.Equals(view))
                {
                    var mainHolder = RecyclerView.GetChildViewHolder(view);
                    if (mainHolder is StoryShowAdapterViewHolder holder)
                    {
                        holder.Destroy();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion 
    }

    public class StoryShowAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener, View.IOnLongClickListener
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView StoryImageView { get; private set; }

        public View ReverseView { get; private set; }
        public View CenterView { get; private set; }
        public View SkipView { get; private set; }

        public LinearLayout StoryBodyLayout { get; private set; }
        public AppCompatButton MoreInfoButton { get; private set; }

        public LinearLayout OpenSeenListLayout { get; private set; }
        public TextView SeenCounterTextView { get; private set; }
        public TextView IconSeen { get; private set; }

        #endregion

        private long PressTime;
        private readonly long Limit = 500L;
        private bool PlayerPaused, Paused;
        private readonly StoryShowAdapter MAdapter;

        public StoryShowAdapterViewHolder(View itemView, StoryShowAdapter adapter) : base(itemView)
        {
            try
            {
                MAdapter = adapter;
                MainView = itemView;

                StoryImageView = itemView.FindViewById<ImageView>(Resource.Id.imagstoryDisplay);

                ReverseView = itemView.FindViewById<View>(Resource.Id.reverse);
                CenterView = itemView.FindViewById<View>(Resource.Id.center);
                SkipView = itemView.FindViewById<View>(Resource.Id.skip);

                StoryBodyLayout = itemView.FindViewById<LinearLayout>(Resource.Id.story_body_layout);
                MoreInfoButton = itemView.FindViewById<AppCompatButton>(Resource.Id.MoreInfoButton);

                OpenSeenListLayout = itemView.FindViewById<LinearLayout>(Resource.Id.open_seen_list_layout);
                SeenCounterTextView = itemView.FindViewById<TextView>(Resource.Id.seen_counter);
                IconSeen = itemView.FindViewById<TextView>(Resource.Id.iconSeen);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconSeen, FontAwesomeIcon.Eye);

                //Event
                //ReverseView?.SetOnTouchListener(new MyTouchListener(this));
                //SkipView?.SetOnTouchListener(new MyTouchListener(this));
                //CenterView?.SetOnTouchListener(new MyTouchListener(this));

                if (AppSettings.EnableStorySeenList)
                {
                    OpenSeenListLayout?.SetOnClickListener(this);
                    //new ViewSwipeTouchListener(adapter.ActivityContext, OpenSeenListLayout, new MySwipeListener(this));
                }

                MoreInfoButton?.SetOnClickListener(this);

                ReverseView?.SetOnClickListener(this);
                SkipView?.SetOnClickListener(this);
                CenterView?.SetOnClickListener(this);
                CenterView?.SetOnLongClickListener(this);

                MAdapter.StoryFragment.SetStoryStateListener(new MyStoryStateListener(this));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                StoryDataObject dataNowStory = MAdapter.StoryList[BindingAdapterPosition];

                if (v.Id == ReverseView.Id)
                {
                    MAdapter.StoriesProgress?.Reverse();
                }
                else if (v.Id == CenterView.Id || v.Id == SkipView.Id)
                {
                    MAdapter.StoriesProgress?.Skip();
                }
                else if (v.Id == OpenSeenListLayout.Id)
                {
                    if (!Paused)
                    {
                        MAdapter.StoriesProgress?.Pause();
                        PausePlayer();
                        Paused = true;
                    }

                    if (dataNowStory != null)
                    {
                        StorySeenListFragment bottomSheet = new StorySeenListFragment(MAdapter.StoryFragment);
                        Bundle bundle = new Bundle();
                        bundle.PutString("recipientId", dataNowStory.UserId.ToString());
                        bundle.PutString("StoryId", dataNowStory.Id);
                        bundle.PutString("DataNowStory", JsonConvert.SerializeObject(dataNowStory));
                        bottomSheet.Arguments = bundle;
                        bottomSheet.Show(MAdapter.StoryFragment.ChildFragmentManager, bottomSheet.Tag);
                    }
                }
                else if (v.Id == MoreInfoButton.Id)
                {
                    if (!Paused)
                    {
                        MAdapter.StoriesProgress?.Pause();
                        PausePlayer();
                        Paused = true;
                    }

                    new IntentController(MAdapter.ActivityContext).OpenBrowserFromApp(dataNowStory.Url);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool OnLongClick(View v)
        {
            try
            {
                if (v.Id == CenterView.Id)
                {
                    if (!Paused)
                    {
                        MAdapter.StoriesProgress?.Pause();
                        PausePlayer();
                        MenuNavigation(false);
                        Paused = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
            return false;
        }

        #region MediaPlayer 

        public void PlayPlayer()
        {
            try
            {
                if (!StoryShowAdapter.MediaPlayer.IsPlaying)
                    StoryShowAdapter.MediaPlayer?.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PausePlayer()
        {
            try
            {
                StoryShowAdapter.MediaPlayer?.Pause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

        private void MenuNavigation(bool show)
        {
            try
            {
                var isOwner = MAdapter.StoryList[BindingAdapterPosition]?.IsOwner ?? false;
                MAdapter.StoryFragment.OnEventMainThread(show);
                if (show)
                {
                    MAdapter.StoryFragment.FadeInAnimation(StoryBodyLayout, 200);

                    if (isOwner)
                        MAdapter.StoryFragment.FadeInAnimation(OpenSeenListLayout, 200);
                }
                else
                {
                    MAdapter.StoryFragment.FadeOutAnimation(StoryBodyLayout, 200);

                    if (isOwner)
                        MAdapter.StoryFragment.FadeOutAnimation(OpenSeenListLayout, 200);

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyStoryStateListener : StoriesProgressView.IStoryStateListener
        {
            private readonly StoryShowAdapterViewHolder Holder;
            public MyStoryStateListener(StoryShowAdapterViewHolder holder)
            {
                Holder = holder;
            }

            public void OnPause()
            {
                try
                {
                    if (!Holder.Paused)
                    {
                        Holder.MAdapter.StoriesProgress?.Pause();
                        Holder.PausePlayer();
                        Holder.Paused = true;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public void OnResume()
            {
                try
                {
                    if (Holder.Paused)
                    {
                        Holder.MAdapter.StoriesProgress?.Resume();
                        Holder.PlayPlayer();
                        Holder.Paused = false;
                    }

                    var item = Holder.MAdapter.StoryList[Holder.BindingAdapterPosition];
                    bool isOwner = item?.IsOwner ?? false;
                    if (isOwner)
                    {
                        Holder.OpenSeenListLayout.Visibility = ViewStates.Visible;
                        Holder.SeenCounterTextView.Visibility = ViewStates.Visible;
                        Holder.SeenCounterTextView.Text = item.ViewsCount;
                    }
                    else
                    {
                        Holder.OpenSeenListLayout.Visibility = ViewStates.Gone;
                        Holder.SeenCounterTextView.Visibility = ViewStates.Gone;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

    }
}