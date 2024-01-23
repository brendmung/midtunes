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
using AndroidX.AppCompat.Widget;
using Bumptech.Glide;
using Com.Canhub.Cropper;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using System;
using System.Linq;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Artists
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class BecomeArtistActivity : BaseActivity, IDialogListCallBack, IActivityResultCallback
    {
        #region Variables Basic

        private ImageView ImagePersonal, ImagePassport;
        private LinearLayout BtnSelectImagePersonal, BtnSelectImagePassport;
        private AppCompatButton BtnSave;
        private TextView IconName, IconDescription, IconGenres, IconWebsite;
        private EditText NameEditText, DescriptionEditText, GenresEditText, WebsiteEditText;
        private string PathImagePersonal = "", PathImagePassport = "", ImageType = "", TypeDialog = "", IdGenres = "";
        private AdManagerAdView AdManagerAdView;
        private DialogGalleryController GalleryController;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(DeepSoundTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.BecomeAnArtistLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                GalleryController = new DialogGalleryController(this, this);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
                AddOrRemoveEvent(true);
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
                AddOrRemoveEvent(false);
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");
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
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
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
                ImagePassport = FindViewById<ImageView>(Resource.Id.imagePassport);
                BtnSelectImagePassport = FindViewById<LinearLayout>(Resource.Id.btn_selectImagePassport);

                ImagePersonal = FindViewById<ImageView>(Resource.Id.imagePersonal);
                BtnSelectImagePersonal = FindViewById<LinearLayout>(Resource.Id.btn_selectImagePersonal);

                IconName = FindViewById<TextView>(Resource.Id.IconName);
                NameEditText = FindViewById<EditText>(Resource.Id.NameEditText);

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                DescriptionEditText = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                IconGenres = FindViewById<TextView>(Resource.Id.IconGenres);
                GenresEditText = FindViewById<EditText>(Resource.Id.GenresEditText);

                IconWebsite = FindViewById<TextView>(Resource.Id.IconWebsite);
                WebsiteEditText = FindViewById<EditText>(Resource.Id.WebsiteEditText);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                Methods.SetColorEditText(NameEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(DescriptionEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(GenresEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(WebsiteEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconName, FontAwesomeIcon.TextWidth);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.AudioDescription);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGenres, FontAwesomeIcon.LayerGroup);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconWebsite, IonIconsFonts.Globe);

                Methods.SetFocusable(GenresEditText);
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_BecomeAnArtist);
                    toolbar.SetTitleTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    BtnSelectImagePassport.Click += BtnSelectImagePassportOnClick;
                    BtnSelectImagePersonal.Click += BtnSelectImagePersonalOnClick;
                    BtnSave.Click += BtnSaveOnClick;
                    GenresEditText.Touch += GenresEditTextOnClick;
                }
                else
                {
                    BtnSelectImagePassport.Click -= BtnSelectImagePassportOnClick;
                    BtnSelectImagePersonal.Click -= BtnSelectImagePersonalOnClick;
                    BtnSave.Click -= BtnSaveOnClick;
                    GenresEditText.Touch -= GenresEditTextOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Click Save data Playlist
        private async void BtnSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    if (string.IsNullOrEmpty(NameEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterName), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(GenresEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChooseGenres), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(PathImagePersonal) || string.IsNullOrEmpty(PathImagePassport))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectImage), ToastLength.Short)?.Show();
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                    var (apiStatus, respond) = await RequestsAsync.User.BecomeAsync(NameEditText.Text, IdGenres, WebsiteEditText.Text, DescriptionEditText.Text, PathImagePersonal, PathImagePassport); //Sent api 
                    if (apiStatus.Equals(200))
                    {
                        if (respond is MessageObject result)
                        {
                            Toast.MakeText(this, result.Message, ToastLength.Short)?.Show();

                            AndHUD.Shared.Dismiss();

                            Finish();
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Gallery 
        private void BtnSelectImagePersonalOnClick(object sender, EventArgs e)
        {
            try
            {
                ImageType = "Personal";
                GalleryController?.OpenDialogGallery();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Gallery 
        private void BtnSelectImagePassportOnClick(object sender, EventArgs e)
        {
            try
            {
                ImageType = "Passport";
                GalleryController?.OpenDialogGallery();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Genres
        private void GenresEditTextOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Genres";

                var dialogList = new MaterialAlertDialogBuilder(this);

                var arrayAdapter = ListUtils.GenresList.Select(item => item.CateogryName).ToList();

                dialogList.SetTitle(GetText(Resource.String.Lbl_Genres));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
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
                string text = itemString;

                if (TypeDialog == "Genres")
                {
                    IdGenres = ListUtils.GenresList[position]?.Id.ToString();
                    GenresEditText.Text = text;
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
                            switch (ImageType)
                            {
                                case "Personal":
                                    PathImagePersonal = filepath;

                                    //Do something with your Uri 
                                    Glide.With(this).Load(filepath).Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(ImagePersonal);
                                    break;
                                case "Passport":
                                    PathImagePassport = filepath;

                                    //Do something with your Uri 
                                    Glide.With(this).Load(filepath).Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(ImagePassport);
                                    break;
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}