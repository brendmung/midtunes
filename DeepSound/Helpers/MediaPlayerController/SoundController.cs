using Android;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Com.Sothree.Slidinguppanel;
using DeepSound.Activities.Comments;
using DeepSound.Activities.Songs;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using Google.Android.Material.Dialog;
using Google.Android.Material.FloatingActionButton;
using Refractored.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace DeepSound.Helpers.MediaPlayerController
{
    public class SoundController : Object, SeekBar.IOnSeekBarChangeListener, Animator.IAnimatorListener
    {
        #region Variables Basic

        private AppCompatSeekBar SeekSongProgressbar;
        private ProgressBar TopSeekSongProgressbar;
        public FloatingActionButton BtPlay;
        private TextView TvTitleSound, TvDescriptionSound;
        private TextView TvSongCurrentDuration, TvSongTotalDuration, TxtArtistName, TxtArtistAbout, TxtPlaybackSpeed;
        private ImageView BtnIconComments, BtnIconFavorite, BtnIconLike, BtnIconDislike;
        public ImageView BtnIconDownload;
        public ProgressBar ProgressBarDownload;
        private ImageView BtnIconAddTo, BtnIconShare, IconInfo;
        private FrameLayout LinearAddTo, LinearShare, LinearComments, LinearFavorite, LinearLike, LinearDislike;
        private RelativeLayout LinearDownload;
        private ImageView ImageCover, ImageToolbar;
        public ImageView BackIcon, CloseIcon, BtnPlayImage, BtnNextImage;
        private ImageButton BtnSkipPrev, BtnNext, BtnBackward, BtnForward, BtnShuffle, BtnRepeat;
        private CircleImageView ArtistImageView;
        private Timer Timer;
        private readonly Activity ActivityContext;
        private readonly HomeActivity GlobalContext;
        public readonly SocialIoClickListeners ClickListeners;
        private SoundDownloadAsyncController SoundDownload;
        private RowSoundAdapter Adapter;
        private LinearLayout ToolbarCloseLayout, ToolbarOpenLayout;

        #endregion

        #region General

        public SoundController(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                GlobalContext = HomeActivity.GetInstance();

                ClickListeners = new SocialIoClickListeners(activity);

                PlayerService.ActionSeekTo = ActivityContext.PackageName + ".action.ACTION_SEEK_TO";
                PlayerService.ActionPlay = ActivityContext.PackageName + ".action.ACTION_PLAY";
                PlayerService.ActionPause = ActivityContext.PackageName + ".action.PAUSE";
                PlayerService.ActionStop = ActivityContext.PackageName + ".action.STOP";
                PlayerService.ActionSkip = ActivityContext.PackageName + ".action.SKIP";
                PlayerService.ActionRewind = ActivityContext.PackageName + ".action.REWIND";
                PlayerService.ActionToggle = ActivityContext.PackageName + ".action.ACTION_TOGGLE";
                PlayerService.ActionBackward = ActivityContext.PackageName + ".action.ACTION_BACKWARD";
                PlayerService.ActionForward = ActivityContext.PackageName + ".action.ACTION_FORWARD";
                PlayerService.ActionPlaybackSpeed = ActivityContext.PackageName + ".action.ACTION_PLAYBACK_SPEED";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitializeUi()
        {
            try
            {
                SeekSongProgressbar = ActivityContext.FindViewById<AppCompatSeekBar>(Resource.Id.seek_song_progressbar);
                TopSeekSongProgressbar = ActivityContext.FindViewById<ProgressBar>(Resource.Id.seek_song_progressbar2);
                BtPlay = ActivityContext.FindViewById<FloatingActionButton>(Resource.Id.bt_play);
                BtnSkipPrev = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_skipPrev);
                BtnNext = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_next);
                BtnRepeat = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_repeat);
                BtnShuffle = ActivityContext.FindViewById<ImageButton>(Resource.Id.bt_shuffle);
                BtnBackward = ActivityContext.FindViewById<ImageButton>(Resource.Id.btn_Backward);
                BtnForward = ActivityContext.FindViewById<ImageButton>(Resource.Id.btn_Forward);

                TvSongCurrentDuration = ActivityContext.FindViewById<TextView>(Resource.Id.tv_song_current_duration);
                TvSongTotalDuration = ActivityContext.FindViewById<TextView>(Resource.Id.tv_song_total_duration);
                TxtArtistAbout = ActivityContext.FindViewById<TextView>(Resource.Id.artist_about);

                if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                {
                    TxtArtistName = ActivityContext.FindViewById<TextView>(Resource.Id.artist_name);
                    ArtistImageView = ActivityContext.FindViewById<CircleImageView>(Resource.Id.image);
                    ImageCover = ActivityContext.FindViewById<ImageView>(Resource.Id.image_Cover);
                }
                else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                {
                    ToolbarCloseLayout = ActivityContext.FindViewById<LinearLayout>(Resource.Id.ToolbarCloseLayout);
                    ToolbarOpenLayout = ActivityContext.FindViewById<LinearLayout>(Resource.Id.ToolbarOpenLayout);
                    ImageToolbar = ActivityContext.FindViewById<ImageView>(Resource.Id.image_Toolbar);
                    ImageCover = ActivityContext.FindViewById<ImageView>(Resource.Id.image_Sound);
                    BtnNextImage = ActivityContext.FindViewById<ImageView>(Resource.Id.next_button);

                    CloseIcon = ActivityContext.FindViewById<ImageView>(Resource.Id.close_button);
                }

                BtnPlayImage = ActivityContext.FindViewById<ImageView>(Resource.Id.play_button);
                BackIcon = ActivityContext.FindViewById<ImageView>(Resource.Id.BackIcon);

                LinearAddTo = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_playlist);
                BtnIconAddTo = ActivityContext.FindViewById<ImageView>(Resource.Id.bottombar_addtoplay);
                LinearShare = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_share);
                BtnIconShare = ActivityContext.FindViewById<ImageView>(Resource.Id.bottombar_shareicon);
                LinearComments = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_comments);
                BtnIconComments = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_comments);
                LinearDownload = ActivityContext.FindViewById<RelativeLayout>(Resource.Id.ll_download);
                BtnIconDownload = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_download);
                ProgressBarDownload = ActivityContext.FindViewById<ProgressBar>(Resource.Id.progressBar);
                ProgressBarDownload.Visibility = ViewStates.Invisible;

                LinearFavorite = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_fav);
                BtnIconFavorite = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_fav);
                LinearLike = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_like);
                BtnIconLike = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_like);
                LinearDislike = ActivityContext.FindViewById<FrameLayout>(Resource.Id.ll_Dislike);
                BtnIconDislike = ActivityContext.FindViewById<ImageView>(Resource.Id.icon_Dislike);
                IconInfo = ActivityContext.FindViewById<ImageView>(Resource.Id.info);
                TvTitleSound = ActivityContext.FindViewById<TextView>(Resource.Id.titleSound);
                TvDescriptionSound = ActivityContext.FindViewById<TextView>(Resource.Id.descriptionSound);
                TxtPlaybackSpeed = ActivityContext.FindViewById<TextView>(Resource.Id.bt_playbackSpeed);

                BtnIconDownload.Tag = "Download";
                BtnSkipPrev.Tag = "no";
                BtnNext.Tag = "no";
                BtnRepeat.Tag = "no";
                BtnShuffle.Tag = "no";
                BtnBackward.Tag = "no";
                BtnForward.Tag = "no";

                if (!AppSettings.ShowForwardTrack)
                    BtnForward.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowBackwardTrack)
                    BtnBackward.Visibility = ViewStates.Gone;

                SetProgress();

                // set Event 
                if (!BtnPlayImage.HasOnClickListeners)
                {
                    BtnPlayImage.Click += BtPlayOnClick;
                    IconInfo.Click += IconInfoOnClick;

                    BtPlay.Click += BtPlayOnClick;
                    BackIcon.Click += BackIconOnClick;

                    BtnSkipPrev.Click += BtnSkipPrevOnClick;
                    BtnNext.Click += BtnNextOnClick;
                    BtnRepeat.Click += BtnRepeatOnClick;
                    BtnShuffle.Click += BtnShuffleOnClick;
                    BtnBackward.Click += BtnBackwardOnClick;
                    BtnForward.Click += BtnForwardOnClick;

                    LinearAddTo.Click += ShowMoreOptions;
                    LinearShare.Click += LinearShareOnClick;
                    LinearComments.Click += LinearCommentsOnClick;
                    LinearDownload.Click += LinearDownloadOnClick;
                    LinearFavorite.Click += LinearFavoriteOnClick;
                    LinearLike.Click += LinearLikeOnClick;
                    LinearDislike.Click += LinearDislikeOnClick;
                    TxtPlaybackSpeed.Click += TxtPlaybackSpeedOnClick;

                    if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                    {
                        BtnNextImage.Click += BtnNextOnClick;
                        CloseIcon.Click += BackIconOnClick;
                    }
                }
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
                if (Timer != null)
                {
                    Timer?.Stop();
                    Timer?.Dispose();
                }

                if (GlobalContext.SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Hidden)
                    GlobalContext.SlidingUpPanel?.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);

                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null) item.IsPlay = false;

                Adapter?.NotifyItemChanged(Constant.PlayPos);

                Constant.ArrayPlayingQueue = new ObservableCollection<SoundDataObject>();
                Constant.ArrayListPlay = new ObservableCollection<SoundDataObject>();

                if (Constant.Player == null)
                    return;

                if (Constant.Player.PlayWhenReady)
                {
                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    intent.SetAction(PlayerService.ActionStop);
                    ContextCompat.StartForegroundService(GlobalContext, intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event Click

        //Show info Data song 
        private void IconInfoOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    new DialogInfoSong(GlobalContext).Display(item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //click play Sound
        private void BtPlayOnClick(object sender, EventArgs e)
        {
            try
            {
                // check for already playing
                StartOrPausePlayer();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //click Back
        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                if (GlobalContext.SlidingUpPanel != null && (GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    GlobalContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else if (GlobalContext.SlidingUpPanel != null && GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    StopFragmentSound();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopFragmentSound()
        {
            try
            {
                if (GlobalContext.SlidingUpPanel != null && GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    intent.SetAction(PlayerService.ActionStop);
                    ContextCompat.StartForegroundService(GlobalContext, intent);

                    GlobalContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Shuffle Sound
        private void BtnShuffleOnClick(object sender, EventArgs e)
        {
            ToggleShuffleButton();
        }

        public void ToggleShuffleButton()
        {
            try
            {
                Constant.IsSuffle = !Constant.IsSuffle;
                ToggleButtonColor(BtnShuffle);

                if (BtnRepeat?.Tag?.ToString() == "selected")
                {
                    Constant.IsRepeat = false;
                    ToggleButtonColor(BtnRepeat); // clear 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Repeat Sound
        private void BtnRepeatOnClick(object sender, EventArgs e)
        {
            try
            {
                Constant.IsRepeat = !Constant.IsRepeat;
                ToggleButtonColor(BtnRepeat);

                if (BtnShuffle?.Tag?.ToString() == "selected")
                {
                    Constant.IsSuffle = !Constant.IsSuffle;
                    ToggleButtonColor(BtnShuffle);  // clear 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Forward 10 Sec
        private void BtnForwardOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionForward);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Backward 10 Sec
        private void BtnBackwardOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionBackward);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Run Next Sound
        private void BtnNextOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                    {
                        item.IsPlay = false;
                        Adapter?.NotifyItemChanged(Constant.PlayPos);

                        if (!Constant.IsOnline || Methods.CheckConnectivity())
                        {
                            Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                            intent.SetAction(PlayerService.ActionSkip);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") ||
                                                                                           item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                    {
                        item.IsPlay = false;
                        Adapter?.NotifyItemChanged(Constant.PlayPos);

                        Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                        intent.SetAction(PlayerService.ActionSkip);
                        ContextCompat.StartForegroundService(GlobalContext, intent);
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Skip Prev 
        private void BtnSkipPrevOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && item.AudioLocation.Contains("http"))
                    {
                        if (!Constant.IsOnline || Methods.CheckConnectivity())
                        {
                            Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                            intent.SetAction(PlayerService.ActionRewind);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                    else if (item != null && !string.IsNullOrEmpty(item.AudioLocation) && (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") ||
                                                                                           item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/")))
                    {
                        Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                        intent.SetAction(PlayerService.ActionRewind);
                        ContextCompat.StartForegroundService(GlobalContext, intent);
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Change Like
        private void LinearLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnLikeSongsClick(new MoreClickEventArgs { Button = BtnIconLike, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Change Dislike
        private void LinearDislikeOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnDislikeSongsClick(new MoreClickEventArgs { Button = BtnIconDislike, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add Or remove Favorite 
        private void LinearFavoriteOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnFavoriteSongsClick(new MoreClickEventArgs { Button = BtnIconFavorite, SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Comments
        private void LinearCommentsOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    new DialogComment(GlobalContext).Display(item, TvSongCurrentDuration.Text);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Download
        private void LinearDownloadOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        SetDownload();
                    }
                    else
                    {
                        if (PermissionsController.CheckPermissionStorage(GlobalContext))
                        {
                            SetDownload();
                        }
                        else
                        {
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                            {
                                ActivityCompat.RequestPermissions(GlobalContext, new[]
                                {
                                    Manifest.Permission.ReadMediaImages,
                                    Manifest.Permission.ReadMediaVideo,
                                    Manifest.Permission.ReadMediaAudio,
                                    Manifest.Permission.ReadExternalStorage,
                                }, 1005);
                            }
                            else if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                            {
                                //below android 11
                                ActivityCompat.RequestPermissions(GlobalContext, new[]
                                {
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                    Manifest.Permission.ManageExternalStorage,
                                    Manifest.Permission.AccessMediaLocation,
                                }, 1005);
                            }
                            else
                            {
                                ActivityCompat.RequestPermissions(GlobalContext, new[]
                                {
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                }, 1005);
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

        public void SetDownload()
        {
            try
            {
                Methods.Path.Chack_MyFolder();

                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    if (item.IsOwner != null && !item.IsOwner.Value && item.Price != 0 && item.IsPurchased != null && !item.IsPurchased.Value)
                    {
                        GlobalContext?.OpenDialogPurchaseSound(item);
                        return;
                    }

                    var filePath = SoundDownloadAsyncController.GetDownloadedDiskVideoUri(item.Title);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var dialog = new MaterialAlertDialogBuilder(ActivityContext);
                        dialog.SetTitle(Resource.String.Lbl_DeleteSong);
                        dialog.SetMessage(ActivityContext.GetText(Resource.String.Lbl_Do_You_want_to_remove_Song));
                        dialog.SetPositiveButton(ActivityContext.GetText(Resource.String.Lbl_Yes), (o, args) =>
                        {
                            try
                            {
                                SoundDownload = new SoundDownloadAsyncController(item.AudioLocation, item.Title, ActivityContext);
                                SoundDownload?.RemoveDiskSoundFile(item.Title, item.Id);

                                BtnIconDownload.Tag = "Download";
                                BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                                if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                                    BtnIconDownload.SetColorFilter(Color.White);
                                else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                                    BtnIconDownload.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialog.SetNegativeButton(ActivityContext.GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                        dialog.Show();
                    }
                    else
                    {
                        SoundDownload = new SoundDownloadAsyncController(item.AudioLocation, item.Title, ActivityContext);

                        if (!SoundDownload.CheckDownloadLinkIfExits())
                        {
                            SoundDownload.StartDownloadManager(item.Title, item, "Main");

                            ProgressBarDownload.Visibility = ViewStates.Visible;
                            BtnIconDownload.Visibility = ViewStates.Invisible;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Share
        private void LinearShareOnClick(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnShareClick(new MoreClickEventArgs { SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShowMoreOptions(object sender, EventArgs e)
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];
                if (item != null)
                {
                    ClickListeners.OnMoreClick(new MoreClickEventArgs { SongsClass = item });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Speed playback (1x, 1.5x, 2x) 
        private void TxtPlaybackSpeedOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionPlaybackSpeed);

                if (TxtPlaybackSpeed.Text == "1x")
                {
                    intent.PutExtra("PlaybackSpeed", "Medium");
                    TxtPlaybackSpeed.Text = "1.5x";
                }
                else if (TxtPlaybackSpeed.Text == "1.5x")
                {
                    intent.PutExtra("PlaybackSpeed", "High");
                    TxtPlaybackSpeed.Text = "2x";
                }
                else if (TxtPlaybackSpeed.Text == "2x")
                {
                    intent.PutExtra("PlaybackSpeed", "Normal");
                    TxtPlaybackSpeed.Text = "1x";
                }

                TxtPlaybackSpeed.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                ContextCompat.StartForegroundService(GlobalContext, intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Fun >> SeekBar

        // set Progress bar values
        public void SetProgress()
        {
            try
            {
                // Run timer
                Timer = new Timer
                {
                    Interval = 1000
                };
                Timer.Elapsed += TimerOnElapsed;

                SeekSongProgressbar.Max = MusicUtils.MaxProgress;
                SeekSongProgressbar.SetOnSeekBarChangeListener(this);

                if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                    TopSeekSongProgressbar.Max = MusicUtils.MaxProgress;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    SeekSongProgressbar.SetProgress(0, true);

                    if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                        TopSeekSongProgressbar.SetProgress(0, true);
                }
                else
                {
                    try
                    {
                        // For API < 24 
                        SeekSongProgressbar.Progress = 0;

                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            TopSeekSongProgressbar.Progress = 0;
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {

        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            try
            {
                if (Timer != null)
                {
                    // remove message Handler from updating progress bar
                    Timer.Enabled = false;
                    Timer.Stop();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                long progress = seekBar.Progress;

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionSeekTo);
                intent.PutExtra("seekTo", progress);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Fun Player

        public void StartPlaySound(SoundDataObject soundObject, ObservableCollection<SoundDataObject> listSound, RowSoundAdapter adapter = null, bool isSuffle = false)
        {
            try
            {
                Adapter = adapter;
                GlobalContext?.SetWakeLock();

                if (listSound.Count > 0)
                {
                    Constant.IsPlayed = false;
                    Constant.IsOnline = true;
                    Constant.ArrayListPlay = new ObservableCollection<SoundDataObject>(listSound);

                    if (Constant.ArrayPlayingQueue.Count > 0)
                    {
                        ListUtils.AddRange(Constant.ArrayListPlay, Constant.ArrayPlayingQueue);
                    }
                }

                if (soundObject != null)
                {
                    LoadSoundData(soundObject, false);

                    ReleaseSound();

                    if (isSuffle)
                    {
                        Constant.IsSuffle = false;
                        ToggleShuffleButton();
                    }
                    else
                    {
                        if (Constant.IsSuffle)
                            ToggleShuffleButton();  // clear 
                    }

                    //Play Song  
                    if (GlobalContext?.SlidingUpPanel != null)
                    {
                        StartOrPausePlayer();

                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            BackIcon.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);

                        BackIcon.Tag = "Open";

                        if (GlobalContext?.SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Collapsed)
                            GlobalContext?.SlidingUpPanel?.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleaseSound()
        {
            try
            {
                if (Constant.Player != null)
                {
                    if (Constant.Player.PlayWhenReady)
                    {
                        Constant.Player.Stop();
                        Constant.Player.Release();
                        Constant.Player = null;
                    }
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartOrPausePlayer()
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    if (Constant.ArrayListPlay.Count == 1)
                        Constant.PlayPos = 0;

                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item == null) return;

                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    if (Constant.IsPlayed)
                    {
                        if (Constant.Player.PlayWhenReady)
                        {
                            item.IsPlay = false;
                            intent.SetAction(PlayerService.ActionPause);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                        else
                        {
                            if (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") || item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/"))
                            {
                                item.IsPlay = true;
                                intent.SetAction(PlayerService.ActionPlay);
                                ContextCompat.StartForegroundService(GlobalContext, intent);
                            }
                            else
                            {
                                if (!Constant.IsOnline || Methods.CheckConnectivity())
                                {
                                    item.IsPlay = true;
                                    intent.SetAction(PlayerService.ActionPlay);
                                    ContextCompat.StartForegroundService(GlobalContext, intent);
                                }
                                else
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            } 
                        }
                    }
                    else
                    {
                        if (item.AudioLocation.Contains("file://") || item.AudioLocation.Contains("content://") || item.AudioLocation.Contains("storage") || item.AudioLocation.Contains("/data/user/0/"))
                        {
                            item.IsPlay = true;
                            intent.SetAction(PlayerService.ActionPlay);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                        else
                        {
                            if (!Constant.IsOnline || Methods.CheckConnectivity())
                            {
                                item.IsPlay = true;
                                intent.SetAction(PlayerService.ActionPlay);
                                ContextCompat.StartForegroundService(GlobalContext, intent);
                            }
                            else
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                            }
                        }
                    }

                    Adapter?.NotifyItemChanged(Constant.PlayPos);
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void LoadSoundData(SoundDataObject soundObject, bool isPlay)
        {
            try
            {
                GlobalContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (isPlay)
                        {
                            try
                            {
                                var list = Adapter?.SoundsList.Where(sound => sound.IsPlay).ToList();
                                if (list?.Count > 0)
                                    foreach (var all in list)
                                        all.IsPlay = false;

                                var item = Adapter?.GetItem(Constant.PlayPos);
                                if (item != null)
                                    item.IsPlay = true;

                                Adapter?.NotifyDataSetChanged();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        }

                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, soundObject.Thumbnail, ImageCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                            GlideImageLoader.LoadImage(ActivityContext, soundObject.Thumbnail, ArtistImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                            TxtArtistName.Text = Methods.FunString.DecodeString(soundObject.Publisher.Name);
                            TxtArtistAbout.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(soundObject.Title), 30);
                        }
                        else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, soundObject.Thumbnail, ImageCover, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                            GlideImageLoader.LoadImage(ActivityContext, soundObject.Thumbnail, ImageToolbar, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                            TxtArtistAbout.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(soundObject.Title), 22);
                        }

                        TvTitleSound.Text = Methods.FunString.DecodeString(soundObject.Title);

                        if (!AppSettings.ShowTitleAlbumOnly)
                        {
                            TvDescriptionSound.Text = string.IsNullOrEmpty(soundObject.AlbumName)
                             ? Methods.FunString.DecodeString(soundObject.CategoryName) + " " + ActivityContext.GetText(Resource.String.Lbl_Music)
                             : Methods.FunString.DecodeString(soundObject.CategoryName) + " " + ActivityContext.GetText(Resource.String.Lbl_Music) + ", " + ActivityContext.GetText(Resource.String.Lbl_InAlbum) + " " + Methods.FunString.DecodeString(soundObject.AlbumName);
                        }
                        else
                        {
                            TvDescriptionSound.Text = ActivityContext.GetText(Resource.String.Lbl_InAlbum) + " " + Methods.FunString.DecodeString(soundObject.AlbumName);
                        }

                        TvSongTotalDuration.Text = soundObject.Duration;
                        TvSongCurrentDuration.Text = "0:00";

                        BtnIconLike.Tag = soundObject.IsLiked != null && soundObject.IsLiked.Value ? "Like" : "Liked" ?? "Liked";
                        ClickListeners.SetLike(BtnIconLike);

                        BtnIconDislike.Tag = soundObject.IsDisLiked != null && soundObject.IsDisLiked.Value ? "Dislike" : "Disliked" ?? "Disliked";
                        ClickListeners.SetDislike(BtnIconDislike);

                        BtnIconFavorite.Tag = soundObject.IsFavoriated != null && soundObject.IsFavoriated.Value ? "Add" : "Added";
                        ClickListeners.SetFav(BtnIconFavorite);

                        //Add CreateCacheMediaSource if have or no 
                        //var fileSplit = soundObject.Id.Split('/').Last();

                        if (!DeepSoundTools.CheckAllowedDownloadFile())
                        {
                            LinearDownload.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            var canDownload = Constant.ShouldShowMusicPurchaseDialog();
                            if (soundObject.AllowDownloads == 0 && canDownload)
                            {
                                LinearDownload.Visibility = ViewStates.Gone;
                            }
                            else
                                LinearDownload.Visibility = ViewStates.Visible;
                        }

                        TxtPlaybackSpeed.Text = "1x";
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            TxtPlaybackSpeed.SetTextColor(Color.ParseColor("#999999"));
                        else
                            TxtPlaybackSpeed.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                        var sqlEntity = new SqLiteDatabase();
                        SoundDataObject dataSound = sqlEntity.Get_LatestDownloadsSound(soundObject.Id);
                        if (dataSound != null)
                        {
                            var filePath = SoundDownloadAsyncController.GetDownloadedDiskVideoUri(dataSound.Title);

                            if (!string.IsNullOrEmpty(filePath) && (filePath.Contains("file://") || filePath.Contains("content://") || filePath.Contains("storage") || filePath.Contains("/data/user/0/")))
                            {
                                BtnIconDownload.Tag = "Downloaded";
                                BtnIconDownload.SetImageResource(Resource.Drawable.ic_check_circle);
                                BtnIconDownload.SetColorFilter(Color.Red);
                            }
                            else
                            {
                                BtnIconDownload.Tag = "Download";
                                BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                                if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                                    BtnIconDownload.SetColorFilter(Color.White);
                                else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                                    BtnIconDownload.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                            }
                        }
                        else
                        {
                            BtnIconDownload.Tag = "Download";
                            BtnIconDownload.SetImageResource(Resource.Drawable.icon_player_download);
                            if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                                BtnIconDownload.SetColorFilter(Color.White);
                            else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                                BtnIconDownload.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                        }

                        ProgressBarDownload.Visibility = ViewStates.Invisible;
                        BtnIconDownload.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ChangePlayPauseIcon()
        {
            try
            {
                var item = Constant.ArrayListPlay[Constant.PlayPos];

                GlobalContext?.RunOnUiThread(() =>
                {
                    // check for already playing
                    if (Constant.Player != null && Constant.Player.PlayWhenReady)
                    {
                        // Changing button image to pause button
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                        {
                            BtPlay.SetImageResource(Resource.Drawable.icon_player_pause);
                            BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_pause);
                        }
                        else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                        {
                            BtPlay.SetImageResource(Resource.Drawable.icon_player2_pause);
                            BtnPlayImage.SetImageResource(Resource.Drawable.icon_player2_pause);
                        }

                        if (Timer != null)
                        {
                            Timer.Enabled = true;
                            Timer.Start();
                        }

                        if (item != null)
                        {
                            item.IsPlay = true;
                            Adapter?.NotifyItemChanged(Constant.PlayPos);
                        }
                    }
                    else
                    {
                        // Changing button image to play button
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                        {
                            BtPlay.SetImageResource(Resource.Drawable.icon_player_play);
                            BtnPlayImage.SetImageResource(Resource.Drawable.icon_player_play);
                        }
                        else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                        {
                            BtPlay.SetImageResource(Resource.Drawable.icon_player2_play);
                            BtnPlayImage.SetImageResource(Resource.Drawable.icon_player2_play);
                        }

                        if (Timer != null)
                        {
                            Timer.Enabled = false;
                            Timer.Stop();
                        }

                        if (item != null)
                        {
                            item.IsPlay = false;
                            Adapter?.NotifyItemChanged(Constant.PlayPos);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SeekUpdate()
        {
            try
            {
                ActivityContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (Constant.Player == null)
                        {
                            return;
                        }

                        var totalDuration = Constant.Player.Duration;
                        var currentDuration = Constant.Player.CurrentPosition;

                        if (PlayerService.GetPlayerService().ShouldShowMusicPurchaseDialog() && currentDuration >= 2)
                        {
                            PlayerService.GetPlayerService().ShowMusicPurchaseDialog();
                        }

                        // Displaying Total Duration time
                        TvSongTotalDuration.Text = MusicUtils.MilliSecondsToTimer(totalDuration);
                        // Displaying time completed playing
                        TvSongCurrentDuration.Text = MusicUtils.MilliSecondsToTimer(currentDuration);

                        // Updating progress bar
                        int progress = MusicUtils.GetProgressSeekBar(currentDuration, totalDuration);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        {
                            SeekSongProgressbar.SetProgress(progress, true);

                            if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                                TopSeekSongProgressbar.SetProgress(progress, true);
                        }
                        else
                        {
                            try
                            {
                                // For API < 24 
                                SeekSongProgressbar.Progress = progress;

                                if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                                    TopSeekSongProgressbar.Progress = progress;
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        }

                        if (currentDuration >= totalDuration)
                        {
                            if (Constant.IsRepeat)
                            {
                                Constant.Player.SeekTo(0);
                                Constant.Player.PlayWhenReady = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Runnable

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Running this thread after 10 milliseconds
                SeekUpdate();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Animation Image 

        public void RotateImageAlbum()
        {
            try
            {
                if (AppSettings.PlayerTheme != PlayerTheme.Theme1) return;
                if (Constant.Player != null && !Constant.Player.PlayWhenReady) return;
                ArtistImageView?.Animate()?.SetDuration(100)?.Rotation(ArtistImageView.Rotation + 2f)?.SetListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnAnimationCancel(Animator animation)
        {

        }

        public void OnAnimationEnd(Animator animation)
        {
            try
            {
                RotateImageAlbum();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnAnimationRepeat(Animator animation)
        {

        }

        public void OnAnimationStart(Animator animation)
        {

        }

        #endregion

        public void ToggleButtonColor(ImageButton bt)
        {
            try
            {
                if (bt != null)
                {
                    string selected = bt.Tag?.ToString();
                    if (selected == "selected")
                    {
                        // selected
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                            bt.SetColorFilter(Color.ParseColor("#999999"));
                        else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                            bt.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                        bt.Tag = "no";
                    }
                    else
                    {
                        bt.Tag = "selected";
                        bt.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetUiSliding(bool show)
        {
            try
            {
                var soundObject = Constant.ArrayListPlay[Constant.PlayPos];
                if (soundObject != null)
                {
                    if (show)
                    {
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                        {
                            IconInfo.Visibility = ViewStates.Gone;
                            BtnPlayImage.Visibility = ViewStates.Visible;
                            TopSeekSongProgressbar.Visibility = ViewStates.Visible;

                            TxtArtistAbout.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(soundObject.Title), 30);
                        }
                        else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                        {
                            ToolbarCloseLayout.Visibility = ViewStates.Visible;
                            ToolbarOpenLayout.Visibility = ViewStates.Gone;
                        }
                    }
                    else
                    {
                        if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                        {
                            IconInfo.Visibility = ViewStates.Visible;
                            BtnPlayImage.Visibility = ViewStates.Gone;
                            TopSeekSongProgressbar.Visibility = ViewStates.Invisible;

                            TxtArtistAbout.Text = soundObject.TimeFormatted;
                        }
                        else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                        {
                            ToolbarCloseLayout.Visibility = ViewStates.Gone;
                            ToolbarOpenLayout.Visibility = ViewStates.Visible;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}