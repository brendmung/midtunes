using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo;
using DeepSoundClient.Classes.Global;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace DeepSound.Activities.Songs.Adapters
{
    public class RowSoundAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext;
        public event EventHandler<RowSoundAdapterClickEventArgs> ItemClick;
        public event EventHandler<RowSoundAdapterClickEventArgs> ItemLongClick;

        public ObservableCollection<SoundDataObject> SoundsList = new ObservableCollection<SoundDataObject>();
        private readonly SocialIoClickListeners ClickListeners;
        private readonly string NamePage;
        public RowSoundAdapter(Activity context, string namePage)
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

        public override int ItemCount => SoundsList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_SongView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SongView, parent, false);
                var vh = new RowSoundAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    var item = SoundsList[position];
                    switch (payloads[0].ToString())
                    {
                        case "playerAction":
                            {
                                if (viewHolder is not RowSoundAdapterViewHolder holder) return;

                                ClickListeners.SetPlaySongs(holder.PlayButton, item.IsPlay);

                                if (item.IsPlay)
                                {
                                    holder.Image.Visibility = ViewStates.Gone;
                                    holder.CardViewImage.Visibility = ViewStates.Gone;
                                    holder.Equalizer.Visibility = ViewStates.Visible;
                                    holder.Equalizer.AnimateBars();
                                }
                                else
                                {
                                    holder.Image.Visibility = ViewStates.Visible;
                                    holder.CardViewImage.Visibility = ViewStates.Visible;
                                    holder.Equalizer.Visibility = ViewStates.Gone;
                                    holder.Equalizer.StopBars();
                                }
                                //NotifyItemChanged(position);
                                break;
                            }
                        default:
                            base.OnBindViewHolder(viewHolder, position, payloads);
                            break;
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is not RowSoundAdapterViewHolder holder) return;

                var item = SoundsList[position];
                if (item == null)
                    return;

                holder.CountItemTextView.Text = position.ToString("D2");

                //Glide.With(ActivityContext?.BaseContext).Load(item.Thumbnail).Apply(GlideRequestOptions).Transition(DrawableTransitionOptions.WithCrossFade(400)).Into(holder.Image);
                GlideImageLoader.LoadImage(ActivityContext, item.Thumbnail, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                holder.TxtSongName.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Title), 80);

                if (item.Publisher != null)
                    holder.TxtGenresName.Text = DeepSoundTools.GetNameFinal(item.Publisher);
                else
                    holder.TxtGenresName.Text = item.CategoryName + " " + ActivityContext.GetText(Resource.String.Lbl_Music);

                //holder.CountLike.Text = item.CountLikes.ToString();
                //holder.CountStars.Text = item.CountFavorite.ToString(); 
                //holder.CountViews.Text = item.CountViews;
                //holder.CountShare.Text = item.CountShares.ToString();
                //holder.CountComment.Text = item.CountComment.ToString();

                if (item.Src == "radio")
                {
                    holder.TxtSongDuration.Visibility = ViewStates.Invisible;
                    holder.ImageSeperator.Visibility = ViewStates.Invisible;
                }
                else
                    holder.TxtSongDuration.Text = item.Duration + " " + ActivityContext.GetText(Resource.String.Lbl_CutMinutes);

                ClickListeners.SetPlaySongs(holder.PlayButton, item.IsPlay);

                if (item.IsPlay)
                {
                    holder.Image.Visibility = ViewStates.Gone;
                    holder.CardViewImage.Visibility = ViewStates.Gone;
                    holder.Equalizer.Visibility = ViewStates.Visible;
                    holder.Equalizer.AnimateBars();
                }
                else
                {

                    holder.Image.Visibility = ViewStates.Visible;
                    holder.CardViewImage.Visibility = ViewStates.Visible;
                    holder.Equalizer.Visibility = ViewStates.Gone;
                    holder.Equalizer.StopBars();
                }

                if (!holder.MoreButton.HasOnClickListeners)
                    holder.MoreButton.Click += (sender, e) => ClickListeners.OnMoreClick(new MoreClickEventArgs { View = holder.MainView, SongsClass = item }, NamePage);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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

        private void OnClick(RowSoundAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(RowSoundAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

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

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop());
        }

    }

    public class RowSoundAdapterViewHolder : RecyclerView.ViewHolder
    {
        public View MainView { get; private set; }
        public CardView CardViewImage { get; private set; }
        public ImageView Image { get; private set; }
        public EqualizerView Equalizer { get; private set; }
        public TextView TxtSongName { get; private set; }
        public TextView TxtGenresName { get; private set; }

        public TextView TxtSongDuration { get; private set; }
        public ImageButton MoreButton { get; private set; }

        public ImageView PlayButton { get; private set; }
        public ImageView ImageSeperator { get; private set; }

        public TextView CountItemTextView { get; private set; }

        public RowSoundAdapterViewHolder(View itemView, Action<RowSoundAdapterClickEventArgs> clickListener, Action<RowSoundAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                CardViewImage = MainView.FindViewById<CardView>(Resource.Id.cardview2);
                Image = MainView.FindViewById<ImageView>(Resource.Id.imageView_songlist);
                Equalizer = MainView.FindViewById<EqualizerView>(Resource.Id.equalizer_view);
                TxtSongName = MainView.FindViewById<TextView>(Resource.Id.textView_songname);
                TxtGenresName = MainView.FindViewById<TextView>(Resource.Id.textView_catname);
                PlayButton = MainView.FindViewById<ImageView>(Resource.Id.playImageview);
                ImageSeperator = MainView.FindViewById<ImageView>(Resource.Id.seperatorImageview);

                TxtSongDuration = MainView.FindViewById<TextView>(Resource.Id.textView_songduration);

                MoreButton = MainView.FindViewById<ImageButton>(Resource.Id.more);
                CountItemTextView = MainView.FindViewById<TextView>(Resource.Id.textView_count);

                //Event
                PlayButton.Click += (sender, e) => clickListener(new RowSoundAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new RowSoundAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class RowSoundAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}