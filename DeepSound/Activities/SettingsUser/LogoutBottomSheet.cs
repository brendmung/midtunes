using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Utils;
using Google.Android.Material.BottomSheet;
using System;
using Exception = System.Exception;

namespace DeepSound.Activities.SettingsUser
{
    public class LogoutBottomSheet : BottomSheetDialogFragment
    {
        #region Variables Basic

        private AppCompatButton BtnCancel, BtnYes;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = DeepSoundTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.BottomSheetLogout, container, false);
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
                BtnCancel = view.FindViewById<AppCompatButton>(Resource.Id.btnCancel);
                BtnYes = view.FindViewById<AppCompatButton>(Resource.Id.btnYes);

                BtnCancel.Click += BtnCancelOnClick;
                BtnYes.Click += BtnYesOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void BtnYesOnClick(object sender, EventArgs e)
        {
            try
            {
                ApiRequest.Logout(Activity);
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnCancelOnClick(object sender, EventArgs e)
        {
            Dismiss();
        }

        #endregion 
    }
}