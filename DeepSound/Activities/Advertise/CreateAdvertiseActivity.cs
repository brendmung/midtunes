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
using Bumptech.Glide.Request;
using Com.Canhub.Cropper;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Advertise;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Advertise
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateAdvertiseActivity : BaseActivity, IActivityResultCallback, IDialogListCallBack
    {
        #region Variables Basic

        private ImageView ImageAd, BtnSelectImage;
        private EditText TxtName, TxtTitle, TxtUrl, TxtDescription, TxtAudience, TxtPlacement, TxtPricing, TxtSpending, TxtType;
        private string PathFile, TotalIdAudienceChecked, PlacementStatus, PricingStatus, TypeStatus, TypeDialog;
        private AdManagerAdView AdManagerAdView;
        private AppCompatButton BtnApply;
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
                SetContentView(Resource.Layout.CreateAdvertiseLayout);

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
                ImageAd = FindViewById<ImageView>(Resource.Id.image);
                BtnSelectImage = FindViewById<ImageView>(Resource.Id.ChooseImageText);

                TxtName = FindViewById<EditText>(Resource.Id.NameEditText);
                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);
                TxtUrl = FindViewById<EditText>(Resource.Id.UrlEditText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);
                TxtAudience = FindViewById<EditText>(Resource.Id.TargetAudienceEditText);
                TxtPlacement = FindViewById<EditText>(Resource.Id.PlacementEditText);
                TxtPricing = FindViewById<EditText>(Resource.Id.PricingEditText);
                TxtSpending = FindViewById<EditText>(Resource.Id.SpendingEditText);
                TxtType = FindViewById<EditText>(Resource.Id.TypeEditText);

                BtnApply = FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                Methods.SetColorEditText(TxtName, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTitle, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtUrl, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAudience, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPlacement, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPricing, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtSpending, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtType, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtAudience);
                Methods.SetFocusable(TxtPlacement);
                Methods.SetFocusable(TxtPricing);
                Methods.SetFocusable(TxtType);
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
                    toolbar.Title = GetString(Resource.String.Lbl_Create_Ad);
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        BtnApply.Click += TxtAddOnClick;
                        BtnSelectImage.Click += BtnSelectImageOnClick;
                        TxtAudience.Touch += TxtAudienceOnTouch;
                        TxtPlacement.Touch += TxtPlacementOnTouch;
                        TxtPricing.Touch += TxtPricingOnTouch;
                        TxtType.Touch += TxtTypeOnTouch;
                        break;
                    default:
                        BtnApply.Click -= TxtAddOnClick;
                        BtnSelectImage.Click -= BtnSelectImageOnClick;
                        TxtAudience.Touch -= TxtAudienceOnTouch;
                        TxtPlacement.Touch -= TxtPlacementOnTouch;
                        TxtPricing.Touch -= TxtPricingOnTouch;
                        TxtType.Touch -= TxtTypeOnTouch;
                        break;
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

                TxtName = null;
                ImageAd = null;
                BtnSelectImage = null;
                TxtTitle = null;
                TxtDescription = null;
                TxtAudience = null;
                TxtPlacement = null;
                TxtPricing = null;

                AdManagerAdView = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Add Image
        private void BtnSelectImageOnClick(object sender, EventArgs e)
        {
            try
            {
                if (TypeStatus == "banner")
                {
                    GalleryController?.OpenDialogGallery();
                }
                else if (TypeStatus == "audio")
                {
                    OpenDialogAudio();
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_Please_select_Type), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtTypeOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Type";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_TypeBanners)); //banner
                arrayAdapter.Add(GetText(Resource.String.Lbl_TypeAudio)); //audio

                dialogList.SetTitle(GetText(Resource.String.Lbl_Type));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtPricingOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Pricing";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_PricingClick)); //1
                arrayAdapter.Add(GetText(Resource.String.Lbl_PricingViews)); //2

                dialogList.SetTitle(GetText(Resource.String.Lbl_Pricing));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtPlacementOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Placement";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Placement_TrackPage)); //1
                arrayAdapter.Add(GetText(Resource.String.Lbl_Placement_AllPage)); //2

                dialogList.SetTitle(GetText(Resource.String.Lbl_Placement));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtAudienceOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;
                TypeDialog = "Audience";

                var countriesArray = DeepSoundTools.GetCountryList(this);
                var listItems = countriesArray.Select(item => item.Value).ToList();

                var checkedItems = new bool[listItems.Count];
                var selectedItems = new List<string>(listItems);

                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_TargetAudience);
                dialogList.SetCancelable(false);
                dialogList.SetMultiChoiceItems(listItems.ToArray(), checkedItems, (o, args) =>
                {
                    try
                    {
                        checkedItems[args.Which] = args.IsChecked;

                        var text = selectedItems[args.Which] ?? "";
                        Console.WriteLine(text);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), (o, args) =>
                {
                    try
                    {
                        TotalIdAudienceChecked = "";
                        for (int i = 0; i < checkedItems.Length; i++)
                        {
                            if (checkedItems[i])
                            {
                                var text = selectedItems[i];
                                var check = countriesArray.FirstOrDefault(a => a.Value == text).Key;
                                if (check != null)
                                {
                                    TotalIdAudienceChecked += check + ",";
                                }
                            }
                        }

                        TxtAudience.Text = TypeDialog == "Audience" && !string.IsNullOrEmpty(TotalIdAudienceChecked) ? GetText(Resource.String.Lbl_Selected) : TxtAudience.Text;
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetNeutralButton(Resource.String.Lbl_SelectAll, (o, args) =>
                {
                    try
                    {
                        Arrays.Fill(checkedItems, true);

                        TotalIdAudienceChecked = "";
                        foreach (var item in countriesArray)
                        {
                            TotalIdAudienceChecked += item.Key + ",";
                        }

                        TxtAudience.Text = TypeDialog == "Audience" && !string.IsNullOrEmpty(TotalIdAudienceChecked) ? GetText(Resource.String.Lbl_Selected) : TxtAudience.Text;
                    }
                    catch (Exception ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                    }
                });

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save 
        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_PleaseEnterName), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtTitle.Text) || string.IsNullOrWhiteSpace(TxtTitle.Text))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_PleaseEnterTitle), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtUrl.Text) || string.IsNullOrWhiteSpace(TxtUrl.Text))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_PleaseEnterUrl), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtDescription.Text) || string.IsNullOrWhiteSpace(TxtDescription.Text))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_PleaseEnterDescription), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtAudience.Text) || string.IsNullOrWhiteSpace(TxtAudience.Text) || string.IsNullOrEmpty(TotalIdAudienceChecked))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Please_select_Audience), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtPlacement.Text) || string.IsNullOrWhiteSpace(TxtPlacement.Text))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Please_select_Placement), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtPricing.Text) || string.IsNullOrWhiteSpace(TxtPricing.Text))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Please_select_Pricing), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtSpending.Text) || string.IsNullOrWhiteSpace(TxtSpending.Text))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Please_select_Spending), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtType.Text) || string.IsNullOrWhiteSpace(TxtType.Text))
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Please_select_Type), ToastLength.Short)?.Show();
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    TotalIdAudienceChecked = TotalIdAudienceChecked.Length switch
                    {
                        > 0 => TotalIdAudienceChecked.Remove(TotalIdAudienceChecked.Length - 1, 1),
                        _ => TotalIdAudienceChecked
                    };

                    var dictionary = new Dictionary<string, string>
                    {
                        {"name", TxtName.Text},
                        {"url",TxtUrl.Text},
                        {"title",TxtTitle.Text},
                        {"desc", TxtDescription.Text},
                        {"audience-list", TotalIdAudienceChecked},
                        {"cost",PricingStatus},
                        {"placement", PlacementStatus},
                        {"type", TypeStatus},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Advertise.CreateAdvertiseAsync(dictionary, TypeStatus, PathFile);
                    if (apiStatus == 200)
                    {
                        if (respond is CreateAdvertiseObject result)
                        {
                            AndHUD.Shared.Dismiss();
                            Toast.MakeText(this, GetString(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short)?.Show();

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

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 505 && resultCode == Result.Ok) //==> Audio
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        PathFile = filepath;
                        GlideImageLoader.LoadImage(this, "Audio_File", ImageAd, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        GalleryController?.OpenDialogGallery();
                        break;
                    case 108:
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Permission_is_denied), ToastLength.Short)?.Show();
                        break;
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        OpenDialogAudio();
                        break;
                    case 100:
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Permission_is_denied), ToastLength.Short)?.Show();
                        break;
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
                switch (TypeDialog)
                {
                    case "Placement":
                        {
                            if (itemString == GetText(Resource.String.Lbl_Placement_TrackPage))
                            {
                                PlacementStatus = "1";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_Placement_AllPage))
                            {
                                PlacementStatus = "2";
                            }

                            TxtPlacement.Text = itemString;
                            break;
                        }
                    case "Pricing":
                        {
                            if (itemString == GetText(Resource.String.Lbl_PricingClick))
                            {
                                PricingStatus = "1";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_PricingViews))
                            {
                                PricingStatus = "2";
                            }

                            TxtPricing.Text = itemString;
                            break;
                        }
                    case "Type":
                        {
                            if (itemString == GetText(Resource.String.Lbl_TypeBanners))
                            {
                                TypeStatus = "banner";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_TypeAudio))
                            {
                                TypeStatus = "audio";
                            }

                            PathFile = "";
                            ImageAd.SetImageResource(0);

                            TxtType.Text = itemString;
                            break;
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
                            PathFile = filepath;
                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(ImageAd);
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

        private void OpenDialogAudio()
        {
            try
            {
                if (!DeepSoundTools.CheckAllowedFileUpload())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_AllowedFileUpload), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                    new IntentController(this).OpenIntentAudio(); //505
                else
                {
                    if (PermissionsController.CheckPermissionStorage(this))
                        new IntentController(this).OpenIntentAudio(); //505
                    else
                        new PermissionsController(this).RequestPermission(100);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}