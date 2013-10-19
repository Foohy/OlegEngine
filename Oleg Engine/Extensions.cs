using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using OpenTK;

namespace OlegEngine
{
    public static class Extensions
    {
        public static Vector3 Multiply(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3 Divide(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3 Cross(this Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static Vector2 Multiply(this Vector2 a, Vector2 b)
        {
            return new Vector2(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2 Divide(this Vector2 a, Vector2 b)
        {
            return new Vector2(a.X / b.X, a.Y / b.Y);
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
            sy = (float)Math.Sin(this.Yaw * Utilities.F_DEG2RAD); cy = (float)Math.Cos(this.Yaw * Utilities.F_DEG2RAD);
            sp = (float)Math.Sin(this.Pitch * Utilities.F_DEG2RAD); cp = (float)Math.Cos(this.Pitch * Utilities.F_DEG2RAD);
            sr = (float)Math.Sin(this.Roll * Utilities.F_DEG2RAD); cr = (float)Math.Cos(this.Roll * Utilities.F_DEG2RAD);

            Forward = new Vector3(cp * cy, sp, cp * sy);
            Right = new Vector3((sr * sp * cy + -1 * cr * -sy), (-1 * sr * cp), (sr * sp * sy + -1 * cr * cy));
            Up = new Vector3((cr * -sp * cy + -sr * -sy), cr * cp, (cr * -sp * sy + -sr * cy));
        }

        public Vector3 Forward()
        {
            float sp, sy, cp, cy;

            sy = (float)Math.Sin(this.Yaw * Utilities.F_DEG2RAD); cy = (float)Math.Cos(this.Yaw * Utilities.F_DEG2RAD);
            sp = (float)Math.Sin(this.Pitch * Utilities.F_DEG2RAD); cp = (float)Math.Cos(this.Pitch * Utilities.F_DEG2RAD);

            return new Vector3(cp * cy, sp, cp * sy);
        }

        public Vector3 Right()
        {
            Vector3 forward, up, right;
            AngleVectors(out forward, out up, out right);
            return right;
        }

        public Vector3 Up()
        {
            Vector3 forward, up, right;
            AngleVectors(out forward, out up, out right);
            return up;
        }
    }

    /// <summary>
    /// Vertex class to hold important information about the data that makes up a mesh
    /// TODO: Optimize this so it doesn't all use floats for things that don't need it
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public static readonly int SizeInBytes = BlittableValueType.StrideOf(new Vertex());

        /// <summary>
        /// The position in model-space of the vertex
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// The UV coordinate for textures for this vertex
        /// </summary>
        public Vector2 UV;
        /// <summary>
        /// The normal pointing away from the vertex/face
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// The tangent of the vertex. This can be generated automatically by calling <code>Utilities.GenerateTangents()</code> with a list of vertices
        /// </summary>
        public Vector3 Tangent;
        /// <summary>
        /// The color of the vertex. Will be blended over textures/stuff
        /// </summary>
        public Vector3 Color;

        public Vertex( Vector3 position, Vector3 normal, Vector3 tangent, Vector3 color, Vector2 uv)
        {
            Position    = position;
            Normal      = normal;
            Tangent     = tangent;
            Color       = color;
            UV          = uv;
        }

        public Vertex(Vector3 position)
        {
            Position    = position;
            Normal      = Vector3.Zero;
            Tangent     = Vector3.Zero;
            Color       = Vector3.One;
            UV          = Vector2.Zero;
        }

        public Vertex(Vector3 position, Vector3 normal)
        {
            Position    = position;
            Normal      = normal;
            Tangent     = Vector3.Zero;
            Color       = Vector3.One;
            UV          = Vector2.Zero;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            Position    = position;
            Normal      = normal;
            Tangent     = Vector3.Zero;
            Color       = Vector3.One;
            UV          = uv;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 uv, Vector3 color)
        {
            Position    = position;
            Normal      = normal;
            Tangent     = Vector3.Zero;
            Color       = color;
            UV          = uv;
        }
    }
}
