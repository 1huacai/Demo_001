using System;
using System.Collections.Generic;

namespace CoreFrameWork
{
    public struct MYVector3f
    {
        public const float kEpsilon = 1E-05f;
        public const float equalNum = 9.999999E-11f;
        public float x;
        public float y;
        public float z;

        public static MYVector3f UnitX
        {
            get
            {
                return new MYVector3f(1, 0, 0);
            }
        }

        public static MYVector3f UnitY
        {
            get
            {
                return new MYVector3f(0, 1, 0);
            }
        }

        public static MYVector3f UnitZ
        {
            get
            {
                return new MYVector3f(0, 0, 1);
            }
        }

        public static MYVector3f Zero
        {
            get
            {
                return new MYVector3f(0, 0, 0);
            }
        }

        public static MYVector3f One
        {
            get
            {
                return new MYVector3f(1, 1, 1);
            }
        }

        public static MYVector3f Lerp(MYVector3f from, MYVector3f to, float t)
        {
            t = MYMath.Clamp01(t);
            return new MYVector3f(from.x + ((to.x - from.x) * t), from.y + ((to.y - from.y) * t), from.z + ((to.z - from.z) * t));
        }

        public static MYVector3f SmoothDamp(MYVector3f current, MYVector3f target, ref MYVector3f currentVelocity, float smoothTime, float maxSpeed = 1, float deltaTime = 0.02f)
        {
            smoothTime = Math.Max(0.0001f, smoothTime);
            float num = 2f / smoothTime;
            float num2 = num * deltaTime;
            float num3 = 1f / (((1f + num2) + ((0.48f * num2) * num2)) + (((0.235f * num2) * num2) * num2));
            MYVector3f vector = current - target;
            MYVector3f vector2 = target;
            float maxLength = maxSpeed * smoothTime;
            vector = ClampMagnitude(vector, maxLength);
            target = current - vector;
            MYVector3f MYVector3f = ((currentVelocity + (num * vector)) * deltaTime);
            currentVelocity = ((currentVelocity - (num * MYVector3f)) * num3);
            MYVector3f vector4 = target + (((vector + MYVector3f) * num3));
            if (Dot(vector2 - current, vector4 - vector2) > 0f)
            {
                vector4 = vector2;
                currentVelocity = ((vector4 - vector2) / deltaTime);
            }
            return vector4;
        }

        public static MYVector3f ClampMagnitude(MYVector3f vector, float maxLength)
        {
            if (vector.SqrMagnitude > (maxLength * maxLength))
            {
                return vector.Normalized * maxLength;
            }
            return vector;
        }

        public float SqrMagnitude
        {
            get
            {
                return (((x * x) + (y * y)) + (z * z));
            }
        }

        public float Magnitude
        {
            get
            {
                return (float)Math.Sqrt(((x * x) + (y * y)) + (z * z));
            }
        }

        public MYVector3f Normalized
        {
            get
            {
                float num = Magnitude;
                if (num > kEpsilon)
                {
                    return (this / num);
                }
                return Zero;
            }
        }

        public void Normalize()
        {
            float num = Magnitude;
            if (num > kEpsilon)
            {
                x /= num;
                y /= num;
                z /= num;
            }
            else
            {
                x = 0;
                y = 0;
                z = 0;
            }
        }

        public void Inverse()
        {
            x = -x;
            y = -y;
            z = -z;
        }

        public MYVector3f Inversed
        {
            get
            {
                return new MYVector3f(-x, -y, -z);
            }
        }

        public static float Dot(MYVector3f lhs, MYVector3f rhs)
        {
            return (((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z));
        }

        public float Dot(MYVector3f rhs)
        {
            return (((x * rhs.x) + (y * rhs.y)) + (z * rhs.z));
        }

        public static MYVector3f Scale(MYVector3f a, MYVector3f b)
        {
            return new MYVector3f(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static MYVector3f Cross(MYVector3f lhs, MYVector3f rhs)
        {
            return new MYVector3f((lhs.y * rhs.z) - (lhs.z * rhs.y), (lhs.z * rhs.x) - (lhs.x * rhs.z), (lhs.x * rhs.y) - (lhs.y * rhs.x));
        }

        public static MYVector3f Project(MYVector3f vector, MYVector3f onNormal)
        {
            float num = Dot(onNormal, onNormal);
            if (num < float.Epsilon)
            {
                return Zero;
            }
            return ((onNormal * Dot(vector, onNormal)) / num);
        }

        public static MYVector3f Exclude(MYVector3f excludeThis, MYVector3f fromThat)
        {
            return (fromThat - Project(fromThat, excludeThis));
        }

        public static float Angle(MYVector3f from, MYVector3f to)
        {
            return (float)(Math.Acos(MYMath.Clamp(Dot(from.Normalized, to.Normalized), -1f, 1f)) * 57.29578f);
        }

        public static float Distance(MYVector3f a, MYVector3f b)
        {
            MYVector3f vector = new MYVector3f(a.x - b.x, a.y - b.y, a.z - b.z);
            return (float)Math.Sqrt(((vector.x * vector.x) + (vector.y * vector.y)) + (vector.z * vector.z));
        }

        public static MYVector3f Min(MYVector3f lhs, MYVector3f rhs)
        {
            return new MYVector3f(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
        }

        public static MYVector3f Max(MYVector3f lhs, MYVector3f rhs)
        {
            return new MYVector3f(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
        }

        public static MYVector3f MoveTowards(MYVector3f current, MYVector3f target, float maxDistanceDelta)
        {
            MYVector3f vector = target - current;
            float magnitude = vector.Magnitude;
            if ((magnitude > maxDistanceDelta) && (magnitude != 0f))
            {
                return (current + ((MYVector3f)((vector / magnitude) * maxDistanceDelta)));
            }
            return target;
        }

        public MYVector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public MYVector3f(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0f;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.x;

                    case 1:
                        return this.y;

                    case 2:
                        return this.z;
                }
                throw new IndexOutOfRangeException("Invalid Vector3 index!");
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;

                    case 1:
                        this.y = value;
                        break;

                    case 2:
                        this.z = value;
                        break;

                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public void Set(float new_x, float new_y, float new_z)
        {
            this.x = new_x;
            this.y = new_y;
            this.z = new_z;
        }

        public void Scale(MYVector3f scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
            this.z *= scale.z;
        }

        public void Add(MYVector3f v)
        {
            x += v.x;
            y += v.y;
            z += v.z;
        }

        public void Sub(MYVector3f v)
        {
            x -= v.x;
            y -= v.y;
            z -= v.z;
        }

        public void Mul(MYVector3f v)
        {
            x *= v.x;
            y *= v.y;
            z *= v.z;
        }

        public string ToString(string format)
        {
            object[] args = new object[] { this.x.ToString(format), this.y.ToString(format), this.z.ToString(format) };
            return string.Format("({0}, {1}, {2})", args);
        }

        #region override funtion

        public override int GetHashCode()
        {
            return ((this.x.GetHashCode() ^ (this.y.GetHashCode() << 2)) ^ (this.z.GetHashCode() >> 2));
        }

        public override bool Equals(object other)
        {
            if (!(other is MYVector3f))
            {
                return false;
            }
            MYVector3f vector = (MYVector3f)other;
            return ((this.x.Equals(vector.x) && this.y.Equals(vector.y)) && this.z.Equals(vector.z));
        }

        public override string ToString()
        {
            object[] args = new object[] { this.x, this.y, this.z };
            return string.Format("({0:F}, {1:F}, {2:F})", args);
        }
        #endregion

        #region reload function
        public static MYVector3f operator +(MYVector3f a, MYVector3f b)
        {
            return new MYVector3f(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static MYVector3f operator +(MYVector3f a, float b)
        {
            return new MYVector3f(a.x + b, a.y + b, a.z + b);
        }

        public static MYVector3f operator -(MYVector3f a, MYVector3f b)
        {
            return new MYVector3f(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static MYVector3f operator -(MYVector3f a)
        {
            return new MYVector3f(-a.x, -a.y, -a.z);
        }

        public static MYVector3f operator *(MYVector3f a, float d)
        {
            return new MYVector3f(a.x * d, a.y * d, a.z * d);
        }

        public static MYVector3f operator *(float d, MYVector3f a)
        {
            return new MYVector3f(a.x * d, a.y * d, a.z * d);
        }

        public static MYVector3f operator /(MYVector3f a, float d)
        {
            return new MYVector3f(a.x / d, a.y / d, a.z / d);
        }

        public static bool operator ==(MYVector3f lhs, MYVector3f rhs)
        {
            return ((lhs - rhs).SqrMagnitude < equalNum);
        }

        public static bool operator !=(MYVector3f lhs, MYVector3f rhs)
        {
            return ((lhs - rhs).SqrMagnitude >= equalNum);
        }
        #endregion
    }
}
