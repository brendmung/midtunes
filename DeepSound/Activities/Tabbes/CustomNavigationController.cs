using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using DeepSound.Helpers.Ads;
using DeepSound.Helpers.Controller;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;
using System;
using System.Collections.Generic;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace DeepSound.Activities.Tabbes
{
    public class CustomNavigationController : Java.Lang.Object, View.IOnClickListener
    {
        private readonly Activity MainContext;
        private readonly HomeActivity Context;

        private LinearLayout MainLayout;
        private LinearLayout CustomButton0, CustomButton2, CustomButton3, CustomButton4, CustomButton5;
        private ImageView CustomImage0, CustomImage2, CustomImage3, CustomImage4, CustomImage5;
        private TextView CustomText0, CustomText2, CustomText3, CustomText4, CustomText5;
        public int PageNumber;

        public readonly List<Fragment> FragmentListTab0 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab1 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab2 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab3 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab4 = new List<Fragment>();

        public CustomNavigationController(Activity activity)
        {
            try
            {
                MainContext = activity;

                if (activity is HomeActivity cont)
                    Context = cont;

                Initialize();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Initialize()
        {
            try
            {
                MainLayout = MainContext.FindViewById<LinearLayout>(Resource.Id.llMain);

                CustomButton0 = MainContext.FindViewById<LinearLayout>(Resource.Id.llcustom0);
                CustomButton2 = MainContext.FindViewById<LinearLayout>(Resource.Id.llcustom2);
                CustomButton3 = MainContext.FindViewById<LinearLayout>(Resource.Id.llcustom3);
                CustomButton4 = MainContext.FindViewById<LinearLayout>(Resource.Id.llcustom4);
                CustomButton5 = MainContext.FindViewById<LinearLayout>(Resource.Id.llcustom5);

                CustomImage0 = MainContext.FindViewById<ImageView>(Resource.Id.ivcustom0);
                CustomImage2 = MainContext.FindViewById<ImageView>(Resource.Id.ivcustom2);
                CustomImage3 = MainContext.FindViewById<ImageView>(Resource.Id.ivcustom3);
                CustomImage4 = MainContext.FindViewById<ImageView>(Resource.Id.ivcustom4);
                CustomImage5 = MainContext.FindViewById<ImageView>(Resource.Id.ivcustom5);

                CustomText0 = MainContext.FindViewById<TextView>(Resource.Id.txtcustom0);
                CustomText2 = MainContext.FindViewById<TextView>(Resource.Id.txtcustom2);
                CustomText3 = MainContext.FindViewById<TextView>(Resource.Id.txtcustom3);
                CustomText4 = MainContext.FindViewById<TextView>(Resource.Id.txtcustom4);
                CustomText5 = MainContext.FindViewById<TextView>(Resource.Id.txtcustom5);

                CustomButton0?.SetOnClickListener(this);
                CustomButton2?.SetOnClickListener(this);
                CustomButton3?.SetOnClickListener(this);
                CustomButton4?.SetOnClickListener(this);
                CustomButton5?.SetOnClickListener(this);

                //CustomImage0.Background = null;
                CustomImage0.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                CustomText0.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                CustomText0.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
                CustomText0.SetTextSize(ComplexUnitType.Sp, 14);

                //CustomImage2.Background = null;
                CustomImage2.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText2.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText2.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText2.SetTextSize(ComplexUnitType.Sp, 13);

                //CustomImage3.Background = null;
                CustomImage3.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText3.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText3.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText3.SetTextSize(ComplexUnitType.Sp, 13);

                //CustomImage4.Background = null;
                CustomImage4.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText4.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText4.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText4.SetTextSize(ComplexUnitType.Sp, 13);

                //CustomImage5.Background = null;
                CustomImage5.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText5.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText5.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText5.SetTextSize(ComplexUnitType.Sp, 13);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                switch (v.Id)
                {
                    case Resource.Id.llcustom0:
                        EnableNavigationButton(CustomImage0, CustomText0);
                        PageNumber = 0;
                        ShowFragment0();
                        AdsGoogle.Ad_AppOpenManager(MainContext);
                        break;
                    case Resource.Id.llcustom2:
                        EnableNavigationButton(CustomImage2, CustomText2);
                        PageNumber = 1;
                        ShowFragment1();
                        AdsGoogle.Ad_RewardedVideo(MainContext);
                        break;
                    case Resource.Id.llcustom3:

                        if (!UserDetails.IsLogin)
                        {
                            PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                            dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                            return;
                        }

                        EnableNavigationButton(CustomImage3, CustomText3);
                        PageNumber = 2;
                        ShowFragment2();
                        AdsGoogle.Ad_Interstitial(MainContext);
                        Context.InAppReview();
                        break;
                    case Resource.Id.llcustom4:

                        if (!UserDetails.IsLogin)
                        {
                            PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                            dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                            return;
                        }

                        EnableNavigationButton(CustomImage4, CustomText4);
                        PageNumber = 3;
                        ShowFragment3();
                        AdsGoogle.Ad_RewardedInterstitial(MainContext);
                        break;
                    case Resource.Id.llcustom5:

                        if (!UserDetails.IsLogin)
                        {
                            PopupDialogController dialog = new PopupDialogController(MainContext, null, "Login");
                            dialog.ShowNormalDialog(MainContext.GetText(Resource.String.Lbl_Login), MainContext.GetText(Resource.String.Lbl_Message_Sorry_signin), MainContext.GetText(Resource.String.Lbl_Yes), MainContext.GetText(Resource.String.Lbl_No));
                            return;
                        }

                        EnableNavigationButton(CustomImage5, CustomText5);
                        PageNumber = 4;
                        ShowFragment4();
                        AdsGoogle.Ad_Interstitial(MainContext);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void EnableNavigationButton(ImageView image, TextView text)
        {
            try
            {
                DisableAllNavigationButton();
                //image.Background = MainContext.GetDrawable(Resource.Drawable.shape_bg_bottom_navigation);
                image.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                text.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                text.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
                text.SetTextSize(ComplexUnitType.Sp, 14);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisableAllNavigationButton()
        {
            try
            {
                //CustomImage0.Background = null;
                CustomImage0.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText0.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText0.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText0.SetTextSize(ComplexUnitType.Sp, 13);

                //CustomImage2.Background = null;
                CustomImage2.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText2.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText2.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText2.SetTextSize(ComplexUnitType.Sp, 13);

                //CustomImage3.Background = null;
                CustomImage3.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText3.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText3.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText3.SetTextSize(ComplexUnitType.Sp, 13);

                //CustomImage4.Background = null;
                CustomImage4.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText4.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText4.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText4.SetTextSize(ComplexUnitType.Sp, 13);

                //CustomImage5.Background = null;
                CustomImage5.SetColorFilter(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText5.SetTextColor(DeepSoundTools.IsTabDark() ? Color.White : Color.ParseColor("#9E9E9E"));
                CustomText5.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
                CustomText5.SetTextSize(ComplexUnitType.Sp, 13);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //public void ShowNotificationBadge(bool showBadge)
        //{
        //    LottieAnimationView animationView2 = MainContext.FindViewById<LottieAnimationView>(Resource.Id.animation_view2);

        //    if (showBadge)
        //    {
        //        CustomImage2.SetImageDrawable(null);

        //        animationView2.SetAnimation("NotificationLotti.json");
        //        animationView2.PlayAnimation();
        //    }
        //    else
        //    {
        //        animationView2.Progress = 0;
        //        animationView2.CancelAnimation();
        //        CustomImage2.SetImageResource(Resource.Drawable.icon_notification_vector);
        //    }
        //}

        public Fragment GetSelectedTabBackStackFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0:
                        {
                            var currentFragment = FragmentListTab0[FragmentListTab0.Count - 2];
                            if (currentFragment != null)
                                return currentFragment;
                            break;
                        }
                    case 1:
                        {
                            var currentFragment = FragmentListTab1[FragmentListTab1.Count - 2];
                            if (currentFragment != null)
                                return currentFragment;
                            break;
                        }
                    case 2:
                        {
                            var currentFragment = FragmentListTab2[FragmentListTab2.Count - 2];
                            if (currentFragment != null)
                                return currentFragment;
                            break;
                        }
                    case 3:
                        {
                            var currentFragment = FragmentListTab3[FragmentListTab3.Count - 2];
                            if (currentFragment != null)
                                return currentFragment;
                            break;
                        }
                    case 4:
                        {
                            var currentFragment = FragmentListTab4[FragmentListTab4.Count - 2];
                            if (currentFragment != null)
                                return currentFragment;
                            break;
                        }

                    default:
                        return null;

                }

                return null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public int GetCountFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0:
                        return FragmentListTab0.Count > 1 ? FragmentListTab0.Count : 0;
                    case 1:
                        return FragmentListTab1.Count > 1 ? FragmentListTab1.Count : 0;
                    case 2:
                        return FragmentListTab2.Count > 1 ? FragmentListTab2.Count : 0;
                    case 3:
                        return FragmentListTab3.Count > 1 ? FragmentListTab4.Count : 0;
                    case 4:
                        return FragmentListTab4.Count > 1 ? FragmentListTab4.Count : 0;
                    default:
                        return 0;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public static void HideFragmentFromList(List<Fragment> fragmentList, FragmentTransaction ft)
        {
            try
            {
                if (fragmentList.Count < 0)
                    return;

                foreach (var fra in fragmentList)
                {
                    if (fra.IsVisible)
                        ft.Hide(fra);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayFragment(Fragment newFragment, View sharedElement = null)
        {
            try
            {
                FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

                if (sharedElement != null)
                {
                    if ((int)Build.VERSION.SdkInt >= 22)
                    {
                        ft.AddSharedElement(sharedElement, "Image");
                    }
                }
                HideFragmentFromList(FragmentListTab0, ft);
                HideFragmentFromList(FragmentListTab1, ft);
                HideFragmentFromList(FragmentListTab2, ft);
                HideFragmentFromList(FragmentListTab3, ft);
                HideFragmentFromList(FragmentListTab4, ft);

                if (PageNumber == 0)
                    if (!FragmentListTab0.Contains(newFragment))
                        FragmentListTab0.Add(newFragment);

                if (PageNumber == 1)
                    if (!FragmentListTab1.Contains(newFragment))
                        FragmentListTab1.Add(newFragment);

                if (PageNumber == 2)
                    if (!FragmentListTab2.Contains(newFragment))
                        FragmentListTab2.Add(newFragment);

                if (PageNumber == 3)
                    if (!FragmentListTab3.Contains(newFragment))
                        FragmentListTab3.Add(newFragment);

                if (PageNumber == 4)
                    if (!FragmentListTab4.Contains(newFragment))
                        FragmentListTab4.Add(newFragment);

                if (!newFragment.IsAdded)
                    ft.Add(Resource.Id.mainFragmentHolder, newFragment, newFragment.Id.ToString());


                // ft.SetCustomAnimations(Resource.Animation.fab_scale_down, Resource.Animation.fab_scale_up);


                ft.Show(newFragment)?.Commit();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayFragmentOnSamePage(Fragment newFragment)
        {
            try
            {
                FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

                HideFragmentFromList(FragmentListTab0, ft);
                HideFragmentFromList(FragmentListTab1, ft);
                HideFragmentFromList(FragmentListTab2, ft);
                HideFragmentFromList(FragmentListTab3, ft);
                HideFragmentFromList(FragmentListTab4, ft);

                if (PageNumber == 0)
                    if (!FragmentListTab0.Contains(newFragment))
                        FragmentListTab0.Add(newFragment);

                if (PageNumber == 1)
                    if (!FragmentListTab1.Contains(newFragment))
                        FragmentListTab1.Add(newFragment);

                if (PageNumber == 2)
                    if (!FragmentListTab2.Contains(newFragment))
                        FragmentListTab2.Add(newFragment);

                if (PageNumber == 3)
                    if (!FragmentListTab3.Contains(newFragment))
                        FragmentListTab3.Add(newFragment);

                if (PageNumber == 4)
                    if (!FragmentListTab4.Contains(newFragment))
                        FragmentListTab4.Add(newFragment);

                if (!newFragment.IsAdded)
                    ft.Add(Resource.Id.mainFragmentHolder, newFragment, newFragment.Id.ToString());

                ft.Show(newFragment).CommitAllowingStateLoss();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RemoveFragment(Fragment oldFragment)
        {
            try
            {
                FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

                if (PageNumber == 0)
                    if (FragmentListTab0.Contains(oldFragment))
                        FragmentListTab0.Remove(oldFragment);

                if (PageNumber == 1)
                    if (FragmentListTab1.Contains(oldFragment))
                        FragmentListTab1.Remove(oldFragment);

                if (PageNumber == 2)
                    if (FragmentListTab2.Contains(oldFragment))
                        FragmentListTab2.Remove(oldFragment);

                if (PageNumber == 3)
                    if (FragmentListTab3.Contains(oldFragment))
                        FragmentListTab3.Remove(oldFragment);

                if (PageNumber == 4)
                    if (FragmentListTab4.Contains(oldFragment))
                        FragmentListTab4.Remove(oldFragment);


                HideFragmentFromList(FragmentListTab0, ft);
                HideFragmentFromList(FragmentListTab1, ft);
                HideFragmentFromList(FragmentListTab2, ft);
                HideFragmentFromList(FragmentListTab3, ft);
                HideFragmentFromList(FragmentListTab4, ft);

                if (oldFragment.IsAdded)
                    ft.Remove(oldFragment);

                switch (PageNumber)
                {
                    case 0:
                        {
                            var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        }
                    case 1:
                        {
                            var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        }
                    case 2:
                        {
                            var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        }
                    case 3:
                        {
                            var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        }
                    case 4:
                        {
                            var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnBackStackClickFragment()
        {
            try
            {
                if (PageNumber == 0)
                {
                    if (FragmentListTab0.Count > 1)
                    {
                        var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                    }
                    else
                    {
                        Context.Finish();
                    }
                }
                else if (PageNumber == 1)
                {
                    if (FragmentListTab1.Count > 1)
                    {
                        var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                    }
                    else
                    {
                        Context.Finish();
                    }

                }
                else if (PageNumber == 2)
                {
                    if (FragmentListTab2.Count > 1)
                    {
                        var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                    }
                    else
                    {
                        Context.Finish();
                    }
                }
                else if (PageNumber == 3)
                {
                    if (FragmentListTab3.Count > 1)
                    {
                        var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                    }
                    else
                    {
                        Context.Finish();
                    }
                }
                else if (PageNumber == 4)
                {
                    if (FragmentListTab4.Count > 1)
                    {
                        var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                    }
                    else
                    {
                        Context.Finish();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment0()
        {
            try
            {
                if (FragmentListTab0.Count <= 0)
                    return;
                var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment1()
        {
            try
            {
                if (FragmentListTab1.Count <= 0) return;
                var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment2()
        {
            try
            {
                if (FragmentListTab2.Count <= 0) return;
                var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment3()
        {
            try
            {
                if (FragmentListTab3.Count <= 0) return;
                var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment4()
        {
            try
            {
                if (FragmentListTab4.Count <= 0) return;
                var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static bool BringFragmentToTop(Fragment Tobeshown, FragmentManager fragmentManager, List<Fragment> videoFrameLayoutFragments)
        {

            if (Tobeshown != null)
            {
                FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();


                foreach (var f in fragmentManager.Fragments)
                {
                    if (videoFrameLayoutFragments.Contains(f))
                    {
                        if (f == Tobeshown)
                            fragmentTransaction.Show(f);
                        else
                            fragmentTransaction.Hide(f);
                    }

                }

                fragmentTransaction.Commit();

                return true;
            }
            else
            {
                FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();

                foreach (var f in videoFrameLayoutFragments)
                {
                    fragmentTransaction.Hide(f);
                }

                fragmentTransaction.Commit();
            }

            return false;
        }
    }
}