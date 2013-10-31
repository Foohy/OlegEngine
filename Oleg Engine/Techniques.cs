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

            if (EnvironmentLightEnabled)
                SetDirectionalLight(EnvironmentLight);

            if (SetLights == null) return; //don't bother setting the lights if no one is out there
            SetLights();

            //Now that we have a list of all the lights to render this frame, friggin set em
            SetPointLights(Pointlights.ToArray());
            SetSpotlights(Spotlights.ToArray());
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

        public static void SetEyeWorldPos(Vector3 pos)
        {
            GL.Uniform3(eyeWorldPosLocation, pos.X, pos.Y, pos.Z);
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
        public static Vector3 SunVector { get; set; }

        private static Mesh skycube;
        private static int locSunVector;

        public static bool Init()
        {
            skycube = Resource.GetMesh("engine/skybox.obj");
            skycube.ShouldDrawDebugInfo = false;
            skycube.mat = new Material(Utilities.DefaultSkyboxTex, "skybox");

            return true;
        }

        /// <summary>
        /// Set the material of the sky being drawn around the player. This should probably be using the <code>skybox</code> shader if you don't want it to break horribly.
        /// </summary>
        /// <param name="mat">The material to set the skybox model to</param>
        public static void SetSkyboxMaterial(Material mat)
        {
            skycube.mat = mat;
        }

        /// <summary>
        /// Set the material of the sky DOME. this should probably be using the <code>skydome</code> shader if you don't want it to break horribly.
        /// <remarks>TODO: This entire function can be removed if textureparameters could be integrated into the texture itself. DO THIS.</remarks>
        /// </summary>
        /// <param name="mat">The material to set the skydome model to</param>
        public static void SetSkyGradientMaterial(Material mat)
        {
            skycube.mat = mat;

            //Store it's uniform location for sunvector
            locSunVector = GL.GetUniformLocation(mat.Properties.ShaderProgram, "gSunVector");

            //SUPER MEGA TODO: Figure out how to handle having textures with certain properties. Proprietary file format?
            //For now, we need to make sure that the texture is set to clamp itself so we don't get gross artifacts
            int color = mat.Properties.BaseTexture;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, color);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            //Aaaand the glow
            int glow = mat.Properties.NormalMapTexture;

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, glow);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        public static void Render()
        {
            if (skycube == null || skycube.mat == null) return;

            GL.CullFace(CullFaceMode.Front);
            GL.DepthFunc(DepthFunction.Lequal);

            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.CreateTranslation(View.Position);

            //Bind the material
            skycube.mat.BindMaterial();

            //Update the sun vector
            GL.Uniform3(locSunVector, SunVector);

            //Draw the skybox
            skycube.DrawSimple(modelview);

            //Switch the drawing modes back to nromal
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
    public class FogTechnique : Technique
    {
        public static bool Enabled { get; set; }
        public static FogParams FogParameters { get; private set; }

        private static Dictionary<int, FogParamsLocations> programLocations = new Dictionary<int, FogParamsLocations>();

        /// <summary>
        /// 'Register' your shader program with the fog shader, so it can keep track of its fog uniform locations
        /// This is done automatically when the shader program is loaded. Only call this if you know what you're doing
        /// </summary>
        /// <param name="prog">The integer ID of the shader program</param>
        public static bool BindWithProgram(int prog)
        {
            if (!GL.IsProgram(prog) || programLocations.ContainsKey(prog)) return false;

            FogParamsLocations locations = new FogParamsLocations()
            {
                Enabled = GL.GetUniformLocation(prog, "gFogParams.Enabled" ),
                Color = GL.GetUniformLocation(prog, "gFogParams.Color"),
                Start = GL.GetUniformLocation(prog, "gFogParams.Start"),
                End = GL.GetUniformLocation(prog, "gFogParams.End"),
                Density = GL.GetUniformLocation(prog, "gFogParams.Density"),
                FogType = GL.GetUniformLocation(prog, "gFogParams.FogType"),
            };

            //This wouldn't be correct if it only has _some_ of the locations, but if you're implementing fog in your shader, you either remember or you don't
            if (locations.Color + locations.Start + locations.End + locations.Density + locations.FogType <= 0) return false;

            programLocations.Add(prog, locations);
            return true;
        }

        /// <summary>
        /// Set the fog parameters
        /// </summary>
        /// <param name="parameters">The <code>FogParams</code> parameters to set.</param>
        public static void SetFogParameters(FogParams parameters)
        {
            FogParameters = parameters;
        }


        /// <summary>
        /// Set the color of the fog
        /// </summary>
        /// <param name="color">The new fog color</param>
        public static void SetColor(Vector3 color)
        {
            FogParameters.Color = color;
        }

        /// <summary>
        /// Set the starting point of a linear fog type
        /// </summary>
        /// <param name="start">How far away from the camera to start</param>
        public static void SetStart(float start)
        {
            FogParameters.Start = start;
        }
        /// <summary>
        /// Set the ending point of a linear fog type
        /// </summary>
        /// <param name="end">How far away from the camera to end</param>
        public static void SetEnd(float end)
        {
            FogParameters.End = end;
        }

        /// <summary>
        /// Set both the start and end points of the fog.
        /// </summary>
        /// <param name="start">How far away from the camera to start</param>
        /// <param name="end">How far away from the camera to end</param>
        public static void SetStartEnd(float start, float end)
        {
            FogParameters.Start = start;
            FogParameters.End = end;
        }

        /// <summary>
        /// Set the density of either the <code>FogType.Exp</code> and <code>FogType.Exp2</code> fog types
        /// </summary>
        /// <param name="density">How 'dense' the fog should get</param>
        public static void SetDensity(float density)
        {
            FogParameters.Density = density;
        }

        /// <summary>
        /// Set the type of fog to use
        /// </summary>
        /// <param name="type">The new type of fog</param>
        public static void SetFogType(FogParams.FogType type)
        {
            FogParameters.Type = type;
        }

        /// <summary>
        /// Call to update a program's fog uniforms with the current fog settigs
        /// NOTE: This function assumes you have already bound your program! This is to prevent needless shader switching
        /// </summary>
        /// <param name="prog">The integer ID of the shader program</param>
        public static void UpdateUniforms( int prog )
        {
            FogParamsLocations locs;
            if (!programLocations.TryGetValue(prog, out locs)) return;

            GL.Uniform1(locs.Enabled, Enabled ? 1 : 0);
            GL.Uniform3(locs.Color, FogParameters.Color);
            GL.Uniform1(locs.Start, FogParameters.Start);
            GL.Uniform1(locs.End, FogParameters.End);
            GL.Uniform1(locs.Density, FogParameters.Density);
            GL.Uniform1(locs.FogType, (int)FogParameters.Type);
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

    public class FogParams
    {
        /// <summary>
        /// Whether to draw fog or not
        /// </summary>
        public bool Enabled;
        /// <summary>
        /// The color of the fog
        /// </summary>
        public Vector3 Color;
        /// <summary>
        /// How far in front of the camera to start the fog
        /// This is for the <code>FogType.Linear</code> mode only
        /// </summary>
        public float Start;
        /// <summary>
        /// How far in front of the camera to end the fog
        /// This is for the <code>FogType.Linear</code> mode only
        /// </summary>
        public float End;
        /// <summary>
        /// The maximum density the fog will reach
        /// This is for the <code>FogType.Exp</code> and <code>FogType.Exp2</code> modes only
        /// </summary>
        public float Density;

        /// <summary>
        /// Enumeration outlying the different types of fog
        /// </summary>
        public enum FogType
        {
            Linear  = 0,
            Exp     = 1,
            Exp2    = 2
        }

        /// <summary>
        /// The type of fog to use
        /// </summary>
        public FogType Type;
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

    public struct FogParamsLocations
    {
        public int Enabled;
        public int Color;
        public int Start;
        public int End;
        public int Density;
        public int FogType;
    }

    #endregion
}
