using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Gravity_Car.Entity;

namespace Gravity_Car.Levels
{
    public class LevelBase
    {
        public virtual void Preload()
        {
        }

        public virtual void PlayerSpawn(ent_player p)
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
