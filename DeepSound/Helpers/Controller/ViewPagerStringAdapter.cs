using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using DeepSound.Helpers.Utils;
using System;
using System.Collections.Generic;
using Object = Java.Lang.Object;

namespace DeepSound.Helpers.Controller
{
    public class ViewPagerStringAdapter : PagerAdapter
    {
        public class ViewPagerStrings
        {
            public string Description;
            public string Header;
        }

        private Context Context;
        private readonly List<ViewPagerStrings> ListDescriptions;
        private readonly LayoutInflater Inflater;

        public ViewPagerStringAdapter(Context context, List<ViewPagerStrings> listDescriptions)
        {
            Context = context;
            ListDescriptions = listDescriptions;
            Inflater = LayoutInflater.From(context);
        }

        public override Object InstantiateItem(ViewGroup view, int position)
        {
            try
            {
                var layout = Inflater.Inflate(Resource.Layout.FirstPageViewPagerLayout, view, false);
                var tvDescription = layout.FindViewById<TextView>(Resource.Id.tv_description);

                tvDescription.Text = ListDescriptions[position].Description;

                view.AddView(layout);

                return layout;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view.Equals(@object);
        }

        public override int Count
        {
            get
            {
                if (ListDescriptions != null)
                {
                    return ListDescriptions.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            try
            {
                var view = (View)@object;
                container.RemoveView(view);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

        }

    }
}