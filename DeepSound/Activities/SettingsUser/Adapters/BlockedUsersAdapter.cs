using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;

namespace DeepSound.Activities.SettingsUser.Adapters
{
    public class BlockedUsersAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<BlockedUsersAdapterClickEventArgs> ItemClick;
        public event EventHandler<BlockedUsersAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> BlockedUsersList = new ObservableCollection<UserDataObject>();

        public BlockedUsersAdapter(Activity context)
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
                //Setup your layout here >> Style_HContactView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContactView, parent, false);
                var vh = new BlockedUsersAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is BlockedUsersAdapterViewHolder holder)
                {
                    var item = BlockedUsersList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Name), 25);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public override int ItemCount
        {
            get
            {
                if (BlockedUsersList != null)
                {
                    return BlockedUsersList.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public UserDataObject GetItem(int position)
        {
            return BlockedUsersList[position];
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

        void Click(BlockedUsersAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(BlockedUsersAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = BlockedUsersList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
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
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        }
    }

    public class BlockedUsersAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public AppCompatButton Button { get; private set; }

        #endregion

        public BlockedUsersAdapterViewHolder(View itemView, Action<BlockedUsersAdapterClickEventArgs> clickListener, Action<BlockedUsersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.cont);

                Button.Visibility = ViewStates.Gone;

                //Event
                itemView.Click += (sender, e) => clickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new BlockedUsersAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class BlockedUsersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}