using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;

using Two_and_a_Half_Dimensions.Entity;

namespace Two_and_a_Half_Dimensions
{
    class Editor
    {
        public static bool Active = false;

        public static float Zoom = 1.0f;
        private static float goalZoom = 20f;

        private static Vector3 Pos = new Vector3();
        private static Vector2 MousePos = new Vector2();
        private static float multiplier = 8;

        private static float halfPI = -1.570796326794896f;
        private static ent_static Cursor;
        private static float Multiplier = 0.000924f;

        private static List<Vector2> Points = new List<Vector2>();
        private static ent_editor_build TempEnt;
        public static void Init()
        {
            Pos = new Vector3(Player.ply.Pos.X, Player.ply.Pos.Y, Player.ply.Pos.Z);

            Cursor = EntManager.Create<ent_static>();
            Cursor.Spawn();
            Cursor.SetPos(new Vector3(0, 0, 0));
            Cursor.Model = Resource.GetMesh("cursor.obj");
            Cursor.Mat = Resource.GetMaterial("models/cursor");

            Utilities.window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
        }

        static void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseState state = Mouse.GetState();
            
        }

        static void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TempEnt == null)
            {
                TempEnt = EntManager.Create<ent_editor_build>();
                TempEnt.Spawn();
            }
            TempEnt.AddPoint(MousePos);
            //TempEnt.Points.Add(MousePos);
            Points.Add(MousePos);
        }

        public static void Think(FrameEventArgs e)
        {
            Input.LockMouse = false;
            //curve dat zoom mmm girl u fine
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageDown]) goalZoom -= ((float)Utilities.Frametime * 100);
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageUp]) goalZoom += ((float)Utilities.Frametime * 100);


            goalZoom += Input.deltaZ;
            Zoom += (goalZoom - Zoom) / 4;
            Pos = new Vector3(Pos.X, Pos.Y, Zoom);

            //How fast should we move
            multiplier = 8;
            if (Utilities.window.Keyboard[Key.LShift])
            {
                multiplier = 15;
            }
            else if (Utilities.window.Keyboard[Key.LControl])
            {
                multiplier = 3;
            }
            

            //I SAID MOVE
            if (Utilities.window.Keyboard[Key.W])
            {
                Pos += new Vector3(0.0f, (float)e.Time, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.A])
            {
                Pos += new Vector3(-(float)e.Time, 0.0f, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.S])
            {
                Pos += new Vector3(0.0f, -(float)e.Time, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.D])
            {
                Pos += new Vector3((float)e.Time, 0.0f, 0.0f) * multiplier;
            }

            if (Cursor != null)
            {
                MouseState state = Mouse.GetState();
                if (state.IsButtonDown(MouseButton.Right ) )
                {
                    //Multiplier += (float)Input.deltaY / 100000.0f;
                    //Console.WriteLine(Multiplier);
                }

                Vector2 mousePos = new Vector2((Utilities.window.Mouse.X - (Utilities.window.Width / 2)), -(Utilities.window.Mouse.Y - (Utilities.window.Height / 2)));

                MousePos = new Vector2(Pos.X, Pos.Y);
                MousePos += mousePos * Zoom * Multiplier; //lmao fuck
                Cursor.SetPos(MousePos);
            }
        }

        public static void Draw(FrameEventArgs e)
        {
            //Set the camera matrix itself
            Player.ply.CamAngle = new Vector2d(halfPI, 0.0f); //Clamp it because I can't math correctly

            //find the point where we'll be facing
            Vector3 point = new Vector3((float)Math.Cos(halfPI), (float)Math.Sin(0), (float)Math.Sin(halfPI));

            Player.ply.ViewNormal = point;
            Player.ply.ViewNormal.Normalize();
            Player.ply.camMatrix = Matrix4.LookAt(Pos, (Pos + point), Vector3.UnitY);

            Utilities.ProjectionMatrix = Player.ply.camMatrix;

            Player.ply.SetPos(Pos);

            if (Cursor != null)
            {
                Cursor.SetAngle((float)Utilities.Time);
            }
        }

        public static void Stop()
        {
            if (Cursor != null)
            {
                Cursor.Remove();
            }

            Utilities.window.Mouse.ButtonDown -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Mouse.ButtonUp -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
        }
    }
}
