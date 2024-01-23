using Android.App;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Common;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Notification.Adapters
{
    public class NotificationsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<NotificationsAdapterClickEventArgs> ItemClick;
        public event EventHandler<NotificationsAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<NotificationsObject.Notifiation> NotificationsList = new ObservableCollection<NotificationsObject.Notifiation>();

        public NotificationsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Notifications_view
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_NotificationView, parent, false);
                var vh = new NotificationsAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is NotificationsAdapterViewHolder holder)
                {
                    var item = NotificationsList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.UserData?.UserDataClass.Avatar, holder.ImageUser, ImageStyle.RoundedCrop, ImagePlaceholders.DrawableUser);

                        holder.NotificationsText.Text = item.NType.Replace("_", " ").ToUpper();
                        holder.Time.Text = item.NTime;

                        if (item.NType == "your_song_is_ready")
                        {
                            holder.UserNameNoitfy.Text = AppSettings.ApplicationName;
                            Glide.With(ActivityContext?.BaseContext).Load(Resource.Mipmap.icon).Apply(new RequestOptions().CircleCrop()).Into(holder.ImageUser);
                        }
                        else
                        {
                            var name = DeepSoundTools.GetNameFinal(item.UserData?.UserDataClass);
                            string tempString;

                            switch (item.NType)
                            {
                                case "follow_user":
                                    {
                                        tempString = name + " " + ActivityContext.GetText(Resource.String.Lbl_FollowUser);
                                        break;
                                    }
                                case "liked_track":
                                    {
                                        tempString = name + " " + ActivityContext.GetText(Resource.String.Lbl_LikedTrack);
                                        break;
                                    }
                                case "liked_comment":
                                    {
                                        tempString = name + " " + ActivityContext.GetText(Resource.String.Lbl_LikedComment);
                                        break;
                                    }
                                case "purchased":
                                    {
                                        tempString = name + " " + ActivityContext.GetText(Resource.String.Lbl_PurchasedYourSong);
                                        break;
                                    }
                                case "approved_artist":
                                    {
                                        tempString = name + " " + ActivityContext.GetText(Resource.String.Lbl_ApprovedArtist);
                                        break;
                                    }
                                case "decline_artist":
                                    {
                                        tempString = name + " " + ActivityContext.GetText(Resource.String.Lbl_DeclineArtist);
                                        break;
                                    }
                                case "new_track":
                                    {
                                        tempString = name + " " + ActivityContext.GetText(Resource.String.Lbl_UploadNewTrack);
                                        break;
                                    }
                                default:
                                    tempString = name + " " + item.NText;
                                    break;
                            }

                            try
                            {
                                SpannableString spanString = new SpannableString(tempString);
                                spanString.SetSpan(new StyleSpan(TypefaceStyle.Bold), 0, name.Length, 0);

                                holder.UserNameNoitfy.SetText(spanString, TextView.BufferType.Spannable);
                            }
                            catch
                            {
                                holder.UserNameNoitfy.Text = tempString;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => NotificationsList?.Count ?? 0;

        public NotificationsObject.Notifiation GetItem(int position)
        {
            return NotificationsList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        void Click(NotificationsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(NotificationsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = NotificationsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (!string.IsNullOrEmpty(item.UserData?.UserDataClass?.Avatar))
                {
                    d.Add(item.UserData?.UserDataClass.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop());
        }
    }

    public class NotificationsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView ImageUser { get; private set; }
        public TextView UserNameNoitfy { get; private set; }
        public TextView NotificationsText { get; private set; }
        public TextView Time { get; private set; }

        #endregion

        public NotificationsAdapterViewHolder(View itemView, Action<NotificationsAdapterClickEventArgs> clickListener, Action<NotificationsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                ImageUser = (ImageView)MainView.FindViewById(Resource.Id.ImageUser);
                UserNameNoitfy = (TextView)MainView.FindViewById(Resource.Id.NotificationsName);
                NotificationsText = (TextView)MainView.FindViewById(Resource.Id.NotificationsText);
                Time = (TextView)MainView.FindViewById(Resource.Id.NotificationsTime);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Image = ImageUser });
                itemView.LongClick += (sender, e) => longClickListener(new NotificationsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Image = ImageUser });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class NotificationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public ImageView Image { get; set; }
    }
}