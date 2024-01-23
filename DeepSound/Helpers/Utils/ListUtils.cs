using DeepSound.Helpers.MediaPlayerController;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Classes.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace DeepSound.Helpers.Utils
{
    public static class ListUtils
    {
        //############# DON'T MODIFY HERE #############
        //List Items Declaration 
        //*********************************************************
        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();
        public static OptionsObject.Data SettingsSiteList;
        public static ObservableCollection<UserDataObject> MyUserInfoList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<GenresObject.DataGenres> GenresList = new ObservableCollection<GenresObject.DataGenres>();
        public static ObservableCollection<PricesObject.DataPrice> PriceList = new ObservableCollection<PricesObject.DataPrice>();
        public static ObservableCollection<PlaylistDataObject> PlaylistList = new ObservableCollection<PlaylistDataObject>();
        public static ObservableCollection<DataConversation> ChatList = new ObservableCollection<DataConversation>();
        public static ObservableCollection<DataAlbumsObject> AlbumList = new ObservableCollection<DataAlbumsObject>();
        public static List<SoundDataObject> FavoritesList = new List<SoundDataObject>();
        public static List<SoundDataObject> LikedSongs = new List<SoundDataObject>();
        public static ObservableCollection<SoundDataObject> GlobalNotInterestedList = new ObservableCollection<SoundDataObject>();

        public static void ClearAllList()
        {
            try
            {
                Constant.ArrayListPlay.Clear();
                DataUserLoginList.Clear();
                SettingsSiteList = null;
                MyUserInfoList.Clear();
                GenresList.Clear();
                PriceList.Clear();
                FavoritesList.Clear();
                LikedSongs.Clear();
                AlbumList.Clear();
                GlobalNotInterestedList.Clear();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            try
            {
                items.ToList().ForEach(collection.Add);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        {
            var enumerable = source as T[] ?? source.ToArray();

            return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        }

        public static void Copy<T>(T from, T to)
        {
            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                try
                {
                    if (!p.CanRead || !p.CanWrite) continue;

                    object val = p.GetGetMethod().Invoke(from, null);
                    object defaultVal = p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null;
                    if (null != defaultVal && !val.Equals(defaultVal))
                    {
                        p.GetSetMethod().Invoke(to, new[] { val });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static int Remove<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        /// <summary>
        /// Extends ObservableCollection adding a RemoveAll method to remove elements based on a boolean condition function
        /// </summary>
        /// <typeparam name="T">The type contained by the collection</typeparam>
        /// <param name="observableCollection">The ObservableCollection</param>
        /// <param name="condition">A function that evaluates to true for elements that should be removed</param>
        /// <returns>The number of elements removed</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> observableCollection, Func<T, bool> condition)
        {
            // Find all elements satisfying the condition, i.e. that will be removed
            var toRemove = observableCollection
                .Where(condition)
                .ToList();

            // Remove the elements from the original collection, using the Count method to iterate through the list, 
            // incrementing the count whenever there's a successful removal
            return toRemove.Count(observableCollection.Remove);
        }

        /// <summary>
        /// Extends ObservableCollection adding a RemoveAll method to remove elements based on a boolean condition function
        /// </summary>
        /// <typeparam name="T">The type contained by the collection</typeparam>
        /// <param name="observableCollection">The ObservableCollection</param>
        /// <param name="toRemove">Find all elements satisfying the condition, i.e. that will be removed</param>
        /// <returns>The number of elements removed</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> observableCollection, List<T> toRemove)
        {
            // Remove the elements from the original collection, using the Count method to iterate through the list, 
            // incrementing the count whenever there's a successful removal
            return toRemove.Count(observableCollection.Remove);
        }

    }
}