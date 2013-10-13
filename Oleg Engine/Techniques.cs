using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine
{
    public class Technique
    {
        public static int Program = 0;
        public Technique()
        {
        }
        ~Technique()
        {
        }

        public static void Enable()
        {
            GL.UseProgram(Program);
        }


    }
    public class LightingTechnique : Technique
    {
        public static event Action SetLights;
        public static bool EnvironmentLightEnabled = true;
        new public static int Program = 0; //Override

        const int MAX_SPOTLIGHTS = 2;
        const int MAX_POINTLIGHTS = 2;

        static int samplerLocation;
        static int eyeWorldPosLocation;

        static int matSpecularIntensityLocation;
        static int matSpecularPowerLocation;

        static int numPointLightsLocation;
        static int numSpotLightsLocation;

        static LightLocations lightLocations;
        static SpotLightLocations[] spotlightLocations = new SpotLightLocations[MAX_SPOTLIGHTS];
        static PointLightLocations[] pointlightLocations = new PointLightLocations[MAX_POINTLIGHTS];


        static DirectionalLight EnvironmentLight = new DirectionalLight();
        static List<SpotLight> Spotlights = new List<SpotLight>();
        static List<PointLight> Pointlights = new List<PointLight>();

        public static bool Init()
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
                spotlightLocations[i].UseTexture = GL.GetUniformLocation(prog, beginning + ".UseTexture");
                spotlightLocations[i].Texture = GL.GetUniformLocation(prog, beginning + ".SpotlightTexture");
            }

            Program = prog;

            return true;
        }

        public static void Render()
        {
            GL.UseProgram(Program);
            SetEyeWorldPos(View.Position);

            //Clear the lights for this frame
            Pointlights.Clear(); 
            Spotlights.Clear();

            if (SetLights == null) return; //don't bother setting the lights if no one is out there
            SetLights();

            //Now that we have a list of all the lights to render this frame, friggin set em
            SetPointLights(Pointlights.ToArray());
            SetSpotlights(Spotlights.ToArray());

            if (EnvironmentLightEnabled)
                SetDirectionalLight(EnvironmentLight);
        }

        public static void AddPointLight(PointLight pl)
        {
            Pointlights.Add(pl);
        }

        public static void AddSpotLight(SpotLight sl)
        {
            Spotlights.Add(sl);
        }

        public static void SetEnvironmentLight(DirectionalLight light)
        {
            EnvironmentLight = light;
        }

        public static void EnableEnvironmentLight(bool enabled)
        {
            EnvironmentLightEnabled = enabled;
        }

        static Vector3 day = new Vector3(1.0f, 1.0f, 0.862f);
        static Vector3 dusk = new Vector3(1.0f, 0.2353f, 0.2353f);
        static Vector3 night = new Vector3(0.133f, 0.149f, 0.176f) * 4;
        static Vector3 current = new Vector3();
        public static Vector3 angle = new Vector3();
        private static void DayNightThink()
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

        private static Vector3 ApproachVector(Vector3 vec1, Vector3 vec2, float percent)
        {
            return new Vector3(Approach(vec1.X, vec2.X, percent),
                Approach(vec1.Y, vec2.Y, percent),
                Approach(vec1.Z, vec2.Z, percent));
        }

        private static float Approach(float start, float end, float percent)
        {
            return start + ((end - start) * percent);
        }

        public static void SetEyeWorldPos(Vector3 pos)
        {
            GL.Uniform3(eyeWorldPosLocation, pos.X, pos.Y, pos.Z);
        }

        public static void SetShadowTexture(int tex)
        {
            //GL.ActiveTexture(TextureUnit.Texture2);
            //GL.BindTexture(TextureTarget.Texture2D, tex);
            //GL.BindSampler(2, shadowSamplerLocation);
            //GL.ActiveTexture(TextureUnit.Texture0);
        }

        public static void SetTextureUnit(int unit)
        {
        }

        private static void SetDirectionalLight(DirectionalLight Light)
        {
            GL.Uniform3(lightLocations.Color, Light.Color.X, Light.Color.Y, Light.Color.Z);
            GL.Uniform1(lightLocations.AmbientIntensity, Light.AmbientIntensity);
            Vector3 Direction = Light.Direction;
            Direction.Normalize();
            GL.Uniform3(lightLocations.Direction, Direction.X, Direction.Y, Direction.Z);
            GL.Uniform1(lightLocations.DiffuseIntensity, Light.DiffuseIntensity);
        }


        private static void SetPointLights(PointLight[] pLights)
        {
            GL.Uniform1(numPointLightsLocation, pLights.Length);

            for (int i = 0; i < pLights.Length && i < MAX_POINTLIGHTS; i++)
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

        private static void SetSpotlights(SpotLight[] pLights)
        {
            GL.Uniform1(numSpotLightsLocation, pLights.Length);

            for (int i = 0; i < pLights.Length && i < MAX_POINTLIGHTS; i++) 
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

                GL.Uniform1(spotlightLocations[i].UseTexture, pLights[i].UseTexture);
                //GL.Uniform1(spotlightLocations[i].Texture
            }
        }

        public static void SetMatSpecularIntensity(float Intensity)
        {
            GL.Uniform1(matSpecularIntensityLocation, Intensity);
        }

        public static void SetMatSpecularPower(float Power)
        {
            GL.Uniform1(matSpecularPowerLocation, Power);
        }

        private static double ToRadian(double deg)
        {
            return (Math.PI / 180) * deg;
        }
    }
    public class SkyboxTechnique : Technique
    {
        new public static int Program = 0; //Override

        static Mesh skymodel;

        public static bool Init()
        {
            skymodel = Resource.GetMesh("engine/skybox.obj");
            skymodel.ShouldDrawDebugInfo = false;

            int prog = Resource.GetProgram("skybox");
            GL.UseProgram(prog);

            Program = prog;

            return true;
        }

        /// <summary>
        /// Set the material of the sky being drawn around the player. This should probably be using the <code>skybox</code> shader if you don't want it to break horribly.
        /// </summary>
        /// <param name="mat">The material to set the skybox model to</param>
        public static void SetSkyMaterial(Material mat)
        {
            skymodel.mat = mat;
        }

        public static void Render()
        {
            if (skymodel.mat == null || skymodel.mat.Properties.TextureType != TextureTarget.TextureCubeMap) return;

            GL.CullFace(CullFaceMode.Front);
            GL.DepthFunc(DepthFunction.Lequal );

            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.CreateTranslation(View.Position);

            skymodel.DrawSimple(modelview);

            GL.CullFace(CullFaceMode.Back);
            GL.DepthFunc(DepthFunction.Less);
        }
    }
    public class ShadowTechnique : Technique
    {
        const int MAX_SHADOWCASTERS = 2;
        private static bool _enabled = false;
        public static bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                SetDrawCheapShadows(!value);
            }
        }

        new public static int Program = 0; //Override

        public static event Action SetLights;

        public static List<ShadowInfo> _lights = new List<ShadowInfo>();

        static int shadowSamplerLocation;
        static int shadowTextureLocation;
        static int numShadowCastersLocation;
        static int shadowCheapDraw;
        static int shadowMapSize;

        static ShadowCasterLocations[] shadowcasterLocations = new ShadowCasterLocations[MAX_SHADOWCASTERS];

        public static int lightWVPLocation;

        public static bool Init()
        {
            int prog = Resource.GetProgram("default_lighting");
            shadowSamplerLocation = GL.GetUniformLocation(prog, "sampler_shadow");
            lightWVPLocation = GL.GetUniformLocation(prog, "gLightWVP");
            shadowTextureLocation = GL.GetUniformLocation(prog, "sampler_shadow_tex");
            numShadowCastersLocation = GL.GetUniformLocation(prog, "gNumShadowCasters");
            shadowCheapDraw = GL.GetUniformLocation(prog, "gCheap");
            shadowMapSize = GL.GetUniformLocation(prog, "gShadowMapSize");

            for (int i = 0; i < shadowcasterLocations.Length; i++)
            {
                string beginning = string.Format("gShadowCasters[{0}]", i);

                shadowcasterLocations[i].Color = GL.GetUniformLocation(prog, beginning + ".Base.Base.Color");
                shadowcasterLocations[i].AmbientIntensity = GL.GetUniformLocation(prog, beginning + ".Base.Base.AmbientIntensity");
                shadowcasterLocations[i].Position = GL.GetUniformLocation(prog, beginning + ".Base.Position");
                shadowcasterLocations[i].Direction = GL.GetUniformLocation(prog, beginning + ".Direction");
                shadowcasterLocations[i].Cutoff = GL.GetUniformLocation(prog, beginning + ".Cutoff");
                shadowcasterLocations[i].DiffuseIntensity = GL.GetUniformLocation(prog, beginning + ".Base.Base.DiffuseIntensity");

                shadowcasterLocations[i].Constant = GL.GetUniformLocation(prog, beginning + ".Base.Atten.Constant");
                shadowcasterLocations[i].Linear = GL.GetUniformLocation(prog, beginning + ".Base.Atten.Linear");
                shadowcasterLocations[i].Exp = GL.GetUniformLocation(prog, beginning + ".Base.Atten.Exp");

                shadowcasterLocations[i].Brightness = GL.GetUniformLocation(prog, beginning + ".Brightness");
                shadowcasterLocations[i].Cheap = GL.GetUniformLocation(prog, beginning + ".Cheap");
            }

            if (!GL.IsProgram(prog) ||
                lightWVPLocation == -1)
            {
                return false;
            }

            Program = prog;

            return true;

        }

        public static void Render()
        {
            ShadowInfo info = GetShadowInfo();

            GL.UseProgram(Program);
            Matrix4 mat = info.matrix;
            GL.UniformMatrix4(lightWVPLocation, false, ref mat);
            GL.Uniform1(shadowTextureLocation, info.texture);
            GL.Uniform2(shadowMapSize, Vector2.One * Utilities.EngineSettings.ShadowMapSize);
        }

        public static ShadowInfo GetShadowInfo()
        {
            if (_lights.Count > 0)
            {
                return _lights[0];
            }

            return ShadowInfo.Default;
        }

        /// <summary>
        /// Send out an event to set the positions of all the places we'll render lights from
        /// </summary>
        public static void UpdateLightPositions()
        {
            _lights.Clear();

            if (SetLights != null) //don't bother setting the lights if no one is out there
                SetLights();

            //set the information to the shader
            SetShadowedSpotlights(_lights.ToArray());
        }

        public static void AddLightsource(ShadowInfo info)
        {
            _lights.Add(info);
        }

        public static void AddLightPositionDirection(Vector3 Position, Vector3 Direction)
        {

        }

        public static void SetDrawCheapShadows(bool cheap)
        {
            GL.UseProgram(Program);
            GL.Uniform1(shadowCheapDraw, cheap ? 1 : 0);
        }

        public static void SetLightInfo(ShadowInfo info)
        {
            GL.UseProgram(Program);
            Matrix4 mat = info.matrix;
            GL.UniformMatrix4(lightWVPLocation, false, ref mat);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, info.texture);
        }

        private static void SetShadowedSpotlights(ShadowInfo[] pLights)
        {
            GL.UseProgram(Program);
            GL.Uniform1(numShadowCastersLocation, pLights.Length);

            for (int i = 0; i < pLights.Length && i < MAX_SHADOWCASTERS; i++)
            {
                GL.Uniform3(shadowcasterLocations[i].Color, pLights[i].Color.X, pLights[i].Color.Y, pLights[i].Color.Z);
                GL.Uniform1(shadowcasterLocations[i].AmbientIntensity, pLights[i].AmbientIntensity);
                GL.Uniform1(shadowcasterLocations[i].DiffuseIntensity, pLights[i].DiffuseIntensity);
                GL.Uniform3(shadowcasterLocations[i].Position, pLights[i].Position.X, pLights[i].Position.Y, pLights[i].Position.Z);
                Vector3 Direction = pLights[i].Direction;
                Direction.Normalize();
                GL.Uniform3(shadowcasterLocations[i].Direction, Direction.X, Direction.Y, Direction.Z);
                GL.Uniform1(shadowcasterLocations[i].Cutoff, Math.Cos(ToRadian(pLights[i].Cutoff)));
                GL.Uniform1(shadowcasterLocations[i].Constant, pLights[i].Constant);
                GL.Uniform1(shadowcasterLocations[i].Linear, pLights[i].Linear);
                GL.Uniform1(shadowcasterLocations[i].Exp, pLights[i].Exp);
                GL.Uniform1(shadowcasterLocations[i].Brightness, pLights[i].brightness);
                GL.Uniform1(shadowcasterLocations[i].Cheap, pLights[i].Cheap ? 1 : 0 );
            }
        }

        private static double ToRadian(double deg)
        {
            return (Math.PI / 180) * deg;
        }
    }

    #region technique structures

    public struct DirectionalLight
    {
        public Vector3 Color;
        public float AmbientIntensity;
        public float DiffuseIntensity;

        public Vector3 Direction;
    }

    public struct PointLight
    {
        public Vector3 Color;
        public float AmbientIntensity;
        public float DiffuseIntensity;

        public Vector3 Position;

        public float Constant;
        public float Linear;
        public float Exp;
    }

    public struct SpotLight
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
        public int UseTexture;
        public int Texture;
    }

    public struct ShadowInfo
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
        public bool Cheap;


        public Matrix4 matrix
        {
            get
            {
                return Matrix4.LookAt(Position, Position + Direction, Vector3.UnitY);
            }
        }
        public int texture;
        public float brightness;
        public static ShadowInfo Default;

        public ShadowInfo(Vector3 pos, Vector3 dir )
        {
            texture = Resource.GetTexture("engine/white.png");
            brightness = 1.0f;

            Color = new Vector3(1.0f, 1.0f, 1.0f);
            AmbientIntensity = 0.0f;
            DiffuseIntensity = 1.0f;
            Position = new Vector3(0, 0, 0);
            Direction = new Vector3(0, 0, 0);

            Constant = 0.0f;
            Linear = 0.0f;
            Exp = 0.0f;
            Cutoff = 30.0f;

            Cheap = false;
            
        }
        public ShadowInfo(Vector3 pos, Vector3 dir, int tex)
        {
            texture = tex;
            brightness = 1.0f;

            Color = new Vector3(1.0f, 1.0f, 1.0f);
            AmbientIntensity = 0.0f;
            DiffuseIntensity = 1.0f;
            Position = new Vector3(0, 0, 0);
            Direction = new Vector3(0, 0, 0);

            Constant = 0.0f;
            Linear = 0.0f;
            Exp = 0.0f;
            Cutoff = 30.0f;

            Cheap = false;
        }
        public ShadowInfo(Vector3 pos, Vector3 dir, int tex, float bright)
        {
            texture = tex;
            brightness = bright;

            Color = new Vector3(1.0f, 1.0f, 1.0f);
            AmbientIntensity = 0.0f;
            DiffuseIntensity = 1.0f;
            Position = pos;
            Direction = dir;

            Constant = 0.0f;
            Linear = 0.0f;
            Exp = 0.0f;
            Cutoff = 30.0f;

            Cheap = false;
        }
    }

    public struct LightLocations
    {
        public int Color;
        public int AmbientIntensity;
        public int DiffuseIntensity;
        public int Direction;
    }
    public struct PointLightLocations
    {
        public int Color;
        public int AmbientIntensity;
        public int DiffuseIntensity;
        public int Position;

        public int Constant;
        public int Linear;
        public int Exp;
    }
    public struct SpotLightLocations
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

        public int UseTexture;
        public int Texture;
    }

    public struct ShadowCasterLocations
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

        public int Brightness;
        public int DepthTexture;

        public int Cheap;
        public int Texture;
    }

    #endregion
}
