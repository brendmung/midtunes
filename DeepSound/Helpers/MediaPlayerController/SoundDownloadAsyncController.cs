﻿//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) DeepSound 25/04/2019 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Global;
using System;
using System.IO;
using System.Linq;
using Environment = Android.OS.Environment;

namespace DeepSound.Helpers.MediaPlayerController
{
    public class SoundDownloadAsyncController
    {
        private readonly DownloadManager DownloadManager;
        private readonly DownloadManager.Request Request;
        public static string FilePath = Android.OS.Environment.DirectoryDownloads + "/" + AppSettings.ApplicationName;
        private readonly string Filename;
        private long DownloadId;
        private string FromActivity;
        private SoundDataObject SoundData;
        private static Activity ActivityContext;

        public SoundDownloadAsyncController(string url, string filename, Activity contextActivity)
        {
            try
            {
                ActivityContext = HomeActivity.GetInstance();

                if (!filename.Contains(".mp3"))
                    Filename = filename + ".mp3";
                else
                    Filename = filename;

                DownloadManager = (DownloadManager)ActivityContext.GetSystemService(Context.DownloadService);
                Request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StartDownloadManager(string title, SoundDataObject sound, string fromActivity)
        {
            try
            {
                Methods.Path.Chack_MyFolder();

                if (sound != null && !string.IsNullOrEmpty(title))
                {
                    SoundData = sound;
                    FromActivity = fromActivity;

                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Insert_LatestDownloadsSound(sound);

                    var folder = GetDownloadedFolder();
 
                    Request.SetTitle(title);
                    Request.SetAllowedNetworkTypes(DownloadNetwork.Mobile | DownloadNetwork.Wifi);

                    Request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, "/" + AppSettings.ApplicationName + "/" + Filename);

                    Request.SetNotificationVisibility(DownloadVisibility.Visible);
                    Request.SetAllowedOverRoaming(true);
                    DownloadId = DownloadManager.Enqueue(Request);

                    var onDownloadComplete = new OnDownloadComplete
                    {
                        TypeActivity = fromActivity,
                        Sound = sound
                    };

                    ActivityContext.RegisterReceiver(onDownloadComplete, new IntentFilter(DownloadManager.ActionDownloadComplete));
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Download_failed), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopDownloadManager()
        {
            try
            {
                DownloadManager.Remove(DownloadId);
                RemoveDiskSoundFile(Filename, SoundData.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public bool RemoveDiskSoundFile(string filename, long id)
        {
            try
            { 
                var directories = GetDownloadedFolder();
                string path = new Java.IO.File(directories, filename + ".mp3").Path;
                 
                if (File.Exists(path))
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Remove_LatestDownloadsSound(id);
                    File.Delete(path);
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public bool CheckDownloadLinkIfExits()
        {
            try
            {
                if (File.Exists(FilePath + Filename))
                    return true;

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public static string GetDownloadedFolder()
        {
            try
            {
                string directories;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    var directories1 = Application.Context.GetExternalFilesDir(""); //storage/emulated/0/Android/data/****
                    var pathDefault = directories1.AbsolutePath.Split("/Android/")?.FirstOrDefault();
                    directories = pathDefault + "/" + Environment.DirectoryDownloads + "/" + AppSettings.ApplicationName;
                      
                    //var directories = Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads + "/" + AppSettings.ApplicationName);
                }
                else
                {
                    directories = Methods.Path.GetDirectoryDcim() + "/" + Environment.DirectoryDownloads + "/" + AppSettings.ApplicationName;
                }
                 
                if (!Directory.Exists(directories))
                    Directory.CreateDirectory(directories);

                return directories;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return "";
            }
        }

        public static string GetDownloadedDiskVideoUri(string filename)
        {
            try
            {
                var directories = GetDownloadedFolder(); 
                Java.IO.File file = new Java.IO.File(directories, filename + ".mp3");
               
                //Hbh14ktZ3i4frTd  
                if (file.Exists())
                {
                    return file.Path;
                }

                return "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return "";
            }
        }

        [BroadcastReceiver(Exported = false)]
        [IntentFilter(new[] { DownloadManager.ActionDownloadComplete })]
        public class OnDownloadComplete : BroadcastReceiver
        {
            public string TypeActivity;
            public SoundDataObject Sound;

            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    if (intent.Action == DownloadManager.ActionDownloadComplete)
                    {
                        if (context == null)
                            return;

                        DownloadManager downloadManagerExcuter = (DownloadManager)context.GetSystemService(Context.DownloadService);
                        long downloadId = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
                        DownloadManager.Query query = new DownloadManager.Query();
                        query.SetFilterById(downloadId);
                        ICursor c = downloadManagerExcuter.InvokeQuery(query);
                        var sqlEntity = new SqLiteDatabase();

                        if (c.MoveToFirst())
                        {
                            int columnIndex = c.GetColumnIndex(DownloadManager.ColumnStatus);
                            if (c.GetInt(columnIndex) == (int)DownloadStatus.Successful)
                            {
                                string downloadedPath = c.GetString(c.GetColumnIndex(DownloadManager.ColumnLocalUri));

                                ActivityManager.RunningAppProcessInfo appProcessInfo = new ActivityManager.RunningAppProcessInfo();
                                ActivityManager.GetMyMemoryState(appProcessInfo);
                                if (appProcessInfo.Importance == Importance.Foreground || appProcessInfo.Importance == Importance.Background)
                                {
                                    sqlEntity.Update_LatestDownloadsSound(Sound.Id, downloadedPath);
                                    if (TypeActivity == "Main")
                                    {
                                        if (ActivityContext is HomeActivity tabbedMain)
                                        {
                                            tabbedMain.SoundController.BtnIconDownload.Tag = "Downloaded";
                                            tabbedMain.SoundController.BtnIconDownload.SetImageResource(Resource.Drawable.ic_check_circle);
                                            tabbedMain.SoundController.BtnIconDownload.SetColorFilter(Color.Red);

                                            tabbedMain.SoundController.ProgressBarDownload.Visibility = ViewStates.Invisible;
                                            tabbedMain.SoundController.BtnIconDownload.Visibility = ViewStates.Visible;
                                        }
                                    }
                                }
                                else
                                {
                                    sqlEntity.Update_LatestDownloadsSound(Sound.Id, downloadedPath);
                                }
                            }
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
}