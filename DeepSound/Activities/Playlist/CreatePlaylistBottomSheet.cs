using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using System;
using System.Collections.Generic;
using Exception = System.Exception;

namespace DeepSound.Activities.Playlist
{
    public class CreatePlaylistBottomSheet : BottomSheetDialogFragment, IDialogListCallBack
    {
        #region Variables Basic

        public static CreatePlaylistBottomSheet Instance;
        private HomeActivity GlobalContext;
        public ImageView Image;
        private LinearLayout BtnSelectImage;
        private EditText TxtName, TxtPrivacy;
        private AppCompatButton BtnCancel, BtnCreate;
        private string Status = "0";
        public string PathImage = "";

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
            Instance = this;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = DeepSoundTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.CreatePlaylistLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                InitComponent(view);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        public override void OnDestroy()
        {
            try
            {
                Instance = null;
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                Image = view.FindViewById<ImageView>(Resource.Id.image);
                BtnSelectImage = view.FindViewById<LinearLayout>(Resource.Id.btn_selectimage);
                TxtName = view.FindViewById<EditText>(Resource.Id.NameEditText);
                TxtPrivacy = view.FindViewById<EditText>(Resource.Id.PrivacyEditText);
                BtnCancel = view.FindViewById<AppCompatButton>(Resource.Id.btnCancel);
                BtnCreate = view.FindViewById<AppCompatButton>(Resource.Id.btnCreate);

                Methods.SetColorEditText(TxtName, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPrivacy, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtPrivacy);

                TxtPrivacy.Touch += TxtPrivacyOnTouch;
                BtnSelectImage.Click += BtnSelectImageOnClick;
                BtnCancel.Click += BtnCancelOnClick;
                BtnCreate.Click += BtnCreateOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void TxtPrivacyOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var dialogList = new MaterialAlertDialogBuilder(Context);

                var arrayAdapter = new List<string>
                {
                    GetText(Resource.String.Lbl_Public),
                    GetText(Resource.String.Lbl_Private)
                };

                dialogList.SetTitle(GetText(Resource.String.Lbl_Privacy));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnSelectImageOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.OpenDialogGallery("CreatePlaylist");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void BtnCreateOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtName.Text))
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_PleaseEnterName), ToastLength.Short)?.Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(PathImage))
                    {
                        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_PleaseSelectImage), ToastLength.Short)?.Show();
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(Context, GetText(Resource.String.Lbl_Loading));
                    var (apiStatus, respond) = await RequestsAsync.Playlist.CreatePlaylistAsync(TxtName.Text, Status, PathImage); //Sent api 
                    if (apiStatus.Equals(200))
                    {
                        if (respond is CreatePlaylistObject result)
                        {
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss();

                            try
                            {
                                ListUtils.PlaylistList.Add(result.Data);

                                var dataMyPlaylistFragment = GlobalContext?.ProfileFragment?.PlaylistFragment;
                                dataMyPlaylistFragment?.MAdapter?.PlaylistList.Insert(0, result.Data);
                                dataMyPlaylistFragment?.MAdapter?.NotifyDataSetChanged();

                                dataMyPlaylistFragment?.ShowEmptyPage();

                                Dismiss();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(Activity, respond);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        private void BtnCancelOnClick(object sender, EventArgs e)
        {
            Dismiss();
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_Public))
                {
                    Status = "0";
                }
                else if (itemString == GetText(Resource.String.Lbl_Private))
                {
                    Status = "1";
                }
                TxtPrivacy.Text = itemString;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}