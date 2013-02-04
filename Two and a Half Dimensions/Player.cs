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
        private float Velocity = 0.0f;
        private VBO model;
        private bool OnGround = true;
        private float upVelocity = 0.0f;
        private float downForce = 0.98f;
        private int TextureID = -1;
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

            //Create the player model
            float radius = 0.25f;
            Utilities.VertexP3N3T2[] verts = Utilities.CalculateVertices(radius, radius, 90, 90);
            int[] elements = Utilities.CalculateElements( 90, 90);
            Zoom = 5.0f;

            model = new VBO(verts, elements);

            TextureID = Utilities.LoadTexture("ball.png");
            //TextureID_blur = Utilities.LoadTexture("Resources/Materials/ball_blur.png");
            //TextureID_blur2 = Utilities.LoadTexture("Resources/Materials/ball_blur2.png");
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
            //RunThink(e);
            //Update view angles
            if (!OverrideCamMatrix)
            {
                Vector3 point = new Vector3((float)Math.Cos(CamAngle.X), (float)Math.Sin(CamAngle.Y) - 0.21f, (float)Math.Sin(CamAngle.X));
                Vector3 Eye = Pos + new Vector3(0, scrollCurrent / 7, 0);
                Vector3 Target = Pos + point + new Vector3(0, scrollCurrent / 7, 0);
                this.ViewNormal = Target - Eye;
                this.ViewNormal.Normalize();

                camMatrix = Matrix4.LookAt(Eye, Target, Vector3.UnitY);
            }

        }

        private void NoClipThink(FrameEventArgs e)
        {
            Input.LockMouse = true;
            float multiplier = 8;
            if (window.Keyboard[Key.LShift])
                multiplier = 20;

            if (window.Keyboard[Key.W])
                SetPos(new Vector3(Pos.X + (float)Math.Cos(CamAngle.X) * (float)e.Time * multiplier, Pos.Y, Pos.Z + (float)Math.Sin(CamAngle.X) * (float)e.Time * multiplier));
            if (window.Keyboard[Key.S])
                SetPos(new Vector3(Pos.X - (float)Math.Cos(CamAngle.X) * (float)e.Time * multiplier, Pos.Y, Pos.Z - (float)Math.Sin(CamAngle.X) * (float)e.Time * multiplier));
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
        
        private void RunThink(FrameEventArgs e)
        {
            if (window.Keyboard[Key.A])
            {
                float amt = 40.0f;
                if (Velocity <= 0) amt = 2.0f;

                Velocity -= (float)e.Time * amt;

            }

            if (window.Keyboard[Key.D])
            {
                Velocity += (float)e.Time * 10.0f;
            }

            if (window.Keyboard[Key.Space])
            {
                if (OnGround)
                {
                    upVelocity = 0.25f;
                    OnGround = false;
                }
            }


            Zoom += Input.deltaZ * 0.7f;
            scrollCurrent += (Zoom - scrollCurrent) / 4;

            if (Levels.LevelManager.CurrentLevel != null)
            {
                Levels.TestLevel lvl = (Levels.TestLevel)Levels.LevelManager.CurrentLevel;
                float X = Pos.X + (Velocity * (float)e.Time);
                float height = (float)Math.Sin((X / lvl.Size) / lvl.HeightRatio) * lvl.WaveAmp + 1.5f;

                if (Pos.Y < height && upVelocity < 0)
                {
                    OnGround = true;
                    Console.WriteLine("on da ground");
                }
                if (!OnGround) height = JumpThink(e);


                SetPos(new Vector3(X, height, scrollCurrent));
            }

            CamAngle = new Vector2d(-1.5f, 0.0f);
        }
    

        private float JumpThink(FrameEventArgs e)
        {
            upVelocity -= (float)e.Time * downForce;
            return Pos.Y + upVelocity;
        }

        public void Draw(FrameEventArgs e)
        {
            /*
            if (model != null  && Levels.LevelManager.CurrentLevel != null)
            {
                Levels.TestLevel lvl = (Levels.TestLevel)Levels.LevelManager.CurrentLevel;
                float height = Pos.Y - 1.0f;

                GL.BindTexture(TextureTarget.Texture2D, TextureID);

                if (Velocity > 20)
                {
                    GL.BindTexture(TextureTarget.Texture2D, TextureID_blur);
                }
                if (Velocity > 50)
                {
                    GL.BindTexture(TextureTarget.Texture2D, TextureID_blur2);
                }

                Matrix4 rotate = Matrix4.CreateRotationX((float)Math.PI / 2);
                rotate *= Matrix4.CreateRotationZ(-Pos.X * 2);
                Matrix4 translate = Matrix4.CreateTranslation(new Vector3(Pos.X, height, -1.5f));
                Matrix4 matrix = rotate * translate;
                GL.PushMatrix();
                GL.MultMatrix(ref matrix);
                model.Draw(BeginMode.Triangles);
                GL.PopMatrix();

                rotate = Matrix4.CreateRotationX((float)Math.PI / -2);
                rotate *= Matrix4.CreateRotationZ(-Pos.X * 2 + (float)Math.PI);
                matrix = rotate * translate;
                GL.PushMatrix();
                GL.MultMatrix(ref matrix);
                model.Draw(BeginMode.Triangles);
                GL.PopMatrix();
            }
            */
        }
    }
}
