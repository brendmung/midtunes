using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Activity.Result;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using Com.Canhub.Cropper;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Base;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Product
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateProductActivity : BaseActivity, IDialogListCallBack, IActivityResultCallback
    {
        #region Variables Basic

        private TextView TxtSave;
        private EditText TxtTitle, TxtDescription, TxtTags, TxtPrice, TxtTotalItem, TxtRelatedToSong, TxtCategory;
        private string CategoryId = "", SongId = "", TypeDialog = "";
        private AdManagerAdView AdManagerAdView;
        private AttachmentsAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private List<SoundDataObject> LatestSongsList;
        private DialogGalleryController GalleryController;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.CreateProductLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                GetMyInfoData();
                GalleryController = new DialogGalleryController(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtSave.Text = GetText(Resource.String.Lbl_Save);

                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);
                TxtTags = FindViewById<EditText>(Resource.Id.TagsText);
                TxtPrice = FindViewById<EditText>(Resource.Id.PriceText);
                TxtTotalItem = FindViewById<EditText>(Resource.Id.TotalItemText);
                TxtRelatedToSong = FindViewById<EditText>(Resource.Id.RelatedToSongText);
                TxtCategory = FindViewById<EditText>(Resource.Id.CategoryText);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.imageRecyler);

                Methods.SetColorEditText(TxtTitle, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTags, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTotalItem, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPrice, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtRelatedToSong, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtRelatedToSong);
                Methods.SetFocusable(TxtCategory);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_CreateProduct);
                    toolBar.SetTitleTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new AttachmentsAdapter(this) { AttachmentList = new ObservableCollection<AttachmentsObject>() };
                LayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                var attach = new AttachmentsObject
                {
                    Id = MAdapter.AttachmentList.Count + 1,
                    TypeAttachment = "Default",
                    FileSimple = "addImage",
                    FileUrl = "addImage"
                };

                MAdapter.Add(attach);
                MAdapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                if (addEvent)
                {
                    MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                    MAdapter.ItemClick += MAdapterItemClick;
                    TxtSave.Click += TxtSaveOnClick;
                    TxtCategory.Touch += TxtCategoryOnClick;
                    TxtRelatedToSong.Touch += TxtRelatedToSongOnTouch;
                }
                else
                {
                    MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                    MAdapter.ItemClick -= MAdapterItemClick;
                    TxtSave.Click -= TxtSaveOnClick;
                    TxtCategory.Touch -= TxtCategoryOnClick;
                    TxtRelatedToSong.Touch -= TxtRelatedToSongOnTouch;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");

                TxtSave = null;
                TxtTitle = null;
                TxtPrice = null;
                TxtCategory = null;

                MAdapter = null;
                MRecycler = null;
                LayoutManager = null;
                AdManagerAdView = null;
                CategoryId = "";
                TypeDialog = "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        MAdapter.Remove(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item == null) return;

                    if (item.TypeAttachment != "Default") return;
                    GalleryController?.OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtRelatedToSongOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                if (LatestSongsList?.Count > 0)
                {
                    TypeDialog = "RelatedToSong";

                    var dialogList = new MaterialAlertDialogBuilder(this);

                    var arrayAdapter = LatestSongsList.Select(item => Methods.FunString.DecodeString(item.Title)).ToList();

                    dialogList.SetTitle(GetText(Resource.String.Lbl_RelatedToSong));
                    dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                    dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                    dialogList.Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, GetText(Resource.String.Lbl_Error_NotHaveSong));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCategoryOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                if (CategoriesController.ListCategoriesProducts.Count > 0)
                {
                    TypeDialog = "Categories";

                    var dialogList = new MaterialAlertDialogBuilder(this);

                    var arrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName).ToList();

                    dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCategories));
                    dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                    dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                    dialogList.Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Products");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtTitle.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTitle), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtDescription.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterDescription), ToastLength.Short)?.Show();
                    return;
                }

                if (TxtDescription.Text.Length < 10)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_DescriptionIsShort), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtTags.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTags), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtPrice.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChoosePrice), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtTotalItem.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTotalItem), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtRelatedToSong.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterRelatedToSong), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtCategory.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterCategory), ToastLength.Short)?.Show();
                    return;
                }

                var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                if (list.Count == 0)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectImage), ToastLength.Short)?.Show();
                }
                else
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                    var (apiStatus, respond) = await RequestsAsync.Product.CreateProductAsync(TxtTitle.Text, TxtDescription.Text, TxtTags.Text, TxtPrice.Text, TxtTotalItem.Text, SongId, CategoryId, list);
                    if (apiStatus == 200)
                    {
                        if (respond is GetProductDataObject result)
                        {
                            AndHUD.Shared.Dismiss();
                            Console.WriteLine(result.Message);

                            Intent intent = new Intent();
                            intent.PutExtra("itemData", JsonConvert.SerializeObject(result.Data));
                            SetResult(Result.Ok, intent);

                            Toast.MakeText(this, GetString(Resource.String.Lbl_ProductSuccessfullyEdited), ToastLength.Short)?.Show();

                            Finish();
                        }
                    }
                    else
                        Methods.DisplayAndHudErrorResult(this, respond);
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        GalleryController?.OpenDialogGallery();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (TypeDialog == "Categories")
                {
                    CategoryId = CategoriesController.ListCategoriesProducts.FirstOrDefault(categories => categories.CategoriesName == itemString)?.CategoriesId;
                    TxtCategory.Text = itemString;
                }
                else if (TypeDialog == "RelatedToSong")
                {
                    SongId = LatestSongsList?.FirstOrDefault(item => Methods.FunString.DecodeString(item.Title) == itemString)?.Id.ToString() ?? "";
                    TxtRelatedToSong.Text = itemString;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Result Gallery

        public void OnActivityResult(Java.Lang.Object p0)
        {
            try
            {
                if (p0 is CropImageView.CropResult result)
                {
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.UriContent;
                        string filepath = Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu ? result.GetUriFilePath(this, true) : Methods.AttachmentFiles.GetActualPathFromFile(this, resultUri);
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri 
                            var attach = new AttachmentsObject
                            {
                                Id = MAdapter.AttachmentList.Count + 1,
                                TypeAttachment = "image[]",
                                FileSimple = filepath,
                                FileUrl = filepath
                            };

                            MAdapter.Add(attach);
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetMyInfoData()
        {
            try
            {
                if (ListUtils.MyUserInfoList?.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                }

                LatestSongsList = ListUtils.MyUserInfoList?.FirstOrDefault()?.Latestsongs?.FirstOrDefault();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}