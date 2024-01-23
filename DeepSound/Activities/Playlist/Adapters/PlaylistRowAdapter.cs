using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Activities.Library.Listeners;
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
    public class PlaylistRowAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<PlaylistRowAdapterClickEventArgs> ItemClick;
        public event EventHandler<PlaylistRowAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PlaylistDataObject> PlaylistList = new ObservableCollection<PlaylistDataObject>();
        private readonly LibrarySynchronizer ClickListeners;

        public PlaylistRowAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                ClickListeners = new LibrarySynchronizer(context);
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
                if (viewType == 9999999)
                {
                    //Setup your layout here >> ViewModel_AddNew 
                    View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_AddNew, parent, false);
                    var vh = new AddNewModelAdapterViewHolder(itemView, Click, LongClick);
                    return vh;
                }
                else
                {
                    //Setup your layout here >> Style_PlaylistRowView 
                    View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PlaylistRowView, parent, false);
                    var vh = new PlaylistRowAdapterViewHolder(itemView, Click, LongClick);
                    return vh;
                }
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
                var item = PlaylistList[position];
                if (item != null)
                {
                    if (viewHolder is AddNewModelAdapterViewHolder addHolder)
                    {
                        addHolder.TxtName.Text = ActivityContext.GetText(Resource.String.Lbl_AddNewPlaylist);
                    }
                    else if (viewHolder is PlaylistRowAdapterViewHolder holder)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.ThumbnailReady, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        holder.TxtName.Text = Methods.FunString.DecodeString(item.Name);
                        holder.TxtCountSongs.Text = item.Songs + " " + ActivityContext.GetText(Resource.String.Lbl_Songs);

                        if (!holder.MoreButton.HasOnClickListeners)
                            holder.MoreButton.Click += (sender, e) => ClickListeners.PlaylistMoreOnClick(item);
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
                var item = GetItem(position);
                if (item?.Name == ActivityContext.GetText(Resource.String.Lbl_AddNewPlaylist))
                {
                    return 9999999;
                }

                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        void Click(PlaylistRowAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(PlaylistRowAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

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

    public class PlaylistRowAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView Image { get; private set; }
        public TextView TxtName { get; private set; }
        public TextView TxtCountSongs { get; private set; }
        public ImageButton MoreButton { get; private set; }

        #endregion

        public PlaylistRowAdapterViewHolder(View itemView, Action<PlaylistRowAdapterClickEventArgs> clickListener, Action<PlaylistRowAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.image);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.name);
                TxtCountSongs = MainView.FindViewById<TextView>(Resource.Id.count);
                MoreButton = MainView.FindViewById<ImageButton>(Resource.Id.more);

                //Event
                itemView.Click += (sender, e) => clickListener(new PlaylistRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new PlaylistRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class AddNewModelAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView Icon { get; private set; }
        public TextView TxtName { get; private set; }

        #endregion

        public AddNewModelAdapterViewHolder(View itemView, Action<PlaylistRowAdapterClickEventArgs> clickListener, Action<PlaylistRowAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values 
                Icon = (ImageView)MainView.FindViewById(Resource.Id.icon);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.name);

                //Event
                itemView.Click += (sender, e) => clickListener(new PlaylistRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new PlaylistRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class PlaylistRowAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}