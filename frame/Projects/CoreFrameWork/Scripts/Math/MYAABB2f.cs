using System;
using System.Collections.Generic;

namespace CoreFrameWork
{
    public struct MYAABB2f
    {
        public MYVector2f Max;
        public MYVector2f Min;

        public MYAABB2f(MYVector2f min, MYVector2f max)
        {
            Max = max;
            Min = min;
        }

        public MYAABB2f Reset()
        {
            Max.Set(float.NegativeInfinity, float.NegativeInfinity);
            Min.Set(float.PositiveInfinity, float.PositiveInfinity);

            return this;
        }

        public MYAABB2f AddPoint(MYVector2f v)
        {
            Max.Set(Math.Max(Max.x, v.x), Math.Max(Max.y, v.y));
            Min.Set(Math.Min(Min.x, v.x), Math.Min(Min.y, v.y));
            return this;
        }

        public MYVector2f Center
        {
            get { return (Max + Min) / 2f; }
        }

    }
}
