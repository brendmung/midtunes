using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.User;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Library.Adapters
{
    public class MyPurchasesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MyPurchasesAdapterClickEventArgs> ItemClick;
        public event EventHandler<MyPurchasesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PurchasesDataObject> PurchasesList = new ObservableCollection<PurchasesDataObject>();

        public MyPurchasesAdapter(Activity context)
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
                //Setup your layout here >> Style_PurchasesView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_PurchasesView, parent, false);
                var vh = new MyPurchasesAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is MyPurchasesAdapterViewHolder holder)
                {
                    var item = PurchasesList[position];
                    if (item != null)
                    {
                        if (item.Event != null)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Event.Image, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                            holder.TxtType.Text = ActivityContext.GetText(Resource.String.Lbl_Event);
                        }
                        else if (item.SongData?.SongClass != null)
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.SongData?.SongClass.Thumbnail, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                            holder.TxtType.Text = ActivityContext.GetText(Resource.String.Lbl_Songs);
                        }

                        holder.TxtTitle.Text = Methods.FunString.DecodeString(item.Title);
                        holder.TxtSeconderyText.Text = ActivityContext.GetText(Resource.String.Lbl_Price) + ": $" + Convert.ToDouble(item.Price).ToString("F") + " - " + ActivityContext.GetText(Resource.String.Lbl_Date) + ": " + item.Timestamp;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => PurchasesList?.Count ?? 0;

        public PurchasesDataObject GetItem(int position)
        {
            return PurchasesList[position];
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

        void Click(MyPurchasesAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(MyPurchasesAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PurchasesList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Event != null)
                {
                    d.Add(item.Event.Image);
                }
                else if (item.SongData?.SongClass != null)
                {
                    d.Add(item.SongData?.SongClass.Thumbnail);
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

    public class MyPurchasesAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView Image { get; private set; }
        public TextView TxtType { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtSeconderyText { get; private set; }

        #endregion

        public MyPurchasesAdapterViewHolder(View itemView, Action<MyPurchasesAdapterClickEventArgs> clickListener, Action<MyPurchasesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.Image);
                TxtType = MainView.FindViewById<TextView>(Resource.Id.type);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.title);
                TxtSeconderyText = MainView.FindViewById<TextView>(Resource.Id.description);

                //Event
                itemView.Click += (sender, e) => clickListener(new MyPurchasesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new MyPurchasesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class MyPurchasesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}