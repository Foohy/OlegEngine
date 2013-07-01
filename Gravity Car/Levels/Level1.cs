using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

using OlegEngine.Entity;
using OlegEngine;

using Gravity_Car.Entity;

namespace Gravity_Car.Levels
{
    class Level1 : LevelBase
    {
        MeshGroup levelmodel;
        bool SetShadow = true;
        Vector3 Angle = new Vector3();
        ent_spotlight spotlight;

        public override void Preload()
        {
            levelmodel = Utilities.LoadOBJMulti("Levels/sponza.obj"); //Levels/multi_test.obj Levels/sponza.obj
            //levelmodel = Resource.GetMesh("Levels/level1.obj");
            //levelmodel.mat = Resource.GetMaterial("levels/level1");

            //Create the physics mesh on the ground
            //TODO: create a level format or something
            Body level = FarseerPhysics.Factories.BodyFactory.CreateRectangle(Utilities.PhysicsWorld, 1000, 100, 1.00f);
            level.Position = new Microsoft.Xna.Framework.Vector2(-400, -50);
            level.BodyType = BodyType.Static;


            BaseEntity ent = EntManager.Create<ent_depthscreen>();
            ent.Spawn();

            Vector3 OlegPos = new Vector3(-2.00f, 2.421f, -14.90f);
            //some fires too
            ent_static oleg = EntManager.Create<ent_static>();
            oleg.Spawn();
            oleg.SetModel(Resource.GetMesh("props/oleg.obj"));
            oleg.Mat = Resource.GetMaterial("models/props/oleg"); //Resource.GetMaterial("models/props/oleg");
            oleg.Name = "Oleg";
            oleg.SetPos(OlegPos);
            oleg.Scale = new Vector3(0.75f);
            oleg.SetAngle(new Angle(0, 0, 14.3f));
            /*
            oleg = Entity.EntManager.Create<ent_static>();
            oleg.Spawn();
            oleg.Model = Resource.GetMesh("props/radio.obj");
            oleg.Mat = Resource.GetMaterial("engine/white");
            oleg.Name = "Popcorn Machine";
            oleg.SetPos(new Vector3( 60.614f, 0.0f, -2.1119f ));
            oleg.Scale = new Vector3(0.45f);

            oleg = Entity.EntManager.Create<ent_static>();
            oleg.Spawn();
            oleg.Model = Resource.GetMesh("props/foilage/ivy_01.obj");
            oleg.Mat = Resource.GetMaterial("models/props/foilage/ivy01");
            //oleg.Mat.SetShader("default"); //Temporary until I can figure out shadowmapping on a single plane for alphatested things
            oleg.SetPos(new Vector3(10.614f, 0.45f, -15.5119f));
            oleg.Scale = new Vector3(0.45f);
            */
            ent_pointlight pointlight = (ent_pointlight)EntManager.Create<ent_pointlight>();
            pointlight.Spawn();
            pointlight.AmbientIntensity = 0.40f;
            pointlight.DiffuseIntensity = 0.85f;
            pointlight.Color = new Vector3(1.0f, 0.5f, 0.0f);
            pointlight.SetPos(OlegPos);
            pointlight.Linear = 0.1f;

            spotlight = (ent_spotlight)EntManager.Create<ent_spotlight>();
            spotlight.Spawn();

            spotlight.Color = new Vector3(1.0f, 1.0f, 1.0f);
            spotlight.Constant = 1.0f;
            spotlight.Cutoff = 20.0f;

            //Make us some nice environmental lighting
            DirectionalLight light = new DirectionalLight();
            light.AmbientIntensity = 0.4f;
            light.DiffuseIntensity = 0.9f;
            light.Color = new Vector3(0.133f, 0.149f, 0.176f);
            light.Direction = new Vector3(0.0f, -1.0f, 0.0f);

            LightingTechnique.SetEnvironmentLight(light);
            ShadowTechnique.Enable();

            Utilities.window.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F2)
            {
                SetShadow = !SetShadow;
            }
            if (e.Key == OpenTK.Input.Key.F3)
            {
                ShadowTechnique.Enabled = !ShadowTechnique.Enabled;
            }
            if (e.Key == OpenTK.Input.Key.Q)
            {
                ent_testball ball = EntManager.Create<ent_testball>();
                //ball.radius = rand.Next(0, 3000) / (float)1000;
                ball.Spawn();
                ball.SetPos(new Vector2(View.Player.Position.X, View.Player.Position.Y + 3.00f));
                ball.RenderMode = BaseEntity.RenderModes.Translucent;
            }
            if (e.Key == OpenTK.Input.Key.F12) //Debug print
            {
                Console.WriteLine("==========================");
                Console.WriteLine("Position: {0}", View.Player.Position);
                Console.WriteLine("Orientation: {0}", View.Angles);
                Console.WriteLine("ViewNormal: {0}", View.ViewNormal);
                Console.WriteLine("Matrix: {0}", View.CameraMatrix);
                Console.WriteLine("==========================");
            }
            /*
            if (e.Key == OpenTK.Input.Key.F10)
            {
                if (playerCar == null)
                {
                    playerCar = EntManager.Create<ent_car>();
                    playerCar.SetPos(new Vector3(View.Player.Position.X, View.Player.Position.Y, 3.0f));
                    playerCar.Spawn();
                }
                else
                {
                    playerCar.Remove();
                    playerCar = null;
                }
            }
             * */
        }

        public override void Think(FrameEventArgs e)
        {
            base.Think(e);

            Angle = new Vector3((float)Math.Cos( Utilities.Time / 10), -(float)Math.Abs(Math.Sin( Utilities.Time / 10)), 0.0f);

            if (SetShadow && spotlight != null)
            {
                spotlight.SetAngle(View.Angles);
                spotlight.SetPos( View.Player.Position );
            }

            //Create a camera matrix
            //Matrix4 shadowmat = Matrix4.LookAt(Pos - (Angle * 70), Pos + Angle - (Angle * 70), Vector3.UnitY);
            //Utilities.window.shadows.SetLightMatrix(shadowmat);
        }

        public override void Draw(FrameEventArgs e)
        {
            if (levelmodel != null)
            {
                levelmodel.Draw();
            }
        }
    }
}
