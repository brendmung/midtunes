using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using DeepSound.Activities.Search.Adapters;
using DeepSound.Activities.Tabbes;
using DeepSound.Adapters;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Albums;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Event;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.Playlist;
using DeepSoundClient.Classes.Product;
using DeepSoundClient.Classes.User;
using DeepSoundClient.Requests;
using Google.Android.Material.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeepSound.Activities.Search
{
    public class SearchFragment : Fragment, TabLayoutMediator.ITabConfigurationStrategy, TextView.IOnEditorActionListener
    {
        #region Variables Basic

        private LinearLayout SearchLinearLayout;
        private ImageView BackIcon;
        public SearchSongsFragment SongsTab;
        private SearchAlbumsFragment AlbumsTab;
        private SearchPlaylistFragment PlaylistTab;
        private SearchArtistsFragment ArtistsTab;
        private SearchEventsFragment EventsTab;
        private SearchProductsFragment ProductsTab;
        private TabLayout TabLayout;
        private ViewPager2 ViewPager;
        private HomeActivity GlobalContext;
        private AutoCompleteTextView SearchBox;
        private TextView FilterButton, IconClose;
        private string SearchText = "";
        public string OffsetSongs = "0", OffsetAlbums = "0", OffsetPlaylist = "0", OffsetArtists = "0", OffsetEvents = "0", OffsetProducts = "0";
        private MainTabAdapter Adapter;

        private LastSearchAdapter LastSearchAdapter;
        private ViewStub LastSearchViewStub;
        private View LastSearchInflated;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.SearchLayout, container, false);
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

                SearchText = Arguments?.GetString("Key") ?? "";

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                GetAppData();
                AdsGoogle.Ad_Interstitial(Activity);
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                BackIcon = view.FindViewById<ImageView>(Resource.Id.backArrow);
                BackIcon.SetImageResource(AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                BackIcon.ImageTintList = ColorStateList.ValueOf(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
                BackIcon.Click += BackIconOnClick;

                LastSearchViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubLastSearch);

                TabLayout = view.FindViewById<TabLayout>(Resource.Id.Searchtabs);
                ViewPager = view.FindViewById<ViewPager2>(Resource.Id.Searchviewpager);

                //Set Tab 
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();

                for (int i = 0; i < TabLayout.TabCount; i++)
                {
                    var tab = ((ViewGroup)TabLayout.GetChildAt(0)).GetChildAt(i);
                    var p = tab.LayoutParameters as ViewGroup.MarginLayoutParams;
                    p.SetMargins(0, 0, 30, 0);
                    tab.RequestLayout();
                }

                IconClose = (TextView)view.FindViewById(Resource.Id.IconClose);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconClose, IonIconsFonts.Close);
                IconClose.Click += IconCloseOnClick;

                FilterButton = (TextView)view.FindViewById(Resource.Id.IconFilter);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FilterButton, IonIconsFonts.Options);
                FilterButton.Click += FilterButtonOnClick;

                TabLayout.Visibility = ViewStates.Invisible;
                ViewPager.Visibility = ViewStates.Invisible;
                IconClose.Visibility = ViewStates.Gone;
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
                SearchLinearLayout = (LinearLayout)view.FindViewById(Resource.Id.searchLinearLayout);

                SearchBox = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchBox);
                SearchBox.SetOnEditorActionListener(this);
                SearchBox.AfterTextChanged += SearchBoxOnAfterTextChanged;
                SearchBox.FocusChange += SearchBoxOnFocusChange;

                //Change text colors
                SearchBox.SetHintTextColor(Color.ParseColor(AppSettings.MainColor));
                SearchBox.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.Black);
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
                LastSearchAdapter = new LastSearchAdapter(Activity) { KeywordsList = new ObservableCollection<TrendSearchDataObject>() };
                LastSearchAdapter.ItemClick += LastSearchAdapterItemClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                SongsTab = new SearchSongsFragment();
                AlbumsTab = new SearchAlbumsFragment();
                PlaylistTab = new SearchPlaylistFragment();
                ArtistsTab = new SearchArtistsFragment();
                EventsTab = new SearchEventsFragment();
                ProductsTab = new SearchProductsFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(SongsTab, GetText(Resource.String.Lbl_Songs));
                Adapter.AddFragment(AlbumsTab, GetText(Resource.String.Lbl_Albums));
                Adapter.AddFragment(PlaylistTab, GetText(Resource.String.Lbl_Playlist));
                Adapter.AddFragment(ArtistsTab, GetText(Resource.String.Lbl_Artists));
                Adapter.AddFragment(EventsTab, GetText(Resource.String.Lbl_Event));
                Adapter.AddFragment(ProductsTab, GetText(Resource.String.Lbl_Products));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();

                //TabLayout.SetTabTextColors(DeepSoundTools.IsTabDark() ? Color.White : Color.Black, Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Event

        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.FragmentNavigatorBack();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchBoxOnAfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            try
            {
                if (SearchBox.Text.Length > 0 && !string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    IconClose.Visibility = ViewStates.Visible;
                }
                else
                    IconClose.Visibility = ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchBoxOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    SearchLinearLayout.SetBackgroundResource(Resource.Drawable.new_editbox_active);
                    SearchBox.CompoundDrawableTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(Context, Resource.Color.accent)));
                }
                else
                {
                    SearchLinearLayout.SetBackgroundResource(Resource.Drawable.new_login_status);
                    SearchBox.CompoundDrawableTintList = ColorStateList.ValueOf(DeepSoundTools.IsTabDark() ? Color.ParseColor("#efefef") : Color.ParseColor("#444444"));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LastSearchAdapterItemClick(object sender, LastSearchAdapterClickEventArgs e)
        {
            try
            {
                var item = LastSearchAdapter.GetItem(e.Position);
                if (item != null)
                {
                    SearchText = SearchBox.Text = item.Keyword;

                    SearchBox.ClearFocus();
                    LastSearchViewStub.Visibility = ViewStates.Gone;

                    TabLayout.Visibility = ViewStates.Visible;
                    ViewPager.Visibility = ViewStates.Visible;

                    HideKeyboard();

                    if (SongsTab.MAdapter != null)
                    {
                        SongsTab.MAdapter.SoundsList.Clear();
                        SongsTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (AlbumsTab.MAdapter != null)
                    {
                        AlbumsTab.MAdapter.AlbumsList.Clear();
                        AlbumsTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (PlaylistTab.MAdapter != null)
                    {
                        PlaylistTab.MAdapter.PlaylistList.Clear();
                        PlaylistTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (ArtistsTab.MAdapter != null)
                    {
                        ArtistsTab.MAdapter.UsersList.Clear();
                        ArtistsTab.MAdapter.NotifyDataSetChanged();
                    }

                    OffsetSongs = "0";
                    OffsetAlbums = "0";
                    OffsetPlaylist = "0";
                    OffsetArtists = "0";

                    if (Methods.CheckConnectivity())
                    {
                        ToggleProgressLoaderVisibility(true);
                        ToggleEmptyStateVisibility(false);

                        StartApiService();
                    }
                    else
                    {
                        SongsTab.Inflated ??= SongsTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoConnection);
                        if (x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x.EmptyStateButton.Click -= TryAgainButton_Click;
                        }

                        x.EmptyStateButton.Click += TryAgainButton_Click;
                        ToggleProgressLoaderVisibility(false);
                        ToggleEmptyStateVisibility(true);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchText = SearchBox.Text = "";

                SearchBox.ClearFocus();

                LastSearchViewStub.Visibility = ViewStates.Gone;

                TabLayout.Visibility = ViewStates.Visible;
                ViewPager.Visibility = ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //show Filter
        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var searchFilter = new SearchFilterBottomDialogFragment();
                searchFilter.Show(ChildFragmentManager, "searchFilter");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)Context?.GetSystemService(Android.Content.Context.InputMethodService);
                inputManager?.HideSoftInputFromWindow(SearchBox.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public bool OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
        {
            if (actionId == ImeAction.Search)
            {
                try
                {
                    SearchText = v.Text;

                    SearchBox.ClearFocus();
                    v.ClearFocus();

                    HideKeyboard();

                    LastSearchViewStub.Visibility = ViewStates.Gone;

                    TabLayout.Visibility = ViewStates.Visible;
                    ViewPager.Visibility = ViewStates.Visible;

                    if (SongsTab?.MAdapter != null)
                    {
                        SongsTab.MAdapter.SoundsList.Clear();
                        SongsTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (AlbumsTab?.MAdapter != null)
                    {
                        AlbumsTab.MAdapter.AlbumsList.Clear();
                        AlbumsTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (PlaylistTab?.MAdapter != null)
                    {
                        PlaylistTab.MAdapter.PlaylistList.Clear();
                        PlaylistTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (ArtistsTab?.MAdapter != null)
                    {
                        ArtistsTab.MAdapter.UsersList.Clear();
                        ArtistsTab.MAdapter.NotifyDataSetChanged();
                    }

                    OffsetSongs = "0";
                    OffsetAlbums = "0";
                    OffsetPlaylist = "0";
                    OffsetArtists = "0";

                    if (Methods.CheckConnectivity())
                    {
                        ToggleProgressLoaderVisibility(true);
                        ToggleEmptyStateVisibility(false);

                        StartApiService();
                    }
                    else
                    {
                        if (SongsTab.Inflated == null)
                            SongsTab.Inflated = SongsTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoConnection);
                        if (x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x.EmptyStateButton.Click -= TryAgainButton_Click;
                        }

                        x.EmptyStateButton.Click += TryAgainButton_Click;
                        ToggleProgressLoaderVisibility(false);
                        ToggleEmptyStateVisibility(true);
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }

                return true;
            }

            return false;
        }

        private void ToggleProgressLoaderVisibility(bool isVisible)
        {
            try
            {
                if (SongsTab.ProgressBarLoader != null) SongsTab.ProgressBarLoader.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
                if (ArtistsTab.ProgressBarLoader != null) ArtistsTab.ProgressBarLoader.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
                if (PlaylistTab.ProgressBarLoader != null) PlaylistTab.ProgressBarLoader.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
                if (AlbumsTab.ProgressBarLoader != null) AlbumsTab.ProgressBarLoader.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ToggleEmptyStateVisibility(bool isVisible)
        {
            try
            {
                if (SongsTab.EmptyStateLayout != null) SongsTab.EmptyStateLayout.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
                if (ArtistsTab.EmptyStateLayout != null) ArtistsTab.EmptyStateLayout.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
                if (PlaylistTab.EmptyStateLayout != null) PlaylistTab.EmptyStateLayout.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
                if (AlbumsTab.EmptyStateLayout != null) AlbumsTab.EmptyStateLayout.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchBox.ClearFocus();

                if (SongsTab?.MAdapter != null)
                {
                    SongsTab.MAdapter.SoundsList.Clear();
                    SongsTab.MAdapter.NotifyDataSetChanged();
                }

                if (AlbumsTab?.MAdapter != null)
                {
                    AlbumsTab.MAdapter.AlbumsList.Clear();
                    AlbumsTab.MAdapter.NotifyDataSetChanged();
                }

                if (PlaylistTab?.MAdapter != null)
                {
                    PlaylistTab.MAdapter.PlaylistList.Clear();
                    PlaylistTab.MAdapter.NotifyDataSetChanged();
                }

                if (ArtistsTab?.MAdapter != null)
                {
                    ArtistsTab.MAdapter.UsersList.Clear();
                    ArtistsTab.MAdapter.NotifyDataSetChanged();
                }

                OffsetSongs = "0";
                OffsetAlbums = "0";
                OffsetPlaylist = "0";
                OffsetArtists = "0";

                if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchText = "a";
                }

                //ViewPager.SetCurrentItem(0, true);

                if (Methods.CheckConnectivity())
                {
                    ToggleProgressLoaderVisibility(true);
                    ToggleEmptyStateVisibility(false);
                    StartApiService();
                }
                else
                {
                    if (SongsTab.Inflated == null)
                        SongsTab.Inflated = SongsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ToggleProgressLoaderVisibility(false);
                    ToggleEmptyStateVisibility(true);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Search 

        private void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (Methods.CheckConnectivity())
                    {
                        SongsTab.MAdapter.SoundsList.Clear();
                        SongsTab.MAdapter.NotifyDataSetChanged();

                        AlbumsTab.MAdapter.AlbumsList.Clear();
                        AlbumsTab.MAdapter.NotifyDataSetChanged();

                        PlaylistTab.MAdapter.PlaylistList.Clear();
                        PlaylistTab.MAdapter.NotifyDataSetChanged();

                        ArtistsTab.MAdapter.UsersList.Clear();
                        ArtistsTab.MAdapter.NotifyDataSetChanged();

                        EventsTab.MAdapter.EventsList.Clear();
                        EventsTab.MAdapter.NotifyDataSetChanged();

                        ProductsTab.MAdapter.ProductsList.Clear();
                        ProductsTab.MAdapter.NotifyDataSetChanged();

                        ToggleProgressLoaderVisibility(true);
                        ToggleEmptyStateVisibility(false);

                        StartApiService();
                    }
                }
                else
                {
                    if (SongsTab.Inflated == null)
                        SongsTab.Inflated = SongsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ToggleProgressLoaderVisibility(false);
                    ToggleEmptyStateVisibility(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
        }

        private async Task StartSearchRequest()
        {
            try
            {
                if (SongsTab.MainScrollEvent.IsLoading)
                    return;

                SongsTab.MainScrollEvent.IsLoading = true;
                AlbumsTab.MainScrollEvent.IsLoading = true;
                PlaylistTab.MainScrollEvent.IsLoading = true;
                ArtistsTab.MainScrollEvent.IsLoading = true;
                EventsTab.MainScrollEvent.IsLoading = true;
                ProductsTab.MainScrollEvent.IsLoading = true;

                int countSongsList = SongsTab.MAdapter.SoundsList.Count;
                int countAlbumsList = AlbumsTab.MAdapter.AlbumsList.Count;
                int countPlaylistList = PlaylistTab.MAdapter.PlaylistList.Count;
                int countArtistList = ArtistsTab.MAdapter.UsersList.Count;
                int countEventsList = EventsTab.MAdapter.EventsList.Count;
                int countProductsList = ProductsTab.MAdapter.ProductsList.Count;

                var (apiStatus, respond) = await RequestsAsync.Common.SearchAsync(SearchText, UserDetails.FilterGenres, UserDetails.FilterPrice, "10", OffsetSongs, OffsetAlbums, OffsetArtists, OffsetPlaylist, OffsetEvents, OffsetProducts);
                if (apiStatus == 200)
                {
                    if (respond is SearchObject result)
                    {
                        List<SoundDataObject> soundsList = new List<SoundDataObject>();
                        if (result.Data?.Songs?.AnythingArray?.Count > 0)
                        {
                            soundsList = new List<SoundDataObject>(result.Data?.Songs?.AnythingArray);
                        }
                        else if (result.Data?.Songs?.NewReleasesDataList?.Count > 0)
                        {
                            soundsList = new List<SoundDataObject>(result.Data?.Songs?.NewReleasesDataList?.Values);
                        }

                        var respondSongsList = soundsList?.Count;
                        if (respondSongsList > 0)
                        {
                            soundsList = DeepSoundTools.ListFilter(soundsList);

                            if (countSongsList > 0)
                            {
                                foreach (var item in from item in soundsList let check = SongsTab.MAdapter.SoundsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    SongsTab.MAdapter.SoundsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { SongsTab.MAdapter.NotifyItemRangeInserted(countSongsList, SongsTab.MAdapter.SoundsList.Count - countSongsList); });
                            }
                            else
                            {
                                SongsTab.MAdapter.SoundsList = new ObservableCollection<SoundDataObject>(soundsList);
                                Activity.RunOnUiThread(() => { SongsTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (SongsTab.MAdapter.SoundsList.Count > 10 && !SongsTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreSongs), ToastLength.Short)?.Show();
                        }

                        var respondAlbumsList = result.Data?.Albums?.Count;
                        if (respondAlbumsList > 0)
                        {
                            if (countAlbumsList > 0)
                            {
                                foreach (var item in from item in result.Data?.Albums let check = AlbumsTab.MAdapter.AlbumsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    AlbumsTab.MAdapter.AlbumsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { AlbumsTab.MAdapter.NotifyItemRangeInserted(countAlbumsList, AlbumsTab.MAdapter.AlbumsList.Count - countAlbumsList); });
                            }
                            else
                            {
                                AlbumsTab.MAdapter.AlbumsList = new ObservableCollection<DataAlbumsObject>(result.Data?.Albums);
                                Activity.RunOnUiThread(() => { AlbumsTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (AlbumsTab.MAdapter.AlbumsList.Count > 10 && !AlbumsTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreAlbums), ToastLength.Short)?.Show();
                        }

                        var respondPlaylistList = result.Data?.Playlist?.Count;
                        if (respondPlaylistList > 0)
                        {
                            if (countPlaylistList > 0)
                            {
                                foreach (var item in from item in result.Data?.Playlist let check = PlaylistTab.MAdapter.PlaylistList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    PlaylistTab.MAdapter.PlaylistList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { PlaylistTab.MAdapter.NotifyItemRangeInserted(countPlaylistList, PlaylistTab.MAdapter.PlaylistList.Count - countPlaylistList); });
                            }
                            else
                            {
                                PlaylistTab.MAdapter.PlaylistList = new ObservableCollection<PlaylistDataObject>(result.Data?.Playlist);
                                Activity.RunOnUiThread(() => { PlaylistTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (PlaylistTab.MAdapter.PlaylistList.Count > 10 && !PlaylistTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePlaylist), ToastLength.Short)?.Show();
                        }

                        var respondArtistList = result.Data?.Artist?.Count;
                        if (respondArtistList > 0)
                        {
                            //result.Data?.Artist.RemoveAll(a => a.Artist == 0);

                            if (countArtistList > 0)
                            {
                                foreach (var item in from item in result.Data?.Artist let check = ArtistsTab.MAdapter.UsersList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    ArtistsTab.MAdapter.UsersList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { ArtistsTab.MAdapter.NotifyItemRangeInserted(countArtistList, ArtistsTab.MAdapter.UsersList.Count - countArtistList); });
                            }
                            else
                            {
                                ArtistsTab.MAdapter.UsersList = new ObservableCollection<UserDataObject>(result.Data?.Artist);
                                Activity.RunOnUiThread(() => { ArtistsTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (ArtistsTab.MAdapter.UsersList.Count > 10 && !ArtistsTab.MRecycler.CanScrollVertically(1)) 
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreArtists), ToastLength.Short)?.Show();
                        }

                        var respondEventsList = result.Data?.Events?.Count;
                        if (respondEventsList > 0)
                        {
                            if (countEventsList > 0)
                            {
                                foreach (var item in from item in result.Data?.Events let check = EventsTab.MAdapter.EventsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    EventsTab.MAdapter.EventsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { EventsTab.MAdapter.NotifyItemRangeInserted(countEventsList, EventsTab.MAdapter.EventsList.Count - countEventsList); });
                            }
                            else
                            {
                                EventsTab.MAdapter.EventsList = new ObservableCollection<EventDataObject>(result.Data?.Events);
                                Activity.RunOnUiThread(() => { EventsTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (EventsTab.MAdapter.EventsList.Count > 10 && !EventsTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreEvents), ToastLength.Short)?.Show();
                        }

                        var respondProductsList = result.Data?.Products?.Count;
                        if (respondProductsList > 0)
                        {
                            if (countProductsList > 0)
                            {
                                foreach (var item in from item in result.Data?.Products let check = ProductsTab.MAdapter.ProductsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    ProductsTab.MAdapter.ProductsList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { ProductsTab.MAdapter.NotifyItemRangeInserted(countProductsList, ProductsTab.MAdapter.ProductsList.Count - countProductsList); });
                            }
                            else
                            {
                                ProductsTab.MAdapter.ProductsList = new ObservableCollection<ProductDataObject>(result.Data?.Products);
                                Activity.RunOnUiThread(() => { ProductsTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (ProductsTab.MAdapter.ProductsList.Count > 10 && !ProductsTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short)?.Show();
                        }

                        SongsTab.MainScrollEvent.IsLoading = false;
                        AlbumsTab.MainScrollEvent.IsLoading = false;
                        PlaylistTab.MainScrollEvent.IsLoading = false;
                        ArtistsTab.MainScrollEvent.IsLoading = false;
                        EventsTab.MainScrollEvent.IsLoading = false;
                        ProductsTab.MainScrollEvent.IsLoading = false;
                    }
                }
                else
                {
                    SongsTab.MainScrollEvent.IsLoading = false;
                    AlbumsTab.MainScrollEvent.IsLoading = false;
                    PlaylistTab.MainScrollEvent.IsLoading = false;
                    ArtistsTab.MainScrollEvent.IsLoading = false;
                    EventsTab.MainScrollEvent.IsLoading = false;
                    ProductsTab.MainScrollEvent.IsLoading = false;

                    Methods.DisplayReportResult(Activity, respond);
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                Activity?.RunOnUiThread(ShowEmptyPage);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                ToggleProgressLoaderVisibility(false);

                if (SongsTab.MAdapter.SoundsList.Count > 0)
                {
                    SongsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (SongsTab.Inflated == null)
                        SongsTab.Inflated = SongsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(SongsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    SongsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }


                if (AlbumsTab.MAdapter.AlbumsList.Count > 0)
                {
                    AlbumsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (AlbumsTab.Inflated == null)
                        AlbumsTab.Inflated = AlbumsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(AlbumsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    AlbumsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                if (PlaylistTab.MAdapter.PlaylistList.Count > 0)
                {
                    PlaylistTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (PlaylistTab.Inflated == null)
                        PlaylistTab.Inflated = PlaylistTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(PlaylistTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    PlaylistTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                if (ArtistsTab.MAdapter.UsersList.Count > 0)
                {
                    ArtistsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (ArtistsTab.Inflated == null)
                        ArtistsTab.Inflated = ArtistsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(ArtistsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ArtistsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                if (EventsTab.MAdapter.EventsList.Count > 0)
                {
                    EventsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (EventsTab.Inflated == null)
                        EventsTab.Inflated = EventsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(EventsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    EventsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                if (ProductsTab.MAdapter.ProductsList.Count > 0)
                {
                    ProductsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (ProductsTab.Inflated == null)
                        ProductsTab.Inflated = ProductsTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(ProductsTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    ProductsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }

            }
            catch (Exception e)
            {
                //SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        public void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                SearchText = "a";

                //ViewPager.SetCurrentItem(0, true);

                Search();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void GetAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                ListUtils.GenresList = sqlEntity.Get_GenresList();
                if (ListUtils.GenresList?.Count == 0)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetGenres_Api });

                if (AppSettings.ShowPrice)
                {
                    ListUtils.PriceList = sqlEntity.Get_PriceList();
                    if (ListUtils.PriceList?.Count == 0)
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPrices_Api });
                }

                if (UserDetails.IsLogin)
                {
                    StartApiLastSearch();
                }
                else
                {
                    LastSearchViewStub.Visibility = ViewStates.Gone;

                    TabLayout.Visibility = ViewStates.Visible;
                    ViewPager.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region LastSearch

        private void StartApiLastSearch()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetLastSearch });
        }

        private async Task GetLastSearch()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.User.TrendSearchAsync();
                if (apiStatus == 200)
                {
                    if (respond is TrendSearchObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            LastSearchAdapter.KeywordsList = new ObservableCollection<TrendSearchDataObject>(result.Data);
                            Activity?.RunOnUiThread(() =>
                            {
                                LastSearchInflated ??= LastSearchViewStub.Inflate();

                                TemplateRecyclerInflater recyclerInflater = new TemplateRecyclerInflater();
                                recyclerInflater.InflateLayout<UserDataObject>(Activity, LastSearchInflated, LastSearchAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, Context.GetText(Resource.String.Lbl_TrendingNow), false);
                                if (!recyclerInflater.MainLinear.HasOnClickListeners)
                                {
                                    recyclerInflater.MainLinear.Click += null;
                                }

                                LastSearchViewStub.Visibility = ViewStates.Visible;

                                TabLayout.Visibility = ViewStates.Invisible;
                                ViewPager.Visibility = ViewStates.Invisible;
                            });
                        }
                        else
                        {
                            Activity?.RunOnUiThread(() =>
                            {
                                LastSearchViewStub.Visibility = ViewStates.Gone;

                                TabLayout.Visibility = ViewStates.Visible;
                                ViewPager.Visibility = ViewStates.Visible;
                            });
                        }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
            }
            else
            {
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }


        #endregion

    }
}