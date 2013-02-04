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
    class ent_pointlight : BaseEntity 
    {
        public bool Enabled { get; set; }
        public Vector3 Color { get; set; }
        public float Cutoff { get; set; }
        public float Constant { get; set; }
        public float AmbientIntensity { get; set; }
        public float DiffuseIntensity { get; set; }
        public float Linear { get; set; }

        private PointLight light = new PointLight();
        public override void Init()
        {
            Utilities.window.effect.SetLights += new LightingTechnique.SetLightsHandler(effect_SetLights);
            AmbientIntensity = 0.2f;
            DiffuseIntensity = 0.7f;

            this.Enabled = true;
        }

        public override void Remove()
        {
            base.Remove();
            Utilities.window.effect.SetLights -= effect_SetLights;
        }

        void effect_SetLights(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                light.AmbientIntensity = AmbientIntensity;
                light.DiffuseIntensity = DiffuseIntensity;
                light.Linear = Linear;
                light.Color = Color;
                light.Constant = Constant;
                light.Position = Position;

                Utilities.window.effect.AddPointLight(light);
            }
        }
    }
}
