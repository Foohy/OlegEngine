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

namespace Two_and_a_Half_Dimensions.Entity
{
    class EntManager
    {
        public static bool Paused { get; private set; }

        static List<BaseEntity> Ents = new List<BaseEntity>();
        static int _nmEnts = 0;

        public static T Create<T>() where T : BaseEntity, new()
        {
            _nmEnts++;

            //Create a new instance
            T ent = new T();
            
            //If it was spawned during level loading, set it as world spawn
            if (Levels.LevelManager.IsLoading)
                ent.WorldSpawn = true;

            //Create a unique name
            ent.Name = ent.GetType().Name + "[" + _nmEnts.ToString() + "]";
            ent.Class = ent.GetType().Name;

            Ents.Add(ent);

            return ent;
        }

        public static void Think(FrameEventArgs e)
        {
            for (int i = 0; i < Ents.Count; i++ )
            {
                BaseEntity ent = Ents[i];
                if (ent._toRemove)
                {
                    Ents.Remove(ent);
                    removePhys(ent);
                    i--;
                }

                if (ent.Physics != null)
                {
                    Vector3 pos = new Vector3( ent.Physics.Body.Position.X, ent.Physics.Body.Position.Y, ent.Position.Z );
                    ent.SetPos(pos, false);
                }
                ent.Think();
            }
        }

        public static void DrawOpaque(FrameEventArgs e)
        {
            foreach (BaseEntity ent in Ents)
            {
                if (ent.RenderMode == BaseEntity.RenderModes.Opaque)
                {
                    ent.Draw();
                }
            }
        }

        public static void DrawTranslucent(FrameEventArgs e)
        {
            foreach (BaseEntity ent in Ents)
            {
                if (ent.RenderMode == BaseEntity.RenderModes.Translucent)
                {
                    ent.Draw();
                }
            }
        }

        private static void removePhys(BaseEntity ent)
        {
            if (ent.Physics == null ) return;

            Levels.LevelManager.physWorld.RemoveBody(ent.Physics.Body);
        }

        #region utility functions
        public static BaseEntity[] GetAll()
        {
            return Ents.ToArray();
        }

        public static T[] GetByType<T>() where T : BaseEntity
        {
            List<T> ents = new List<T>();
            for (int i = 0; i < Ents.Count; i++)
            {
                if (Ents[i] is T)
                {
                    ents.Add((T)Ents[i]);
                }
            }

            return ents.ToArray();
        }

        public static BaseEntity[] GetByName(string name)
        {
            List<BaseEntity> ents = new List<BaseEntity>();
            for (int i = 0; i < Ents.Count; i++)
            {
                if (Ents[i].Name == name)
                {
                    ents.Add(Ents[i]);
                }
            }

            return ents.ToArray();
        }
        #endregion

    }
}
