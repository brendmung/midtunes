﻿using static AndroidX.RecyclerView.Widget.GridLayoutManager;

namespace DeepSound.Helpers.Utils
{
    public class MySpanSizeLookup : SpanSizeLookup
    {
        public int SpanPos, SpanCnt1, SpanCnt2;
        public MySpanSizeLookup(int spanPos, int spanCnt1, int spanCnt2)
        {
            SpanPos = spanPos;
            SpanCnt1 = spanCnt1;
            SpanCnt2 = spanCnt2;
        }

        public override int GetSpanSize(int position)
        {

            return position % SpanPos == 0 ? SpanCnt2 : SpanCnt1;
        }
    }

    public class MySpanSizeLookup2 : SpanSizeLookup
    {
        public int SpanPos, SpanCnt1, SpanCnt2;
        public MySpanSizeLookup2(int spanPos, int spanCnt1, int spanCnt2)
        {
            SpanPos = spanPos;
            SpanCnt1 = spanCnt1;
            SpanCnt2 = spanCnt2;
        }

        public override int GetSpanSize(int position)
        {
            if (position < SpanPos)
            {
                return SpanCnt1;
            }
            else
            {
                return SpanCnt2;
            }

        }
    }
}