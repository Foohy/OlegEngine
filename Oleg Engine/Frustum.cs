using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine
{
    public class Frustum
    {
        public float Angle { get; private set; }
        public float Ratio { get; private set; }
        public float NearD { get; private set; }
        public float FarD { get; private set; }

        //Frustum sphere stuff
        public float Radius { get; private set; }
        public Vector3 Center { get; private set; }

        //Frustum cone stuff
        public float ConeAngle;
        public Vector3 ViewPosition;
        public Vector3 ViewAxis;
        float ConeAngSinRecip;
        float ConeAngleSinSqr;
        float ConeAngleCosSqr;

        public float tang { get; private set; }
        public float nh { get; private set; }
        public float nw { get; private set; }
        public float fh { get; private set; }
        public float fw { get; private set; }

        public Vector3 ntl { get; private set; }
        public Vector3 ntr { get; private set; }
        public Vector3 nbl { get; private set; }
        public Vector3 nbr { get; private set; }

        public Vector3 ftl { get; private set; }
        public Vector3 ftr { get; private set; }
        public Vector3 fbl { get; private set; }
        public Vector3 fbr { get; private set; }

        Plane[] pl = new Plane[6];

        public enum FrustumState
        {
            INSIDE,
            OUTSIDE,
            INTERSECT
        }
        enum FrustumPlanes
        {
            TOP = 0,
            BOTTOM,
            LEFT,
            RIGHT,
            NEARP,
            FARP
        }

        public void SetCamInternals(float angle, float ratio, float nearD, float farD)
        {
            Angle = angle;
            Ratio = ratio;
            NearD = nearD;
            FarD = farD;

            tang = (float)Math.Tan(angle * 0.5f);
            nh = nearD * tang;
            nw = nh * ratio;
            fh = farD * tang;
            fw = fh * ratio;

            ConstructPlanes();
            ConstructFrustumSphere();
            ConstructFrustumCone();
        }

        public void SetCameraDef(Vector3 p, Vector3 l, Vector3 u)
        {
            if (pl[0] == null) { ConstructPlanes(); }
            Vector3 nc, fc, X, Y, Z;

            // compute the Z axis of camera
            // this axis points in the opposite direction from
            // the looking direction
            Z = p - l;
            Z.Normalize();

            // X axis of camera with given "up" vector and Z axis

            //X = u * Z;
            X = u.Cross(Z);
            X.Normalize();

            // the real "up" vector is the cross product of Z and X
            //Y = Z * X;
            Y = Z.Cross(X);

            // compute the centers of the near and far planes
            nc = p - Z * NearD;
            fc = p - Z * FarD;

            // compute the 4 corners of the frustum on the near plane
            ntl = nc + Y * nh - X * nw;
            ntr = nc + Y * nh + X * nw;
            nbl = nc - Y * nh - X * nw;
            nbr = nc - Y * nh + X * nw;

            // compute the 4 corners of the frustum on the far plane
            ftl = fc + Y * fh - X * fw;
            ftr = fc + Y * fh + X * fw;
            fbl = fc - Y * fh - X * fw;
            fbr = fc - Y * fh + X * fw;

            // compute the six planes
            // the function set3Points assumes that the points
            // are given in counter clockwise order
            pl[(int)FrustumPlanes.TOP].Set3Points(ntr, ntl, ftl);
            pl[(int)FrustumPlanes.BOTTOM].Set3Points(nbl, nbr, fbr);
            pl[(int)FrustumPlanes.LEFT].Set3Points(ntl, nbl, fbl);
            pl[(int)FrustumPlanes.RIGHT].Set3Points(nbr, ntr, fbr);
            pl[(int)FrustumPlanes.NEARP].Set3Points(ntl, ntr, nbr);
            pl[(int)FrustumPlanes.FARP].Set3Points(ftr, ftl, fbl);

            //Now let's set what is needed for the frustum sphere
            Center = p + (-Z * (((FarD - NearD) * 0.5f) + NearD));

            //And some more stuff for a frustum cone
            ViewAxis = -Z;
            ViewPosition = p;
        }

        public FrustumState BoxInFrustum(Mesh.BoundingBox b, Vector3 Offset)
        {
            FrustumState result = FrustumState.INSIDE;

            for (int i = 0; i < 6; i++)
            {
                if (pl[i].Distance(b.GetVertexP(pl[i].Normal) + Offset) < 0)
                    return FrustumState.OUTSIDE;
                else if (pl[i].Distance(b.GetVertexN(pl[i].Normal) + Offset) < 0)
                    result = FrustumState.INTERSECT;
            }
            return (result);
        }

        public FrustumState PointInFrustum(Vector3 p)
        {
            FrustumState result = FrustumState.INSIDE;
            for (int i = 0; i < 6; i++)
            {

                if (pl[i].Distance(p) < 0)
                    return FrustumState.OUTSIDE;
            }

            return (result);
        }

        /// <summary>
        /// Return the frustum state of the given sphere in relation to the actual view frustum
        /// This compares the sphere with all 6 sides of the view frustum
        /// </summary>
        /// <param name="p">The center point of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <returns>The state of the sphere in relation to the frustum. Inside, partially inside, or completely outside</returns>
        public FrustumState SphereInFrustum(Vector3 p, float radius)
        {
            FrustumState result = FrustumState.INSIDE;
            float distance;

            for (int i = 0; i < 6; i++)
            {
                distance = pl[i].Distance(p);
                if (distance < -radius)
                    return FrustumState.OUTSIDE;
                else if (distance < radius)
                    result = FrustumState.INTERSECT;
            }

            return (result);
        }

        /// <summary>
        /// Return whether the given sphere is within the range of the 'frustum sphere'
        /// This is a very very crude check, but very fast
        /// </summary>
        /// <param name="p">The center point of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <returns>True if the sphere is within or partially within the sphere around the frustum</returns>
        public bool SphereInSphere(Vector3 p, float radius)
        {
            Vector3 seperation = this.Center - p;
            float sum = this.Radius + radius;

            return seperation.LengthSquared < sum * sum;      
        }

        /// <summary>
        /// Return whether the given sphere is within the 'frustum cone'.
        /// This is more precise than the sphere but less precise than checking with the actual sides of the frustum, and impacts performance accordingly
        /// </summary>
        /// <param name="p">The center point of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <returns>True if the sphere is within or partially within the cone around the frustum</returns>
        public bool SphereInCone(Vector3 p, float radius)
        {
            Vector3 U = this.ViewPosition - (radius * ConeAngSinRecip * this.ViewAxis);
            Vector3 D = p - U;
            float dotsqrd = Vector3.Dot(D, D);
            float e = Vector3.Dot(this.ViewAxis, D);

            if (e > 0 && e * e > dotsqrd * ConeAngleCosSqr)
            {
                D = p - this.ViewPosition;
                dotsqrd = Vector3.Dot(D, D);
                e = -Vector3.Dot(this.ViewAxis, D);
                if (e > 0 && e * e > dotsqrd * ConeAngleSinSqr)
                    return dotsqrd <= radius * radius;
                else return true;
           
            }
            else return false;
        }

        /// <summary>
        /// Perform a comprehensive check, using all three methods of frustum culling
        /// You should use this for the best overall performance and accuracy combo
        /// </summary>
        /// <param name="p">The center point of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <returns>The state of the sphere in relation to the frustum. Inside, partially inside, or completely outside</returns>
        public FrustumState SphereInFrustumMulticheck(Vector3 p, float radius)
        {
            if (SphereInSphere(p, radius) && SphereInCone(p, radius))
            {
                return SphereInFrustum(p, radius);
            }
            //If one of the above checks failed, the sphere is definitely outside the view frustum
            else return FrustumState.OUTSIDE;
        }

        private void ConstructPlanes()
        {
            for (int i = 0; i < pl.Length; i++)
            {
                pl[i] = new Plane();
            }
        }

        private void ConstructFrustumSphere()
        {
            //Get some information about the frustum
            float frustumCircumference = FarD - NearD;
            float frustumHeight = tang;
            float frustumWidth = frustumHeight * Ratio;

            //Halfway point between the near and far planes
            Vector3 P = new Vector3(0, 0, NearD + frustumCircumference * 0.5f);

            //far corner
            Vector3 Q = new Vector3(frustumWidth, frustumHeight, frustumCircumference);

            //the vector between p and q
            Vector3 diff = P - Q;

            //The radius is the length of this
            Radius = diff.Length* 2;
        }

        private void ConstructFrustumCone()
        {
            float frustumHeight = tang;
            float frustumWidth = frustumHeight * Ratio;

            float depth = frustumHeight / (float)Math.Tan(Angle * 0.5f);
            float corner = (float)Math.Sqrt(frustumWidth * frustumWidth + frustumHeight * frustumHeight);
            float FOV = (float)Math.Atan(corner / depth);

            //Yay we've got our angle
            this.ConeAngle = FOV;

            //Precompute some useful values of this angle
            float sin = (float)Math.Sin(this.ConeAngle);
            float cos = (float)Math.Cos(this.ConeAngle);
            this.ConeAngSinRecip = 1 / sin;
            this.ConeAngleSinSqr = sin * sin;
            this.ConeAngleCosSqr = cos * cos;
        }

        #region debug drawing
        public void DrawPoints() 
        {
	        GL.Begin(BeginMode.Points);

                GL.Vertex3(ntl.X, ntl.Y, ntl.Z);
                GL.Vertex3(ntr.X, ntr.Y, ntr.Z);
                GL.Vertex3(nbl.X, nbl.Y, nbl.Z);
                GL.Vertex3(nbr.X, nbr.Y, nbr.Z);

                GL.Vertex3(ftl.X, ftl.Y, ftl.Z);
                GL.Vertex3(ftr.X, ftr.Y, ftr.Z);
                GL.Vertex3(fbl.X, fbl.Y, fbl.Z);
                GL.Vertex3(fbr.X, fbr.Y, fbr.Z);

	        GL.End();
        }


        public void DrawLines()
        {
	        GL.Begin(BeginMode.LineLoop);
	        //near plane
                GL.Vertex3(ntl.X, ntl.Y, ntl.Z);
                GL.Vertex3(ntr.X, ntr.Y, ntr.Z);
                GL.Vertex3(nbr.X, nbr.Y, nbr.Z);
                GL.Vertex3(nbl.X, nbl.Y, nbl.Z);
	        GL.End();

	        GL.Begin(BeginMode.LineLoop);
	        //far plane
                GL.Vertex3(ftr.X, ftr.Y, ftr.Z);
                GL.Vertex3(ftl.X, ftl.Y, ftl.Z);
                GL.Vertex3(fbl.X, fbl.Y, fbl.Z);
                GL.Vertex3(fbr.X, fbr.Y, fbr.Z);
	        GL.End();

            GL.Begin(BeginMode.LineLoop);
	        //bottom plane
                GL.Vertex3(nbl.X, nbl.Y, nbl.Z);
                GL.Vertex3(nbr.X, nbr.Y, nbr.Z);
                GL.Vertex3(fbr.X, fbr.Y, fbr.Z);
                GL.Vertex3(fbl.X, fbl.Y, fbl.Z);
            GL.End();

            GL.Begin(BeginMode.LineLoop);
	        //top plane
                GL.Vertex3(ntr.X, ntr.Y, ntr.Z);
                GL.Vertex3(ntl.X, ntl.Y, ntl.Z);
                GL.Vertex3(ftl.X, ftl.Y, ftl.Z);
                GL.Vertex3(ftr.X, ftr.Y, ftr.Z);
             GL.End();

            GL.Begin(BeginMode.LineLoop);
	        //left plane
                GL.Vertex3(ntl.X, ntl.Y, ntl.Z);
                GL.Vertex3(nbl.X, nbl.Y, nbl.Z);
                GL.Vertex3(fbl.X, fbl.Y, fbl.Z);
                GL.Vertex3(ftl.X, ftl.Y, ftl.Z);
            GL.End();

            GL.Begin(BeginMode.LineLoop);
	        // right plane
                GL.Vertex3(nbr.X, nbr.Y, nbr.Z);
                GL.Vertex3(ntr.X, ntr.Y, ntr.Z);
                GL.Vertex3(ftr.X, ftr.Y, ftr.Z);
                GL.Vertex3(fbr.X, fbr.Y, fbr.Z);

            GL.End();
        }


        public void DrawPlanes() 
        {
	        GL.Begin(BeginMode.Quads);

	            //near plane
                GL.Vertex3(ntl.X, ntl.Y, ntl.Z);
                GL.Vertex3(ntr.X, ntr.Y, ntr.Z);
                GL.Vertex3(nbr.X, nbr.Y, nbr.Z);
                GL.Vertex3(nbl.X, nbl.Y, nbl.Z);

	            //far plane
                GL.Vertex3(ftr.X, ftr.Y, ftr.Z);
                GL.Vertex3(ftl.X, ftl.Y, ftl.Z);
                GL.Vertex3(fbl.X, fbl.Y, fbl.Z);
                GL.Vertex3(fbr.X, fbr.Y, fbr.Z);

	            //bottom plane
                GL.Vertex3(nbl.X, nbl.Y, nbl.Z);
                GL.Vertex3(nbr.X, nbr.Y, nbr.Z);
                GL.Vertex3(fbr.X, fbr.Y, fbr.Z);
                GL.Vertex3(fbl.X, fbl.Y, fbl.Z);

	            //top plane
                GL.Vertex3(ntr.X, ntr.Y, ntr.Z);
                GL.Vertex3(ntl.X, ntl.Y, ntl.Z);
                GL.Vertex3(ftl.X, ftl.Y, ftl.Z);
                GL.Vertex3(ftr.X, ftr.Y, ftr.Z);

	            //left plane

                GL.Vertex3(ntl.X, ntl.Y, ntl.Z);
                GL.Vertex3(nbl.X, nbl.Y, nbl.Z);
                GL.Vertex3(fbl.X, fbl.Y, fbl.Z);
                GL.Vertex3(ftl.X, ftl.Y, ftl.Z);

	            // right plane
                GL.Vertex3(nbr.X, nbr.Y, nbr.Z);
                GL.Vertex3(ntr.X, ntr.Y, ntr.Z);
                GL.Vertex3(ftr.X, ftr.Y, ftr.Z);
                GL.Vertex3(fbr.X, fbr.Y, fbr.Z);

	        GL.End();

        }
        #endregion
    }

    class Plane
    {
        public Vector3 Normal;
        public Vector3 Point;
        public float d;

        public void Set3Points(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 aux1, aux2;

            aux1 = v1 - v2;
            aux2 = v3 - v2;
            Normal = aux2.Cross(aux1);
            //Normal = new Vector3(aux1.X * aux2.X, aux1.Y * aux2.Y, aux1.Z * aux2.Z);
            Normal.Normalize();
            Point = v2;
            d = -(Normal.InnerProduct(Point));
        }

        public float Distance(Vector3 p )
        {
            return (d + Normal.InnerProduct(p));
        }
    }
}
