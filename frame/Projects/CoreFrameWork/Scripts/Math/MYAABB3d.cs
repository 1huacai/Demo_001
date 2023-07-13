using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreFrameWork
{
    public struct MYAABB3d
    {
        public MYVector3d Max;
        public MYVector3d Min;

        public MYAABB3d(MYVector3d min, MYVector3d max)
        {
            Max = max;
            Min = min;
        }

        public MYAABB3d Reset()
        {
            Max.Set(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            Min.Set(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);

            return this;
        }

        public MYAABB3d AddPoint(MYVector3d v)
        {
            Max.Set(Math.Max(Max.x, v.x), Math.Max(Max.y, v.y), Math.Max(Max.z, v.z));
            Min.Set(Math.Min(Min.x, v.x), Math.Min(Min.y, v.y), Math.Min(Min.z, v.z));
            return this;
        }

        public MYVector3d Center
        {
            get { return (Max + Min) / 2; }
        }

    }
}
