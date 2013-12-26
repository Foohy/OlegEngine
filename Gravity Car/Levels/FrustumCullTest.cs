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
    class FrustumCullTest : LevelBase
    {
        bool SetShadow = true;
        Vector3 Angle = new Vector3();
        ent_spotlight spotlight;
        ent_pointlight pointlight;
        Mesh popcorn;

        //Dome for stars n stuff
        private static Mesh dome;
        private static Material starBox;

        public override void Preload()
        {
            //Spawn the little depth screen thingy
            EntManager.Create<ent_depthscreen>().Spawn();

            //Create a mesh that will be our bottom floor because we need floors
            float RectangleSize = 500;
            Vertex[] Vertices = new Vertex[]
            {
                new Vertex(new Vector3(-RectangleSize, 0, RectangleSize), Vector3.UnitY, new Vector2( 0, RectangleSize) ),
                new Vertex(new Vector3(RectangleSize, 0, RectangleSize), Vector3.UnitY, new Vector2( RectangleSize, RectangleSize) ),
                new Vertex(new Vector3(RectangleSize, 0, -RectangleSize), Vector3.UnitY, new Vector2( RectangleSize, 0) ),
                new Vertex(new Vector3(-RectangleSize, 0, -RectangleSize), Vector3.UnitY, new Vector2( 0, 0) ),
            };
            int[] Elements = new int[]
            {
                0, 1, 2,
                2, 3, 0,
            };

            var bbox = MeshGenerator.CalculateBoundingBox(Vertices, Vector3.One);
            Mesh plane = new Mesh(Vertices, Elements);
            plane.BBox = bbox;

            ent_static WorldEntity = EntManager.Create<ent_static>();
            WorldEntity.Spawn();
            WorldEntity.SetModel(plane, false);
            WorldEntity.WorldSpawn = true;
            WorldEntity.Material = Resource.GetMaterial("engine/white");

            Vector3 OlegPos = new Vector3(-2.00f, 2.421f, -14.90f);
            
            //and the boys
            popcorn = Resource.GetMesh("cube.obj");
            popcorn.mat = Resource.GetMaterial("engine/white");

            WorldEntity = EntManager.Create<ent_static>();
            WorldEntity.Spawn();
            WorldEntity.SetModel(Resource.GetMesh("props/popcorn_machine.obj"));
            WorldEntity.WorldSpawn = true;
            WorldEntity.SetPos(new Vector3(1000, 0, 0));

            pointlight = (ent_pointlight)EntManager.Create<ent_pointlight>();
            pointlight.Spawn();
            pointlight.AmbientIntensity = 0.40f;
            pointlight.DiffuseIntensity = 0.85f;
            pointlight.Color = new Vector3(1.0f, 0.5f, 0.0f);
            pointlight.SetPos(new Vector3(-2.00f, 2.421f, -14.90f));
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

            if (e.Key == OpenTK.Input.Key.F12) //Debug print
            {
                Console.WriteLine("==========================");
                Console.WriteLine("Position: {0}", View.Player.Position);
                Console.WriteLine("Angles: {0}", View.Angles);
                Console.WriteLine("ViewNormal: {0}", View.ViewNormal);
                Console.WriteLine("ViewMatrix: {0}", View.ViewMatrix);
                Console.WriteLine("==========================");
            }
        }

        public override void Think(FrameEventArgs e)
        {
            base.Think(e);

            Angle = new Vector3((float)Math.Cos( Utilities.Time / 10), -(float)Math.Abs(Math.Sin( Utilities.Time / 10)), 0.0f);

            if (SetShadow && spotlight != null)
            {
                spotlight.SetAngle(View.Angles);
                spotlight.SetPos( View.Player.Position );

                SkyboxTechnique.SunVector = -View.Angles.Forward();
            }

            FogTechnique.SetDensity(((float)Math.Sin(Utilities.Time/70)+1)/220);
        }

        private float fastRelDist( float x1, float z1, float x2, float z2 )
        {
            return (float)(Math.Pow(x1-x2, 2) + Math.Pow(z1-z2, 2));
        }

        public override void Draw(FrameEventArgs e)
        {
            base.Draw(e);

            float timeOffset = (float)Utilities.Time + 10000f;
            Vector3 pos = new Vector3((float)Math.Cos(Utilities.Time) * 7, (float)Math.Sin(Utilities.Time)*10+10, (float)Math.Sin(Utilities.Time) * 7);
            pointlight.SetPos(Vector3.Zero);

            popcorn.mat = Resource.GetMaterial("engine/white");

            for (float i = 0; i < 100; i+=0.05f)
            {
                float spin = i + (timeOffset / (float)Math.Pow(i, 1.2f)) * 50;
                float heightRand = (float)Math.Pow(i, 0.6f) * 10 -(float)Math.Sin(i * 10000) * 8;
                Vector3 popPos = new Vector3((float)Math.Cos(spin) * i, heightRand, (float)Math.Sin(spin) * i);
                popcorn.Position = popPos;
                popcorn.Angles = new OlegEngine.Angle(0, spin, (timeOffset / i) * 1000 );
                popcorn.Draw();
            }
            
            /*
            for (float x = -50; x < 50; x += 2)
            {
                for (float y = -50; y < 50; y += 2)
                {

                    //popcorn.Position = new Vector3(x, ((float)Math.Sin((x) * 0.1f + Utilities.Time) + (float)Math.Cos((y) * 0.1f + Utilities.Time)) * 10 + 20, y);
                    popcorn.Position = new Vector3(x, -fastRelDist(pos.X, pos.Z, x, y) / 100f + pos.Y, y);
                    //popcorn.Scale = new Vector3(1, 10, 1);
                    popcorn.Draw();
                }
            }
             * */

            Graphics.ShouldDrawFrustum = false;
            Graphics.ShouldDrawBoundingSpheres = false;
            Graphics.ShouldDrawBoundingBoxes = false;
            Utilities.engine.ShouldSlowFrametimeWhenUnfocused = false;
            //hahaheyo
           // Graphics.ViewFrustum.SetCameraDef(spotlight.Position, (spotlight.Position + new Vector3((float)Math.Cos(Utilities.Time), 0,(float)Math.Sin(Utilities.Time))), Vector3.UnitY);
        }
    }
}
