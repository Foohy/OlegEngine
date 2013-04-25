using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Two_and_a_Half_Dimensions
{
    class View
    {
        public static Vector3 Position { get; private set; }
        public static Vector3 Angles { get; set; }
        public static Vector3 ViewNormal { get; set; }
        public static Matrix4 CameraMatrix { get; set; }

        public static Entity.ent_player Player { get; private set; }

        public class CalcViewEventArgs : EventArgs { Entity.ent_player ply; Vector3 Pos; Vector3 Ang; }
        public static event Action CalcView;

        public static void Init()
        {
            Two_and_a_Half_Dimensions.Levels.LevelManager.PlayerSpawn += new Action<Entity.ent_player>(LevelManager_PlayerSpawn);
        }

        static void LevelManager_PlayerSpawn(Entity.ent_player ply)
        {
            SetLocalPlayer(ply);
        }

        public static void Think(FrameEventArgs e)
        {
            Input.LockMouse = true;

            if (Player != null)
            {
                //Run calcview on the player that will control the normal camera
                Player.CalcView();
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

        public static Entity.ent_player GetLocalPlayer()
        {
            return Player;
        }

        public static void SetLocalPlayer( Entity.ent_player ply )
        {
            Player = ply;
        }

    }
}
