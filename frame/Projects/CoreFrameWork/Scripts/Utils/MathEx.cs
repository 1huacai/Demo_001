
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFrameWork
{
    public static class MathEx
    {

        public static int Clamp(int v, int min, int max)
        {
            return v < min ? min : (v > max ? max : v);
        }

        public static float Clamp01(float v)
        {
            return v < 0 ? 0 : (v > 1 ? 1 : v);
        }
        public static double Clamp01(double v)
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
            return (from + ((to - from) * Clamp01(t)));
        }

        public static double Lerp(double from, double to, double t)
        {
            return (from + ((to - from) * Clamp01(t)));
        }

        public static float Repeat(float t, float length)
        {
            return (t - ((float)(Math.Floor(t / length)) * length));
        }
        public static double Repeat(double t, double length)
        {
            return (t - ((Math.Floor(t / length)) * length));
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
            return (a + (num * Clamp01(t)));
        }
        public static double LerpAngle(double a, double b, double t)
        {
            double num;
            num = Repeat(b - a, 360);
            if (num <= 180)
            {
                goto Label_0021;
            }
            num -= 360;
            Label_0021:
            return (a + (num * Clamp01(t)));
        }

        public static float Distance(float a, float b)
        {
            return Math.Abs(a - b);
        }
        public static double Distance(double a, double b)
        {
            return Math.Abs(a - b);
        }

        /// <summary>
        /// 返回
        /// 0:在范围内;
        /// 1:过大了;
        /// -1:过小了;
        /// </summary>
        public static int IsRange(float value, float min, float max)
        {
            return value < min ? -1 : (value > max ? 1 : 0);
        }
        public static int IsRange(double value, double min, double max)
        {
            return value < min ? -1 : (value > max ? 1 : 0);
        }

        /// <summary>
        /// 返回两个或更多值中最大的值。
        /// </summary>
        public static float Max(float a, float b)
        {
            return ((a <= b) ? b : a);
        }
        public static double Max(double a, double b)
        {
            return ((a <= b) ? b : a);
        }


        /// <summary>
        /// 返回两个或更多值中最小的值。
        /// </summary>
        public static float Min(float a, float b)
        {
            return ((a <= b) ? a : b);
        }
        public static double Min(double a, double b)
        {
            return ((a <= b) ? a : b);
        }


        /// <summary>
        /// 比较两个浮点数值，看它们是否非常接近。
        /// </summary>
        public static bool Approximately(float a, float b)
        {
            return (Math.Abs(b - a) < Max(1E-06f * Max(Math.Abs(a), Math.Abs(b)), 1.121039E-44f));
        }
        public static bool Approximately(double a, double b)
        {
            return (Math.Abs(b - a) < Max(1E-06 * Max(Math.Abs(a), Math.Abs(b)), 1.121039E-44));
        }

        public static float Distance(Vector3 a, Vector3 b) =>
            Vector3.Distance(a, b);

        public static float RadianXZ(Vector3 a, Vector3 b) =>
            Mathf.Atan2(b.x - a.x, b.z - a.z);

        public static float RadianXY(Vector2 a, Vector2 b) =>
            Mathf.Atan2(b.x - a.x, b.y - a.y);

        public static float RadianUni(Vector2 a, Vector2 b) =>
            1.5707963f - Mathf.Atan2(b.x - a.x, b.y - a.y);

        /// <summary>
        /// -180 ~ 180
        /// </summary>
        public static float AngleXY(Vector2 a, Vector2 b) =>
            RadianXY(a, b) * (180f / Mathf.PI);

        /// <summary>
        /// -180 ~ 180
        /// </summary>
        public static float AngleXZ(Vector3 a, Vector3 b) =>
            RadianXZ(a, b) * (180f / Mathf.PI);


        public static Vector2 ForwardXY(float dis, float radian) =>
            new Vector2(Mathf.Cos(radian) * dis, Mathf.Sin(radian) * dis);
        public static Vector3 ForwardXZ(float dis, float radian) =>
            new Vector3(Mathf.Sin(radian) * dis, 0, Mathf.Cos(radian) * dis);

        public static bool GetCirclePolygonsIntersect(float cx, float cy, float cradius, float[] posList)
        {
            Vector2 p = new Vector2(cx, cy);
            int len = posList.Length / 2;
            List<Vector3> polyPoints = new List<Vector3>();
            for (int i = 0; i < len; i++)
            {
                int indexZ = i * 2;
                Vector3 v2 = new Vector3(posList[indexZ], 0, posList[indexZ + 1]);
                polyPoints.Add(v2);
            }

            int count = 32;
            var radians = (3.1415926f / 180) * Math.Round(360.0 / count);
            List<Vector3> CirclePoints = new List<Vector3>();
            for (int i = 0; i < 32; i++)
            {
                double x = cx + cradius * Math.Sin(radians * i);
                double y = cy + cradius * Math.Cos(radians * i);
                Vector3 v2 = new Vector3((float)x, 0, (float)y);
                CirclePoints.Add(v2);
            }
            return GetPolygonsIntersect(polyPoints, CirclePoints);
        }

        /// <summary>
        /// 判断2个多边形相交
        /// </summary>
        /// <param name="vertices1"></param>
        /// <param name="vertices2"></param>
        /// <returns></returns>
        public static bool GetPolygonsIntersect(List<Vector3> vertices1, List<Vector3> vertices2)
        {

            for (int i = 2; i < vertices1.Count + 1; i++)
            {
                for (int x = 2; x < vertices2.Count + 1; x++)
                {
                    Vector3 pos = Vector3.zero;
                    var lineIntersect = GetLineIntersect(vertices1[i - 1], vertices1[i % vertices1.Count],
                        vertices2[x - 1], vertices2[x % vertices2.Count], out pos);
                    if (lineIntersect)
                        return true;
                }
            }

            var v2List1 = new List<Vector2>();
            var v2List2 = new List<Vector2>();
            for (int i = 0; i < vertices1.Count; i++)
            {
                v2List1.Add(new Vector2(vertices1[i].x, vertices1[i].z));
            }
            for (int i = 0; i < vertices2.Count; i++)
            {
                v2List2.Add(new Vector2(vertices2[i].x, vertices2[i].z));
            }
            for (int i = 0; i < v2List1.Count; i++)
            {
                if (ContainsPoint(v2List2.ToArray(), v2List1[i]) == true)
                    return true;
            }
            for (int i = 0; i < v2List2.Count; i++)
            {
                if (ContainsPoint(v2List1.ToArray(), v2List2[i]) == true)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 计算AB与CD两条线段是否相交.
        /// </summary>
        /// <param name="a">A点</param>
        /// <param name="b">B点</param>
        /// <param name="c">C点</param>
        /// <param name="d">D点</param>
        /// <param name="intersectPos">AB与CD的交点</param>
        /// <returns>是否相交 true:相交 false:未相交</returns>
        public static bool GetLineIntersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersectPos)
        {
            intersectPos = Vector3.zero;

            Vector3 ab = b - a;
            Vector3 ca = a - c;
            Vector3 cd = d - c;

            Vector3 v1 = Vector3.Cross(ca, cd);

            if (Mathf.Abs(Vector3.Dot(v1, ab)) > 1e-6)
            {
                // 不共面
                return false;
            }

            if (Vector3.Cross(ab, cd).sqrMagnitude <= 0)
            {
                // 平行
                return false;
            }

            Vector3 ad = d - a;
            Vector3 cb = b - c;
            // 快速排斥
            if (Mathf.Min(a.x, b.x) > Mathf.Max(c.x, d.x) || Mathf.Max(a.x, b.x) < Mathf.Min(c.x, d.x)
               || Mathf.Min(a.y, b.y) > Mathf.Max(c.y, d.y) || Mathf.Max(a.y, b.y) < Mathf.Min(c.y, d.y)
               || Mathf.Min(a.z, b.z) > Mathf.Max(c.z, d.z) || Mathf.Max(a.z, b.z) < Mathf.Min(c.z, d.z)
            )
                return false;

            // 跨立试验
            if (Vector3.Dot(Vector3.Cross(-ca, ab), Vector3.Cross(ab, ad)) > 0
                && Vector3.Dot(Vector3.Cross(ca, cd), Vector3.Cross(cd, cb)) > 0)
            {
                Vector3 v2 = Vector3.Cross(cd, ab);
                float ratio = Vector3.Dot(v1, v2) / v2.sqrMagnitude;
                intersectPos = a + ab * ratio;
                return true;
            }
            return false;
        }


        /// <summary>
        /// 计算点到线段的最短距离
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <returns></returns>
        public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 rhs = point - lineStart;
            Vector3 vector3 = lineEnd - lineStart;
            float magnitude = vector3.magnitude;
            Vector3 lhs = vector3;
            if ((double)magnitude > 9.99999997475243E-07)
                lhs /= magnitude;
            float num = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0.0f, magnitude);
            Vector3 v3 = lineStart + lhs * num;
            return Vector3.Magnitude(v3 - point);
        }

        public static Vector3 GetClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 p, bool withinSegment)
        {
            Vector3 a = lineStart;
            Vector3 b = lineEnd;

            //Assume the line goes from a to b
            Vector3 ab = b - a;
            //Vector from start of the line to the point outside of line
            Vector3 ap = p - a;

            //The normalized "distance" from a to the closest point, so [0,1] if we are within the line segment
            float distance = Vector3.Dot(ap, ab) / Vector3.SqrMagnitude(ab);


            ///This point may not be on the line segment, if so return one of the end points
            float epsilon = 0.00001f;

            if (withinSegment && distance < 0f - epsilon)
            {
                return a;
            }
            else if (withinSegment && distance > 1f + epsilon)
            {
                return b;
            }
            else
            {
                //This works because a_b is not normalized and distance is [0,1] if distance is within ab
                return a + ab * distance;
            }
        }

        /// <summary>
        /// 点是否在线段上
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <returns></returns>
        public static bool IsPointOnSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            return Mathf.Approximately(Mathf.Abs((lineStart - point).magnitude) + Mathf.Abs((lineEnd - point).magnitude),
                            Mathf.Abs((lineEnd - lineStart).magnitude));
        }

        /// <summary>
        /// 计算点是否在多边形中
        /// </summary>
        /// <param name="polyPoints"></param>
        /// <param name="p"></param>
        /// <param name="InSide">必须在内部 在点上或线段上都不算</param>
        /// <returns></returns>
        public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p, bool InSide = false, float dis = 0)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                if (InSide == false)
                    if (p == pi || p == pj)
                        return true;
                if (((pi.y < p.y && p.y < pj.y) || (pj.y < p.y && p.y < pi.y)) &&
                    p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x)
                    inside = !inside;
            }
            if (InSide)
                return inside;
            else if (inside)
                return true;

            float distanceToLine = Mathf.Infinity;
            for (int i = 1; i < polyPoints.Length + 1; i++)
            {
                var distance = DistancePointLine(p, polyPoints[i - 1], polyPoints[i % polyPoints.Length]);

                if (distance < distanceToLine)
                    distanceToLine = distance;
            }

            return inside || distanceToLine <= dis;

        }

        public static bool ContainsPoint2(Vector2[] polyPoints, Vector2 p, bool InSide = false, float dis = 0)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                if (InSide == false)
                    if (p == pi || p == pj)
                        return true;
                if (((pi.y < p.y && p.y < pj.y) || (pj.y < p.y && p.y < pi.y)) &&
                    p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x)
                    inside = !inside;
            }
            if (InSide)
                return inside;
            else if (inside)
                return true;

            for (int i = 1; i < polyPoints.Length + 1; i++)
            {
                if (IsPointOnSegment(p, polyPoints[i - 1], polyPoints[i % polyPoints.Length]))
                    return true;
            }

            return inside;

        }

        /// <summary>
        /// 计算点是否在多边形中 V3只计算X,Z轴
        /// </summary>
        /// <param name="polyPoints"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool ContainsPoint(Vector3[] polyPoints, Vector3 p, bool InSide = false, float dis = 0)
        {
            var Points = new List<Vector2>();
            for (int i = 0; i < polyPoints.Length; i++)
            {
                Points.Add(new Vector2(polyPoints[i].x, polyPoints[i].z));
            }

            return ContainsPoint(Points.ToArray(), new Vector2(p.x, p.z), InSide, dis);
        }

        public static bool ContainsPoint2(Vector3[] polyPoints, Vector3 p, bool InSide = false, float dis = 0)
        {
            var Points = new List<Vector2>();
            for (int i = 0; i < polyPoints.Length; i++)
            {
                Points.Add(new Vector2(polyPoints[i].x, polyPoints[i].z));
            }

            return ContainsPoint2(Points.ToArray(), new Vector2(p.x, p.z), InSide, dis);
        }
    }
}

