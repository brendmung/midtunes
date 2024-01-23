using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using DeepSound.Helpers.Fonts;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Exception = System.Exception;

namespace DeepSound.Activities.Search
{
    public class SearchFilterBottomDialogFragment : BottomSheetDialogFragment
    {
        #region Variables Basic

        private TextView IconPrice, IconGenres, BtnSelectAll;
        private TextView TxtTitlePage, TxtPrice, TxtGenres;
        private RelativeLayout PriceLayout, GenresLayout;
        private AppCompatButton ButtonApply;
        //private HomeActivity GlobalContext;
        private string TypeDialog, CurrencySymbol, TotalIdGenresChecked = "", TotalIdPriceChecked = "";

        #endregion

        #region General

        //public override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);
        //    // Create your fragment here
        //    //GlobalContext = (HomeActivity)Activity;
        //}

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = DeepSoundTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.ButtomSheetSearchFilter, container, false);
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

                //Get Value And Set Toolbar
                InitComponent(view);
                CurrencySymbol = ListUtils.SettingsSiteList?.CurrencySymbol ?? "$";
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
                IconPrice = view.FindViewById<TextView>(Resource.Id.IconPrice);
                IconGenres = view.FindViewById<TextView>(Resource.Id.IconGenres);

                TxtTitlePage = view.FindViewById<TextView>(Resource.Id.titlepage);
                TxtPrice = view.FindViewById<TextView>(Resource.Id.PricePlace);
                TxtGenres = view.FindViewById<TextView>(Resource.Id.GenresPlace);

                PriceLayout = view.FindViewById<RelativeLayout>(Resource.Id.LayoutPrice);
                GenresLayout = view.FindViewById<RelativeLayout>(Resource.Id.LayoutGenres);

                BtnSelectAll = view.FindViewById<TextView>(Resource.Id.SelectAllbutton);
                ButtonApply = view.FindViewById<AppCompatButton>(Resource.Id.ApplyButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconPrice, IonIconsFonts.LogoUsd);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconGenres, IonIconsFonts.LogoBuffer);

                TxtPrice.Text = "";
                TxtGenres.Text = "";

                PriceLayout.Click += PriceLayoutOnClick;
                GenresLayout.Click += GenresLayoutOnClick;
                ButtonApply.Click += ButtonApplyOnClick;
                BtnSelectAll.Click += BtnSelectAllOnClick;

                if (!AppSettings.ShowPrice)
                {
                    PriceLayout.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Choose Genres
        private void GenresLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Genres";

                var listItems = ListUtils.GenresList.Select(item => item.CateogryName).ToList();

                var checkedItems = new bool[listItems.Count];
                var selectedItems = new List<string>(listItems);

                var dialogList = new MaterialAlertDialogBuilder(Activity);

                dialogList.SetTitle(Resource.String.Lbl_ChooseGenres);
                dialogList.SetCancelable(false);
                dialogList.SetMultiChoiceItems(listItems.ToArray(), checkedItems, (o, args) =>
                {
                    try
                    {
                        checkedItems[args.Which] = args.IsChecked;

                        var text = selectedItems[args.Which] ?? "";
                        Console.WriteLine(text);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), (o, args) =>
                {
                    try
                    {
                        TotalIdGenresChecked = "";
                        for (int i = 0; i < checkedItems.Length; i++)
                        {
                            if (checkedItems[i])
                            {
                                var text = selectedItems[i];
                                var check = ListUtils.GenresList.FirstOrDefault(a => a.CateogryName == text);
                                if (check != null)
                                {
                                    TotalIdGenresChecked += check.Id + ",";
                                }
                            }
                        }

                        TxtGenres.Text = Context.GetText(Resource.String.Lbl_Selected);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Choose Price
        private void PriceLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Price";

                var listItems = new List<string>();
                foreach (var item in ListUtils.PriceList)
                    if (item.Price is "0.00" or "0")
                        listItems.Add(GetText(Resource.String.Lbl_Free));
                    else
                        listItems.Add(CurrencySymbol + item.Price);

                var checkedItems = new bool[listItems.Count];
                var selectedItems = new List<string>(listItems);

                var dialogList = new MaterialAlertDialogBuilder(Activity);

                dialogList.SetTitle(Resource.String.Lbl_ChoosePrice);
                dialogList.SetCancelable(false);
                dialogList.SetMultiChoiceItems(listItems.ToArray(), checkedItems, (o, args) =>
                {
                    try
                    {
                        checkedItems[args.Which] = args.IsChecked;

                        var text = selectedItems[args.Which] ?? "";
                        Console.WriteLine(text);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), (o, args) =>
                {
                    try
                    {
                        TotalIdPriceChecked = "";
                        for (int i = 0; i < checkedItems.Length; i++)
                        {
                            if (checkedItems[i])
                            {
                                var text = selectedItems[i];
                                var check = ListUtils.PriceList.FirstOrDefault(a => a.Price.Contains(text));
                                if (check != null)
                                {
                                    TotalIdPriceChecked += check.Id + ",";
                                }
                            }
                        }

                        TxtPrice.Text = Context.GetText(Resource.String.Lbl_Selected);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save and sent data and set new search 
        private void ButtonApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (TotalIdGenresChecked?.Length > 0)
                    UserDetails.FilterGenres = TotalIdGenresChecked.Remove(TotalIdGenresChecked.Length - 1, 1);

                if (TotalIdPriceChecked?.Length > 0)
                    UserDetails.FilterPrice = TotalIdPriceChecked.Remove(TotalIdPriceChecked.Length - 1, 1);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
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

        //Select all data 
        private void BtnSelectAllOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ListUtils.GenresList.Count > 0)
                {
                    //Get all id 
                    foreach (var item in ListUtils.GenresList)
                    {
                        TotalIdGenresChecked += item.Id + ",";
                    }
                    UserDetails.FilterGenres = TotalIdGenresChecked.Remove(TotalIdGenresChecked.Length - 1, 1);

                    TxtGenres.Text = GetString(Resource.String.Lbl_SelectAll);
                }

                if (ListUtils.PriceList.Count > 0)
                {
                    //Get all id 
                    foreach (var item in ListUtils.PriceList)
                    {
                        TotalIdPriceChecked += item.Id + ",";
                    }

                    UserDetails.FilterPrice = TotalIdPriceChecked.Remove(TotalIdPriceChecked.Length - 1, 1);

                    TxtPrice.Text = GetString(Resource.String.Lbl_SelectAll);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}