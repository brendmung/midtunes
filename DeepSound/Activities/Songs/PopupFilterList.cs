using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Albums.Adapters;
using DeepSound.Activities.Playlist.Adapters;
using DeepSound.Activities.Songs.Adapters;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeepSound.Activities.Songs
{
    public class PopupFilterList
    {
        private readonly Activity ActivityContext;
        private readonly RowSoundAdapter SoundAdapter;
        private readonly HAlbumsAdapter AlbumsAdapter;
        private readonly PlaylistRowAdapter PlaylistAdapter;

        public RelativeLayout TopLayout;
        public TextView TxtSongName, TxtSwap;
        private readonly string Type = "";

        public PopupFilterList(View view, Activity activity, RowSoundAdapter adapter)
        {
            try
            {
                ActivityContext = activity;
                SoundAdapter = adapter;

                Type = "Songs";

                Init(view);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public PopupFilterList(View view, Activity activity, HAlbumsAdapter adapter)
        {
            try
            {
                ActivityContext = activity;
                AlbumsAdapter = adapter;

                Type = "Albums";

                Init(view);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public PopupFilterList(View view, Activity activity, PlaylistRowAdapter adapter)
        {
            try
            {
                ActivityContext = activity;
                PlaylistAdapter = adapter;

                Type = "Playlist";

                Init(view);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Init(View view)
        {
            try
            {
                TopLayout = view.FindViewById<RelativeLayout>(Resource.Id.Toplayout);
                TxtSongName = view.FindViewById<TextView>(Resource.Id.textView_songname);
                TxtSwap = view.FindViewById<TextView>(Resource.Id.swaptext);
                TxtSwap.Click += TxtSwapOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static string Filter = "0";
        private static string NameType = "";
        private ImageView MenuCheckAscending, MenuCheckDescending, MenuCheckDataAdded;
        private void TxtSwapOnClick(object sender, EventArgs e)
        {
            try
            {
                LayoutInflater layoutInflater = (LayoutInflater)ActivityContext?.GetSystemService(Context.LayoutInflaterService);
                View popupView = layoutInflater?.Inflate(Resource.Layout.PopupFilterSongLayout, null);

                int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 180, ActivityContext.Resources.DisplayMetrics);
                var popupWindow = new PopupWindow(popupView, px, ViewGroup.LayoutParams.WrapContent);

                var menuAscending = popupView.FindViewById<LinearLayout>(Resource.Id.menu_ascending);
                MenuCheckAscending = popupView.FindViewById<ImageView>(Resource.Id.menu_check_ascending);

                var menuDescending = popupView.FindViewById<LinearLayout>(Resource.Id.menu_descending);
                MenuCheckDescending = popupView.FindViewById<ImageView>(Resource.Id.menu_check_descending);

                var menuDataAdded = popupView.FindViewById<LinearLayout>(Resource.Id.menu_dataAdded);
                MenuCheckDataAdded = popupView.FindViewById<ImageView>(Resource.Id.menu_check_dataAdded);

                CheckType(Filter);

                //By name Ascending
                menuAscending.Click += (sender, args) =>
                {
                    try
                    {
                        Filter = "1";
                        NameType = ActivityContext.GetText(Resource.String.Lbl_Ascending);

                        MenuCheckAscending.SetImageResource(Resource.Drawable.ic_check_circle);

                        CheckType(Filter);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                //By name Descending
                menuDescending.Click += (sender, args) =>
                {
                    try
                    {
                        Filter = "2";
                        NameType = ActivityContext.GetText(Resource.String.Lbl_Descending);

                        MenuCheckDescending.SetImageResource(Resource.Drawable.ic_check_circle);

                        CheckType(Filter);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                //By Data Added
                menuDataAdded.Click += (sender, args) =>
                {
                    try
                    {
                        Filter = "3";
                        NameType = ActivityContext.GetText(Resource.String.Lbl_DataAdded);

                        MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_check_circle);

                        CheckType(Filter);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                MenuCheckAscending.SetImageResource(Resource.Drawable.ic_uncheck_circle);

                popupWindow.SetBackgroundDrawable(new ColorDrawable());
                popupWindow.Focusable = true;
                popupWindow.ClippingEnabled = true;
                popupWindow.OutsideTouchable = false;
                popupWindow.DismissEvent += delegate (object sender, EventArgs args)
                {
                    try
                    {
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                popupWindow.ShowAsDropDown(TxtSwap);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CheckType(string type)
        {
            try
            {
                TxtSwap.Text = NameType;

                switch (type)
                {
                    case "1":
                        MenuCheckAscending.SetImageResource(Resource.Drawable.ic_check_circle);

                        MenuCheckDescending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
                        MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_uncheck_circle);
                        break;
                    case "2":
                        MenuCheckDescending.SetImageResource(Resource.Drawable.ic_check_circle);

                        MenuCheckAscending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
                        MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_uncheck_circle);
                        break;
                    case "3":
                        MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_check_circle);

                        MenuCheckAscending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
                        MenuCheckDescending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
                        break;
                    default:
                        MenuCheckAscending.SetImageResource(Resource.Drawable.ic_check_circle);

                        MenuCheckDescending.SetImageResource(Resource.Drawable.ic_uncheck_circle);
                        MenuCheckDataAdded.SetImageResource(Resource.Drawable.ic_uncheck_circle);
                        TxtSwap.Text = ActivityContext.GetText(Resource.String.Lbl_Ascending);
                        break;
                }

                if (type == "0")
                    return;

                if (Type == "Songs")
                {
                    var list = SoundAdapter.SoundsList.Where(sound => sound.IsPlay).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var all in list)
                        {
                            all.IsPlay = false;

                            var index = SoundAdapter.SoundsList.IndexOf(all);
                            SoundAdapter.NotifyItemChanged(index);
                        }
                    }

                    List<SoundDataObject> sortList = new List<SoundDataObject>(SoundAdapter.SoundsList);
                    switch (type)
                    {
                        case "1":
                            sortList = SoundAdapter.SoundsList.OrderBy(sound => sound.Title).ToList();
                            break;
                        case "2":
                            sortList = SoundAdapter.SoundsList.OrderByDescending(sound => sound.Title).ToList();
                            break;
                        case "3":
                            sortList = SoundAdapter.SoundsList.OrderBy(sound => sound.Time).ToList();
                            break;
                    }

                    SoundAdapter.SoundsList = new ObservableCollection<SoundDataObject>(sortList);
                    SoundAdapter.NotifyDataSetChanged();
                }
                else if (Type == "Albums")
                {
                    List<DataAlbumsObject> sortList = new List<DataAlbumsObject>(AlbumsAdapter.AlbumsList);
                    switch (type)
                    {
                        case "1":
                            sortList = AlbumsAdapter.AlbumsList.OrderBy(sound => sound.Title).ToList();
                            break;
                        case "2":
                            sortList = AlbumsAdapter.AlbumsList.OrderByDescending(sound => sound.Title).ToList();
                            break;
                        case "3":
                            sortList = AlbumsAdapter.AlbumsList.OrderBy(sound => sound.Time).ToList();
                            break;
                    }

                    AlbumsAdapter.AlbumsList = new ObservableCollection<DataAlbumsObject>(sortList);
                    AlbumsAdapter.NotifyDataSetChanged();
                }
                else if (Type == "Playlist")
                {
                    List<PlaylistDataObject> sortList = new List<PlaylistDataObject>(PlaylistAdapter.PlaylistList);
                    switch (type)
                    {
                        case "1":
                            sortList = PlaylistAdapter.PlaylistList.OrderBy(sound => sound.Name).ToList();
                            break;
                        case "2":
                            sortList = PlaylistAdapter.PlaylistList.OrderByDescending(sound => sound.Name).ToList();
                            break;
                        case "3":
                            sortList = PlaylistAdapter.PlaylistList.OrderBy(sound => sound.Time).ToList();
                            break;
                    }

                    PlaylistAdapter.PlaylistList = new ObservableCollection<PlaylistDataObject>(sortList);
                    var data = PlaylistAdapter.PlaylistList.FirstOrDefault(a => a.Name == ActivityContext.GetText(Resource.String.Lbl_AddNewPlaylist));
                    if (data != null)
                        PlaylistAdapter.PlaylistList.Move(sortList.IndexOf(data), 0);

                    PlaylistAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}