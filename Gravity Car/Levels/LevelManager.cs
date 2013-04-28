using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;

using OlegEngine.Entity;
using OlegEngine;

using Gravity_Car.Entity;

namespace Gravity_Car.Levels
{
    public class LevelManager
    {
        public static bool PausePhysics { get; set; }
        public static float PhysicsTimescale = 1.0f;
        public static bool IsLoading { get; private set; }

        public static LevelBase CurrentLevel { get; private set; }

        #region Events
        public static event Action<ent_player> PlayerSpawn;
        #endregion

        public static void InitalizeLevel(LevelBase level)
        {
            if (Utilities.PhysicsWorld != null)
                Utilities.PhysicsWorld.Clear();

            Utilities.PhysicsWorld = new World(new Microsoft.Xna.Framework.Vector2(0, -9.82f));
            
            CurrentLevel = level;
            IsLoading = true;
            CurrentLevel.Preload();
            IsLoading = false;
            PausePhysics = false;
            
            //Create the player
            ent_player ply = EntManager.Create<ent_player>();
            ply.Spawn();

            //Set the player to control the view
            View.SetLocalPlayer(ply);

            //Tell the level the player spawned so it can do whatever
            CurrentLevel.PlayerSpawn(ply);

            //Anyone else wanna participate?
            if (PlayerSpawn != null)
            {
                PlayerSpawn(ply);
            }
        }

        public static void Think(FrameEventArgs e)
        {
            if (CurrentLevel != null)
            {
                if (Utilities.PhysicsWorld != null && !PausePhysics)
                {
                    Utilities.PhysicsWorld.Step(0.033333f * PhysicsTimescale);
                }
                CurrentLevel.Think(e);
            }
        }

        public static void Draw(FrameEventArgs e)
        {
            if (CurrentLevel != null) CurrentLevel.Draw(e);
        }
    }
}
