using Android.App;
using Android.OS;
using DeepSound.Activities.Albums;
using DeepSound.Activities.Playlist;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Utils;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Linq;
using Exception = System.Exception;

namespace DeepSound.Activities.Library.Listeners
{
    public class LibrarySynchronizer : Java.Lang.Object
    {
        private readonly HomeActivity GlobalContext;
        private readonly Activity ActivityContext;

        public LibrarySynchronizer(Activity activityContext)
        {
            try
            {
                ActivityContext = activityContext;
                GlobalContext = HomeActivity.GetInstance();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AddToRecentlyPlayed(SoundDataObject item)
        {
            try
            {
                var check = GlobalContext?.LibraryFragment?.MAdapter?.SoundsList?.FirstOrDefault(a => a.Id == item.Id);
                if (check != null)
                {
                    GlobalContext.LibraryFragment.MAdapter.SoundsList.Remove(check);
                    GlobalContext.LibraryFragment.MAdapter.SoundsList.Insert(0, item);
                }
                else
                {
                    GlobalContext?.LibraryFragment?.MAdapter?.SoundsList?.Insert(0, item);
                }

                GlobalContext?.LibraryFragment?.MAdapter?.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlaylistMoreOnClick(PlaylistDataObject playlistClass)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("ItemData", JsonConvert.SerializeObject(playlistClass));

                OptionsPlaylistBottomSheet optionsPlaylistBottomSheet = new OptionsPlaylistBottomSheet()
                {
                    Arguments = bundle
                };
                optionsPlaylistBottomSheet.Show(GlobalContext.SupportFragmentManager, optionsPlaylistBottomSheet.Tag);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AlbumsOnMoreClick(DataAlbumsObject albumsClass, ObservableCollection<SoundDataObject> list)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("ItemData", JsonConvert.SerializeObject(albumsClass));
                bundle.PutString("ListSong", JsonConvert.SerializeObject(list));

                OptionsAlbumBottomSheet optionsAlbumBottomSheet = new OptionsAlbumBottomSheet();
                optionsAlbumBottomSheet.Arguments = bundle;
                optionsAlbumBottomSheet.Show(GlobalContext.SupportFragmentManager, optionsAlbumBottomSheet.Tag);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}