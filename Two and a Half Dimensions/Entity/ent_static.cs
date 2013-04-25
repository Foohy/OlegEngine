using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

namespace Two_and_a_Half_Dimensions.Entity
{
    class ent_static : BaseEntity 
    {
        public override void Draw()
        {
            base.Draw();

            //Console.WriteLine( Graphics.ViewFrustum.BoxInFrustum( this.Model.BBox ));
        }
    }
}
