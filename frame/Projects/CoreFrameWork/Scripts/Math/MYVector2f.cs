//-----------------------------------------------------------------------
//| by:Qcbf                                                             |
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace CoreFrameWork
{
    public struct MYVector2f
    {
        public const double kEpsilon = 1E-5d;
        public const double equalNum = 9.999999E-11d;

        public float x;
        public float y;

        public static MYVector2f Zero
        {
            get
            {
                return new MYVector2f(0, 0);
            }
        }

        public static MYVector2f One
        {
            get
            {
                return new MYVector2f(1, 1);
            }
        }

        public static MYVector2f Max
        {
            get
            {
                return new MYVector2f(float.MaxValue, float.MaxValue);
            }
        }

        public static MYVector2f UnitX
        {
            get
            {
                return new MYVector2f(1, 0);
            }
        }

        public static MYVector2f UnitY
        {
            get
            {
                return new MYVector2f(0, 1);
            }
        }

        public float Magnitude
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y);
            }
        }

        public float SqrMagnitude
        {
            get
            {
                return x * x + y * y;
            }
        }


        public MYVector2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public MYVector2f(MYVector2f vec2)
        {
            this.x = vec2.x;
            this.y = vec2.y;
        }

        public MYVector2f Normalized
        {
            get
            {

                MYVector2f res = new MYVector2f(this);
                float num = res.Magnitude;
                if (num <= 1e-05f)
                {
                    res.x = 0;
                    res.y = 0;
                }
                else
                {
                    res.x /= num;
                    res.y /= num;
                }

                return res;
            }
        }

        public MYVector2f Inversed
        {
            get
            {
                return new MYVector2f(-x, -y);
            }
        }

        public void Inverse()
        {
            x = -x;
            y = -y;
        }

        public void Normalize()
        {
            float num = Magnitude;
            if (num <= 1e-05f)
            {
                x = 0;
                y = 0;
            }
            else
            {
                x /= num;
                y /= num;
            }
        }

        public void Set(float _x, float _y)
        {
            x = _x;
            y = _y;
        }

        public void Add(MYVector2f a)
        {
            x += a.x;
            y += a.y;
        }

        public void Add(float _x, float _y)
        {
            x += _x;
            y += _y;
        }

        public void Sub(MYVector2f a)
        {
            x -= a.x;
            y -= a.y;
        }
        public void Sub(float _x, float _y)
        {
            x -= _x;
            y -= _y;
        }

        public void Mul(float _x, float _y)
        {
            x *= _x;
            y *= _y;
        }

        public float Dot(MYVector2f a)
        {
            return x * a.x + y * a.y;
        }

        public void MoveForward(float angle, float speed)
        {
            x += speed * (float)Math.Cos(angle);
            y += speed * (float)Math.Sin(angle);
        }

        public float Distance(MYVector2f b)
        {
            return (float)Math.Sqrt(DistanceSquared(b));
        }

        public float DistanceSquared(MYVector2f b)
        {
            return (float)((x - b.x) * (x - b.x) + (y - b.y) * (y - b.y));
        }

        public float Angle(MYVector2f b)
        {
            return (float)Math.Atan2(b.y - this.y, b.x - this.x) * 180f / (float)Math.PI;
        }

        public override string ToString()
        {
            string dx = x.ToString("f4");
            string dy = y.ToString("f4");
            return string.Format("SimpleVector2({0}, {1})", dx, dy);
        }


        public static MYVector2f Lerp(MYVector2f from, MYVector2f to, float t)
        {
            return new MYVector2f(from.x + ((to.x - from.x) * t), from.y + ((to.y - from.y) * t));
        }


        public static float Distance(MYVector2f a, MYVector2f b)
        {
            return a.Distance(b);
        }

        public static float Angle(MYVector2f from, MYVector2f to)
        {
            return from.Angle(to);
        }

        public static MYVector2f operator +(MYVector2f a, MYVector2f b)
        {
            return new MYVector2f(a.x + b.x, a.y + b.y);
        }
        public static MYVector2f operator +(MYVector2f a, float[] b)
        {
            return new MYVector2f(a.x + b[0], a.y + b[1]);
        }

        public static MYVector2f operator -(MYVector2f a, MYVector2f b)
        {
            return new MYVector2f(a.x - b.x, a.y - b.y);
        }
        public static MYVector2f operator -(MYVector2f a, float[] b)
        {
            return new MYVector2f(a.x - b[0], a.y - b[1]);
        }

        public static MYVector2f operator *(MYVector2f a, int b)
        {
            return new MYVector2f(a.x * b, a.y * b);
        }

        public static MYVector2f operator *(MYVector2f a, float b)
        {
            return new MYVector2f(a.x * b, a.y * b);
        }

        public static MYVector2f operator /(MYVector2f a, int b)
        {
            return new MYVector2f(a.x / b, a.y / b);
        }

        public static MYVector2f operator /(MYVector2f a, float b)
        {
            return new MYVector2f(a.x / b, a.y / b);
        }

        public static bool operator ==(MYVector2f lhs, MYVector2f rhs)
        {
            return ((lhs - rhs).SqrMagnitude < equalNum);
        }

        public static bool operator !=(MYVector2f lhs, MYVector2f rhs)
        {
            return ((lhs - rhs).SqrMagnitude >= equalNum);
        }

        public override bool Equals(object obj)
        {
            return obj == null ? false : (x == ((MYVector2f)obj).x && y == ((MYVector2f)obj).y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
