using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using OlegEngine;
using OlegEngine.Entity;

namespace Gravity_Car.Entity
{
    public class ent_player : BaseEntity
    {
        public enum PlayerMode
        {
            NOCLIP,
            EDIT,
            NONE
        }


        public float radius = 2.0f;

        public Matrix4 camMatrix;
        public float Zoom { get; set; }
        public PlayerMode Mode { get; private set; }

        private float crZoom = 0.0f;

        private static Vector2 Normal = new OpenTK.Vector2();
        private static float Fraction = 0;
        private static Audio phys_hit;

        public override void Init()
        {
            //Create the model
            //Utilities.VertexP3N3T2[] vertss = Utilities.CalculateVertices(radius, radius, 90, 90);
            //int[] elements = Utilities.CalculateElements(90, 90); ;

            this.Model = Resource.GetMesh("cow.obj");
            //Utilities.LoadOBJ("Resources/Models/cow.obj", out verts, out elements, out normals, out lsUV);

            //this.Mat = Resource.GetMaterial("Resources/Materials/cow.jpg"); //, "Resources/Shaders/default.vert", "Resources/Shaders/default.frag"
            //this.Mat = Resource.GetMaterial("Resources/Materials/cow.jpg", "Resources/Shaders/default.vert", "Resources/Shaders/default.frag");
            this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.SetPos(new Vector3(0, 3.0f, -3.0f));

            Zoom = 10.0f;
            Normal = new Vector2(0, 1);
            
            //Create the sound effect for the physics of our ball
            phys_hit = Audio.LoadSong("Resources/Audio/Physics/rock_hit_hard.wav", false);

            Utilities.engine.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
            this.SetMode(PlayerMode.NOCLIP);
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                this.SetMode(PlayerMode.EDIT);
            }

            if (e.Key == Key.F10)
            {
                this.SetMode(PlayerMode.NOCLIP);
            }

            if (e.Key == Key.F9)
            {
                this.SetMode(PlayerMode.NONE);
            }
        }

        public void SetMode(PlayerMode newMode)
        {
            if (newMode != PlayerMode.NONE)
            {
                this.Movetype = MoveTypes.NONE;
                this.ShouldDraw = false;
                //if (this.Physics != null) this.Physics.CollisionCategories = Category.None;
            }
            else
            {
                //if (this.Physics != null) this.Physics.CollisionCategories = Category.Cat1;
            }

            if (this.Mode == PlayerMode.EDIT) Editor.Stop();
            if (newMode == PlayerMode.EDIT) Editor.Init();
            else
            {
                Input.LockMouse = true;
            }

            this.Mode = newMode;
        }

        public override void Think()
        {
            if (this.Mode != PlayerMode.NONE)
            {
                if (this.Mode == PlayerMode.EDIT)
                {
                    Editor.Think();
                    this.SetPos(Editor.ViewPosition);
                }
                return;
            }

            this.Movetype = MoveTypes.PHYSICS;
            //this.SetAngle(Physics.Body.Rotation);
            this.ShouldDraw = true;
            //this.SetAngle(new Vector3((float)Utilities.Time + Physics.Body.LinearVelocity.Length() / 10, (float) Utilities.Time + Physics.Body.LinearVelocity.Length() / 10, Physics.Body.Rotation));

            Zoom += Input.deltaZ * 0.7f;
            crZoom += (Zoom - crZoom) / 4;

            float multiplier = 1;
            if (Utilities.engine.Keyboard[OpenTK.Input.Key.ShiftLeft])
            {
                multiplier = 4;

                if (Utilities.engine.Keyboard[OpenTK.Input.Key.W])
                {
                    //this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(0, multiplier * 100));
                }
                
            }
            if (Utilities.engine.Keyboard[OpenTK.Input.Key.A])
            {
                //this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(-5.0f, 0.0f));
                //float amt = 70 * radius * multiplier;
                //if (this.Physics.Body.AngularVelocity < 0) amt = amt * 4 * multiplier;
                //this.Physics.Body.ApplyTorque(amt);
            }
            if (Utilities.engine.Keyboard[OpenTK.Input.Key.D])
            {
                //this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(5.0f, 0.0f));
                //float amt = -70 * radius * multiplier;
                //if (this.Physics.Body.AngularVelocity > 0) amt = amt * 4 * multiplier;
                //this.Physics.Body.ApplyTorque(amt);
            }


            if (Utilities.engine.Keyboard[OpenTK.Input.Key.Space] && Fraction < 0.001f)
            {
                //this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(0, 200.0f));
            }

        }

        public void CalcView()
        {
            GameWindow window = Utilities.engine;

            float multiplier = 8;
            if (window.Keyboard[Key.LShift])
                multiplier = 20;

            Vector3 NewPos = this.Position;
            //Calculate the new angle of the camera
            this.SetAngle(this.Angles + new Angle(Input.deltaY / -15f, Input.deltaX / 15f, 0));

            Vector3 Forward, Right, Up;
            this.Angles.AngleVectors(out Forward, out Up, out Right);

            //Calculate the new position
            if (window.Keyboard[Key.W])
            {
                NewPos += Forward * (float)Utilities.ThinkTime * multiplier;
            }

            if (window.Keyboard[Key.S])
            {
                NewPos -= Forward * (float)Utilities.ThinkTime * multiplier;
            }

            if (window.Keyboard[Key.D])
            {
                NewPos -= Right * (float)Utilities.ThinkTime * multiplier;
            }

            if (window.Keyboard[Key.A])
            {
                NewPos += Right * (float)Utilities.ThinkTime * multiplier;
            }

            if (window.Keyboard[Key.Space])
            {
                if (window.Keyboard[Key.ControlLeft])
                {
                    NewPos.Y -= (float)Utilities.ThinkTime * multiplier;
                }
                else
                {
                    NewPos.Y += (float)Utilities.ThinkTime * multiplier;
                }
            }


            this.SetPos(NewPos, false);

            View.SetPos(this.Position);
            View.SetAngles(this.Angles);
        }

        /*
        public override void Draw()
        {
            GL.Disable(EnableCap.Lighting);
            if (Model != null)
            {
                Utilities.ErrorMat.BindMaterial();
                if (this.Mat != null )
                    this.Mat.BindMaterial();

                Vector3 oldscale = this.Scale;
                this.Scale = Vector3.One;
                base.Draw();
                this.Scale = oldscale;

                
                //Draw the surrounding ball
                GL.BindTexture(TextureTarget.Texture2D, 0);
                Matrix4 translate = Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, Position.Z));
                GL.PushMatrix();

                GL.Translate(Position);
                GL.Scale(new Vector3(radius, radius, radius));
                GL.Rotate(this.Angle.Z * 57.2957795, Vector3d.UnitZ);
                GL.Translate(Vector3.Zero);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Line); //turn on wireframe
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                circle.Draw(BeginMode.Triangles);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill); //turn off wireframe
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                GL.PopMatrix();

            }
            GL.Enable(EnableCap.Lighting);
        }
         * */
    }
}
