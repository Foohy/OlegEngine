using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OlegEngine
{
    public class View
    {
        public static Vector3 Position { get; private set; }
        public static Vector3 Angles { get; set; }
        public static Vector3 ViewNormal { get; set; }
        public static Matrix4 CameraMatrix { get; set; }

        public static Entity.BaseEntity Player { get; private set; }

        public class CalcViewEventArgs : EventArgs { Entity.BaseEntity ply; Vector3 Pos; Vector3 Ang; }
        public static event Action CalcView;

        private static System.Reflection.MethodInfo PlyCalcView;

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
            Vector3 point = new Vector3((float)Math.Cos(Angles.X), (float)Math.Sin(Utilities.Clamp((float)Angles.Y, 1.0f, -1.0f)), (float)Math.Sin(Angles.X));

            ViewNormal = point;
            ViewNormal.Normalize();
            CameraMatrix = Matrix4.LookAt(Position, (Position + point), Vector3.UnitY);

            Graphics.ViewFrustum.SetCameraDef(Position, (Position + point), Vector3.UnitY);
        }

        //Will do nothing if view is not being overwritten
        public static void SetPos(Vector3 Pos)
        {
            Position = Pos;
        }

        //Will do nothing if view is not being overwritten
        public static void SetAngles(Vector3 Ang)
        {
            Angles = Ang;
        }

        public static Entity.BaseEntity GetLocalPlayer()
        {
            return Player;
        }

        public static void SetLocalPlayer( Entity.BaseEntity ply )
        {
            System.Reflection.MethodInfo inf = GetMethod(ply, "CalcView");

            if (inf != null)
            {
                Player = ply;
                PlyCalcView = inf;
            }
        }

        private static System.Reflection.MethodInfo GetMethod(object obj, string methodname)
        {
            var type = obj.GetType();
            return type.GetMethod(methodname);
        }

    }
}
