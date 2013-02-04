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
    class ent_spotlight : BaseEntity 
    {
        public bool Enabled { get; set; }
        public Vector3 Color { get; set; }
        public float Cutoff { get; set; }
        public float Constant { get; set; }
        public float AmbientIntensity { get; set; }
        public float DiffuseIntensity { get; set; }

        private SpotLight light = new SpotLight();
        public override void Init()
        {
            Utilities.window.shadows.SetLights += new ShadowTechnique.SetLightsHandler(shadows_SetLights);
            Utilities.window.effect.SetLights += new LightingTechnique.SetLightsHandler(effect_SetLights);
            AmbientIntensity = 0.2f;
            DiffuseIntensity = 0.7f;

            this.Enabled = true;
        }

        public override void Remove()
        {
            base.Remove();
            Utilities.window.effect.SetLights -= shadows_SetLights;
            Utilities.window.effect.SetLights -= effect_SetLights;
        }

        void effect_SetLights(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                light.AmbientIntensity = AmbientIntensity;
                light.DiffuseIntensity = DiffuseIntensity;
                light.Color = Color;
                light.Constant = Constant;
                light.Cutoff = Cutoff;
                light.Direction = this.Angle;
                light.Position = Position;

                Utilities.window.effect.AddSpotLight(light);
            }
        }

        void shadows_SetLights(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                Matrix4 shadowmat = Matrix4.LookAt(Position, Position + Angle, Vector3.UnitY);
                Utilities.window.shadows.AddLightMatrix(shadowmat);
            }
        }
    }
}
