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
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Utils;
using DeepSoundClient;
using DeepSoundClient.Classes.Tracks;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Upload
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class UploadSongActivity : BaseActivity, IDialogListCallBack, IActivityResultCallback
    {
        #region Variables Basic

        private ImageView Image;
        private LinearLayout BtnSelectImage;
        private AppCompatButton BtnSave;
        private TextView TxtSubTitle, IconTitle, IconDescription, IconTags, IconGenres, IconPrice, IconAvailability, IconAgeRestriction, IconLyrics, IconAllowDownloads;
        private EditText TitleEditText, DescriptionEditText, TagsEditText, GenresEditText, PriceEditText, AgeRestrictionEditText, LyricsEditText, AllowDownloadsEditText;
        private RadioButton RbPublic, RbPrivate;
        private string CurrencySymbol = "$", Status = "0", PathImage = "", SongLocation = "", TypeDialog = "", IdGenres = "", IdPrice = "0", IdAgeRestriction = "", IdAllowDownloads = "";
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
                SetContentView(Resource.Layout.UploadSongLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                GalleryController = new DialogGalleryController(this, this);

                CurrencySymbol = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";

                var data = Intent?.GetStringExtra("SongLocation") ?? "Data not available";
                if (data != "Data not available" && !string.IsNullOrEmpty(data))
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => UploadSong(data) });
                }
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
                TxtSubTitle = FindViewById<TextView>(Resource.Id.subTitle);

                Image = FindViewById<ImageView>(Resource.Id.image);
                BtnSelectImage = FindViewById<LinearLayout>(Resource.Id.btn_selectimage);

                IconTitle = FindViewById<TextView>(Resource.Id.IconTitle);
                TitleEditText = FindViewById<EditText>(Resource.Id.TitleEditText);

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                DescriptionEditText = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                IconLyrics = FindViewById<TextView>(Resource.Id.IconLyrics);
                LyricsEditText = FindViewById<EditText>(Resource.Id.LyricsEditText);

                IconTags = FindViewById<TextView>(Resource.Id.IconTags);
                TagsEditText = FindViewById<EditText>(Resource.Id.TagsEditText);

                IconPrice = FindViewById<TextView>(Resource.Id.IconPrice);
                PriceEditText = FindViewById<EditText>(Resource.Id.PriceEditText);

                IconGenres = FindViewById<TextView>(Resource.Id.IconGenres);
                GenresEditText = FindViewById<EditText>(Resource.Id.GenresEditText);

                IconAvailability = FindViewById<TextView>(Resource.Id.IconAvailability);
                RbPublic = FindViewById<RadioButton>(Resource.Id.radioPublic);
                RbPrivate = FindViewById<RadioButton>(Resource.Id.radioPrivate);

                IconAgeRestriction = FindViewById<TextView>(Resource.Id.IconAgeRestriction);
                AgeRestrictionEditText = FindViewById<EditText>(Resource.Id.AgeRestrictionEditText);

                IconAllowDownloads = FindViewById<TextView>(Resource.Id.IconAllowDownloads);
                AllowDownloadsEditText = FindViewById<EditText>(Resource.Id.AllowDownloadsEditText);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                RbPublic.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                RbPrivate.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetColorEditText(TitleEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(DescriptionEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(LyricsEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TagsEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(GenresEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(PriceEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(AgeRestrictionEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(AllowDownloadsEditText, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.TextWidth);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTags, FontAwesomeIcon.Tags);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.AudioDescription);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGenres, FontAwesomeIcon.LayerGroup);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconPrice, IonIconsFonts.Cash);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAvailability, FontAwesomeIcon.ShieldAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAgeRestriction, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconLyrics, FontAwesomeIcon.FileAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAllowDownloads, FontAwesomeIcon.Download);

                Methods.SetFocusable(GenresEditText);
                Methods.SetFocusable(PriceEditText);
                Methods.SetFocusable(AgeRestrictionEditText);
                Methods.SetFocusable(AllowDownloadsEditText);

                RbPublic.Checked = true;
                Status = "0";

                if (!AppSettings.ShowPrice)
                {
                    PriceEditText.Visibility = ViewStates.Gone;
                    IconPrice.Visibility = ViewStates.Gone;
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
                    toolbar.Title = GetString(Resource.String.Lbl_UploadSong);
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
                    BtnSelectImage.Click += BtnSelectImageOnClick;
                    RbPublic.CheckedChange += RadioPublicOnCheckedChange;
                    RbPrivate.CheckedChange += RadioPrivateOnCheckedChange;
                    BtnSave.Click += BtnSaveOnClick;
                    GenresEditText.Touch += GenresEditTextOnClick;
                    PriceEditText.Touch += PriceEditTextOnClick;
                    AgeRestrictionEditText.Touch += AgeRestrictionEditTextOnClick;
                    AllowDownloadsEditText.Touch += AllowDownloadsEditTextOnTouch;
                }
                else
                {
                    BtnSelectImage.Click -= BtnSelectImageOnClick;
                    RbPublic.CheckedChange -= RadioPublicOnCheckedChange;
                    RbPrivate.CheckedChange -= RadioPrivateOnCheckedChange;
                    BtnSave.Click -= BtnSaveOnClick;
                    GenresEditText.Touch -= GenresEditTextOnClick;
                    PriceEditText.Touch -= PriceEditTextOnClick;
                    AgeRestrictionEditText.Touch -= AgeRestrictionEditTextOnClick;
                    AllowDownloadsEditText.Touch -= AllowDownloadsEditTextOnTouch;
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
                    if (string.IsNullOrEmpty(TitleEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTitleSong), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(DescriptionEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterDescriptionSong), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TagsEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseEnterTags), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(GenresEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChooseGenres), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(PriceEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChoosePrice), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(AgeRestrictionEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChooseAgeRestriction), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(AllowDownloadsEditText.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseChooseAllowDownloads), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(PathImage))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectImage), ToastLength.Short)?.Show();
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                    var (apiStatus, respond) = await RequestsAsync.Tracks.SubmitTrackAsync(TitleEditText.Text, DescriptionEditText.Text, LyricsEditText.Text, TagsEditText.Text, PathImage, SongLocation, IdGenres, Status, IdAgeRestriction, IdAllowDownloads, IdPrice); //Sent api 
                    if (apiStatus.Equals(200))
                    {
                        if (respond is SubmitTrackObject result)
                        {
                            var ffmpegSystem = ListUtils.SettingsSiteList?.FfmpegSystem ?? "on";
                            if (ffmpegSystem == "on")
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Tracks.ConvertTrackAsync(result.AudioId, result.SongLocation) });

                            Toast.MakeText(this, GetText(Resource.String.Lbl_AddedSuccessfully), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss();

                            result.TrackData.AudioLocation = !result.TrackData.AudioLocation.Contains(InitializeDeepSound.WebsiteUrl)
                                    ? InitializeDeepSound.WebsiteUrl + "/" + result.SongLocation
                                    : result.SongLocation;

                            var dataProfileFragment = HomeActivity.GetInstance()?.ProfileFragment?.SongsFragment?.MAdapter;
                            dataProfileFragment?.SoundsList.Insert(0, result.TrackData);
                            dataProfileFragment?.NotifyItemInserted(dataProfileFragment.SoundsList.IndexOf(dataProfileFragment.SoundsList.FirstOrDefault()));

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
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        //Private
        private void RadioPrivateOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RbPrivate.Checked;
                if (isChecked)
                {
                    RbPublic.Checked = false;
                    Status = "1";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Public
        private void RadioPublicOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RbPublic.Checked;
                if (isChecked)
                {
                    RbPrivate.Checked = false;
                    Status = "0";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Gallery 
        private void BtnSelectImageOnClick(object sender, EventArgs e)
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

        //Price
        private void PriceEditTextOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Price";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                foreach (var item in ListUtils.PriceList)
                    if (item.Price == "0.00" || item.Price == "0.0" || item.Price == "0")
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Free));
                    else
                        arrayAdapter.Add(CurrencySymbol + item.Price);

                dialogList.SetTitle(GetText(Resource.String.Lbl_Price));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
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

        //AgeRestriction
        private void AgeRestrictionEditTextOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "AgeRestriction";

                var dialogList = new MaterialAlertDialogBuilder(this);

                var arrayAdapter = new List<string>
                {
                    GetString(Resource.String.Lbl_AgeRestrictionText0),
                    GetString(Resource.String.Lbl_AgeRestrictionText1)
                };

                dialogList.SetTitle(GetText(Resource.String.Lbl_AgeRestriction));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //AllowDownloads
        private void AllowDownloadsEditTextOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "AllowDownloads";

                var dialogList = new MaterialAlertDialogBuilder(this);

                var arrayAdapter = new List<string>
                {
                    GetString(Resource.String.Lbl_Yes),
                    GetString(Resource.String.Lbl_No),
                };

                dialogList.SetTitle(GetText(Resource.String.Lbl_AllowDownloads));
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
                else if (TypeDialog == "Price")
                {
                    IdPrice = ListUtils.PriceList[position]?.Price;
                    PriceEditText.Text = text;
                }
                else if (TypeDialog == "AgeRestriction")
                {
                    IdAgeRestriction = position.ToString();
                    AgeRestrictionEditText.Text = text;
                }
                else if (TypeDialog == "AllowDownloads")
                {
                    if (text == GetString(Resource.String.Lbl_Yes))
                    {
                        IdAllowDownloads = "1";
                    }
                    else if (text == GetString(Resource.String.Lbl_No))
                    {
                        IdAllowDownloads = "0";
                    }

                    AllowDownloadsEditText.Text = text;
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
                            Glide.With(this).Load(filepath).Apply(GlideImageLoader.GetOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(Image);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => UploadImage(filepath) });
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

        private async Task UploadImage(string path)
        {
            if (!Methods.CheckConnectivity())
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            else
            {
                var (apiStatus, respond) = await RequestsAsync.Tracks.UploadThumbnailAsync(path);
                if (apiStatus.Equals(200))
                {
                    if (respond is UploadThumbnailObject resultUpload)
                        PathImage = resultUpload.Thumbnail;
                }
                else
                {
                    Methods.DisplayAndHudErrorResult(this, respond);
                }
            }
        }

        private async Task UploadSong(string path)
        {
            if (!Methods.CheckConnectivity())
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            else
            {
                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Processing));

                var (apiStatus, respond) = await RequestsAsync.Tracks.UploadTrackAsync(path);
                if (apiStatus.Equals(200))
                {
                    if (respond is UploadTrackObject resultUpload)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_AddedSuccessfully), ToastLength.Short)?.Show();

                        SongLocation = resultUpload.FilePath;

                        TxtSubTitle.Text = GetText(Resource.String.Lbl_subTitleUploadSong) + " " + resultUpload.FileName;

                        AndHUD.Shared.Dismiss();
                    }
                }
                else
                {
                    Methods.DisplayAndHudErrorResult(this, respond);
                    Finish();
                }
            }
        }
    }
}