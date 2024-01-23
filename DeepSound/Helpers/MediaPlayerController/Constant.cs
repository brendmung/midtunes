using Android.Content;
using DeepSoundClient.Classes.Global;
using System;
using System.Collections.ObjectModel;
using Androidx.Media3.Exoplayer;

namespace DeepSound.Helpers.MediaPlayerController
{
    public static class Constant
    {
        public static bool IsLoggingOut { get; set; } = false;
        public static bool IsChangingTheme { get; set; } = false;
        public static bool IsRepeat { get; set; } = false;
        public static bool IsPlayed { get; set; } = false;
        public static bool IsSuffle { get; set; } = false;
        public static bool IsOnline { get; set; } = true;
        public static bool IsNewSong { get; set; } = false;

        public static IExoPlayer Player { get; set; }

        public static ObservableCollection<SoundDataObject> ArrayListPlay = new ObservableCollection<SoundDataObject>();
        public static ObservableCollection<SoundDataObject> ArrayPlayingQueue = new ObservableCollection<SoundDataObject>();
        public static int PlayPos { get; set; } = 0;
        public static Context Context { get; set; }

        public static bool ShouldShowMusicPurchaseDialog()
        {
            try
            {
                var item = ArrayListPlay[PlayPos];
                if (item == null)
                    return false;

                if (item.IsOwner != null && item.IsOwner.Value || item.Price == 0)
                    return false;

                if (item.IsPurchased != null && item.IsPurchased.Value)
                    return false;

                if (item.Price != 0 && !string.IsNullOrEmpty(item.DemoTrack))
                    return true;

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}