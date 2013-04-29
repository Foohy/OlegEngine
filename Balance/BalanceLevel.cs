using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Graphical stuff
using OpenTK;

//Physics
using FarseerPhysics.Dynamics;

//Engine calls
using OlegEngine;
using OlegEngine.Entity;

//Our own stuff
using Balance.Entity;

namespace Balance
{
    class BalanceLevel
    {
        private static MeshGroup WorldMesh;
        private static ent_spotlight spotlight;
        public static void Initialize()
        {
            //Create the physics world
            if (Utilities.PhysicsWorld != null)
                Utilities.PhysicsWorld.Clear();

            Utilities.PhysicsWorld = new World(new Microsoft.Xna.Framework.Vector2(0, -9.82f)); //Create a world with a gravity of 9.82 m/s^2 downward

            //Create the player
            ent_camera cam = EntManager.Create<ent_camera>();
            cam.Spawn();
            cam.SetPos(new OpenTK.Vector2(0, 4));

            //Set the player as the object that'll be controlling the view
            View.SetLocalPlayer(cam);

            SetUpScene(Utilities.PhysicsWorld);
        }

        private static void SetUpScene(World w)
        {
            //Load the world
            WorldMesh = Utilities.LoadOBJMulti("balancelevel.obj");

            //Create the floor's physics mesh
            Body level = FarseerPhysics.Factories.BodyFactory.CreateRectangle(Utilities.PhysicsWorld, 1000, 100, 1.00f);
            level.Position = new Microsoft.Xna.Framework.Vector2(-400, 0.00f);
            level.BodyType = BodyType.Static;

            ent_static ent = EntManager.Create<ent_static>();
            ent.Spawn();
            ent.SetModel(Resource.GetMesh("debug/ball.obj"));
            ent.Mat = Resource.GetMaterial("engine/white");
            ent.Mat.Properties.ShaderProgram = Resource.GetProgram("default");

            //Turn on the lights
            DirectionalLight light = new DirectionalLight();
            light.AmbientIntensity = 0.4f;
            light.DiffuseIntensity = 0.9f;
            light.Color = new Vector3(0.133f, 0.149f, 0.176f);
            light.Direction = new Vector3(0.0f, -1.0f, 0.0f);

            ent_pointlight pointlight = EntManager.Create<ent_pointlight>();
            pointlight.Spawn();
            pointlight.AmbientIntensity = 0.4f;
            pointlight.DiffuseIntensity = 0.85f;
            pointlight.Color = new Vector3(1.0f, 0.5f, 0.0f);
            pointlight.SetPos(new Vector3(-2.00f, 2.421f, -14.90f));
            pointlight.Linear = 0.7f;

            spotlight = (ent_spotlight)EntManager.Create<ent_spotlight>();
            spotlight.Spawn();

            spotlight.Color = new Vector3(1.0f, 1.0f, 1.0f);
            spotlight.Constant = 1.0f;
            spotlight.Cutoff = 20.0f;

            LightingTechnique.SetEnvironmentLight(light);
            ShadowTechnique.Enable();
        }

        public static void Think()
        {
            //Tell the physics to go forward a step
            if (Utilities.PhysicsWorld != null)
            {
                Utilities.PhysicsWorld.Step(0.033333f );
            }

            if ( spotlight != null && Utilities.window.Keyboard[OpenTK.Input.Key.Q])
            {
                spotlight.SetAngle(View.ViewNormal);
                spotlight.SetPos(View.Player.Position);
            }

        }

        public static void Draw()
        {
            if (WorldMesh != null)
            {
                WorldMesh.Draw();
            }
        }
    }
}
