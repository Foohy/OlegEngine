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
    class Campfire : BaseEntity 
    {
        public override void Init()
        {
            //Create the model
            this.Model = Resource.GetMesh("props/oleg.obj");
            this.Mat = Resource.GetMaterial("models/props/oleg.png", "default_lighting");
            this.Name = "Fire";
        }
    }
}
