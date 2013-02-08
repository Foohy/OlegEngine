using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

namespace Two_and_a_Half_Dimensions
{
    enum PlayerMode
    {
        NOCLIP,
        EDIT,
        CUSTOM
    }

    class Player
    {
        public static Player ply { get; private set; }
        public Vector3 Pos { get; private set; }
        public float Height { get; set; }
        public Matrix4 camMatrix { get; set; }
        public Vector2d CamAngle { get; set; }
        public Vector3 ViewOffset = new Vector3(2.0f, 0, 0.0f);
        public Vector3 ViewNormal = new Vector3();
        public float Zoom { get; set; }
        public bool OverrideCamMatrix { get; set; }
        private Program window = null;

        public PlayerMode Mode { get; private set; }

        public Player(Program Win, Vector3 pos, float height = 4.0f)
        {
            OverrideCamMatrix = false;
            ply = this;
            Pos = pos;
            Height = height;

            camMatrix = Matrix4.Identity;
            CamAngle = new Vector2d(0f, 0f);

            window = Win;

            Zoom = 5.0f;
            this.Mode = PlayerMode.NOCLIP;

            this.window.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (this.Mode == PlayerMode.NOCLIP)
                {
                    this.SetMode(PlayerMode.EDIT);
                }
                else if (this.Mode == PlayerMode.EDIT)
                {
                    this.SetMode(PlayerMode.NOCLIP);
                }
            }
        }

        public void SetPos(Vector3 vec)
        {
            this.Pos = vec;
        }
        public void SetMode( PlayerMode mode )
        {
            if (Mode == PlayerMode.EDIT) Editor.Stop();
            if (mode == PlayerMode.EDIT) Editor.Init();

            this.Mode = mode;
        }

        public void Think(FrameEventArgs e)
        {
            if (window == null) return;
            //Update position

            switch (this.Mode)
            {
                case PlayerMode.NOCLIP:
                    NoClipThink(e);
                    break;

                case PlayerMode.EDIT:
                    EditorThink(e);
                    break;
            }

        }

        private void NoClipThink(FrameEventArgs e)
        {
            //Update the internal variables for the camera location
            Input.LockMouse = true;
            float multiplier = 8;
            if (window.Keyboard[Key.LShift])
                multiplier = 20;

            if (window.Keyboard[Key.W])
                SetPos(new Vector3(Pos.X + (float)Math.Cos(CamAngle.X) * (float)e.Time * multiplier, Pos.Y + (float)Math.Sin(CamAngle.Y) * (float)e.Time * multiplier, Pos.Z + (float)Math.Sin(CamAngle.X) * (float)e.Time * multiplier));
            if (window.Keyboard[Key.S])
                SetPos(new Vector3(Pos.X - (float)Math.Cos(CamAngle.X) * (float)e.Time * multiplier, Pos.Y - (float)Math.Sin(CamAngle.Y) * (float)e.Time * multiplier, Pos.Z - (float)Math.Sin(CamAngle.X) * (float)e.Time * multiplier));
            if (window.Keyboard[Key.D])
                SetPos(new Vector3(Pos.X + (float)Math.Cos(CamAngle.X + Math.PI / 2) * (float)e.Time * multiplier, Pos.Y, Pos.Z + (float)Math.Sin(CamAngle.X + Math.PI / 2) * (float)e.Time * multiplier));
            if (window.Keyboard[Key.A])
                SetPos(new Vector3(Pos.X - (float)Math.Cos(CamAngle.X + Math.PI / 2) * (float)e.Time * multiplier, Pos.Y, Pos.Z - (float)Math.Sin(CamAngle.X + Math.PI / 2) * (float)e.Time * multiplier));

            if (window.Keyboard[Key.Space])
            {
                if (window.Keyboard[Key.ControlLeft])
                {
                    SetPos(new Vector3(Pos.X, Pos.Y - (float)e.Time * multiplier, Pos.Z));
                }
                else
                {
                    SetPos(new Vector3(Pos.X, Pos.Y + (float)e.Time * multiplier, Pos.Z));
                }
            }

            CamAngle += new Vector2d(Input.deltaX / 350f, Input.deltaY / -350f);



            //Set the camera matrix itself
            CamAngle = new Vector2d((float)CamAngle.X, Utilities.Clamp((float)CamAngle.Y, 1.0f, -1.0f)); //Clamp it because I can't math correctly

            //find the point where we'll be facing
            Vector3 point = new Vector3((float)Math.Cos(CamAngle.X), (float)Math.Sin(Utilities.Clamp((float)CamAngle.Y, 1.0f, -1.0f)), (float)Math.Sin(CamAngle.X));

            this.ViewNormal = point;
            this.ViewNormal.Normalize();
            this.camMatrix = Matrix4.LookAt(Pos, (Pos + point), Vector3.UnitY);
        }

        private void EditorThink(FrameEventArgs e)
        {
            Editor.Think(e);
        }


        public void Draw(FrameEventArgs e)
        {
            if (this.Mode == PlayerMode.EDIT) { Editor.Draw(e); }
        }
    }
}
