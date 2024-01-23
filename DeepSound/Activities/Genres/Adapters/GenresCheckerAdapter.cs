using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.User;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Genres.Adapters
{
    public class GenresCheckerAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<GenresCheckerAdapterClickEventArgs> ItemClick;
        public event EventHandler<GenresCheckerAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<GenresObject.DataGenres> GenresList = new ObservableCollection<GenresObject.DataGenres>();
        public List<int> AlreadySelectedGenres = new List<int>();
        private readonly RequestOptions Options;

        public GenresCheckerAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
                Options = new RequestOptions().Apply(RequestOptions.CircleCropTransform().AutoClone()
                    .CenterCrop()
                    .Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(30)))
                    .SetPriority(Priority.High)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All));
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
                //Setup your layout here >> Style_GenresSoundView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_GenresView, parent, false);
                var vh = new GenresCheckerAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is GenresCheckerAdapterViewHolder holder)
                {
                    var item = GenresList[position];
                    if (item != null)
                    {
                        holder.MainCardView.SetCardBackgroundColor(Color.ParseColor(item.Color));
                        if ((int)Build.VERSION.SdkInt >= 28)
                        {
                            holder.MainCardView.SetOutlineAmbientShadowColor(Color.ParseColor(item.Color));
                            holder.MainCardView.SetOutlineSpotShadowColor(Color.ParseColor(item.Color));
                        }

                        Glide.With(ActivityContext?.BaseContext).Load(item.BackgroundThumb).Apply(Options).Into(holder.GenresImage);

                        holder.TxtName.Text = item.CateogryName;

                        var selected = AlreadySelectedGenres.Contains(item.Id);
                        holder.Check.Visibility = selected ? ViewStates.Visible : ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => GenresList?.Count ?? 0;

        public GenresObject.DataGenres GetItem(int position)
        {
            return GenresList[position];
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

        void Click(GenresCheckerAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(GenresCheckerAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = GenresList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.BackgroundThumb != "")
                {
                    d.Add(item.BackgroundThumb);
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
                .Apply(new RequestOptions().CircleCrop());
        }
    }

    public class GenresCheckerAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public CardView MainCardView { get; private set; }
        public ImageView GenresImage { get; private set; }
        public TextView TxtName { get; private set; }
        public ImageView Check { get; private set; }

        #endregion

        public GenresCheckerAdapterViewHolder(View itemView, Action<GenresCheckerAdapterClickEventArgs> clickListener, Action<GenresCheckerAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                MainCardView = (CardView)MainView.FindViewById(Resource.Id.mainCardView);
                GenresImage = (ImageView)MainView.FindViewById(Resource.Id.image);
                TxtName = MainView.FindViewById<TextView>(Resource.Id.titleText);
                Check = MainView.FindViewById<ImageView>(Resource.Id.Check);

                //Event
                itemView.Click += (sender, e) => clickListener(new GenresCheckerAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new GenresCheckerAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class GenresCheckerAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}