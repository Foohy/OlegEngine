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
        private bool OnGround = true;
        private float upVelocity = 0.0f;
        private float scrollCurrent = 0f;

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
        }

        public void SetPos(Vector3 vec)
        {
            this.Pos = vec;
        }

        public void Think(FrameEventArgs e)
        {
            if (window == null) return;
            //Update position


            NoClipThink(e);

            if (!OverrideCamMatrix)
            {
                CamAngle = new Vector2d((float)CamAngle.X, Utilities.Clamp((float)CamAngle.Y, 1.0f, -1.0f) );
                Vector3 point = new Vector3((float)Math.Cos(CamAngle.X), (float)Math.Sin(Utilities.Clamp((float)CamAngle.Y, 1.0f, -1.0f)), (float)Math.Sin(CamAngle.X));
                Vector3 Eye = Pos;// +new Vector3(0, scrollCurrent / 7, 0);
                Vector3 Target = Pos + point;// ; +new Vector3(0, scrollCurrent / 7, 0);

                //Matrix4 cam = Matrix4.CreateTranslation(Vector3.Zero);
                //cam *= Matrix4.CreateRotationX((float)CamAngle.X);
                //cam *= Matrix4.CreateRotationX((float)CamAngle.X);
                //cam *= Matrix4.CreateTranslation(-Eye);

                this.ViewNormal = Target - Eye;
                this.ViewNormal.Normalize();

                //this.ViewNormal = point;
                //this.ViewNormal.Normalize();
                //this.camMatrix = cam;
                this.camMatrix = Matrix4.LookAt(Eye, Target, Vector3.UnitY);
            }

        }

        private void NoClipThink(FrameEventArgs e)
        {
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
        }
        


        public void Draw(FrameEventArgs e)
        {
        }
    }
}
