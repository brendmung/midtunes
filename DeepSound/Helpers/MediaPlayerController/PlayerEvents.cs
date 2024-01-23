using DeepSound.Helpers.Utils;
using System;
using Androidx.Media3.Common;
using Androidx.Media3.Common.Text;
using Object = Java.Lang.Object;

namespace DeepSound.Helpers.MediaPlayerController
{
    public class PlayerEvents : Object, IPlayer.IListener
    {
        private static PlayerService PlayerController;

        public PlayerEvents(PlayerService controller)
        {
            PlayerController = controller;
        }

        public void OnAudioAttributesChanged(AudioAttributes audioAttributes)
        {

        }

        public void OnAudioSessionIdChanged(int audioSessionId)
        {

        }

        public void OnAvailableCommandsChanged(IPlayer.Commands availableCommands)
        {

        }

        public void OnCues(CueGroup cueGroup)
        {

        }

        public void OnDeviceInfoChanged(DeviceInfo deviceInfo)
        {

        }

        public void OnDeviceVolumeChanged(int volume, bool muted)
        {

        }

        public void OnEvents(IPlayer player, IPlayer.Events events)
        {

        }

        public void OnIsLoadingChanged(bool isLoading)
        {

        }

        public void OnIsPlayingChanged(bool isPlaying)
        {

        }

        public void OnLoadingChanged(bool p0)
        {

        }

        public void OnMaxSeekToPreviousPositionChanged(long maxSeekToPreviousPositionMs)
        {

        }

        public void OnMediaItemTransition(MediaItem mediaItem, int reason)
        {

        }

        public void OnMediaMetadataChanged(MediaMetadata mediaMetadata)
        {

        }

        public void OnMetadata(Metadata metadata)
        {

        }

        public void OnPlayWhenReadyChanged(bool playWhenReady, int reason)
        {

        }

        public void OnPlaybackParametersChanged(PlaybackParameters p0)
        {

        }

        public void OnPlaybackStateChanged(int playbackState)
        {

        }

        public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
        {

        }

        public void OnPlayerError(PlaybackException error)
        {
            try
            {
                Constant.Player.PlayWhenReady = false;
                PlayerController.SetBuffer(false);
                PlayerController.ChangePlayPauseIcon();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPlayerErrorChanged(PlaybackException error)
        {

        }

        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            try
            {
                if (playbackState == IPlayer.StateEnded)
                {
                    PlayerController.OnCompletion();
                }
                else if (playbackState == IPlayer.StateReady && playWhenReady)
                {

                }

                if (playWhenReady)
                {
                    PlayerController.GlobalContext?.SetWakeLock();
                }
                else
                {
                    PlayerController.GlobalContext?.OffWakeLock();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPlaylistMetadataChanged(MediaMetadata mediaMetadata)
        {

        }

        public void OnPositionDiscontinuity(int p0)
        {

        }

        public void OnRenderedFirstFrame()
        {

        }

        public void OnRepeatModeChanged(int p0)
        {

        }

        public void OnSeekBackIncrementChanged(long seekBackIncrementMs)
        {

        }

        public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs)
        {

        }

        public void OnSeekProcessed()
        {

        }

        public void OnShuffleModeEnabledChanged(bool p0)
        {

        }

        public void OnSkipSilenceEnabledChanged(bool skipSilenceEnabled)
        {

        }

        public void OnSurfaceSizeChanged(int width, int height)
        {

        }

        public void OnTimelineChanged(Timeline timeline, int reason)
        {

        }

        public void OnTrackSelectionParametersChanged(TrackSelectionParameters parameters)
        {

        }

        public void OnTracksChanged(Tracks tracks)
        {

        }

        public void OnVideoSizeChanged(VideoSize videoSize)
        {

        }

        public void OnVolumeChanged(float volume)
        {

        }
    }
}