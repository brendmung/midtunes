using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using DeepSound.Activities.Tabbes;
using DeepSound.Adapters;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
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

namespace DeepSound.Activities.Playlist
{
    public class OptionsPlaylistBottomSheet : BottomSheetDialogFragment
    {
        #region Variables Basic

        private HomeActivity GlobalContext;
        private ImageView Image, IconHeart;
        private TextView TxtTitle, TxtSeconderText;

        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ItemOptionAdapter MAdapter;

        private PlaylistDataObject PlaylistObject;

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
                InitComponent(view);
                SetRecyclerViewAdapters(view);

                LoadDataChat();
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
                        OnMenuEditPlaylistOnClick();
                        Dismiss();
                    }
                    else if (item?.Id == "2") //Delete
                    {
                        OnMenuDeletePlaylistOnClick();
                    }
                    else if (item?.Id == "3") //Share
                    {
                        SharePlaylist();
                        Dismiss();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void SharePlaylist()
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
                    Title = PlaylistObject?.Name,
                    Text = "",
                    Url = PlaylistObject?.Url
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OnMenuDeletePlaylistOnClick()
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
                    dialog.SetTitle(GlobalContext.GetText(Resource.String.Lbl_DeletePlaylist));
                    dialog.SetMessage(GlobalContext.GetText(Resource.String.Lbl_AreYouSureDeletePlaylist));
                    dialog.SetPositiveButton(Activity.GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                    {
                        try
                        {
                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (Methods.CheckConnectivity())
                                    {
                                        var dataPlaylist = ListUtils.PlaylistList?.FirstOrDefault(a => a.Id == PlaylistObject?.Id);
                                        if (dataPlaylist != null)
                                        {
                                            ListUtils.PlaylistList.Remove(dataPlaylist);
                                        }

                                        var dataMyPlaylistFragment = GlobalContext?.LibraryFragment.MyPlaylistFragment?.MAdapter;
                                        var list2 = dataMyPlaylistFragment?.PlaylistList;
                                        var dataMyPlaylist = list2?.FirstOrDefault(a => a.Id == PlaylistObject?.Id);
                                        if (dataMyPlaylist != null)
                                        {
                                            int index = list2.IndexOf(dataMyPlaylist);
                                            if (index >= 0)
                                            {
                                                list2?.Remove(dataMyPlaylist);
                                                dataMyPlaylistFragment?.NotifyItemRemoved(index);
                                            }
                                        }

                                        Toast.MakeText(GlobalContext, GlobalContext.GetText(Resource.String.Lbl_PlaylistSuccessfullyDeleted), ToastLength.Short)?.Show();

                                        //Sent Api >>
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Playlist.DeletePlaylistAsync(PlaylistObject?.Id.ToString()) });
                                        Dismiss();
                                    }
                                    else
                                    {
                                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
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

        private void OnMenuEditPlaylistOnClick()
        {
            try
            {
                if (!UserDetails.IsLogin)
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Login), Activity.GetText(Resource.String.Lbl_Message_Sorry_signin), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                    return;
                }

                Bundle bundle = new Bundle();
                bundle.PutString("ItemData", JsonConvert.SerializeObject(PlaylistObject));
                bundle.PutString("PlaylistId", PlaylistObject.Id.ToString());

                EditPlaylistBottomSheet editPlaylistBottomSheet = new EditPlaylistBottomSheet()
                {
                    Arguments = bundle
                };
                editPlaylistBottomSheet.Show(GlobalContext.SupportFragmentManager, editPlaylistBottomSheet.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void LoadDataChat()
        {
            try
            {
                PlaylistObject = JsonConvert.DeserializeObject<PlaylistDataObject>(Arguments?.GetString("ItemData") ?? "");
                if (PlaylistObject != null)
                {
                    Glide.With(this).AsBitmap().Load(PlaylistObject.ThumbnailReady).Into(Image);

                    var d = PlaylistObject.Name.Replace("<br>", "");
                    TxtTitle.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(d), 70);

                    if (!string.IsNullOrEmpty(PlaylistObject.Songs.ToString()))
                        TxtSeconderText.Text = PlaylistObject.Songs + " " + GetText(Resource.String.Lbl_Songs);
                    else
                        TxtSeconderText.Text = "0 " + GetText(Resource.String.Lbl_Songs);

                    if (PlaylistObject.IsOwner && UserDetails.IsLogin)
                    {
                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "1",
                            Text = GetText(Resource.String.Lbl_EditPlaylist),
                            Icon = Resource.Drawable.icon_edit_vector,
                        });

                        MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                        {
                            Id = "2",
                            Text = GetText(Resource.String.Lbl_DeletePlaylist),
                            Icon = Resource.Drawable.icon_delete_vector,
                        });
                    }

                    MAdapter.ItemOptionList.Add(new Classes.ItemOptionObject()
                    {
                        Id = "3",
                        Text = GetText(Resource.String.Lbl_Share),
                        Icon = Resource.Drawable.icon_send_vector,
                    });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}