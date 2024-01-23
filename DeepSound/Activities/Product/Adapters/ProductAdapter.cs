using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Product;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IList = System.Collections.IList;

namespace DeepSound.Activities.Product.Adapters
{
    public class ProductAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<ProductAdapterClickEventArgs> AddButtonItemClick;
        public event EventHandler<ProductAdapterClickEventArgs> ItemClick;
        public event EventHandler<ProductAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<ProductDataObject> ProductsList = new ObservableCollection<ProductDataObject>();

        public ProductAdapter(Activity context)
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
                //Setup your layout here >> Style_ProductView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ProductView, parent, false);
                var vh = new ProductAdapterViewHolder(itemView, AddButtonClick, Click, LongClick);
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
                if (viewHolder is ProductAdapterViewHolder holder)
                {
                    var item = ProductsList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Images.FirstOrDefault()?.Image, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        holder.Title.Text = Methods.FunString.DecodeString(item.Title);

                        var currencyIcon = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";
                        holder.Price.Text = currencyIcon + " " + item.Price;

                        holder.Cat.Text = CategoriesController.Get_Translate_Categories_Communities(item.CatId.ToString(), "", "Products");

                        if (item.AddedToCart == 1)
                        {
                            holder.AddButton.Text = ActivityContext.GetText(Resource.String.Lbl_RemoveFromCart);
                            holder.AddButton.Tag = "true";
                        }
                        else
                        {
                            holder.AddButton.Text = ActivityContext.GetText(Resource.String.Lbl_AddToCart);
                            holder.AddButton.Tag = "false";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => ProductsList?.Count ?? 0;

        public ProductDataObject GetItem(int position)
        {
            return ProductsList[position];
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

        void AddButtonClick(ProductAdapterClickEventArgs args) => AddButtonItemClick?.Invoke(this, args);
        void Click(ProductAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(ProductAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = ProductsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                var image = item.Images.FirstOrDefault()?.Image;
                if (!string.IsNullOrEmpty(image))
                {
                    d.Add(image);
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

    public class ProductAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Price { get; private set; }
        public TextView Cat { get; private set; }
        public AppCompatButton AddButton { get; private set; }

        #endregion

        public ProductAdapterViewHolder(View itemView, Action<ProductAdapterClickEventArgs> addButtonClickListener, Action<ProductAdapterClickEventArgs> clickListener, Action<ProductAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.image);
                Title = MainView.FindViewById<TextView>(Resource.Id.title);
                Price = MainView.FindViewById<TextView>(Resource.Id.price);
                Cat = MainView.FindViewById<TextView>(Resource.Id.catText);
                AddButton = MainView.FindViewById<AppCompatButton>(Resource.Id.AddButton);

                //Create an Event
                AddButton.Click += (sender, e) => addButtonClickListener(new ProductAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, AddButton = AddButton });
                itemView.Click += (sender, e) => clickListener(new ProductAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, AddButton = AddButton });
                itemView.LongClick += (sender, e) => longClickListener(new ProductAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, AddButton = AddButton });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class ProductAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public AppCompatButton AddButton { get; set; }
    }
}