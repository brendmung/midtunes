using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.RecyclerView.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Adapters;
using DeepSound.Helpers.CacheLoaders;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.MediaPlayerController;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Requests;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace DeepSound.Activities.Albums
{
    public class OptionsAlbumBottomSheet : BottomSheetDialogFragment
    {
        public static OptionsAlbumBottomSheet Instance;

        #region Variables Basic

        private HomeActivity GlobalContext;
        private ImageView Image, IconHeart;
        private TextView TxtTitle, TxtSeconderText;

        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ItemOptionAdapter MAdapter;

        private DataAlbumsObject AlbumsObject;
        private ObservableCollection<SoundDataObject> ListSong;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = DeepSoundTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.BottomSheetDefaultLayout, container, false);
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

                Instance = this;

                InitComponent(view);
                SetRecyclerViewAdapters(view);

                LoadDataAlbum();
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                Image = view.FindViewById<ImageView>(Resource.Id.Image);
                TxtTitle = view.FindViewById<TextView>(Resource.Id.title);
                TxtSeconderText = view.FindViewById<TextView>(Resource.Id.brief);
                IconHeart = view.FindViewById<ImageView>(Resource.Id.heart);
                IconHeart.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);

                MAdapter = new ItemOptionAdapter(Activity)
                {
                    ItemOptionList = new ObservableCollection<Classes.ItemOptionObject>()
                };
                MAdapter.ItemClick += MAdapterItemClick;
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.GetRecycledViewPool().Clear();
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void MAdapterItemClick(object sender, ItemOptionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = MAdapter.GetItem(position);
                    if (item?.Id == "1") //Edit
                    {
                        OnMenuEditAlbumOnClick();
                        Dismiss();
                    }
                    else if (item?.Id == "2") //Delete
                    {
                        OnMenuDeleteAlbumOnClick();
                    }
                    else if (item?.Id == "3") //Share
                    {
                        ShareAlbum();
                        Dismiss();
                    }
                    else if (item?.Id == "4") //Add to Playlist
                    {
                        AlbumToPlaylist();
                        Dismiss();
                    }
                    else if (item?.Id == "5") //download Album
                    {
                        if (PermissionsController.CheckPermissionStorage(Context))
                        {
                            SoundDownloadAlbum();
                        }
                        else
                        {
                            GlobalContext.PermissionsType = "DownloadAlbum";
                            new PermissionsController(Activity).RequestPermission(100);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SoundDownloadAlbum()
        {
            try
            {
                var isPro = ListUtils.MyUserInfoList?.FirstOrDefault()?.IsPro ?? 0;
                if (isPro == 0)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "GoPro");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_ActivateWithUpgraded), GetText(Resource.String.Lbl_Ok), GetText(Resource.String.Lbl_Cancel));
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                AndHUD.Shared.Show(Context, GetText(Resource.String.Lbl_Downloading));

                Methods.Path.Chack_MyFolder();
                 
                foreach (var item in ListSong)
                {
                    var SoundDownload = new SoundDownloadAsyncController(item.AudioLocation, item.Title, Activity);
                    if (!SoundDownload.CheckDownloadLinkIfExits())
                    {
                        SoundDownload.StartDownloadManager(AlbumsObject.Title, item, "Album");
                    }
                }

                AndHUD.Shared.Dismiss();
                Dismiss(); 
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void AlbumToPlaylist()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Context.GetText(Resource.String.Lbl_Login), Context.GetText(Resource.String.Lbl_Message_Sorry_signin), Context.GetText(Resource.String.Lbl_Yes), Context.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                var fileName = AlbumsObject.Thumbnail.Split('/').Last();
                var image = DeepSoundTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskImage, fileName, AlbumsObject.Thumbnail);

                AndHUD.Shared.Show(Context, GetText(Resource.String.Lbl_Loading));
                var (apiStatus, respond) = await RequestsAsync.Playlist.CreatePlaylistAsync(AlbumsObject.Title, "1", image); //Sent api 
                if (apiStatus.Equals(200))
                {
                    if (respond is CreatePlaylistObject result)
                    {
                        foreach (var song in ListSong)
                        {
                            if (song != null)
                            {
                                //Sent Api
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Playlist.AddToPlaylistAsync(song.Id.ToString(), result.Data.Id.ToString()) });
                            }
                        }

                        result.Data.Songs = ListSong.Count;
                        ListUtils.PlaylistList.Add(result.Data);

                        AndHUD.Shared.Dismiss();
                        Dismiss();
                    }
                }
                else
                {
                    Methods.DisplayAndHudErrorResult(Activity, respond);
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void ShareAlbum()
        {
            try
            {
                //Share Plugin same as Song
                if (!CrossShare.IsSupported)
                {
                    return;
                }

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = AlbumsObject?.Title,
                    Text = "",
                    Url = AlbumsObject?.Url
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OnMenuDeleteAlbumOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Login), Activity.GetText(Resource.String.Lbl_Message_Sorry_signin), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    var dialog = new MaterialAlertDialogBuilder(Activity);
                    dialog.SetTitle(GlobalContext.GetText(Resource.String.Lbl_DeleteAlbum));
                    dialog.SetMessage(GlobalContext.GetText(Resource.String.Lbl_AreYouSureDeleteAlbum));
                    dialog.SetPositiveButton(Activity.GetText(Resource.String.Lbl_YesButKeepSongs), (sender, args) =>
                    {
                        try
                        {
                            var dataAlbumFragment = GlobalContext?.HomeFragment?.LatestHomeTab?.AlbumsAdapter;
                            var list2 = dataAlbumFragment?.AlbumsList;
                            var dataMyAlbum = list2?.FirstOrDefault(a => a.Id == AlbumsObject?.Id);
                            if (dataMyAlbum != null)
                            {
                                int index = list2.IndexOf(dataMyAlbum);
                                if (index >= 0)
                                {
                                    list2?.Remove(dataMyAlbum);
                                    dataAlbumFragment?.NotifyItemRemoved(index);
                                }
                            }

                            Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_AlbumSuccessfullyDeleted), ToastLength.Short)?.Show();

                            //Sent Api >>
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Albums.DeleteAlbumAsync("single", AlbumsObject?.Id.ToString()) });
                            Dismiss();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.SetNegativeButton(Activity.GetText(Resource.String.Lbl_YesDeleteEverything), (sender, args) =>
                    {
                        try
                        {
                            var dataAlbumFragment = GlobalContext?.HomeFragment?.LatestHomeTab?.AlbumsAdapter;
                            var list2 = dataAlbumFragment?.AlbumsList;
                            var dataMyAlbum = list2?.FirstOrDefault(a => a.Id == AlbumsObject?.Id);
                            if (dataMyAlbum != null)
                            {
                                int index = list2.IndexOf(dataMyAlbum);
                                if (index >= 0)
                                {
                                    list2?.Remove(dataMyAlbum);
                                    dataAlbumFragment?.NotifyItemRemoved(index);
                                }
                            }

                            Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_AlbumSuccessfullyDeleted), ToastLength.Short)?.Show();

                            //Sent Api >>
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Albums.DeleteAlbumAsync("all", AlbumsObject?.Id.ToString()) });
                            Dismiss();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.SetNegativeButton(Activity.GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                    dialog.Show();
                }
                else
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OnMenuEditAlbumOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Login), Activity.GetText(Resource.String.Lbl_Message_Sorry_signin), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                    return;
                }

                //Bundle bundle = new Bundle();
                //bundle.PutString("ItemData", JsonConvert.SerializeObject(AlbumsObject));
                //bundle.PutString("AlbumId", AlbumsObject.Id.ToString());

                //EditAlbumBottomSheet editAlbumBottomSheet = new EditAlbumBottomSheet()
                //{
                //    Arguments = bundle
                //};
                //editAlbumBottomSheet.Show(GlobalContext.SupportFragmentManager, editAlbumBottomSheet.Tag);

                Intent intent = new Intent(GlobalContext, typeof(EditAlbumActivity));
                intent.PutExtra("ItemData", JsonConvert.SerializeObject(AlbumsObject));
                GlobalContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void LoadDataAlbum()
        {
            try
            {
                AlbumsObject = JsonConvert.DeserializeObject<DataAlbumsObject>(Arguments?.GetString("ItemData") ?? "");
                if (AlbumsObject != null)
                {
                    GlideImageLoader.LoadImage(Activity, AlbumsObject.Thumbnail, Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    TxtTitle.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(AlbumsObject.Title), 80);

                    var count = !string.IsNullOrEmpty(AlbumsObject.CountSongs) ? AlbumsObject.CountSongs : AlbumsObject.SongsCount ?? "0";
                    var text = count + " " + Context.GetText(Resource.String.Lbl_Songs);
                    if (AppSettings.ShowCountPurchases)
                        text = text + " - " + AlbumsObject.Purchases + " " + Context.GetText(Resource.String.Lbl_Purchases);

                    TxtSeconderText.Text = text;

                    if (AlbumsObject.IsOwner != null && AlbumsObject.IsOwner.Value && UserDetails.IsLogin)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "1",
                            Text = GetText(Resource.String.Lbl_EditAlbum),
                            Icon = Resource.Drawable.icon_edit_vector,
                        });

                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "2",
                            Text = GetText(Resource.String.Lbl_DeleteAlbum),
                            Icon = Resource.Drawable.icon_delete_vector,
                        });
                    }

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                    {
                        Id = "3",
                        Text = GetText(Resource.String.Lbl_Share),
                        Icon = Resource.Drawable.icon_send_vector,
                    });

                    ListSong = JsonConvert.DeserializeObject<ObservableCollection<SoundDataObject>>(Arguments?.GetString("ListSong") ?? "");
                    if (ListSong?.Count > 0 && (Math.Abs(AlbumsObject.Price) <= 0 || AlbumsObject.IsPurchased == 1))
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "4",
                            Text = GetText(Resource.String.Lbl_Add_To_Playlist),
                            Icon = Resource.Drawable.icon_add_vector,
                        });

                        var fileName = AlbumsObject.Thumbnail.Split('/').Last();
                        DeepSoundTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskImage, fileName, AlbumsObject.Thumbnail);


                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "5",
                            Text = GetText(Resource.String.Lbl_Downloads),
                            Icon = Resource.Drawable.icon_player_download,
                        });

                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}