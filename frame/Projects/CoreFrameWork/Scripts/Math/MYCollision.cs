using System;
using System.Collections.Generic;

namespace CoreFrameWork
{
    public static class MYCollision
    {
        public struct Sphere
        {
            public MYVector2f Center;
            public float Radius;

            public Sphere(MYVector2f center, float Radius)
            {
                this.Center = center;
                this.Radius = Radius;
            }
        }
    }
}
