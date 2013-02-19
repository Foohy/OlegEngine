using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

namespace Two_and_a_Half_Dimensions.Entity
{
    class ent_car : BaseEntity
    {
        public Matrix4 camMatrix;
        public Vector2d CamAngle = new Vector2d(-1.5f, 0.0f);
        public float Zoom { get; set; }
        private float crZoom = 0;

        public bool NoClip = false;

        private float physScale = 0.44f;
        //physics for wheels and stuff
        Fixture[] wheels;
        private float wheelsize = 0.9f;
        private Vector2[] offsets = new Vector2[]
        {
            new Vector2(-3.56707f, -2.38319f ),
            new Vector2(5.45208f, -2.43036f ),
        };
        private Mesh wheel;
        private Vector3[] wheelpos = new Vector3[]
        {
            Vector3.Zero,
            Vector3.Zero
        };
        private float[] wheelang = new float[]
        {
            0,
            0
        };
        Audio horn;

        public override void Init()
        {
            this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.Scale = new Vector3(physScale, physScale, physScale);
            offsets[0] *= physScale;
            offsets[1] *= physScale;
            wheelsize *= physScale;

            //Load models
            this.Model = Resource.GetMesh("vehicles/van.obj");
            this.Mat = Resource.GetMaterial("models/vehicles/van");
            wheel = Resource.GetMesh("wheel.obj");
            wheel.mat = this.Mat;

            //Create the physics
            Body bod = FarseerPhysics.Factories.BodyFactory.CreateRectangle(Levels.LevelManager.physWorld, 14f * physScale, 4.7f * physScale, 0.7f);
            bod.BodyType = BodyType.Dynamic;

            this.Physics = bod.FixtureList[0];
            this.Physics.Body.Restitution = 0.2f;
            this.Physics.Body.Friction = 2.0f;
            this.Physics.UserData = (object)this.GetType().Name;

            //Create da weelz
            wheels = new Fixture[2];

            Body wheelbod = FarseerPhysics.Factories.BodyFactory.CreateCircle( Levels.LevelManager.physWorld, wheelsize, 0.7f );
            wheelbod.BodyType = BodyType.Dynamic;
            wheelbod.Friction = 20.0f;
            wheelbod.Position = new Microsoft.Xna.Framework.Vector2(offsets[0].X, offsets[0].Y );
            wheelbod.Restitution = 1.0f;
            FarseerPhysics.Factories.JointFactory.CreateRevoluteJoint(Levels.LevelManager.physWorld, wheelbod, this.Physics.Body, wheelbod.Position ); //Microsoft.Xna.Framework.Vector2.Zero 
            wheels[0] = wheelbod.FixtureList[0];

            

            wheelbod = FarseerPhysics.Factories.BodyFactory.CreateCircle(Levels.LevelManager.physWorld, wheelsize, 0.7f);
            wheelbod.BodyType = BodyType.Dynamic;
            wheelbod.Position = new Microsoft.Xna.Framework.Vector2(offsets[1].X, offsets[1].Y);
            wheelbod.Restitution = 1.0f;
            FarseerPhysics.Factories.JointFactory.CreateRevoluteJoint(Levels.LevelManager.physWorld, wheelbod, this.Physics.Body, wheelbod.Position);
            wheels[1] = wheelbod.FixtureList[0];

            //Set their positions
            this.Physics.Body.Position = new Microsoft.Xna.Framework.Vector2(this.Position.X, this.Position.Y);
            wheels[0].Body.Position = new Microsoft.Xna.Framework.Vector2(offsets[0].X, offsets[0].Y) + this.Physics.Body.Position;
            wheels[1].Body.Position = new Microsoft.Xna.Framework.Vector2(offsets[1].X, offsets[1].Y) + this.Physics.Body.Position;

            //Setup overriding the camera and stuff

            Two_and_a_Half_Dimensions.Player.ply.OverrideCamMatrix = !NoClip;

            Zoom = 15.0f;

            Utilities.window.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            Two_and_a_Half_Dimensions.Player.ply.CalcView += new Two_and_a_Half_Dimensions.Player.CalcViewHandler(ply_CalcView);
            Two_and_a_Half_Dimensions.Player.ply.SetMode(PlayerMode.CUSTOM);
            horn = Audio.LoadSong("Resources/Audio/horn.mp3", false, true, this);
        }

        void ply_CalcView(object sender, EventArgs e)
        {
            //Camera
            Vector3 point = new Vector3((float)Math.Cos(CamAngle.X), (float)Math.Sin(CamAngle.Y) - 0.21f, (float)Math.Sin(CamAngle.X));
            camMatrix = Matrix4.LookAt(Position + new Vector3(0, crZoom / 90, crZoom), Position + point + new Vector3(0, crZoom / 90, 0), Vector3.UnitY);
            Two_and_a_Half_Dimensions.Player.ply.camMatrix = camMatrix;

            Two_and_a_Half_Dimensions.Player.ply.SetPos(Position + new Vector3(0, crZoom / 90, crZoom));
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Space)
            {
                horn.Play(true);
            }
            if (e.Key == OpenTK.Input.Key.Enter)
            {
                this.SetPos(new Vector2(0, 0));
            }
        }

        public override void Think()
        {
            if (NoClip) return;
            this.SetAngle(Physics.Body.Rotation);

            wheelpos[0] = new Vector3(wheels[0].Body.Position.X, wheels[0].Body.Position.Y, this.Position.Z);
            wheelpos[1] = new Vector3(wheels[1].Body.Position.X, wheels[1].Body.Position.Y, this.Position.Z);

            wheelang[0] = wheels[0].Body.Rotation;
            wheelang[1] = wheels[1].Body.Rotation;

            //MAKE IT A FRICKIN PLANE
            float upforce = (float)Math.Sin( this.Physics.Body.Rotation - 6 ) * this.Physics.Body.LinearVelocity.X;
            //this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(0, upforce));
            //Console.WriteLine(upforce);
            float multiplier = Math.Abs( Physics.Body.Mass * physScale * this.Physics.Body.LinearVelocity.X * 0.07f) + 0.1f;
            //Physics
            if (Utilities.window.Keyboard[OpenTK.Input.Key.W])
            {
                this.Physics.Body.ApplyForce( new Microsoft.Xna.Framework.Vector2( 0, 170.0f ) );
            }
            if (Utilities.window.Keyboard[OpenTK.Input.Key.D])
            {
                //this.Physics.Body.ApplyTorque(-9.0f * multiplier);
                //this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(3.0f * multiplier, 0));
                wheels[0].Body.ApplyTorque(-15.0f * multiplier - 20.0f);
            }
            if (Utilities.window.Keyboard[OpenTK.Input.Key.A])
            {
                //this.Physics.Body.ApplyTorque(9.0f * multiplier);
                //this.Physics.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(-3.0f * multiplier, 0));
                wheels[0].Body.ApplyTorque(8.0f * multiplier + 20.0f);
            }

            //Console.WriteLine(this.Physics.Body.LinearVelocity);

            //Zoom
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageDown]) Zoom -= (float)Utilities.Frametime * 100;
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageUp]) Zoom += (float)Utilities.Frametime * 100;
            Zoom += Input.deltaZ * 0.7f;
            crZoom += (Zoom - crZoom) / 4;
        }


        Matrix4 wheelmat = Matrix4.Identity;
        public override void Draw()
        {
            this.Mat.Properties.SpecularPower = 8.0f;
            this.Mat.Properties.SpecularIntensity = 1.0f;
            base.Draw();
            this.Mat.Properties.SpecularPower = 0;
            this.Mat.Properties.SpecularIntensity = 0;
            
            wheelmat = Matrix4.CreateTranslation(Vector3.Zero); //
            wheelmat *= Matrix4.Scale(this.Scale);
            wheelmat *= Matrix4.CreateRotationZ(wheelang[0]);
            wheelmat *= Matrix4.CreateTranslation(wheelpos[0]);
            wheel.Draw(wheelmat);

            wheelmat = Matrix4.CreateTranslation(Vector3.Zero);
            wheelmat *= Matrix4.Scale(this.Scale);
            wheelmat *= Matrix4.CreateRotationZ(wheelang[1]);
            wheelmat *= Matrix4.CreateTranslation(wheelpos[1]);

            wheel.Draw(wheelmat);
        }

        public override void Remove()
        {
            base.Remove();

            Levels.LevelManager.physWorld.RemoveBody(wheels[0].Body);
            Levels.LevelManager.physWorld.RemoveBody(wheels[1].Body);

            Two_and_a_Half_Dimensions.Player.ply.SetMode(PlayerMode.NOCLIP);

            Utilities.window.Keyboard.KeyDown -= Keyboard_KeyDown;
            Two_and_a_Half_Dimensions.Player.ply.CalcView -= ply_CalcView;
        }
    }
}
