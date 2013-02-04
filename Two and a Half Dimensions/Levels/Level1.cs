using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

using Two_and_a_Half_Dimensions.Entity;

namespace Two_and_a_Half_Dimensions.Levels
{
    class Level1 : LevelBase
    {
        Mesh levelmodel;
        bool SetShadow = true;
        Vector3 Angle = new Vector3();
        Vector3 Pos = new Vector3();
        ent_spotlight spotlight;
        public override void Preload()
        {
            levelmodel = Resource.GetMesh("Levels/level1.obj");
            levelmodel.mat = Resource.GetMaterial("engine/white.png", "default_lighting");

            //Create the physics mesh on the ground
            //TODO: create a level format or something
            Body level = FarseerPhysics.Factories.BodyFactory.CreateRectangle(LevelManager.physWorld, 1000, 100, 1.0f);
            level.Position = new Microsoft.Xna.Framework.Vector2(-400, -52);
            level.BodyType = BodyType.Static;


            BaseEntity ent = EntManager.Create<DepthScreen>();
            ent.Spawn();

            //some fires too
            Entity.BaseEntity fire = Entity.EntManager.Create<Entity.Campfire>();
            fire.Spawn();
            fire.SetPos(new Vector3(-2.00f, 2.421f, 15.90f));
            fire.Scale = new Vector3(0.75f);
            fire.SetAngle(new Vector3(0, (float)Math.PI, 0.25f));
            //fire.SetAngle((float)(Math.PI / 16));

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
            Utilities.window.effect.SetEnvironmentLight(light);

            Utilities.window.ply.SetPos(new Vector3(0, 0, 0));
            Utilities.window.shadows.Enable();
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
                spotlight.ExpensiveShadows = !spotlight.ExpensiveShadows;
            }
            if (e.Key == OpenTK.Input.Key.Q)
            {
                Entity.TestBall ball = (Entity.TestBall)Entity.EntManager.Create<Entity.TestBall>();
                //ball.radius = rand.Next(0, 3000) / (float)1000;
                ball.Spawn();
                ball.SetPos(new Vector2(Player.ply.Pos.X, Player.ply.Pos.Y + 3.0f));
            }
            if (e.Key == OpenTK.Input.Key.F12) //Debug print
            {
                Console.WriteLine("==========================");
                Console.WriteLine("Position: {0}", Player.ply.Pos );
                Console.WriteLine("Orientation: {0}", Player.ply.CamAngle);
                Console.WriteLine("ViewNormal: {0}", Player.ply.ViewNormal);
                Console.WriteLine("Matrix: {0}", Player.ply.camMatrix);
                if (Player.ply.OverrideCamMatrix)
                {
                    Console.WriteLine("VIEW IS BEING OVERWRITTEN!");
                }
                Console.WriteLine("==========================");
            }
        }

        public override void Think(FrameEventArgs e)
        {
            base.Think(e);

            Angle = new Vector3((float)Math.Cos( Utilities.Time / 10), -(float)Math.Abs(Math.Sin( Utilities.Time / 10)), 0.0f);

            if (SetShadow && spotlight != null)
            {
                spotlight.SetAngle( Player.ply.ViewNormal);
                spotlight.SetPos( Player.ply.Pos );
            }

            //Create a camera matrix
            //Matrix4 shadowmat = Matrix4.LookAt(Pos - (Angle * 70), Pos + Angle - (Angle * 70), Vector3.UnitY);
            //Utilities.window.shadows.SetLightMatrix(shadowmat);
        }

        public override void Draw(FrameEventArgs e)
        {
            /*
            SpotLight[] sp = new SpotLight[1];
            sp[0].AmbientIntensity = 0.4f;
            sp[0].Color = new Vector3(1.0f, 1.0f, 1.0f);
            sp[0].Constant = 1.0f;
            sp[0].Cutoff = 20.0f;
            sp[0].Direction = Angle;
            sp[0].Position = Pos - (Angle * 70);

            Utilities.window.effect.SetSpotlights(sp);
             * */


            if (levelmodel != null)
            {
                levelmodel.Render(Matrix4.Identity);
            }
        }
    }
}
