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

namespace Two_and_a_Half_Dimensions.Levels
{
    class LevelManager
    {
        public static bool PausePhysics { get; set; }
        public static float PhysicsTimescale = 1.0f;
        public static bool IsLoading { get; private set; }

        public static LevelBase CurrentLevel { get; private set; }
        public static World physWorld { get; private set; }

        #region Events
        public static event Action<Entity.ent_player> PlayerSpawn;
        #endregion

        public static void InitalizeLevel(LevelBase level)
        {
            if (physWorld != null)
                physWorld.Clear();

            physWorld = new World(new Microsoft.Xna.Framework.Vector2(0, -9.82f));
            
            CurrentLevel = level;
            IsLoading = true;
            CurrentLevel.Preload();
            IsLoading = false;
            PausePhysics = false;
            
            //Create the player
            Entity.ent_player ply = Entity.EntManager.Create<Entity.ent_player>();
            ply.Spawn();
            CurrentLevel.PlayerSpawn(ply);

            if (PlayerSpawn != null)
            {
                PlayerSpawn(ply);
            }
        }

        public static void Think(FrameEventArgs e)
        {
            if (CurrentLevel != null)
            {
                if (physWorld != null && !PausePhysics)
                {
                    physWorld.Step(0.033333f * PhysicsTimescale );
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
