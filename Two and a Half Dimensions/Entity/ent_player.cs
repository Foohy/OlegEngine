using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

namespace Two_and_a_Half_Dimensions.Entity
{
    class ent_player : BaseEntity
    {
        public enum PlayerMode
        {
            NOCLIP,
            EDIT,
            NONE
        }


        public float radius = 2.0f;

        public Matrix4 camMatrix;
        public Vector2d CamAngle = new Vector2d(-1.5f, 0.0f);
        public float Zoom { get; set; }
        public PlayerMode Mode { get; private set; }

        private float crZoom = 0.0f;

        private static Vector2 Normal = new OpenTK.Vector2();
        private static float Fraction = 0;
        private static Audio phys_hit;
        private static VBO circle;

        RayCastCallback callback = new RayCastCallback(CastCallbackFunc);
        public override void Init()
        {
            //Create the model
            //Utilities.VertexP3N3T2[] vertss = Utilities.CalculateVertices(radius, radius, 90, 90);
            //int[] elements = Utilities.CalculateElements(90, 90); ;

            this.Model = Resource.GetMesh("cow.obj");
            circle = Resource.GetModel("ball.obj");
            //Utilities.LoadOBJ("Resources/Models/cow.obj", out verts, out elements, out normals, out lsUV);

            //this.Mat = Resource.GetMaterial("Resources/Materials/cow.jpg"); //, "Resources/Shaders/default.vert", "Resources/Shaders/default.frag"
            //this.Mat = Resource.GetMaterial("Resources/Materials/cow.jpg", "Resources/Shaders/default.vert", "Resources/Shaders/default.frag");
            this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.SetPos(new Vector3(0, 3.0f, -3.0f));


            Body bod = new Body(Levels.LevelManager.physWorld);
            bod.BodyType = BodyType.Dynamic;

            FarseerPhysics.Collision.Shapes.CircleShape circleshape = new FarseerPhysics.Collision.Shapes.CircleShape(radius, 0.5f);
            this.Physics = bod.CreateFixture(circleshape);
            this.Physics.Body.Position = new Microsoft.Xna.Framework.Vector2(30, 10);
            this.Physics.Body.AngularVelocity = -1f;
            this.Physics.Body.Restitution = 0f;
            this.Physics.Body.Friction = 23.0f;
            this.Physics.UserData = (object)this.GetType().Name;

            Zoom = 10.0f;
            Normal = new Vector2(0, 1);

            //this.Physics.OnCollision += CollideSounds;
            Levels.LevelManager.physWorld.ContactManager.PostSolve += CollideSounds;
            
            //Create the sound effect for the physics of our ball
            phys_hit = Audio.LoadSong("Resources/Audio/Physics/rock_hit_hard.wav", false);

            Utilities.window.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
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
            }

            if (this.Mode == PlayerMode.EDIT) Editor.Stop();
            if (newMode == PlayerMode.EDIT) Editor.Init();

            this.Mode = newMode;
        }

        public override void Think()
        {
            if (this.Mode != PlayerMode.NONE)
            {
                return;
            }

            this.Movetype = MoveTypes.PHYSICS;
            this.SetAngle(Physics.Body.Rotation);
            this.ShouldDraw = true;
            //this.SetAngle(new Vector3((float)Utilities.Time + Physics.Body.LinearVelocity.Length() / 10, (float) Utilities.Time + Physics.Body.LinearVelocity.Length() / 10, Physics.Body.Rotation));

            Zoom += Input.deltaZ * 0.7f;
            crZoom += (Zoom - crZoom) / 4;

            float multiplier = 1;
            if (Utilities.window.Keyboard[OpenTK.Input.Key.ShiftLeft])
            {
                multiplier = 4;

                if (Utilities.window.Keyboard[OpenTK.Input.Key.W])
                {
                    this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(0, multiplier * 100));
                }
                
            }
            if (Utilities.window.Keyboard[OpenTK.Input.Key.A])
            {
                this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(-5.0f, 0.0f));
                float amt = 70 * radius * multiplier;
                if (this.Physics.Body.AngularVelocity < 0) amt = amt * 4 * multiplier;
                this.Physics.Body.ApplyTorque(amt);
            }
            if (Utilities.window.Keyboard[OpenTK.Input.Key.D])
            {
                this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(5.0f, 0.0f));
                float amt = -70 * radius * multiplier;
                if (this.Physics.Body.AngularVelocity > 0) amt = amt * 4 * multiplier;
                this.Physics.Body.ApplyTorque(amt);
            }
            RayCastInput input = new RayCastInput();

            input.Point1 = new Microsoft.Xna.Framework.Vector2( this.Position.X, this.Position.Y );
            input.Point2 = new Microsoft.Xna.Framework.Vector2( this.Position.X, this.Position.Y - 100 );

            Levels.LevelManager.physWorld.RayCast(callback, input.Point1, input.Point2);
            //RayCastOutput output;
            //this.Physics.RayCast( out output, ref input, 0 );
            //Console.WriteLine("{0}, {1}", Normal, Fraction );
            if (Utilities.window.Keyboard[OpenTK.Input.Key.Space] && Fraction < 0.001f)
            {
                this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(0, 200.0f));
            }

            //Levels.LevelManager.physWorld.Gravity = new Microsoft.Xna.Framework.Vector2(-Normal.X, -Normal.Y) * 10;

            Vector3 point = new Vector3((float)Math.Cos(CamAngle.X), (float)Math.Sin(CamAngle.Y) - 0.21f, (float)Math.Sin(CamAngle.X));
            camMatrix = Matrix4.LookAt(Position + new Vector3(0, crZoom / 90, crZoom), Position + point + new Vector3(0, crZoom / 90, 0), Vector3.UnitY);

            //Two_and_a_Half_Dimensions.Player.ply.SetPos(Position + new Vector3(0, crZoom / 900, 1.0f + crZoom / 10));

        }

        public void CalcView()
        {
            GameWindow window = Utilities.window;

            float multiplier = 8;
            if (window.Keyboard[Key.LShift])
                multiplier = 20;

            Vector3 NewPos = this.Position;

            if (window.Keyboard[Key.W])
            {
                NewPos.X += (float)Math.Cos(CamAngle.X) * (float)Utilities.Frametime * multiplier;
                NewPos.Y += (float)Math.Sin(CamAngle.Y) * (float)Utilities.Frametime * multiplier;
                NewPos.Z += (float)Math.Sin(CamAngle.X) * (float)Utilities.Frametime * multiplier;
            }
                //SetPos(new Vector3(this.Position.X + (float)Math.Cos(CamAngle.X) * (float)Utilities.Frametime * multiplier, this.Position.Y + (float)Math.Sin(CamAngle.Y) * (float)Utilities.Frametime * multiplier, this.Position.Z + (float)Math.Sin(CamAngle.X) * (float)Utilities.Frametime * multiplier));
            if (window.Keyboard[Key.S])
            {
                NewPos.X -= (float)Math.Cos(CamAngle.X) * (float)Utilities.Frametime * multiplier;
                NewPos.Y -= (float)Math.Sin(CamAngle.Y) * (float)Utilities.Frametime * multiplier;
                NewPos.Z -= (float)Math.Sin(CamAngle.X) * (float)Utilities.Frametime * multiplier;
            }
             //   SetPos(new Vector3(this.Position.X - (float)Math.Cos(CamAngle.X) * (float)Utilities.Frametime * multiplier, this.Position.Y - (float)Math.Sin(CamAngle.Y) * (float)Utilities.Frametime * multiplier, this.Position.Z - (float)Math.Sin(CamAngle.X) * (float)Utilities.Frametime * multiplier));
            if (window.Keyboard[Key.D])
            {
                NewPos.X += (float)Math.Cos(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier;
                NewPos.Z += (float)Math.Sin(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier;
            }
                //SetPos(new Vector3(this.Position.X + (float)Math.Cos(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier, this.Position.Y, this.Position.Z + (float)Math.Sin(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier));
            if (window.Keyboard[Key.A])
            {
                NewPos.X -= (float)Math.Cos(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier;
                NewPos.Z -= (float)Math.Sin(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier;
            }
            //SetPos(new Vector3(this.Position.X - (float)Math.Cos(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier, this.Position.Y, this.Position.Z - (float)Math.Sin(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier));

            if (window.Keyboard[Key.Space])
            {
                if (window.Keyboard[Key.ControlLeft])
                {
                    NewPos.Y-= (float)Utilities.Frametime * multiplier;
                    //SetPos(new Vector3(this.Position.X, this.Position.Y - (float)Utilities.Frametime * multiplier, this.Position.Z));
                }
                else
                {
                    NewPos.Y += (float)Utilities.Frametime * multiplier;
                    //SetPos(new Vector3(this.Position.X, this.Position.Y + (float)Utilities.Frametime * multiplier, this.Position.Z));
                }
            }

            CamAngle += new Vector2d(Input.deltaX / 350f, Input.deltaY / -350f);
            CamAngle = new Vector2d((float)CamAngle.X, Utilities.Clamp((float)CamAngle.Y, 1.0f, -1.0f)); //Clamp it because I can't math correctly


            this.SetPos(NewPos, false);
            this.SetAngle(new Vector3((float)CamAngle.X, (float)CamAngle.Y, 0));


            View.SetPos(this.Position);
            View.SetAngles(this.Angle);
        }

        private static float CastCallbackFunc(Fixture fixture, Microsoft.Xna.Framework.Vector2 point, Microsoft.Xna.Framework.Vector2 normal, float fraction)
        {
            if (fixture != null && fixture.UserData == null)
            {
                Normal = new Vector2(normal.X, normal.Y); ;
                Fraction = fraction;
                return fraction;
            }
            return -1;
        }
        /*
        private bool CollideSounds( Fixture f1, Fixture f2, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            double force = CalculateForce(f1, f2, contact);
            if (force > 15.0f)
            {
                phys_hit.Play(false);
                phys_hit.SetFrequency((100 - (float)force) * 1000); // (100 - (float)force) / 100
            }
            return true;
        }*/

        private void CollideSounds( FarseerPhysics.Dynamics.Contacts.Contact contact, FarseerPhysics.Dynamics.Contacts.ContactConstraint impulse )
        {
            double force = CalculateForce(contact, impulse);
            if (force > 85.0f)
            {
                //Console.WriteLine((500 - (float)force) * 100);
                //phys_hit.Play(false);
                //phys_hit.SetFrequency((100 - (float)force) * 1000); // (100 - (float)force) / 100
                int freq = 60000 - (int)force * 100;
                Console.WriteLine(freq);
                Audio.PlaySound("rock_hit", 1.0f, freq);
            }

        }

        private double CalculateForce(FarseerPhysics.Dynamics.Contacts.Contact contact, FarseerPhysics.Dynamics.Contacts.ContactConstraint impulse)
        {
            /*
            Vector2 position = new Vector2(manifold.LocalNormal.X, manifold.LocalNormal.Y);
            float angle = (float)Math.Atan2(position.Y, position.X);
            Vector2 force = Vector2.Zero;
            if (angle < 0)
                force = new Vector2((float)(Math.Cos(angle) * f1.Body.LinearVelocity.X),
                (float)Math.Sin(MathHelper.TwoPi + angle) * f1.Body.LinearVelocity.Y);

            else
                force = new Vector2((float)(Math.Cos(angle) * f1.Body.LinearVelocity.X),
                (float)Math.Sin(MathHelper.TwoPi - angle) * f1.Body.LinearVelocity.Y);

            double XForce = Math.Sqrt(force.X * force.X);
            double YForce = Math.Sqrt(force.Y * force.Y);
            double totalForce = XForce + YForce;
            */
            if ((string)contact.FixtureA.UserData == "Player" || (string)contact.FixtureB.UserData == "Player")
            {
                float maxImpulse = 0.0f;
                int count = contact.Manifold.PointCount;

                for (int i = 0; i < count; ++i)
                {
                    maxImpulse = Math.Max(maxImpulse, impulse.Points[i].NormalImpulse);
                }

                return maxImpulse;
            }

            return 0;
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
