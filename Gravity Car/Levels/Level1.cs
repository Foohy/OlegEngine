using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

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
        ent_static oleg;

        //Dome for stars n stuff
        private static Mesh dome;
        private static Material starBox;

        public override void Preload()
        {
            levelmodel = MeshGenerator.LoadOBJMulti("Levels/sponza.obj"); //Levels/multi_test.obj Levels/sponza.obj
            //levelmodel = Resource.GetMesh("Levels/level1.obj");
            //levelmodel.mat = Resource.GetMaterial("levels/level1");

            //Spawn the little depth screen thingy
            EntManager.Create<ent_depthscreen>().Spawn();

            Vector3 OlegPos = new Vector3(-2.00f, 2.421f, -14.90f);
            //don't forget oleg
            oleg = EntManager.Create<ent_static>();
            oleg.Spawn();
            oleg.SetModel(Resource.GetMesh("props/oleg.obj"));
            oleg.Material = Resource.GetMaterial("models/props/oleg");
            oleg.Name = "Oleg";
            oleg.SetPos(OlegPos);
            oleg.Scale = new Vector3(0.75f);
            oleg.SetAngle(new Angle(0, 0, 14.3f));
            
            //and the boys
            var ent = EntManager.Create<ent_static>();
            ent.Spawn();
            ent.SetModel(Resource.GetMesh("props/oleg.obj"));
            ent.Material = Resource.GetMaterial("engine/white");
            ent.SetPos(OlegPos + new Vector3(0, 0.0f, -0.1f));
            ent.Scale = new Vector3(0.75f);
            ent.SetParent(oleg);
            
            ent_pointlight pointlight = (ent_pointlight)EntManager.Create<ent_pointlight>();
            pointlight.Spawn();
            pointlight.AmbientIntensity = 0.40f;
            pointlight.DiffuseIntensity = 0.85f;
            pointlight.Color = new Vector3(1.0f, 0.5f, 0.0f);
            pointlight.SetPos(OlegPos);
            pointlight.Linear = 0.1f;
            pointlight.SetParent(oleg);

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

            //Gimme some skyboxes!
            SkyboxTechnique.SetSkyGradientMaterial(Resource.GetMaterial("skybox/skygradient_default"));
            SkyboxTechnique.PreDrawSkybox += new Action(SkyboxTechnique_PreDrawSkybox);
            dome = Resource.GetMesh("engine/skybox.obj");
            starBox = Resource.GetMaterial("skybox/skybox_starrynight");

            //Some fog!
            FogTechnique.SetFogParameters(new FogParams()
            {
                Color = new Vector3(0.37254f, 0.368627f, 0.427450f),
                Start = 20,
                End = 200,
                Density = 0.03f,
                Type = FogParams.FogType.Exp2,
            });
            FogTechnique.Enabled = true;

            Utilities.engine.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
        }

        void SkyboxTechnique_PreDrawSkybox()
        {
            if (dome == null) return;

            //Because we're sharing this mesh with the actual skybox, store their current material so we can set it back
            var mat = dome.mat;
            dome.mat = starBox;

            //draw our stuff
            dome.Position = View.Position;
            dome.Draw();

            //Set the material back to the skybox
            dome.mat = mat;
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

                SkyboxTechnique.SunVector = View.Angles.Forward();
            }

            if (oleg != null)
            {
                oleg.SetPos(new Vector3((float)Math.Cos(Utilities.Time)*10, (float)Math.Sin(Utilities.Time) + 3.9f, oleg.Position.Z));
                oleg.SetAngle(oleg.Angles.SetRoll((float)Math.Sin(Utilities.Time * 2) * 20f)); 
            }

            FogTechnique.SetDensity(((float)Math.Sin(Utilities.Time/70)+1)/220);
            //FogTechnique.SetEnd((float)Math.Sin(Utilities.Time / 100f) * 70 + 130);

            //Create a camera matrix
            //Matrix4 shadowmat = Matrix4.LookAt(Pos - (Angle * 70), Pos + Angle - (Angle * 70), Vector3.UnitY);
            //Utilities.engine.shadows.SetLightMatrix(shadowmat);
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
