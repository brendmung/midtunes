using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Story;
using ImageViews.Rounded;
using Java.Util;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Story.Adapters
{
    public class StoryAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<StoryAdapterClickEventArgs> ItemClick;
        public event EventHandler<StoryAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<StoryDataObject> StoryList = new ObservableCollection<StoryDataObject>();

        public StoryAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
              
                if (UserDetails.IsLogin)
                {
                    var dataOwner = StoryList.FirstOrDefault(a => a.Type == "Your");
                    if (dataOwner == null)
                    {
                        var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                        StoryList.Insert(0, new StoryDataObject
                        {
                            Type = "Your",
                            UserData = dataUser
                        });
                    }
                } 
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
                //Setup your layout here >> Style_StoryView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_StoryView, parent, false);
                var vh = new StoryAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is StoryAdapterViewHolder holder)
                {
                    var item = StoryList[position];
                    if (item != null)
                    {
                        if (item.Type == "Your")
                        {
                            GlideImageLoader.LoadImage(ActivityContext, UserDetails.Avatar, holder.RoundImage, ImageStyle.RoundedCrop, ImagePlaceholders.DrawableUser);
                            GlideImageLoader.LoadImage(ActivityContext, UserDetails.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                            holder.Name.Text = ActivityContext.GetText(Resource.String.Lbl_CreateStory);
                        }
                        else
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Image, holder.RoundImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                            GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                            holder.Name.Text = Methods.FunString.SubStringCutOf(DeepSoundTools.GetNameFinal(item.UserData), 12);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => StoryList?.Count ?? 0;

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

        void Click(StoryAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(StoryAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = StoryList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Image != "")
                {
                    d.Add(item.Image);
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

    public class StoryAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        private View MainView { get; set; }
        public RoundedImageView RoundImage { get; set; }
        public CircleImageView Image { get; set; }
        public TextView Name { get; private set; }

        #endregion

        public StoryAdapterViewHolder(View itemView, Action<StoryAdapterClickEventArgs> clickListener, Action<StoryAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                RoundImage = MainView.FindViewById<RoundedImageView>(Resource.Id.iv_round_story);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.civ_story_avatar);
                Name = MainView.FindViewById<TextView>(Resource.Id.Txt_Username);

                //Event
                itemView.Click += (sender, e) => clickListener(new StoryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new StoryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class StoryAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }

    }
}