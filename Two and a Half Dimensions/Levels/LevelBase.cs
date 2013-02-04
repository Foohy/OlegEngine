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

        public virtual void Think(FrameEventArgs e)
        {
        }

        public virtual void Draw(FrameEventArgs e)
        {
        }
    }
}
