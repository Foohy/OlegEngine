using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace OlegEngine
{
    public static class Extensions
    {
        public static Vector3 Multiply(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3 Cross(this Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static float InnerProduct(this Vector3 a, Vector3 b)
        {
            return (a.X * b.X + a.Y * b.Y + a.Z * b.Z);
        }

        //Rand
        public static double NextDouble(this Random rand, double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }
    }
}
