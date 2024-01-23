using Android;
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
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Canhub.Cropper;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using Java.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Event
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditEventActivity : BaseActivity, View.IOnClickListener, IDialogListCallBack, IActivityResultCallback
    {
        #region Variables Basic

        private TextView TxtSave;
        private RelativeLayout LayoutEventPhoto, LayoutEventVideoTrailer;
        private ImageView ImageCover, ImageVideoTrailer;
        private EditText TxtName, TxtDescription, TxtLocation, TxtLocationData, TxtStartDate, TxtStartTime, TxtEndDate, TxtEndTime, TxtTimezone, TxtSellTickets, TxtTicketsAvailable, TxtTicketPrice;
        private LinearLayout LayoutTicketsData;
        private string Timezone, Location, SellTicket = "no", EventPathImage, EventPathVideo, ImageType, TypeDialog = "";
        private AdManagerAdView AdManagerAdView;
        private Dictionary<string, string> TimezonesList;

        private EventDataObject EventData;
        private string EventId;
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
                SetContentView(Resource.Layout.CreateEventLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                GetDataEvent();
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

                LayoutEventPhoto = FindViewById<RelativeLayout>(Resource.Id.LayoutEventPhoto);
                ImageCover = FindViewById<ImageView>(Resource.Id.imageCover);

                LayoutEventVideoTrailer = FindViewById<RelativeLayout>(Resource.Id.LayoutEventVideoTrailer);
                ImageVideoTrailer = FindViewById<ImageView>(Resource.Id.imageVideoTrailer);

                TxtName = FindViewById<EditText>(Resource.Id.NameEditText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                TxtLocation = FindViewById<EditText>(Resource.Id.LocationText);
                TxtLocationData = FindViewById<EditText>(Resource.Id.LocationDataText);

                TxtStartDate = FindViewById<EditText>(Resource.Id.StartDateEditText);
                TxtStartTime = FindViewById<EditText>(Resource.Id.StartTimeEditText);

                TxtEndDate = FindViewById<EditText>(Resource.Id.EndDateEditText);
                TxtEndTime = FindViewById<EditText>(Resource.Id.EndTimeEditText);

                TxtTimezone = FindViewById<EditText>(Resource.Id.TimezoneText);
                TxtSellTickets = FindViewById<EditText>(Resource.Id.SellTicketsText);

                LayoutTicketsData = FindViewById<LinearLayout>(Resource.Id.LayoutTicketsData);
                TxtTicketsAvailable = FindViewById<EditText>(Resource.Id.TicketsAvailableEditText);
                TxtTicketPrice = FindViewById<EditText>(Resource.Id.TicketPriceEditText);

                Methods.SetColorEditText(TxtName, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocationData, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartDate, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartTime, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEndDate, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEndTime, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTimezone, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtSellTickets, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTicketsAvailable, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTicketPrice, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtStartTime);
                Methods.SetFocusable(TxtEndTime);
                Methods.SetFocusable(TxtStartDate);
                Methods.SetFocusable(TxtEndDate);

                Methods.SetFocusable(TxtLocation);
                Methods.SetFocusable(TxtTimezone);
                Methods.SetFocusable(TxtSellTickets);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                LayoutTicketsData.Visibility = ViewStates.Gone;
                TxtSellTickets.Hint = GetText(Resource.String.Lbl_No);
                SellTicket = "no";

                TxtStartTime.SetOnClickListener(this);
                TxtEndTime.SetOnClickListener(this);
                TxtStartDate.SetOnClickListener(this);
                TxtEndDate.SetOnClickListener(this);

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
                    toolBar.Title = GetText(Resource.String.Lbl_EditEvent);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                if (addEvent)
                {
                    LayoutEventPhoto.Click += LayoutEventPhotoOnClick;
                    LayoutEventVideoTrailer.Click += LayoutEventVideoTrailerOnClick;
                    TxtLocation.Touch += TxtLocationOnTouch;
                    TxtTimezone.Touch += TxtTimezoneOnTouch;
                    TxtSellTickets.Touch += TxtSellTicketsOnTouch;
                    TxtSave.Click += TxtSaveOnClick;
                }
                else
                {
                    LayoutEventPhoto.Click -= LayoutEventPhotoOnClick;
                    LayoutEventVideoTrailer.Click -= LayoutEventVideoTrailerOnClick;
                    TxtLocation.Touch -= TxtLocationOnTouch;
                    TxtTimezone.Touch -= TxtTimezoneOnTouch;
                    TxtSellTickets.Touch -= TxtSellTicketsOnTouch;
                    TxtSave.Click -= TxtSaveOnClick;
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
                AdManagerAdView = null;
                TypeDialog = "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void LayoutEventVideoTrailerOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogVideo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LayoutEventPhotoOnClick(object sender, EventArgs e)
        {
            try
            {
                ImageType = "Image";
                GalleryController?.OpenDialogGallery("Image");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtTimezoneOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Timezone";

                var dialogList = new MaterialAlertDialogBuilder(this);

                var arrayAdapter = TimezonesList.Select(item => item.Value).ToList();

                dialogList.SetTitle(GetText(Resource.String.Lbl_Timezone));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Location";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Online));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RealLocation));

                dialogList.SetTitle(GetText(Resource.String.Lbl_Location));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtSellTicketsOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "SellTicket";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Yes));
                arrayAdapter.Add(GetText(Resource.String.Lbl_No));

                dialogList.SetTitle(GetText(Resource.String.Lbl_SellTickets));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
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

                if (string.IsNullOrEmpty(TxtName.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterName), ToastLength.Short)?.Show();
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

                if (string.IsNullOrEmpty(TxtDescription.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterDescription), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtStartDate.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectStartDate), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtEndDate.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectEndDate), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtLocation.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectLocation), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtStartTime.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectStartTime), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtEndTime.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectEndTime), ToastLength.Short)?.Show();
                    return;
                }

                if (string.IsNullOrEmpty(EventPathImage))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectImage), ToastLength.Short)?.Show();
                    return;
                }
                else
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", TxtName.Text},
                        {"desc", TxtDescription.Text},
                        {"location", Location},
                        {"start_date", TxtStartDate.Text},
                        {"start_time", TxtStartTime.Text},
                        {"end_date", TxtEndDate.Text},
                        {"end_time", TxtEndTime.Text},
                        {"timezone", Timezone},
                        {"sell_tickets", SellTicket},
                    };

                    switch (Location)
                    {
                        case "online":
                            keyValues.Add("online_url", TxtLocationData.Text);
                            break;
                        case "real":
                            keyValues.Add("real_address", TxtLocationData.Text);
                            break;
                    }

                    if (SellTicket == "yes")
                    {
                        keyValues.Add("available_tickets", TxtTicketsAvailable.Text);
                        keyValues.Add("ticket_price", TxtTicketPrice.Text);
                    }

                    var (apiStatus, respond) = await RequestsAsync.Event.CreateEventAsync(keyValues, EventPathImage, EventPathVideo);
                    if (apiStatus == 200)
                    {
                        if (respond is CreateEventObject result)
                        {
                            AndHUD.Shared.Dismiss();
                            Console.WriteLine(result.Message);

                            Intent intent = new Intent();
                            intent.PutExtra("itemData", JsonConvert.SerializeObject(result.Data));
                            SetResult(Result.Ok, intent);

                            Toast.MakeText(this, GetString(Resource.String.Lbl_EventSuccessfullyEdited), ToastLength.Short)?.Show();

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

                if (requestCode == 501 && resultCode == Result.Ok)
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            //"Uri" >> filepath
                            //"Thumbnail" >> fullPathFile.Path

                            EventPathVideo = filepath;

                            File file2 = new File(fullPathFile.Path);
                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImageVideoTrailer);

                        }
                    }
                }
                else if (requestCode == 513 && resultCode == Result.Ok)
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            //"Uri" >> filepath
                            //"Thumbnail" >> fullPathFile.Path

                            EventPathVideo = filepath;

                            File file2 = new File(fullPathFile.Path);
                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImageVideoTrailer);

                        }
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
                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        switch (ImageType)
                        {
                            //requestCode >> 500 => Image Gallery
                            case "Image":
                                GalleryController?.OpenDialogGallery("Image");
                                break;
                            case "Video":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "VideoCamera":
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                        }
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
                if (TypeDialog == "Timezone")
                {
                    Timezone = TimezonesList.FirstOrDefault(item => item.Value == itemString).Key;
                    TxtTimezone.Text = itemString;
                }
                else if (TypeDialog == "Location")
                {
                    TxtLocation.Text = itemString;
                    if (itemString == GetText(Resource.String.Lbl_Online))
                    {
                        Location = "online";
                        TxtLocationData.Hint = GetText(Resource.String.Lbl_Url);
                    }
                    else if (itemString == GetText(Resource.String.Lbl_RealLocation))
                    {
                        Location = "real";
                        TxtLocationData.Hint = GetText(Resource.String.Lbl_Address);
                    }
                }
                else if (TypeDialog == "SellTicket")
                {
                    TxtSellTickets.Text = itemString;
                    if (itemString == GetText(Resource.String.Lbl_Yes))
                    {
                        SellTicket = "yes";
                        LayoutTicketsData.Visibility = ViewStates.Visible;
                    }
                    else if (itemString == GetText(Resource.String.Lbl_No))
                    {
                        SellTicket = "no";
                        LayoutTicketsData.Visibility = ViewStates.Gone;
                    }
                }
                else if (TypeDialog == "DialogVideo")
                {
                    if (itemString == GetText(Resource.String.Lbl_VideoGallery))
                    {
                        ImageType = "Video";

                        switch ((int)Build.VERSION.SdkInt)
                        {
                            // Check if we're running on Android 5.0 or higher
                            case < 23:
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            default:
                                {
                                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage(this))
                                    {
                                        //requestCode >> 501 => video Gallery
                                        new IntentController(this).OpenIntentVideoGallery();
                                    }
                                    else
                                    {
                                        new PermissionsController(this).RequestPermission(108);
                                    }

                                    break;
                                }
                        }
                    }
                    else if (itemString == GetText(Resource.String.Lbl_RecordVideoFromCamera))
                    {
                        ImageType = "VideoCamera";

                        switch ((int)Build.VERSION.SdkInt)
                        {
                            // Check if we're running on Android 5.0 or higher
                            case < 23:
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                            default:
                                {
                                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage(this))
                                    {
                                        //requestCode >> 513 => video Camera
                                        new IntentController(this).OpenIntentVideoCamera();
                                    }
                                    else
                                    {
                                        new PermissionsController(this).RequestPermission(108);
                                    }

                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialogVideo()
        {
            try
            {
                TypeDialog = "DialogVideo";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));

                dialogList.SetTitle(GetText(Resource.String.Lbl_SelectVideoFrom));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
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
                            EventPathImage = filepath;
                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(ImageCover);
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

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == TxtStartTime.Id)
                {
                    var frag = PopupDialogController.TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtStartTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.TimePickerFragment.Tag);
                }
                else if (v.Id == TxtEndTime.Id)
                {
                    var frag = PopupDialogController.TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtEndTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.TimePickerFragment.Tag);
                }
                else if (v.Id == TxtStartDate.Id)
                {
                    var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtStartDate.Text = time.Date.ToString("yyyy-MM-dd");
                    }, "StartDate");

                    frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
                }
                else if (v.Id == TxtEndDate.Id)
                {
                    var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtEndDate.Text = time.Date.ToString("yyyy-MM-dd");
                    }, "StartDate");
                    frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetDataEvent()
        {
            try
            {
                TimezonesList = DeepSoundTools.GetTimezonesList();

                EventData = JsonConvert.DeserializeObject<EventDataObject>(Intent?.GetStringExtra("EventView") ?? "");
                if (EventData != null)
                {
                    EventId = EventData.Id.ToString();

                    TxtName.Text = Methods.FunString.DecodeString(EventData.Name);
                    TxtDescription.Text = Methods.FunString.DecodeString(EventData.Desc);

                    if (!string.IsNullOrEmpty(EventData.OnlineUrl))
                    {
                        TxtLocation.Hint = GetText(Resource.String.Lbl_Url);
                        Location = "online";
                        TxtLocationData.Text = EventData.OnlineUrl;
                    }
                    else if (!string.IsNullOrEmpty(EventData.RealAddress))
                    {
                        TxtLocation.Hint = GetText(Resource.String.Lbl_Address);
                        Location = "real";
                        TxtLocationData.Text = EventData.RealAddress;
                    }

                    TxtStartDate.Text = EventData.StartDate;
                    TxtStartTime.Text = EventData.StartTime;
                    TxtEndDate.Text = EventData.EndDate;
                    TxtEndTime.Text = EventData.EndTime;

                    Timezone = EventData.Timezone;
                    TxtTimezone.Text = TimezonesList.FirstOrDefault(item => item.Key == EventData.Timezone).Value;

                    if (EventData.AvailableTickets > 0 || EventData.TicketPrice > 0)
                    {
                        SellTicket = "yes";
                        LayoutTicketsData.Visibility = ViewStates.Visible;
                        TxtSellTickets.Text = GetText(Resource.String.Lbl_Yes);

                        TxtTicketsAvailable.Text = EventData.AvailableTickets?.ToString();
                        TxtTicketPrice.Text = EventData.TicketPrice?.ToString();
                    }
                    else
                    {
                        SellTicket = "no";
                        LayoutTicketsData.Visibility = ViewStates.Gone;
                        TxtSellTickets.Text = GetText(Resource.String.Lbl_No);

                        TxtTicketsAvailable.Text = "";
                        TxtTicketPrice.Text = "";
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}