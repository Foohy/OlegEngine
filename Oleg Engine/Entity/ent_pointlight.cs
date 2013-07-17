using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

namespace OlegEngine.Entity
{
    public class ent_pointlight : BaseEntity 
    {
        public bool Enabled { get; set; }
        public float Cutoff { get; set; }
        public float Constant { get; set; }
        public float AmbientIntensity { get; set; }
        public float DiffuseIntensity { get; set; }
        public float Linear { get; set; }

        private PointLight light = new PointLight();
        public override void Init()
        {
            LightingTechnique.SetLights += new Action(LightingTechnique_SetLights);

            AmbientIntensity = 0.2f;
            DiffuseIntensity = 0.7f;

            this.Enabled = true;
            this.ShouldDraw = false;
        }

        void LightingTechnique_SetLights()
        {
            if (this.Enabled)
            {
                light.AmbientIntensity = AmbientIntensity;
                light.DiffuseIntensity = DiffuseIntensity;
                light.Linear = Linear;
                light.Color = Color;
                light.Constant = Constant;
                light.Position = Position;

                LightingTechnique.AddPointLight(light);
            }
        }

        public override void Remove()
        {
            base.Remove();
            LightingTechnique.SetLights -= LightingTechnique_SetLights;
        }
    }
}
