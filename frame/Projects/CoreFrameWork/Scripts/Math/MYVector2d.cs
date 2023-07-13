using System;
using System.Collections.Generic;

namespace CoreFrameWork
{
    public struct MYVector2d
    {
        public double x;
        public double y;

        public const double kEpsilon = 1E-5d;
        public const double equalNum = 9.999999E-11d;

        public static MYVector2d Zero
        {
            get
            {
                return new MYVector2d(0, 0);
            }
        }

        public static MYVector2d One
        {
            get
            {
                return new MYVector2d(1, 1);
            }
        }

        public static MYVector2d Max
        {
            get
            {
                return new MYVector2d(double.MaxValue, double.MaxValue);
            }
        }

        public static MYVector2d UnitX
        {
            get
            {
                return new MYVector2d(1, 0);
            }
        }

        public static MYVector2d UnitY
        {
            get
            {
                return new MYVector2d(0, 1);
            }
        }

        public double Magnitude
        {
            get
            {
                return Math.Sqrt(x * x + y * y);
            }
        }

        public double SqrMagnitude
        {
            get
            {
                return x * x + y * y;
            }
        }


        public MYVector2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public MYVector2d(MYVector2d vec2)
        {
            this.x = vec2.x;
            this.y = vec2.y;
        }

        public MYVector2d Normalized
        {
            get
            {
                MYVector2d res = new MYVector2d(this);
                double num = res.Magnitude;
                if (num <= 1e-05d)
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

        public MYVector2d Inversed
        {
            get
            {
                return new MYVector2d(-x, -y);
            }
        }

        public void Inverse()
        {
            x = -x;
            y = -y;
        }

        public void Normalize()
        {
            double num = Magnitude;
            if (num <= 1e-05d)
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

        public void Set(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

        public void Add(MYVector2d a)
        {
            x += a.x;
            y += a.y;
        }

        public void Add(double _x, double _y)
        {
            x += _x;
            y += _y;
        }

        public void Sub(MYVector2d a)
        {
            x -= a.x;
            y -= a.y;
        }
        public void Sub(double _x, double _y)
        {
            x -= _x;
            y -= _y;
        }

        public void Mul(float _x, float _y)
        {
            x *= _x;
            y *= _y;
        }

        public void Mul(MYVector2d v)
        {
            x *= v.x;
            y *= v.y;
        }

        public double Dot(MYVector2d v)
        {
            return x * v.x + y * v.y;
        }

        public void MoveForward(double angle, double speed)
        {
            x += speed * Math.Cos(angle);
            y += speed * Math.Sin(angle);
        }

        public double Distance(MYVector2d b)
        {
            return Math.Sqrt(DistanceSquared(b));
        }

        public double DistanceSquared(MYVector2d b)
        {
            return (x - b.x) * (x - b.x) + (y - b.y) * (y - b.y);
        }

        public double Angle(MYVector2d b)
        {
            return Math.Atan2(b.y - this.y, b.x - this.x) * 180f / Math.PI;
        }

        public override string ToString()
        {
            string dx = x.ToString("f4");
            string dy = y.ToString("f4");
            return string.Format("SimpleVector2({0}, {1})", dx, dy);
        }


        public static MYVector2d Lerp(MYVector2d from, MYVector2d to, double t)
        {
            return new MYVector2d(from.x + ((to.x - from.x) * t), from.y + ((to.y - from.y) * t));
        }


        public static double Distance(MYVector2d a, MYVector2d b)
        {
            return a.Distance(b);
        }

        public static double Angle(MYVector2d from, MYVector2d to)
        {
            return from.Angle(to);
        }

        public static MYVector2d operator +(MYVector2d a, MYVector2d b)
        {
            return new MYVector2d(a.x + b.x, a.y + b.y);
        }
        public static MYVector2d operator +(MYVector2d a, double[] b)
        {
            return new MYVector2d(a.x + b[0], a.y + b[1]);
        }

        public static MYVector2d operator -(MYVector2d a, MYVector2d b)
        {
            return new MYVector2d(a.x - b.x, a.y - b.y);
        }
        public static MYVector2d operator -(MYVector2d a, double[] b)
        {
            return new MYVector2d(a.x - b[0], a.y - b[1]);
        }

        public static MYVector2d operator *(MYVector2d a, int b)
        {
            return new MYVector2d(a.x * b, a.y * b);
        }

        public static MYVector2d operator *(MYVector2d a, double b)
        {
            return new MYVector2d(a.x * b, a.y * b);
        }

        public static MYVector2d operator /(MYVector2d a, int b)
        {
            return new MYVector2d(a.x / b, a.y / b);
        }

        public static MYVector2d operator /(MYVector2d a, double b)
        {
            return new MYVector2d(a.x / b, a.y / b);
        }

        public static bool operator ==(MYVector2d lhs, MYVector2d rhs)
        {
            return ((lhs - rhs).SqrMagnitude < equalNum);
        }

        public static bool operator !=(MYVector2d lhs, MYVector2d rhs)
        {
            return ((lhs - rhs).SqrMagnitude >= equalNum);
        }

        public override bool Equals(object obj)
        {
            return obj == null ? false : (x == ((MYVector2d)obj).x && y == ((MYVector2d)obj).y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
