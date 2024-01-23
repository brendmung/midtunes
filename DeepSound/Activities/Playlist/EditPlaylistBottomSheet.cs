using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = System.Exception;

namespace DeepSound.Activities.Playlist
{
    public class EditPlaylistBottomSheet : BottomSheetDialogFragment, IDialogListCallBack
    {
        #region Variables Basic

        public static EditPlaylistBottomSheet Instance;
        private HomeActivity GlobalContext;
        public ImageView Image;
        private LinearLayout BtnSelectImage;
        private TextView TxtTitle;
        private EditText TxtName, TxtPrivacy;
        private AppCompatButton BtnCancel, BtnCreate;
        private string Status = "0";
        public string PathImage = "";
        private PlaylistDataObject PlaylistObject;

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
                SetDataPlaylist();
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
                TxtTitle = view.FindViewById<TextView>(Resource.Id.title);
                Image = view.FindViewById<ImageView>(Resource.Id.image);
                BtnSelectImage = view.FindViewById<LinearLayout>(Resource.Id.btn_selectimage);
                TxtName = view.FindViewById<EditText>(Resource.Id.NameEditText);
                TxtPrivacy = view.FindViewById<EditText>(Resource.Id.PrivacyEditText);
                BtnCancel = view.FindViewById<AppCompatButton>(Resource.Id.btnCancel);
                BtnCreate = view.FindViewById<AppCompatButton>(Resource.Id.btnCreate);

                Methods.SetColorEditText(TxtName, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPrivacy, DeepSoundTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtPrivacy);

                TxtTitle.Text = GetText(Resource.String.Lbl_EditPlaylist);
                BtnCreate.Text = GetText(Resource.String.Lbl_Edit);

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
                GlobalContext.OpenDialogGallery("EditPlaylist");
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

                    //Show a progress
                    AndHUD.Shared.Show(Context, GetText(Resource.String.Lbl_Loading));
                    var (apiStatus, respond) = await RequestsAsync.Playlist.UpdatePlaylistAsync(PlaylistObject.Id.ToString(), TxtName.Text, Status, PathImage); //Sent api 
                    if (apiStatus.Equals(200))
                    {
                        if (respond is CreatePlaylistObject result)
                        {
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss();

                            var playlistData = ListUtils.PlaylistList.FirstOrDefault(a => a.Id == PlaylistObject?.Id);
                            if (playlistData != null)
                            {
                                playlistData.Name = TxtName.Text;
                                playlistData.Privacy = Convert.ToInt32(Status);

                                if (!string.IsNullOrEmpty(PathImage))
                                    playlistData.ThumbnailReady = PathImage;
                            }

                            var dataMyPlaylistFragment = GlobalContext?.ProfileFragment?.PlaylistFragment;
                            var dataMyPlaylist = dataMyPlaylistFragment?.MAdapter?.PlaylistList?.FirstOrDefault(a => a.Id == PlaylistObject?.Id);
                            if (dataMyPlaylist != null)
                            {
                                dataMyPlaylist.Name = TxtName.Text;
                                dataMyPlaylist.Privacy = Convert.ToInt32(Status);

                                if (!string.IsNullOrEmpty(PathImage))
                                    dataMyPlaylist.ThumbnailReady = PathImage;

                                dataMyPlaylistFragment?.MAdapter?.NotifyDataSetChanged();
                            }

                            Dismiss();
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

        private void SetDataPlaylist()
        {
            try
            {
                PlaylistObject = JsonConvert.DeserializeObject<PlaylistDataObject>(Arguments?.GetString("ItemData") ?? "");
                if (PlaylistObject != null)
                {
                    PathImage = "";
                    GlideImageLoader.LoadImage(Activity, PlaylistObject.Thumbnail, Image, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                    TxtName.Text = Methods.FunString.DecodeString(PlaylistObject.Name);

                    if (PlaylistObject.Privacy == 0)
                    {
                        TxtPrivacy.Text = GetText(Resource.String.Lbl_Public);
                        Status = "0";
                    }
                    else
                    {
                        TxtPrivacy.Text = GetText(Resource.String.Lbl_Private);
                        Status = "1";
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