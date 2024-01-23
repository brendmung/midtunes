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
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Canhub.Cropper;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Requests;
using System;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Payment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PaymentLocalActivity : BaseActivity, IActivityResultCallback
    {
        #region Variables Basic

        private TextView CardNumber, CardCode, CardCountry, CardName;
        private ImageView Image;
        private CircleButton ImageClose;
        private AppCompatButton BtnAddImage, BtnApply;
        private string Id, Price, PayType, PathImage = "";
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
                SetContentView(Resource.Layout.PaymentLocalLayout);

                Id = Intent?.GetStringExtra("Id") ?? "";
                Price = Intent?.GetStringExtra("Price");
                PayType = Intent?.GetStringExtra("payType"); // AddFunds 

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                CardNumber = (TextView)FindViewById(Resource.Id.card_number);
                CardCode = (TextView)FindViewById(Resource.Id.card_code);
                CardCountry = (TextView)FindViewById(Resource.Id.card_country);
                CardName = (TextView)FindViewById(Resource.Id.card_name);
                Image = (ImageView)FindViewById(Resource.Id.Image);

                ImageClose = (CircleButton)FindViewById(Resource.Id.ImageCircle);
                BtnAddImage = (AppCompatButton)FindViewById(Resource.Id.btn_AddPhoto);
                BtnApply = (AppCompatButton)FindViewById(Resource.Id.ApplyButton);

                var bankDescription = ListUtils.SettingsSiteList?.BankDescription;
                bankDescription = bankDescription?.Replace("&lt;", "<").Replace("&gt;", ">");
                var splitText = bankDescription?.Split(new[] { "<p>", "</p>" }, StringSplitOptions.None);
                Console.WriteLine(splitText);

                if (splitText != null)
                {
                    CardNumber.Text = splitText[1];
                    CardName.Text = splitText[3];
                    CardCode.Text = splitText[5];
                    CardCountry.Text = splitText[7];
                }

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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_BankTransfer);
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
                    ImageClose.Click += ImageCloseOnClick;
                    BtnAddImage.Click += BtnAddImageOnClick;
                    BtnApply.Click += BtnApplyOnClick;
                }
                else
                {
                    ImageClose.Click -= ImageCloseOnClick;
                    BtnAddImage.Click -= BtnAddImageOnClick;
                    BtnApply.Click -= BtnApplyOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BtnAddImageOnClick(object sender, EventArgs e)
        {
            try
            {
                GalleryController?.OpenDialogGallery();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (string.IsNullOrEmpty(PathImage) || (string.IsNullOrWhiteSpace(PathImage)))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorPleaseSelectImage), ToastLength.Long)?.Show();
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var (apiStatus, respond) = await RequestsAsync.Common.UploadBankRecipeAsync(PayType, Id, Price, "Pay the card " + Price, PathImage);
                    if (apiStatus == 200)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_YourWasReceiptSuccessfullyUploaded), ToastLength.Long)?.Show();

                        AndHUD.Shared.Dismiss();
                        Finish();
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        private void ImageCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                PathImage = "";
                GlideImageLoader.LoadImage(this, "Grey_Offline", Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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

                if (requestCode == 106)
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
                            PathImage = filepath;
                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(Image);
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

    }
}