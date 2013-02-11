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
    class ent_cursor : BaseEntity 
    {
        private PointLight pl = new PointLight();
        public override void Init()
        {
            this.SetModel( Resource.GetMesh("cursor.obj"));
            this.Mat = Resource.GetMaterial("models/cursor");
            Utilities.window.effect.SetLights += new LightingTechnique.SetLightsHandler(effect_SetLights);

            pl.AmbientIntensity = 0.3f;
            pl.DiffuseIntensity = 0.6f;
            pl.Linear = 2.0f;
            pl.Position = this.Position;
            pl.Color = new Vector3(1.0f, 1.0f, 1.0f);
            this.EmitSound("Resources/Audio/horn.mp3");
        }

        void effect_SetLights(object sender, EventArgs e)
        {
            pl.Position = this.Position;
            Utilities.window.effect.AddPointLight(pl);
        }

        public override void Remove()
        {
            base.Remove();
            Utilities.window.effect.SetLights -= new LightingTechnique.SetLightsHandler(effect_SetLights);
        }
    }
}
