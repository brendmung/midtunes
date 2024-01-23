using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Playlist;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Playlist.Adapters
{
    public class PlaylistAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<PlaylistAdapterClickEventArgs> ItemClick;
        public event EventHandler<PlaylistAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PlaylistDataObject> PlaylistList = new ObservableCollection<PlaylistDataObject>();

        public PlaylistAdapter(Activity context)
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
                //Setup your layout here >> Style_PlaylistView 
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PlaylistView, parent, false);
                var vh = new PlaylistAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is PlaylistAdapterViewHolder holder)
                {
                    var item = PlaylistList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.ThumbnailReady, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        holder.TxtName.Text = Methods.FunString.DecodeString(item.Name);
                        //holder.TxtCountSongs.Text = item.Songs + " " + ActivityContext.GetText(Resource.String.Lbl_Songs);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => PlaylistList?.Count ?? 0;

        public PlaylistDataObject GetItem(int position)
        {
            return PlaylistList[position];
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

        void Click(PlaylistAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(PlaylistAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PlaylistList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.ThumbnailReady != "")
                {
                    d.Add(item.ThumbnailReady);
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

    public class PlaylistAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView Image { get; private set; }
        public TextView TxtName { get; private set; }

        #endregion

        public PlaylistAdapterViewHolder(View itemView, Action<PlaylistAdapterClickEventArgs> clickListener, Action<PlaylistAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.image);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.name);

                //Event
                itemView.Click += (sender, e) => clickListener(new PlaylistAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new PlaylistAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class PlaylistAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}