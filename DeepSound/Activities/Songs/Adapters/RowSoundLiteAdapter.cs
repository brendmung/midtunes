using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Songs.Adapters
{
    public class RowSoundLiteAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<RowSoundLiteAdapterClickEventArgs> ItemClick;
        public event EventHandler<RowSoundLiteAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<SoundDataObject> SoundsList = new ObservableCollection<SoundDataObject>();
        private readonly SocialIoClickListeners ClickListeners;
        private readonly string NamePage;
        public RowSoundLiteAdapter(Activity context, string namePage)
        {
            try
            {
                ActivityContext = context;
                NamePage = namePage;
                HasStableIds = true;
                ClickListeners = new SocialIoClickListeners(context);
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
                //Setup your layout here >> Style_RowSongsPlaylistView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_RowSongsLiteView, parent, false);
                var vh = new RowSoundLiteAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is RowSoundLiteAdapterViewHolder holder)
                {
                    var item = SoundsList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Thumbnail, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        holder.TxtTitle.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Title), 25);

                        string seconderText;
                        if (item.Publisher != null)
                            seconderText = DeepSoundTools.GetNameFinal(item.Publisher);
                        else
                            seconderText = item.CategoryName + " " + ActivityContext.GetText(Resource.String.Lbl_Music);

                        if (item.Src == "radio")
                        {

                        }
                        else
                            seconderText += " |  " + item.Duration + " " + ActivityContext.GetText(Resource.String.Lbl_CutMinutes);

                        holder.TxtSeconderText.Text = seconderText;

                        holder.IconHeart.Tag = item.IsLiked != null && item.IsLiked.Value ? "Like" : "Liked";
                        ClickListeners.SetLike(holder.IconHeart);
                        //SetLike(holder.IconHeart);

                        if (!holder.IconHeart.HasOnClickListeners)
                            holder.IconHeart.Click += (s, e) => ClickListeners.OnLikeSongsClick(new MoreClickEventArgs { Button = holder.IconHeart, SongsClass = item }, NamePage);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => SoundsList?.Count ?? 0;

        public SoundDataObject GetItem(int position)
        {
            return SoundsList[position];
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

        void Click(RowSoundLiteAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(RowSoundLiteAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = SoundsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Thumbnail != "")
                {
                    d.Add(item.Thumbnail);
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

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString())
                .Apply(new RequestOptions().CenterCrop());
        }
    }

    public class RowSoundLiteAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public ImageView Image { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtSeconderText { get; private set; }
        public ImageView IconHeart { get; private set; }



        #endregion

        public RowSoundLiteAdapterViewHolder(View itemView, Action<RowSoundLiteAdapterClickEventArgs> clickListener, Action<RowSoundLiteAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.title);
                TxtSeconderText = MainView.FindViewById<TextView>(Resource.Id.brief);

                IconHeart = MainView.FindViewById<ImageView>(Resource.Id.heart);



                //Event
                itemView.Click += (sender, e) => clickListener(new RowSoundLiteAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new RowSoundLiteAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class RowSoundLiteAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}