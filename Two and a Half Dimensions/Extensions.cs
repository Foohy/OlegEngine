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

    public struct Angle
    {
        public float Pitch;
        public float Yaw;
        public float Roll;
        public static readonly Angle Zero;

        private const double DEG2RAD = Math.PI / 180;

        //Operator overloads
        public static Angle operator +(Angle ang1, Angle ang2)
        {
            return new Angle(ang1.Pitch + ang2.Pitch, ang1.Yaw + ang2.Yaw, ang1.Roll + ang2.Roll);
        }
        public static Angle operator -(Angle ang1, Angle ang2)
        {
            return new Angle(ang1.Pitch - ang2.Pitch, ang1.Yaw - ang2.Yaw, ang1.Roll - ang2.Roll);
        }
        public static Angle operator *(Angle ang1, Angle ang2)
        {
            return new Angle(ang1.Pitch * ang2.Pitch, ang1.Yaw * ang2.Yaw, ang1.Roll * ang2.Roll);
        }
        public static Angle operator /(Angle ang1, Angle ang2)
        {
            return new Angle(ang1.Pitch / ang2.Pitch, ang1.Yaw / ang2.Yaw, ang1.Roll / ang2.Roll);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", this.Pitch, this.Yaw, this.Roll);
        }

        public Angle(float p, float y, float r)
        {
            Pitch = p;
            Yaw = y;
            Roll = r;
        }

        public Angle SetPitch(float p)
        {
            this.Pitch = p;
            return this;
        }

        public Angle SetYaw(float y)
        {
            this.Yaw = y;
            return this;
        }

        public Angle SetRoll(float r)
        {
            this.Roll = r;
            return this;
        }

        public void AngleVectors(out Vector3 Forward, out Vector3 Up, out Vector3 Right)
        {
            float sr, sp, sy, cr, cp, cy;
            sy = (float)Math.Sin(this.Yaw * DEG2RAD); cy = (float)Math.Cos(this.Yaw * DEG2RAD);
            sp = (float)Math.Sin(this.Pitch * DEG2RAD); cp = (float)Math.Cos(this.Pitch * DEG2RAD);
            sr = (float)Math.Sin(this.Roll * DEG2RAD); cr = (float)Math.Cos(this.Roll * DEG2RAD);

            Forward = new Vector3(cp * cy, sp, cp * sy);
            Right = new Vector3((sr * sp * cy + -1 * cr * -sy), (-1 * sr * cp), (sr * sp * sy + -1 * cr * cy));
            Up = new Vector3((cr * -sp * cy + -sr * -sy), cr * cp, (cr * -sp * sy + -sr * cy));
        }

        public Vector3 Forward()
        {
            float sp, sy, cp, cy;

            sy = (float)Math.Sin(this.Yaw * DEG2RAD); cy = (float)Math.Cos(this.Yaw * DEG2RAD);
            sp = (float)Math.Sin(this.Pitch * DEG2RAD); cp = (float)Math.Cos(this.Pitch * DEG2RAD);

            return new Vector3(cp * cy, sp, cp * sy);
        }
    }
}
