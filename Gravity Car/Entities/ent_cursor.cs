using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

using OlegEngine.Entity;
using OlegEngine;

namespace Gravity_Car.Entity
{
    class ent_cursor : BaseEntity 
    {
        private PointLight pl = new PointLight();
        public override void Init()
        {
            this.SetModel( Resource.GetMesh("cursor.obj"));
            this.Mat = Resource.GetMaterial("models/cursor");
            LightingTechnique.SetLights += new Action(LightingTechnique_SetLights);

            pl.AmbientIntensity = 0.3f;
            pl.DiffuseIntensity = 0.6f;
            pl.Linear = 2.0f;
            pl.Position = this.Position;
            //pl.Color = new Vector3(1.0f, 1.0f, 1.0f);
            //this.EmitSound("Resources/Audio/horn.mp3");
        }

        void LightingTechnique_SetLights()
        {
            pl.Position = this.Position;
            LightingTechnique.AddPointLight(pl);
        }

        public override void Remove()
        {
            base.Remove();
            LightingTechnique.SetLights -= LightingTechnique_SetLights;
        }
    }
}
