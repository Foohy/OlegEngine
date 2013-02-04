using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions
{
    class Technique
    {
        public int Program = 0;

        public Technique()
        {
        }
        ~Technique()
        {
        }

        public virtual bool Init()
        {
            return true;
        }

        public virtual void Render()
        {

        }

        public void Enable()
        {
            GL.UseProgram(this.Program);
        }


    }
    class LightingTechnique : Technique
    {
        public delegate void SetLightsHandler(object sender, EventArgs e);
        public event SetLightsHandler SetLights;

        const int MAX_SPOTLIGHTS = 2;
        const int MAX_POINTLIGHTS = 2;

        int samplerLocation;
        int eyeWorldPosLocation;

        int matSpecularIntensityLocation;
        int matSpecularPowerLocation;

        int numPointLightsLocation;
        int numSpotLightsLocation;

        LightLocations lightLocations;
        SpotLightLocations[] spotlightLocations = new SpotLightLocations[MAX_SPOTLIGHTS];
        PointLightLocations[] pointlightLocations = new PointLightLocations[MAX_POINTLIGHTS];

        public LightingTechnique()
        {
            this.Program = 0;
        }

        public override bool Init()
        {
            lightLocations = new LightLocations();

            //Create our shader
            int prog = Resource.GetProgram("default_lighting");
            GL.UseProgram(prog);

            samplerLocation = GL.GetUniformLocation(prog, "sampler");
            eyeWorldPosLocation = GL.GetUniformLocation(prog, "gEyeWorldPos");
            numPointLightsLocation = GL.GetUniformLocation(prog, "gNumPointLights");
            numSpotLightsLocation = GL.GetUniformLocation(prog, "gNumSpotLights");

            lightLocations.Color = GL.GetUniformLocation(prog, "gDirectionalLight.Base.Color");
            lightLocations.AmbientIntensity = GL.GetUniformLocation(prog, "gDirectionalLight.Base.AmbientIntensity");
            lightLocations.DiffuseIntensity = GL.GetUniformLocation(prog, "gDirectionalLight.Base.DiffuseIntensity");
            lightLocations.Direction = GL.GetUniformLocation(prog, "gDirectionalLight.Direction");
            matSpecularIntensityLocation = GL.GetUniformLocation(prog, "gMatSpecularIntensity");
            matSpecularPowerLocation = GL.GetUniformLocation(prog, "gSpecularPower");

            for (int i = 0; i < pointlightLocations.Length; i++)
            {
                string beginning = string.Format("gPointLights[{0}]", i);

                pointlightLocations[i].Color = GL.GetUniformLocation(prog, beginning + ".Base.Color");
                pointlightLocations[i].AmbientIntensity = GL.GetUniformLocation(prog, beginning + ".Base.AmbientIntensity");
                pointlightLocations[i].Position = GL.GetUniformLocation(prog, beginning + ".Position");
                pointlightLocations[i].DiffuseIntensity = GL.GetUniformLocation(prog, beginning + ".Base.DiffuseIntensity");

                pointlightLocations[i].Constant = GL.GetUniformLocation(prog, beginning + ".Atten.Constant");
                pointlightLocations[i].Linear = GL.GetUniformLocation(prog, beginning + ".Atten.Linear");
                pointlightLocations[i].Exp = GL.GetUniformLocation(prog, beginning + ".Atten.Exp");
            }

            for (int i = 0; i < spotlightLocations.Length; i++)
            {
                string beginning = string.Format("gSpotLights[{0}]", i);

                spotlightLocations[i].Color = GL.GetUniformLocation(prog, beginning + ".Base.Base.Color");
                spotlightLocations[i].AmbientIntensity = GL.GetUniformLocation(prog, beginning + ".Base.Base.AmbientIntensity");
                spotlightLocations[i].Position = GL.GetUniformLocation(prog, beginning + ".Base.Position");
                spotlightLocations[i].Direction = GL.GetUniformLocation(prog, beginning + ".Direction");
                spotlightLocations[i].Cutoff = GL.GetUniformLocation(prog, beginning + ".Cutoff");
                spotlightLocations[i].DiffuseIntensity = GL.GetUniformLocation(prog, beginning + ".Base.Base.DiffuseIntensity");

                spotlightLocations[i].Constant = GL.GetUniformLocation(prog, beginning + ".Base.Atten.Constant");
                spotlightLocations[i].Linear = GL.GetUniformLocation(prog, beginning + ".Base.Atten.Linear");
                spotlightLocations[i].Exp = GL.GetUniformLocation(prog, beginning + ".Base.Atten.Exp");
            }

            this.Program = prog;

            return true;
        }

        //lmao
        Entity.Car car;
        Entity.BaseEntity fire;
        private EventArgs ev =  new EventArgs();
        public override void Render()
        {
            this.SetEyeWorldPos(Player.ply.Pos);
            //m_pEffect->SetDirectionalLight(m_directionalLight);
            //m_pEffect->SetEyeWorldPos(m_pGameCamera->GetPos());
            //this.SetMatSpecularIntensity(1.0f);
            //this.SetMatSpecularPower(16);
            //TEMPORARY!!

            if (SetLights == null) return;
            SetLights(this, ev);

            if (car == null)
            {
                Entity.BaseEntity[] ents = Entity.EntManager.GetByType<Entity.Car>();
                if (ents.Length > 0)
                {
                    car = (Entity.Car)ents[0];
                }
            }

            if (fire == null)
            {
                Entity.BaseEntity[] ents = Entity.EntManager.GetByName("Fire");
                if (ents.Length > 0)
                {
                    fire = ents[0];
                }
            }

            if (true) { return; }
            DayNightThink();

            DirectionalLight light = new DirectionalLight();
            light.Color = current;// new Vector3(0.133f, 0.149f, 0.176f);
            light.AmbientIntensity = 0.4f;
            light.DiffuseIntensity = 1.00f;
            light.Direction = angle; // new Vector3(0.661814f, -0.6181439f, -0.4251322f);
            this.SetDirectionalLight(light);

            if (fire != null)
            {
                PointLight[] pl = new PointLight[1];
                pl[0].AmbientIntensity = 0.4f;
                pl[0].DiffuseIntensity = 0.85f;
                pl[0].Color = new Vector3(1.0f, 0.5f, 0.0f);
                pl[0].Position = fire.Position;
                pl[0].Linear = 0.1f;
                this.SetPointLights(pl);
            }
            /*
            pl[1].AmbientIntensity = 0.4f;
            pl[1].DiffuseIntensity = 0.95f;
            pl[1].Color = new Vector3(0.0f, 0.5f, 1.0f);
            pl[1].Position = new Vector3(190, 300, 0);
            pl[1].Constant = 4.0f;
             * */

            if (car != null)
            {
                float x = (float)Math.Cos(car.Angle.Z);
                float y = (float)Math.Sin(car.Angle.Z);

                SpotLight[] sl = new SpotLight[2];
                sl[0].AmbientIntensity = 0.4f;
                sl[0].DiffuseIntensity = 0.9f;
                sl[0].Color = new Vector3(1.0f, 1.0f, 0.6f);
                sl[0].Position = car.Position + new Vector3(x * 3.0f, y * 3.0f, -1.5f);
                sl[0].Direction = new Vector3(x, y - 0.5f, 0);
                sl[0].Linear = 0.1f;
                sl[0].Cutoff = 40.0f;

                sl[1].AmbientIntensity = 0.4f;
                sl[1].DiffuseIntensity = 0.9f;
                sl[1].Color = new Vector3(1.0f, 1.0f, 1.0f);
                sl[1].Position = new Vector3(88.94199f, 23.27345f, 5.085441f);
                sl[1].Direction = new Vector3(0.661814f, -0.6181439f, -0.4251322f);
                sl[1].Linear = 0.1f;
                sl[1].Cutoff = 40.0f;


                this.SetSpotlights(sl);
            }
        }

        Vector3 day = new Vector3(1.0f, 1.0f, 0.862f);
        Vector3 dusk = new Vector3(1.0f, 0.2353f, 0.2353f);
        Vector3 night = new Vector3(0.133f, 0.149f, 0.176f) * 4;
        Vector3 current = new Vector3();
        public Vector3 angle = new Vector3();
        private void DayNightThink()
        {
            float time = (float)Math.Sin(Utilities.Time / 10);
            if (time > 0)
            {
                current = ApproachVector(dusk, day, time);
            }
            else
            {
                current = ApproachVector(dusk, night, Math.Abs(time));
            }
            angle = new Vector3((float)Math.Cos(Utilities.Time / 10), -(float)Math.Sin(Utilities.Time / 10), -1.0f);

            
        }

        private Vector3 ApproachVector(Vector3 vec1, Vector3 vec2, float percent)
        {
            return new Vector3(Approach(vec1.X, vec2.X, percent),
                Approach(vec1.Y, vec2.Y, percent),
                Approach(vec1.Z, vec2.Z, percent));
        }

        private float Approach(float start, float end, float percent)
        {
            return start + ((end - start) * percent);
        }

        public void SetEyeWorldPos(Vector3 pos)
        {
            GL.Uniform3(this.eyeWorldPosLocation, pos.X, pos.Y, pos.Z);
        }

        public void SetShadowTexture(int tex)
        {
            //GL.ActiveTexture(TextureUnit.Texture2);
            //GL.BindTexture(TextureTarget.Texture2D, tex);
            //GL.BindSampler(2, shadowSamplerLocation);
            //GL.ActiveTexture(TextureUnit.Texture0);
        }

        public void SetTextureUnit(int unit )
        {
        }

        public void SetDirectionalLight(DirectionalLight Light)
        {
            GL.Uniform3(lightLocations.Color, Light.Color.X, Light.Color.Y, Light.Color.Z);
            GL.Uniform1(lightLocations.AmbientIntensity, Light.AmbientIntensity);
            Vector3 Direction = Light.Direction;
            Direction.Normalize();
            GL.Uniform3(lightLocations.Direction, Direction.X, Direction.Y, Direction.Z);
            GL.Uniform1(lightLocations.DiffuseIntensity, Light.DiffuseIntensity);
        }


        public void SetPointLights( PointLight[] pLights )
        {
            GL.Uniform1(numPointLightsLocation, pLights.Length);

            for (int i = 0; i < pLights.Length; i++)
            {
                GL.Uniform3(pointlightLocations[i].Color, pLights[i].Color.X, pLights[i].Color.Y, pLights[i].Color.Z);
                GL.Uniform1(pointlightLocations[i].AmbientIntensity, pLights[i].AmbientIntensity);
                GL.Uniform1(pointlightLocations[i].DiffuseIntensity, pLights[i].DiffuseIntensity);
                GL.Uniform3(pointlightLocations[i].Position, pLights[i].Position.X, pLights[i].Position.Y, pLights[i].Position.Z);
                GL.Uniform1(pointlightLocations[i].Constant, pLights[i].Constant);
                GL.Uniform1(pointlightLocations[i].Linear, pLights[i].Linear);
                GL.Uniform1(pointlightLocations[i].Exp, pLights[i].Exp);
            }
        }

        public void SetSpotlights(SpotLight[] pLights)
        {
            GL.Uniform1(numSpotLightsLocation, pLights.Length);

            for (int i = 0; i < pLights.Length; i++) 
            {
                GL.Uniform3(spotlightLocations[i].Color, pLights[i].Color.X, pLights[i].Color.Y, pLights[i].Color.Z);
                GL.Uniform1(spotlightLocations[i].AmbientIntensity, pLights[i].AmbientIntensity);
                GL.Uniform1(spotlightLocations[i].DiffuseIntensity, pLights[i].DiffuseIntensity);
                GL.Uniform3(spotlightLocations[i].Position, pLights[i].Position.X, pLights[i].Position.Y, pLights[i].Position.Z);
                Vector3 Direction = pLights[i].Direction;
                Direction.Normalize();
                GL.Uniform3(spotlightLocations[i].Direction, Direction.X, Direction.Y, Direction.Z);
                GL.Uniform1(spotlightLocations[i].Cutoff, Math.Cos(ToRadian(pLights[i].Cutoff)));
                GL.Uniform1(spotlightLocations[i].Constant, pLights[i].Constant);
                GL.Uniform1(spotlightLocations[i].Linear, pLights[i].Linear);
                GL.Uniform1(spotlightLocations[i].Exp, pLights[i].Exp);
            }
        }

        public void SetMatSpecularIntensity(float Intensity)
        {
            GL.Uniform1(matSpecularIntensityLocation, Intensity);
        }

        public void SetMatSpecularPower(float Power)
        {
            GL.Uniform1(matSpecularPowerLocation, Power);
        }


        private double ToRadian(double deg)
        {
            return (Math.PI / 180) * deg;
        }
    }
    class SkyboxTechnique : Technique
    {
        Mesh skymodel;

        int v3CameraPosLocation;
        int v3LightPosLocation;
        int v3InvWavelengthLocation;
        int fCameraHeightLocation;
        int fCameraHeight2Location;
        int fOuterRadiusLocation;
        int fOuterRadius2Location;
        int fInnerRadiusLocation;
        int fInnerRadius2Location;
        int fKrESunLocation;
        int fKmESunLocation;
        int fKr4PILocation;
        int fKm4PILocation;
        int fScaleLocation;
        int fScaleDepthLocation;
        int fScaleOverScaleDepthLocation;

        int gLocation;
        int g2Location;




        float[] fWavelength = new float[3];
        float[] fWavelength4 = new float[3];

        int nSamples = 3;		// Number of sample rays to use in integral equation
	    float Kr = 0.0025f;		// Rayleigh scattering constant
        float Kr4PI;
	    float Km = 0.0010f;		// Mie scattering constant
        float Km4PI;
	    float ESun = 20.0f;		// Sun brightness constant
	    float g = -0.990f;		// The Mie phase asymmetry factor
        float fInnerRadius = 10.0f;
        float fOuterRadius = 10.25f;
        float fScale;
        float fRayleighScaleDepth = 0.25f;
	    float fMieScaleDepth = 0.1f;
        Vector3 vLight = new Vector3( 200, 100, -5);
        Vector3 v3LightDirection;

        public override bool Init()
        {
            fWavelength[0] = 0.650f;		// 650 nm for red
            fWavelength[1] = 0.570f;		// 570 nm for green
            fWavelength[2] = 0.475f;		// 475 nm for blue
            fWavelength4[0] = (float)Math.Pow(fWavelength[0], 4.0f);
            fWavelength4[1] = (float)Math.Pow(fWavelength[1], 4.0f);
            fWavelength4[2] = (float)Math.Pow(fWavelength[2], 4.0f);

            Kr4PI = Kr * 4.0f * (float)Math.PI;
            Km4PI = Km * 4.0f * (float)Math.PI;
            fScale = 1 / (fInnerRadius - fOuterRadius);
            v3LightDirection = vLight / vLight.Length;

            skymodel = Resource.GetMesh("skybox.obj");
            skymodel.mat = new Material("starm.png", "skybox2");

            int prog = Resource.GetProgram("skybox2");
            GL.UseProgram(prog);

            v3CameraPosLocation = GL.GetUniformLocation(prog, "v3CameraPos");
            v3LightPosLocation = GL.GetUniformLocation(prog, "v3LightPos");
            v3InvWavelengthLocation = GL.GetUniformLocation(prog, "v3InvWavelength");
            fCameraHeightLocation = GL.GetUniformLocation(prog, "fCameraHeight");
            fCameraHeight2Location = GL.GetUniformLocation(prog, "fCameraHeight2");//////////
            fOuterRadiusLocation = GL.GetUniformLocation(prog, "fOuterRadius");////////
            fOuterRadius2Location = GL.GetUniformLocation(prog, "fOuterRadius2");///////////
            fInnerRadiusLocation = GL.GetUniformLocation(prog, "fInnerRadius");
            fInnerRadius2Location = GL.GetUniformLocation(prog, "fInnerRadius2");/////////////
            fKrESunLocation = GL.GetUniformLocation(prog, "fKrESun");
            fKmESunLocation = GL.GetUniformLocation(prog, "fKmESun");
            fKr4PILocation = GL.GetUniformLocation(prog, "fKr4PI");
            fKm4PILocation = GL.GetUniformLocation(prog, "fKm4PI");
            fScaleLocation = GL.GetUniformLocation(prog, "fScale");
            fScaleDepthLocation = GL.GetUniformLocation(prog, "fScaleDepth");
            fScaleOverScaleDepthLocation = GL.GetUniformLocation(prog, "fScaleOverScaleDepth");

            gLocation = GL.GetUniformLocation(prog, "g");
            g2Location = GL.GetUniformLocation(prog, "g2");

            this.Program = prog;

            return true;
        }

        public override void Render()
        {
            setUniforms();

            GL.CullFace(CullFaceMode.Front);
            GL.DepthFunc(DepthFunction.Lequal );

            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale(1.5f);
            modelview *= Matrix4.CreateTranslation(Player.ply.Pos);

            skymodel.Render(modelview);

            GL.CullFace(CullFaceMode.Back);
            GL.DepthFunc(DepthFunction.Less);
        }
        
        private void setUniforms()
        {
            GL.UseProgram(this.Program);

            GL.Uniform3(v3CameraPosLocation, Player.ply.Pos);
            GL.Uniform3(v3LightPosLocation, v3LightDirection);
            GL.Uniform3(v3InvWavelengthLocation, 1 / fWavelength4[0], 1 / fWavelength4[1], 1 / fWavelength4[2]);
            GL.Uniform1(fCameraHeightLocation, Player.ply.Pos.Length);
            GL.Uniform1(fCameraHeight2Location, Player.ply.Pos.LengthSquared);
            GL.Uniform1(fInnerRadiusLocation, fInnerRadius);
            GL.Uniform1(fInnerRadius2Location, fInnerRadius * fInnerRadius);
            GL.Uniform1(fOuterRadiusLocation, fOuterRadius);
            GL.Uniform1(fOuterRadius2Location, fOuterRadius * fOuterRadius);
            GL.Uniform1(fKrESunLocation, Kr * ESun);
            GL.Uniform1(fKmESunLocation, Km * ESun);
            GL.Uniform1(fKr4PILocation, Kr4PI);
            GL.Uniform1(fKm4PILocation, Km4PI);
            GL.Uniform1(fScaleLocation, 1.0f / (fOuterRadius - fInnerRadius));
            GL.Uniform1(fScaleDepthLocation, fRayleighScaleDepth);
            GL.Uniform1(fScaleOverScaleDepthLocation, (1.0f / (fOuterRadius - fInnerRadius)) / fRayleighScaleDepth);
            GL.Uniform1(gLocation, g);
            GL.Uniform1(g2Location, g * g);

            GL.UseProgram(0);

        }
    }
    class ShadowTechnique : Technique
    {
        public delegate void SetLightsHandler(object sender, EventArgs e);
        public event SetLightsHandler SetLights;
        public bool Enabled { get; private set; }
        public Matrix4 LightMVP = Matrix4.Identity;

        int shadowSamplerLocation;
        int shadowMapLocation;

        public int lightWVPLocation;

        public override bool Init()
        {
            int prog = Resource.GetProgram("default_lighting");
            shadowSamplerLocation = GL.GetUniformLocation(prog, "sampler_shadow");
            lightWVPLocation = GL.GetUniformLocation(prog, "gLightWVP");

            if (!GL.IsProgram(prog) ||
                shadowMapLocation == -1 ||
                lightWVPLocation == -1)
            {
                return false;
            }

            this.Program = prog;

            return true;

        }

        EventArgs ev = new EventArgs();
        public override void Render()
        {
            GL.UseProgram(this.Program);
            GL.UniformMatrix4(lightWVPLocation, false, ref LightMVP);
        }

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }

        public void SetLightMatrix(Matrix4 mvp)
        {
            GL.UseProgram(this.Program);
            GL.UniformMatrix4(lightWVPLocation, false, ref mvp);
            LightMVP = mvp;
        }

        public void SetLightLocations()
        {
            SetLights(this, ev);
        }
    }

    #region technique structures

    struct DirectionalLight
    {
        public Vector3 Color;
        public float AmbientIntensity;
        public float DiffuseIntensity;

        public Vector3 Direction;
    }

    struct PointLight
    {
        public Vector3 Color;
        public float AmbientIntensity;
        public float DiffuseIntensity;

        public Vector3 Position;

        public float Constant;
        public float Linear;
        public float Exp;
    }

    struct SpotLight
    {
        public Vector3 Color;
        public float AmbientIntensity;
        public float DiffuseIntensity;

        public Vector3 Position;

        public float Constant;
        public float Linear;
        public float Exp;

        public Vector3 Direction;
        public float Cutoff;
    }

    struct LightLocations
    {
        public int Color;
        public int AmbientIntensity;
        public int DiffuseIntensity;
        public int Direction;
    }
    struct PointLightLocations
    {
        public int Color;
        public int AmbientIntensity;
        public int DiffuseIntensity;
        public int Position;

        public int Constant;
        public int Linear;
        public int Exp;
    }
    struct SpotLightLocations
    {
        public int Color;
        public int AmbientIntensity;
        public int DiffuseIntensity;
        public int Position;

        public int Direction;
        public int Cutoff;

        public int Constant;
        public int Linear;
        public int Exp;
    }

    #endregion
}
