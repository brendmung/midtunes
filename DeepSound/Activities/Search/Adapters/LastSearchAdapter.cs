using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.User;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Search.Adapters
{
    public class LastSearchAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<LastSearchAdapterClickEventArgs> ItemClick;
        public event EventHandler<LastSearchAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<TrendSearchDataObject> KeywordsList = new ObservableCollection<TrendSearchDataObject>();

        public LastSearchAdapter(Activity context)
        {
            HasStableIds = true;
            ActivityContext = context;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_LastSearchView, parent, false);
                var vh = new LastSearchAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is LastSearchAdapterViewHolder holder)
                {
                    var item = KeywordsList[position];
                    if (item != null)
                    {
                        holder.KeywordText.Text = item.Keyword;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override int ItemCount => KeywordsList?.Count ?? 0;
        public TrendSearchDataObject GetItem(int position)
        {
            return KeywordsList[position];
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

        void OnClick(LastSearchAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
        void OnLongClick(LastSearchAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
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
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        }
    }

    public class LastSearchAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public TextView KeywordText { get; private set; }

        #endregion

        public LastSearchAdapterViewHolder(View itemView, Action<LastSearchAdapterClickEventArgs> clickListener, Action<LastSearchAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                KeywordText = MainView.FindViewById<TextView>(Resource.Id.text);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new LastSearchAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new LastSearchAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class LastSearchAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}