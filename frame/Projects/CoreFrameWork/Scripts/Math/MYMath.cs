using System;

namespace CoreFrameWork
{

    public static class MYMath
    {

        public static int Clamp(int v, int min, int max)
        {
            return v < min ? min : (v > max ? max : v);
        }

        public static float Clamp01(float v)
        {
            return v < 0 ? 0 : (v > 1 ? 1 : v);
        }
        public static double Clamp02(double v)
        {
            return v < 0 ? 0 : (v > 1 ? 1 : v);
        }

        public static float Clamp(float v, float min, float max)
        {
            return v < min ? min : (v > max ? max : v);
        }
        public static double Clamp(double v, double min, double max)
        {
            return v < min ? min : (v > max ? max : v);
        }

        public static float Lerp(float from, float to, float t)
        {
            return (from + ((to - from)*Clamp01(t)));
        }

        public static float Repeat(float t, float length)
        {
            return (t - ((float) (Math.Floor(t/length))*length));
        }

        public static float LerpAngle(float a, float b, float t)
        {
            float num;
            num = Repeat(b - a, 360f);
            if (num <= 180f)
            {
                goto Label_0021;
            }
            num -= 360f;
            Label_0021:
            return (a + (num*Clamp01(t)));
        }

        public static float Distance(float a, float b)
        {
            return Math.Abs(a - b);
        }

        public static float Ln(float a)
        {
            return (float) Math.Log(a, Math.E);
        }

        public static double Ln(double a)
        {
            return Math.Log(a, Math.E);
        }


        public static bool IsRange(float value, float min, float max)
        {
            return value < min ? false : (value > max ? false : true);
        }

        public static MYVector2d ToVector2(this MYVector3d v)
        {
            return new MYVector2d(v.x, v.z);
        }

        public static MYVector2f ToVector2(this MYVector3f v)
        {
            return new MYVector2f(v.x, v.z);
        }

        public static MYVector3d ToVector3(this MYVector2d v, double y = 0)
        {
            return new MYVector3d(v.x, y, v.y);
        }

        public static MYVector3f ToVector3(this MYVector2f v, float y = 0)
        {
            return new MYVector3f(v.x, y, v.y);
        }
    }
}

