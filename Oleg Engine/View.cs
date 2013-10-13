using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine
{
    public class View
    {
        public static Vector3 Position { get; private set; }
        public static Angle Angles { get; private set; }
        public static Vector3 ViewNormal { get; private set; }
        public static Matrix4 CameraMatrix { get; private set; }

        /// <summary>
        /// The default view matrix for use in 3D projection based rendering
        /// </summary>
        public static Matrix4 ViewMatrix = Matrix4.Identity;
        /// <summary>
        /// The default view for 2D 'orthographic' rendering
        /// </summary>
        public static Matrix4 OrthoMatrix = Matrix4.Identity;

        public static Entity.BaseEntity Player { get; private set; }

        public static event Action CalcView;

        private static System.Reflection.MethodInfo PlyCalcView;
        private const float DEG2RAD =  (float)Math.PI / 180f;

        public static void Think(FrameEventArgs e)
        {
            if (Player != null && PlyCalcView != null )
            {
                //Run calcview on the player that will control the normal camera
                PlyCalcView.Invoke(Player, null);
            }

            //Override the normal camera
            if (CalcView != null)
            {
                CalcView();
            }

            //Create the matrix to be sent to the renderer
            //Find the point where we'll be facing
            ViewNormal = Angles.Forward();
            ViewNormal.Normalize();
            CameraMatrix = Matrix4.LookAt(Position, (Position + ViewNormal), Vector3.UnitY);

            Graphics.ViewFrustum.SetCameraDef(Position, (Position + ViewNormal), Vector3.UnitY);
        }

        /// <summary>
        /// Set the position of the camera
        /// </summary>
        /// <param name="Pos">The new position</param>
        public static void SetPos(Vector3 Pos)
        {
            Position = Pos;
        }

        /// <summary>
        /// Set the angles of the camera. Angles are measured in degrees.
        /// </summary>
        /// <param name="Ang">The new angle to set</param>
        public static void SetAngles(Angle Ang)
        {
            Angles = Ang;
        }

        /// <summary>
        /// Get the entity that has primary control of the camera.
        /// </summary>
        /// <returns>The primary view entity.</returns>
        public static Entity.BaseEntity GetLocalPlayer()
        {
            return Player;
        }

        /// <summary>
        /// Set the primary view entity that'll control the camera
        /// </summary>
        /// <param name="ply">The new view entity</param>
        public static void SetLocalPlayer( Entity.BaseEntity ply )
        {
            System.Reflection.MethodInfo inf = GetMethod(ply, "CalcView");

            if (inf != null)
            {
                Player = ply;
                PlyCalcView = inf;
            }
        }

        public static void UpdateViewOrthoMatrices()
        {
            float FOV = (float)Math.PI / 4;
            float Ratio = Utilities.engine.Width / (float)Utilities.engine.Height;

            var clientRec = Utilities.engine.ClientRectangle;

            GL.Viewport(clientRec.X, clientRec.Y, clientRec.Width, clientRec.Height);
            ViewMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Ratio, Utilities.NearClip, Utilities.FarClip);
            OrthoMatrix = Matrix4.CreateOrthographicOffCenter(0, Utilities.engine.Width, Utilities.engine.Height, 0, Utilities.NearClip, Utilities.FarClip);
            Utilities.ViewMatrix = ViewMatrix;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref ViewMatrix);

            Graphics.ViewFrustum.SetCamInternals(FOV, Ratio, Utilities.NearClip, Utilities.FarClip);
        }

        private static System.Reflection.MethodInfo GetMethod(object obj, string methodname)
        {
            var type = obj.GetType();
            return type.GetMethod(methodname);
        }

    }
}
