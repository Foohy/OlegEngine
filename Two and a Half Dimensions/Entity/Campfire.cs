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
        ent_pointlight light;
        public override void Init()
        {
            //Create the model
            this.Model = Resource.GetMesh("props/oleg.obj");
            this.Mat = Resource.GetMaterial("models/props/oleg");
            this.Name = "Fire";

            light = (ent_pointlight)EntManager.Create<ent_pointlight>();
            light.Spawn();
            light.AmbientIntensity = 0.4f;
            light.DiffuseIntensity = 0.85f;
            light.Color = new Vector3(1.0f, 0.5f, 0.0f);
            light.SetPos(this.Position);
            light.Linear = 0.1f;

        }
        public override void Think()
        {
            light.SetPos(this.Position);
        }

        public override void Remove()
        {
            base.Remove();

            if (light != null)
            {
                light.Remove();
            }
        }
    }
}
