using System;

namespace CoreFrameWork
{
    public struct MYAABB2d
    {
        public MYVector2d Max;
        public MYVector2d Min;

        public MYAABB2d(MYVector2d min, MYVector2d max)
        {
            Max = max;
            Min = min;
        }

        public MYAABB2d Reset()
        {
            Max.Set(double.NegativeInfinity, double.NegativeInfinity);
            Min.Set(double.PositiveInfinity, double.PositiveInfinity);

            return this;
        }

        public MYAABB2d AddPoint(MYVector2d v)
        {
            Max.Set(Math.Max(Max.x, v.x), Math.Max(Max.y, v.y));
            Min.Set(Math.Min(Min.x, v.x), Math.Min(Min.y, v.y));
            return this;
        }

        public MYVector2d Center
        {
            get { return (Max + Min) / 2; }
        }

    }
}
