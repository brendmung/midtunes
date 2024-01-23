using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using DeepSound.Activities.Base;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;
using Google.Android.Material.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.SettingsUser.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyAccountActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private TextView UsernameIcon, EmailIcon, GenderIcon, AgeIcon, CountryIcon, PaypalEmailIcon;
        private EditText EdtUsername, EdtEmail, EdtAge, EdtCountry, EdtPaypalEmail;
        private RadioButton RadioMale, RadioFemale;
        private AppCompatButton BtnSave;
        private Toolbar Toolbar;
        private string Country, IdGender;
        private AdManagerAdView AdManagerAdView;

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
                SetContentView(Resource.Layout.MyAccountLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                GetMyInfoData();
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
                UsernameIcon = FindViewById<TextView>(Resource.Id.IconUsername);
                EdtUsername = FindViewById<EditText>(Resource.Id.UsernameEditText);

                EmailIcon = FindViewById<TextView>(Resource.Id.IconEmail);
                EdtEmail = FindViewById<EditText>(Resource.Id.EmailEditText);

                GenderIcon = FindViewById<TextView>(Resource.Id.IconGender);
                RadioMale = FindViewById<RadioButton>(Resource.Id.radioMale);
                RadioFemale = FindViewById<RadioButton>(Resource.Id.radioFemale);

                AgeIcon = FindViewById<TextView>(Resource.Id.IconAge);
                EdtAge = FindViewById<EditText>(Resource.Id.AgeEditText);

                CountryIcon = FindViewById<TextView>(Resource.Id.IconCountry);
                EdtCountry = FindViewById<EditText>(Resource.Id.CountryEditText);

                PaypalEmailIcon = FindViewById<TextView>(Resource.Id.IconPaypalEmail);
                EdtPaypalEmail = FindViewById<EditText>(Resource.Id.PaypalEmailEditText);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                Methods.SetColorEditText(EdtUsername, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtEmail, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtAge, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtCountry, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtPaypalEmail, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                RadioMale.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                RadioFemale.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, GenderIcon, FontAwesomeIcon.Transgender);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, UsernameIcon, FontAwesomeIcon.Users);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, EmailIcon, FontAwesomeIcon.At);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, AgeIcon, FontAwesomeIcon.BirthdayCake);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, CountryIcon, FontAwesomeIcon.Flag);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PaypalEmailIcon, FontAwesomeIcon.Paypal);

                Methods.SetFocusable(EdtCountry);
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
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = GetString(Resource.String.Lbl_MyAccount);
                    Toolbar.SetTitleTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(Toolbar);
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
                    RadioMale.CheckedChange += RadioMaleOnCheckedChange;
                    RadioFemale.CheckedChange += RadioFemaleOnCheckedChange;
                    BtnSave.Click += BtnSaveOnClick;
                    EdtCountry.Touch += EdtCountryOnClick;
                }
                else
                {
                    RadioMale.CheckedChange -= RadioMaleOnCheckedChange;
                    RadioFemale.CheckedChange -= RadioFemaleOnCheckedChange;
                    BtnSave.Click -= BtnSaveOnClick;
                    EdtCountry.Touch -= EdtCountryOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void RadioFemaleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RadioFemale.Checked;
                if (isChecked)
                {
                    RadioMale.Checked = false;
                    RadioFemale.Checked = true;
                    IdGender = "female";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RadioMaleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                bool isChecked = RadioMale.Checked;
                if (isChecked)
                {
                    RadioMale.Checked = true;
                    RadioFemale.Checked = false;
                    IdGender = "male";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click save data and sent api
        private async void BtnSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {
                        {"username", EdtUsername.Text},
                        {"email", EdtEmail.Text},
                        {"gender",IdGender},
                        {"country",Country},
                        {"age", EdtAge.Text},
                        {"paypal_email", EdtPaypalEmail.Text},
                    };

                    var (apiStatus, respond) = await RequestsAsync.User.UpdateGeneralAsync(UserDetails.UserId.ToString(), dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            var local = ListUtils.MyUserInfoList?.FirstOrDefault();
                            if (local != null)
                            {
                                local.Username = EdtUsername.Text;
                                local.Email = EdtEmail.Text;

                                if (!string.IsNullOrWhiteSpace(EdtAge.Text))
                                    local.Age = Convert.ToInt32(EdtAge.Text);

                                if (!string.IsNullOrWhiteSpace(EdtAge.Text))
                                    local.CountryId = Convert.ToInt32(Country);

                                local.CountryName = EdtCountry.Text;

                                SqLiteDatabase database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyInfo(local);
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_UpdatedSuccessfully), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss();

                            Finish();
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        //Country
        private void EdtCountryOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                var countriesArray = DeepSoundTools.GetCountryList(this);
                var arrayAdapter = countriesArray.Select(item => item.Value).ToList();
                var dialogList = new MaterialAlertDialogBuilder(this);

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

                var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault();
                if (dataUser != null)
                {
                    EdtUsername.Text = dataUser.Username;
                    EdtEmail.Text = dataUser.Email;
                    EdtPaypalEmail.Text = dataUser.PaypalEmail;

                    if (dataUser.Age != 0)
                        EdtAge.Text = dataUser.Age.ToString();

                    var countryName = DeepSoundTools.GetCountryList(this)?.FirstOrDefault(a => a.Key == dataUser.CountryId.ToString()).Value ?? GetText(Resource.String.Lbl_Unknown);

                    EdtCountry.Text = dataUser.CountryId == 0 ? GetText(Resource.String.Lbl_ChooseYourCountry) : countryName;

                    switch (dataUser.Gender)
                    {
                        case "male":
                            RadioMale.Checked = true;
                            RadioFemale.Checked = false;
                            IdGender = "male";
                            break;
                        case "female":
                            RadioMale.Checked = false;
                            RadioFemale.Checked = true;
                            IdGender = "female";
                            break;
                    }

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                Country = DeepSoundTools.GetCountryList(this).FirstOrDefault(a => a.Value == itemString).Key;
                EdtCountry.Text = itemString;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}