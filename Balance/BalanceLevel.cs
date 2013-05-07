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
        public enum LevelState
        {
            WAITING,
            PLAYING,
            BREAK,
            GAMEOVER
        }
        public static event Action<LevelState> OnStateChange;
        public static LevelState CurrentState = LevelState.WAITING;
        public static double Timer = 0; //A generic game timer that'll serve different functions depending on the state
        public static double TimeSinceStart = 0; //The time when the last state began

        private static MeshGroup WorldMesh;
        private static ent_spotlight spotlight;
        private static ent_physics BalanceEnt;
        private static MusicMix Music;

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
            Body level = FarseerPhysics.Factories.BodyFactory.CreateRectangle(Utilities.PhysicsWorld, 1000, 10, 1.00f);
            level.Position = new Microsoft.Xna.Framework.Vector2(0, -5.0f);
            level.BodyType = BodyType.Static;
            //Walls
            level = FarseerPhysics.Factories.BodyFactory.CreateRectangle(Utilities.PhysicsWorld, 10, 1000, 1.00f);
            level.Position = new Microsoft.Xna.Framework.Vector2(-153, 0);
            level.BodyType = BodyType.Static;
            level = FarseerPhysics.Factories.BodyFactory.CreateRectangle(Utilities.PhysicsWorld, 10, 1000, 1.00f);
            level.Position = new Microsoft.Xna.Framework.Vector2(153, 0);
            level.BodyType = BodyType.Static;

            ent_physics pole = EntManager.Create<ent_physics>();
            pole.Spawn();
            pole.SetPos(Vector2.Zero);
            pole.SetModel(new Mesh("pole.obj"));
            pole.Mat = Resource.GetMaterial("Models/wood_log");
            //Create the physics for it
            pole.Physics.Body.BodyType = BodyType.Static;
            pole.SetPos(new Vector2(0, -pole.Model.BBox.Negative.Y));

            ent_physics bar = EntManager.Create<ent_physics>();
            bar.Spawn();
            bar.SetPos(Vector2.Zero);
            bar.SetModel(new Mesh("balancebeam.obj"));
            bar.Mat = Resource.GetMaterial("Models/balancebeam");
            bar.SetPos(new Vector2(0, 30));
            bar.Physics.Body.BodyType = BodyType.Dynamic;
            bar.Physics.Body.Mass = 2;
            bar.Physics.Friction = 1000;

            BalanceEnt = bar;

            //Turn on the lights
            DirectionalLight light = new DirectionalLight();
            light.AmbientIntensity = 0.4f;
            light.DiffuseIntensity = 0.9f;
            light.Color = new Vector3(0.745f, 0.820f, 0.847f); //new Vector3(0.133f, 0.149f, 0.176f);
            light.Direction = new Vector3(-1.0f, -1.0f, 0.0f);
            light.Direction.Normalize();

            ent_pointlight pointlight = EntManager.Create<ent_pointlight>();
            pointlight.Spawn();
            pointlight.AmbientIntensity = 0.4f;
            pointlight.DiffuseIntensity = 20.0f; //0.85f
            pointlight.Color = new Vector3(1.0f, 0.5f, 0.0f);
            pointlight.SetPos(new Vector3(-2.00f, 2.421f, -14.90f));
            pointlight.Linear = 22.7f;

            spotlight = (ent_spotlight)EntManager.Create<ent_spotlight>();
            spotlight.Spawn();

            spotlight.Color = new Vector3(1.0f, 1.0f, 1.0f);
            spotlight.Constant = 1.0f;
            spotlight.Cutoff = 20.0f;

            //Create a radio to set the mood
            ent_radio radio = EntManager.Create<ent_radio>();
            radio.Spawn();
            radio.SetPos(new Vector3(-7.00f, 0, -14.90f));
            radio.Scale = new Vector3(0.3f);

            LightingTechnique.SetEnvironmentLight(light);
            ShadowTechnique.Enable();

            //Load some groovy jams with our custom music handler
            Music = MusicMix.Create("Resources/Audio/Music", "mix_", ".wav");
            Music.Play(true);

            //Start waiting!
            SetState(LevelState.WAITING);
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

            if (CurrentState == LevelState.WAITING && Utilities.Time > Timer )
            {
                ent_camera[] cam = EntManager.GetByType<ent_camera>();
                if (cam.Length > 0) { cam[0].SetBalanceEntity(BalanceEnt); }
                SetState(LevelState.PLAYING);
            }

            if (CurrentState == LevelState.PLAYING)
            {

                if (Timer < Utilities.Time)
                {
                    //Here, the timer will work to be when we spawn our next falling object
                    Timer = Utilities.Time + Utilities.Rand.NextDouble(0, 2);

                    if (Music != null)
                    {
                        Music.SetPercent((float)(Utilities.Time - TimeSinceStart) / 100f);
                    }

                    CreateFallingObject();
                }
            }

        }

        public static void SetState(LevelState state)
        {
            switch (state)
            {
                case LevelState.WAITING:
                    Timer = Utilities.Time + 5;
                    break;

                case LevelState.PLAYING:
                    break;
            }

            CurrentState = state;
            TimeSinceStart = Utilities.Time;
            if (OnStateChange != null)
                OnStateChange(CurrentState);
        }

        private static void CreateFallingObject()
        {
            float DifficultyScale = (float)(Utilities.Time - TimeSinceStart) / 100f;
            ent_ball ball = EntManager.Create<ent_ball>();
            ball.Radius = (float)Utilities.Rand.NextDouble(DifficultyScale + 0.05f, DifficultyScale*1.2f + 0.05f);
            ball.Spawn();
            ball.SetPos(new Vector2((float)Utilities.Rand.NextDouble(-3, 3), 20));
            ball.Physics.Restitution = 0.0f;
            ball.Physics.Body.Mass = ball.Radius * 0.2f;
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
