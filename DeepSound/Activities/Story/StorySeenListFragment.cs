using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using DeepSound.Activities.Story.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.Library.Anjo.IntegrationRecyclerView;
using DeepSound.Library.Anjo.Share;
using DeepSound.Library.Anjo.Share.Abstractions;
using DeepSoundClient.Classes.Story;
using DeepSoundClient.Requests;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace DeepSound.Activities.Story
{
    public class StorySeenListFragment : BottomSheetDialogFragment
    {
        #region Variables Basic

        private LinearLayout BottomSheetLayout;

        private Toolbar ToolBar;
        private TextView TxtEmpty, IconDelete;

        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private StorySeenListAdapter MAdapter;
        private RecyclerViewOnScrollListener MainScrollEvent;

        private string StoryId;
        private StoryDataObject DataStories;
        private readonly ViewStoryFragment StoryFragment;

        #endregion

        #region General

        public StorySeenListFragment(ViewStoryFragment storyFragment)
        {
            try
            {
                StoryFragment = storyFragment;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = DeepSoundTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.StorySeenListLayout, container, false);
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
                if (Arguments != null)
                {
                    StoryId = Arguments?.GetString("StoryId") ?? "";
                    DataStories = JsonConvert.DeserializeObject<StoryDataObject>(Arguments?.GetString("DataNowStory") ?? "");
                }

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnStart()
        {
            try
            {
                base.OnStart();
                var dialog = Dialog;
                //Make dialog full screen with transparent background
                if (dialog != null)
                {
                    var width = ViewGroup.LayoutParams.MatchParent;
                    var height = ViewGroup.LayoutParams.MatchParent;
                    dialog.Window.SetLayout(width, height);
                    dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                StoryFragment?.StoryStateListener?.OnResume();
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
                BottomSheetLayout = view.FindViewById<LinearLayout>(Resource.Id.bottom_sheet);
                TxtEmpty = view.FindViewById<TextView>(Resource.Id.empty_view);
                IconDelete = view.FindViewById<TextView>(Resource.Id.toolbar_delete);
                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.seenList);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconDelete, IonIconsFonts.Trash);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                ToolBar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_ViewedBy) + " " + DataStories.ViewsCount;
                    ToolBar.SetTitleTextColor(Color.White);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new StorySeenListAdapter(Activity)
                {
                    UserList = new ObservableCollection<ViewsStoryDataObject>()
                };
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<ViewsStoryDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    ToolBar.Click += CloseOnClick;
                    IconDelete.Click += IconDeleteOnClick;
                }
                else
                {
                    ToolBar.Click -= CloseOnClick;
                    IconDelete.Click -= IconDeleteOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.UserId.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(item.UserId.ToString());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void IconDeleteOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(StoryId))
                    return;

                if (Methods.CheckConnectivity())
                {
                    (int respondCode, var respond) = await RequestsAsync.Story.DeleteStoryAsync(StoryId);
                    if (respondCode == 200)
                    {
                        var checkSection = HomeActivity.GetInstance()?.HomeFragment?.LatestHomeTab?.StoryAdapter;

                        var story = checkSection?.StoryList?.FirstOrDefault(a => a.Id == StoryId);
                        if (story != null)
                        {
                            checkSection.StoryList.Remove(story);
                            checkSection.NotifyDataSetChanged();
                        }
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_Deleted), ToastLength.Short)?.Show();

                        Dismiss();
                    }
                    else Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //wael
        private async void IconShareOnClick(object sender, EventArgs e)
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = "",
                    Text = "",
                    Url = ""
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CloseOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void Dismiss()
        {
            try
            {
                StoryFragment?.StoryStateListener?.OnResume();

                base.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load User 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadUsers(offset) });
        }

        private async Task LoadUsers(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                var countList = MAdapter.UserList.Count;
                var (apiStatus, respond) = await RequestsAsync.Story.GetStoryViewsAsync(StoryId, "20", offset);
                if (apiStatus == 200)
                {
                    if (respond is ViewsStoryObject result)
                    {
                        result.Data.RemoveAll(o => o.UserId == UserDetails.UserId);

                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    MAdapter.UserList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.UserList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.UserList = new ObservableCollection<ViewsStoryDataObject>(result.Data);
                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                    }
                }
                else
                    Methods.DisplayReportResult(Activity, respond);

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (MAdapter.UserList.Count > 0)
                {
                    TxtEmpty.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Visible;

                }
                else
                {
                    TxtEmpty.Visibility = ViewStates.Visible;
                    MRecycler.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}