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
        public bool ExpensiveShadows { get; set; }
        public Vector3 Color { get; set; }
        public float Cutoff { get; set; }
        public float Constant { get; set; }
        public float AmbientIntensity { get; set; }
        public float DiffuseIntensity { get; set; }

        public ShadowInfo shadowInfo;
        private SpotLight cheapLight;

        public override void Init()
        {
            shadowInfo = new ShadowInfo(Position, Angle, Resource.GetTexture("effects/flashlight.png"), 1.0f );
            shadowInfo.Linear = 0.01f;
            cheapLight = new SpotLight();
            cheapLight.Linear = 0.01f;

            Utilities.window.shadows.SetLights += new ShadowTechnique.SetLightsHandler(shadows_SetLights);
            Utilities.window.effect.SetLights += new LightingTechnique.SetLightsHandler(effect_SetLights);
            AmbientIntensity = 0.0f;
            DiffuseIntensity = 1.0f;

            this.Enabled = true;
            this.ExpensiveShadows = true;
        }

        void effect_SetLights(object sender, EventArgs e)
        {
            if (this.Enabled && !this.ExpensiveShadows)
            {
                cheapLight.AmbientIntensity = 0.0f;
                cheapLight.DiffuseIntensity = 1.0f;
                cheapLight.Color = Color;
                cheapLight.Constant = Constant;
                cheapLight.Cutoff = Cutoff;
                cheapLight.Direction = this.Angle;
                cheapLight.Position = Position;

                Utilities.window.effect.AddSpotLight(cheapLight);
            }
        }

        public override void Remove()
        {
            base.Remove();
            Utilities.window.effect.SetLights -= shadows_SetLights;
        }

        void shadows_SetLights(object sender, EventArgs e)
        {
            if (this.Enabled && this.ExpensiveShadows)
            {
                shadowInfo.AmbientIntensity = 0.0f;
                shadowInfo.DiffuseIntensity = 1.0f;
                shadowInfo.Color = Color;
                shadowInfo.Constant = Constant;
                shadowInfo.Cutoff = Cutoff;
                shadowInfo.Direction = this.Angle;
                shadowInfo.Position = Position;

                Utilities.window.shadows.AddLightsource(shadowInfo);
            }
        }
    }
}
