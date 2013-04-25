using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Two_and_a_Half_Dimensions.Levels
{
    class LevelBase
    {
        public virtual void Preload()
        {
        }

        public virtual void PlayerSpawn(Two_and_a_Half_Dimensions.Entity.ent_player p)
        {
            if (p != null)
            {
                p.SetPos(Vector3.Zero);
            }
        }

        public virtual void Think(FrameEventArgs e)
        {
        }

        public virtual void Draw(FrameEventArgs e)
        {
        }
    }
}
