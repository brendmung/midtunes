using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class NotificationsSettingsActivity : BaseActivity
    {
        #region Variables Basic

        private LinearLayout FollowUserLayout, LikedTrackLayout, ReviewedTrackLayout, LikedCommentLayout, ArtistStatusChangedLayout, ReceiptStatusChangedLayout, NewTrackLayout, CommentMentionLayout, CommentReplayMentionLayout;
        private Switch SwitchFollowUser, SwitchLikedTrack, SwitchReviewedTrack, SwitchLikedComment, SwitchArtistStatusChanged, SwitchReceiptStatusChanged, SwitchNewTrack, SwitchCommentMention, SwitchCommentReplayMention;
        private string EmailOnFollowUserPref, EmailOnLikedTrackPref, EmailOnReviewedTrackPref, EmailOnLikedCommentPref, EmailOnArtistStatusChangedPref, EmailOnReceiptStatusChangedPref, EmailOnNewTrackPref, EmailOnCommentMentionPref, EmailOnCommentReplayMentionPref;

        private Toolbar Toolbar;
        private AdManagerAdView AdManagerAdView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Methods.App.FullScreenApp(this);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                // Create your application here
                SetContentView(Resource.Layout.SettingsNotificationsLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                GetMyInfoData();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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

        protected override void OnDestroy()
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

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                FollowUserLayout = FindViewById<LinearLayout>(Resource.Id.follow_userLayout);
                SwitchFollowUser = FindViewById<Switch>(Resource.Id.Switch_follow_user);

                LikedTrackLayout = FindViewById<LinearLayout>(Resource.Id.liked_trackLayout);
                SwitchLikedTrack = FindViewById<Switch>(Resource.Id.Switch_liked_track);

                ReviewedTrackLayout = FindViewById<LinearLayout>(Resource.Id.reviewed_trackLayout);
                SwitchReviewedTrack = FindViewById<Switch>(Resource.Id.Switch_reviewed_track);

                LikedCommentLayout = FindViewById<LinearLayout>(Resource.Id.liked_commentLayout);
                SwitchLikedComment = FindViewById<Switch>(Resource.Id.Switch_liked_comment);

                ArtistStatusChangedLayout = FindViewById<LinearLayout>(Resource.Id.artist_status_changedLayout);
                SwitchArtistStatusChanged = FindViewById<Switch>(Resource.Id.Switch_artist_status_changed);

                ReceiptStatusChangedLayout = FindViewById<LinearLayout>(Resource.Id.receipt_status_changedLayout);
                SwitchReceiptStatusChanged = FindViewById<Switch>(Resource.Id.Switch_receipt_status_changed);

                NewTrackLayout = FindViewById<LinearLayout>(Resource.Id.new_trackLayout);
                SwitchNewTrack = FindViewById<Switch>(Resource.Id.Switch_new_track);

                CommentMentionLayout = FindViewById<LinearLayout>(Resource.Id.comment_mentionLayout);
                SwitchCommentMention = FindViewById<Switch>(Resource.Id.Switch_comment_mention);

                CommentReplayMentionLayout = FindViewById<LinearLayout>(Resource.Id.comment_replay_mentionLayout);
                SwitchCommentReplayMention = FindViewById<Switch>(Resource.Id.Switch_comment_replay_mention);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = GetString(Resource.String.Lbl_Notifications);
                    Toolbar.SetTitleTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);

                }
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
                    SwitchFollowUser.CheckedChange += SwitchFollowUserOnCheckedChange;
                    SwitchLikedTrack.CheckedChange += SwitchLikedTrackOnCheckedChange;
                    SwitchLikedComment.CheckedChange += SwitchLikedCommentOnCheckedChange;
                    SwitchReviewedTrack.CheckedChange += SwitchReviewedTrackOnCheckedChange;
                    SwitchArtistStatusChanged.CheckedChange += SwitchArtistStatusChangedOnCheckedChange;
                    SwitchReceiptStatusChanged.CheckedChange += SwitchReceiptStatusChangedOnCheckedChange;
                    SwitchNewTrack.CheckedChange += SwitchNewTrackOnCheckedChange;
                    SwitchCommentMention.CheckedChange += SwitchCommentMentionOnCheckedChange;
                    SwitchCommentReplayMention.CheckedChange += SwitchCommentReplayMentionOnCheckedChange;
                }
                else
                {
                    SwitchFollowUser.CheckedChange -= SwitchFollowUserOnCheckedChange;
                    SwitchLikedTrack.CheckedChange -= SwitchLikedTrackOnCheckedChange;
                    SwitchLikedComment.CheckedChange -= SwitchLikedCommentOnCheckedChange;
                    SwitchReviewedTrack.CheckedChange -= SwitchReviewedTrackOnCheckedChange;
                    SwitchArtistStatusChanged.CheckedChange -= SwitchArtistStatusChangedOnCheckedChange;
                    SwitchReceiptStatusChanged.CheckedChange -= SwitchReceiptStatusChangedOnCheckedChange;
                    SwitchNewTrack.CheckedChange -= SwitchNewTrackOnCheckedChange;
                    SwitchCommentMention.CheckedChange -= SwitchCommentMentionOnCheckedChange;
                    SwitchCommentReplayMention.CheckedChange -= SwitchCommentReplayMentionOnCheckedChange;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void SwitchFollowUserOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnFollowUser = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnFollowUserPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnFollowUser = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnFollowUserPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_follow_user", EmailOnFollowUserPref},

                        {"email_on_liked_track", EmailOnLikedTrackPref},
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                        {"email_on_receipt_status_changed",EmailOnReceiptStatusChangedPref },
                        {"email_on_new_track",EmailOnNewTrackPref },
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },
                        {"email_on_comment_replay_mention",EmailOnCommentReplayMentionPref },
                        {"email_on_comment_mention",EmailOnCommentMentionPref },
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchLikedTrackOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnLikedTrack = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnLikedTrackPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnLikedTrack = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnLikedTrackPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_liked_track", EmailOnLikedTrackPref},

                        {"email_on_follow_user", EmailOnFollowUserPref},
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                        {"email_on_receipt_status_changed",EmailOnReceiptStatusChangedPref },
                        {"email_on_new_track",EmailOnNewTrackPref },
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },
                        {"email_on_comment_replay_mention",EmailOnCommentReplayMentionPref },
                        {"email_on_comment_mention",EmailOnCommentMentionPref },
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchReviewedTrackOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnReviewedTrack = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnReviewedTrackPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnReviewedTrack = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnReviewedTrackPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },

                        {"email_on_follow_user", EmailOnFollowUserPref},
                        {"email_on_liked_track", EmailOnLikedTrackPref},
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                        {"email_on_receipt_status_changed",EmailOnReceiptStatusChangedPref },
                        {"email_on_new_track",EmailOnNewTrackPref },
                        {"email_on_comment_replay_mention",EmailOnCommentReplayMentionPref },
                        {"email_on_comment_mention",EmailOnCommentMentionPref },
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchLikedCommentOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnLikedComment = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnLikedCommentPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnLikedComment = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnLikedCommentPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_liked_comment", EmailOnLikedCommentPref},

                        {"email_on_follow_user", EmailOnFollowUserPref},
                        {"email_on_liked_track", EmailOnLikedTrackPref},
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                        {"email_on_receipt_status_changed",EmailOnReceiptStatusChangedPref },
                        {"email_on_new_track",EmailOnNewTrackPref },
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },
                        {"email_on_comment_replay_mention",EmailOnCommentReplayMentionPref },
                        {"email_on_comment_mention",EmailOnCommentMentionPref },
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchArtistStatusChangedOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnArtistStatusChanged = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnArtistStatusChangedPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnArtistStatusChanged = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnArtistStatusChangedPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},

                        {"email_on_follow_user", EmailOnFollowUserPref},
                        {"email_on_liked_track", EmailOnLikedTrackPref},
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                        {"email_on_receipt_status_changed",EmailOnReceiptStatusChangedPref },
                        {"email_on_new_track",EmailOnNewTrackPref },
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },
                        {"email_on_comment_replay_mention",EmailOnCommentReplayMentionPref },
                        {"email_on_comment_mention",EmailOnCommentMentionPref },
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchReceiptStatusChangedOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnReceiptStatusChanged = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnReceiptStatusChangedPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnReceiptStatusChanged = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnReceiptStatusChangedPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_receipt_status_changed", EmailOnReceiptStatusChangedPref},

                        {"email_on_follow_user", EmailOnFollowUserPref},
                        {"email_on_liked_track", EmailOnLikedTrackPref},
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                        {"email_on_new_track",EmailOnNewTrackPref },
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },
                        {"email_on_comment_replay_mention",EmailOnCommentReplayMentionPref },
                        {"email_on_comment_mention",EmailOnCommentMentionPref },

                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchNewTrackOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnNewTrack = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnNewTrackPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnNewTrack = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnNewTrackPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_new_track", EmailOnNewTrackPref},

                        {"email_on_follow_user", EmailOnFollowUserPref},
                        {"email_on_liked_track", EmailOnLikedTrackPref},
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                        {"email_on_receipt_status_changed",EmailOnReceiptStatusChangedPref },
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },
                        {"email_on_comment_replay_mention",EmailOnCommentReplayMentionPref },
                        {"email_on_comment_mention",EmailOnCommentMentionPref },
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchCommentMentionOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnCommentMention = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnCommentMentionPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnCommentMention = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnCommentMentionPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_comment_mention", EmailOnCommentMentionPref},

                        {"email_on_follow_user", EmailOnFollowUserPref},
                        {"email_on_liked_track", EmailOnLikedTrackPref},
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                        {"email_on_receipt_status_changed",EmailOnReceiptStatusChangedPref },
                        {"email_on_new_track",EmailOnNewTrackPref },
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },
                        {"email_on_comment_replay_mention",EmailOnCommentReplayMentionPref },
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchCommentReplayMentionOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                switch (e.IsChecked)
                {
                    //Yes >> value = 1
                    case true:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnCommentReplayMention = 1;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);

                            }

                            EmailOnCommentReplayMentionPref = "1";
                            break;
                        }
                    //No >> value = 0
                    default:
                        {
                            if (dataUser != null)
                            {
                                dataUser.EmailOnCommentReplayMention = 0;
                                var sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            EmailOnCommentReplayMentionPref = "0";
                            break;
                        }
                }

                if (Methods.CheckConnectivity())
                {
                    var dataNotification = new Dictionary<string, string>
                    {
                        {"email_on_comment_replay_mention", EmailOnCommentReplayMentionPref},


                        {"email_on_follow_user", EmailOnFollowUserPref},
                        {"email_on_liked_track", EmailOnLikedTrackPref},
                        {"email_on_liked_comment", EmailOnLikedCommentPref},
                        {"email_on_artist_status_changed", EmailOnArtistStatusChangedPref},
                        {"email_on_receipt_status_changed",EmailOnReceiptStatusChangedPref },
                        {"email_on_new_track",EmailOnNewTrackPref },
                        {"email_on_reviewed_track",EmailOnReviewedTrackPref },
                        {"email_on_comment_mention",EmailOnCommentMentionPref },
                    };

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.UpdateNotificationSettingAsync(UserDetails.UserId.ToString(), dataNotification) });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void GetMyInfoData()
        {
            try
            {
                if (ListUtils.MyUserInfoList?.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                }

                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (dataUser != null)
                {
                    //====================
                    if (dataUser.EmailOnFollowUser == 1)
                    {
                        SwitchFollowUser.Checked = true;
                        EmailOnFollowUserPref = "1";
                    }
                    else
                    {
                        SwitchFollowUser.Checked = false;
                        EmailOnFollowUserPref = "0";
                    }
                    //====================

                    //====================
                    if (dataUser.EmailOnLikedTrack == 1)
                    {
                        SwitchLikedTrack.Checked = true;
                        EmailOnLikedTrackPref = "1";
                    }
                    else
                    {
                        SwitchLikedTrack.Checked = false;
                        EmailOnLikedTrackPref = "0";
                    }
                    //====================

                    //====================
                    if (dataUser.EmailOnReviewedTrack == 1)
                    {
                        SwitchReviewedTrack.Checked = true;
                        EmailOnReviewedTrackPref = "1";
                    }
                    else
                    {
                        SwitchReviewedTrack.Checked = false;
                        EmailOnReviewedTrackPref = "0";
                    }
                    //====================

                    //====================
                    if (dataUser.EmailOnLikedComment == 1)
                    {
                        SwitchLikedComment.Checked = true;
                        EmailOnLikedCommentPref = "1";
                    }
                    else
                    {
                        SwitchLikedComment.Checked = false;
                        EmailOnLikedCommentPref = "0";
                    }
                    //====================

                    //====================
                    if (dataUser.EmailOnArtistStatusChanged == 1)
                    {
                        SwitchArtistStatusChanged.Checked = true;
                        EmailOnArtistStatusChangedPref = "1";
                    }
                    else
                    {
                        SwitchArtistStatusChanged.Checked = false;
                        EmailOnArtistStatusChangedPref = "0";
                    }
                    //====================

                    //====================
                    if (dataUser.EmailOnReceiptStatusChanged == 1)
                    {
                        SwitchReceiptStatusChanged.Checked = true;
                        EmailOnReceiptStatusChangedPref = "1";
                    }
                    else
                    {
                        SwitchReceiptStatusChanged.Checked = false;
                        EmailOnReceiptStatusChangedPref = "0";
                    }
                    //====================

                    //====================
                    if (dataUser.EmailOnNewTrack == 1)
                    {
                        SwitchNewTrack.Checked = true;
                        EmailOnNewTrackPref = "1";
                    }
                    else
                    {
                        SwitchNewTrack.Checked = false;
                        EmailOnNewTrackPref = "0";
                    }
                    //====================

                    //====================
                    if (dataUser.EmailOnCommentMention == 1)
                    {
                        SwitchCommentMention.Checked = true;
                        EmailOnCommentMentionPref = "1";
                    }
                    else
                    {
                        SwitchCommentMention.Checked = false;
                        EmailOnCommentMentionPref = "0";
                    }
                    //====================

                    //====================
                    if (dataUser.EmailOnCommentReplayMention == 1)
                    {
                        SwitchCommentReplayMention.Checked = true;
                        EmailOnCommentReplayMentionPref = "1";
                    }
                    else
                    {
                        SwitchCommentReplayMention.Checked = false;
                        EmailOnCommentReplayMentionPref = "0";
                    }
                    //==================== 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}