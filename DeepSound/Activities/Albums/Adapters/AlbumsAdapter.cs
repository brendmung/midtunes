using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Albums.Adapters
{
    public class AlbumsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext;
        public event EventHandler<AlbumsAdapterClickEventArgs> ItemClick;
        public event EventHandler<AlbumsAdapterClickEventArgs> ItemLongClick;

        public ObservableCollection<DataAlbumsObject> AlbumsList = new ObservableCollection<DataAlbumsObject>();

        public AlbumsAdapter(Activity context)
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

        public override int ItemCount => AlbumsList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_AlbumsView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AlbumsView, parent, false);
                var vh = new AlbumsAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is not AlbumsAdapterViewHolder holder) return;

                var item = AlbumsList[position];

                if (item == null)
                    return;

                GlideImageLoader.LoadImage(ActivityContext, item.Thumbnail, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                holder.TxtTitle.Text = Methods.FunString.DecodeString(item.Title);

                if (Math.Abs(item.Price) > 0)
                {
                    holder.price.Visibility = ViewStates.Visible;
                    var currencySymbol = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";

                    var price = ListUtils.PriceList.FirstOrDefault(a => a.Id == item.Price)?.Price ?? item.Price.ToString();

                    holder.price.Text = currencySymbol + price;
                }

                var count = !string.IsNullOrEmpty(item.CountSongs) ? item.CountSongs : item.SongsCount ?? "0";
                holder.TxtCountSound.Text = count + " " + ActivityContext.GetText(Resource.String.Lbl_Songs);

                holder.TxtSecondaryText.Text = DeepSoundTools.GetNameFinal(item.Publisher ?? item.UserData);

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public DataAlbumsObject GetItem(int position)
        {
            return AlbumsList[position];
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

        private void OnClick(AlbumsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(AlbumsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = AlbumsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (!string.IsNullOrEmpty(item.Thumbnail))
                {
                    d.Add(item.Thumbnail);
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

    public class AlbumsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public ImageView Image { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtSecondaryText { get; private set; }
        public TextView TxtCountSound { get; private set; }
        public TextView price { get; private set; }

        #endregion

        public AlbumsAdapterViewHolder(View itemView, Action<AlbumsAdapterClickEventArgs> clickListener, Action<AlbumsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.imageSound);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.titleTextView);
                TxtSecondaryText = MainView.FindViewById<TextView>(Resource.Id.seconderyText);
                TxtCountSound = MainView.FindViewById<TextView>(Resource.Id.image_countSound);
                price = MainView.FindViewById<TextView>(Resource.Id.price);

                //Event
                itemView.Click += (sender, e) => clickListener(new AlbumsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Image = Image });
                itemView.LongClick += (sender, e) => longClickListener(new AlbumsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, Image = Image });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class AlbumsAdapterClickEventArgs : EventArgs
    {
        public ImageView Image { get; set; }
        public View View { get; set; }
        public int Position { get; set; }
    }
}