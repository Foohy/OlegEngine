using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OlegEngine
{

    public struct GLVersion
    {
        static public int Major;
        static public int Minor;

        public static string SimpleString()
        {
            return string.Format("{0}.{1}", GLVersion.Major, GLVersion.Minor);
        }
    }


    public class Engine : GameWindow 
    {
        public FBO shadowFBO;
        public event Action<FrameEventArgs> OnRenderSceneOpaque;
        public event Action<FrameEventArgs> OnRenderSceneTranslucent;
        public event Action OnSceneResize;

        private DropOutStack<double> AveragedFrametimes = new DropOutStack<double>( 30 );

        public Engine()
            : base( 1024, 768, new GraphicsMode(32, 24, 0, 4), typeof( Engine ).Assembly.GetName().Name, GameWindowFlags.Default )
        {
            var settings = new Settings();
            settings.Width = this.Width;
            settings.Height = this.Height;
            settings.Samples = this.Context.GraphicsMode.Samples;
            settings.WindowMode = this.WindowState;

            _engineInit(settings);
        }

        public Engine(Settings engineSettings)
            : base(engineSettings.Width, engineSettings.Height, new GraphicsMode(32, 24, 0, engineSettings.Samples), typeof(Engine).Assembly.GetName().Name, engineSettings.WindowMode == WindowState.Fullscreen && engineSettings.NoBorder ? GameWindowFlags.Fullscreen : GameWindowFlags.Default)
        {
            _engineInit(engineSettings);
        }

        public Engine(Settings engineSettings, string title)
            : base(engineSettings.Width, engineSettings.Height, new GraphicsMode(32, 24, 0, engineSettings.Samples), title, engineSettings.WindowMode == WindowState.Fullscreen && engineSettings.NoBorder ? GameWindowFlags.Fullscreen : GameWindowFlags.Default)
        {
            _engineInit(engineSettings);
        }

        private void _engineInit(Settings engineSettings)
        {
            Utilities.Print("==================================", Utilities.PrintCode.INFO);
            Utilities.Print("OLEG ENGINE \n{0}", Utilities.PrintCode.INFO, typeof(Engine).Assembly.GetName().Version.ToString());
            Utilities.Print("==================================\n", Utilities.PrintCode.INFO);

            //Store the current settings
            Utilities.EngineSettings = engineSettings;

            //Toggle VSync
            this.VSync = engineSettings.VSync;

            //Noborder if applicable
            if (engineSettings.NoBorder)
                this.WindowBorder = OpenTK.WindowBorder.Hidden;

            //If we don't want noborder and we're set to fullscreen, change our windowmode to whatever
            if (!engineSettings.NoBorder && engineSettings.WindowMode == OpenTK.WindowState.Fullscreen)
                this.WindowState = engineSettings.WindowMode;
            //BUT if we're set to noborder, require the windowstate is 'normal'
            else if (engineSettings.NoBorder)
                this.WindowState = OpenTK.WindowState.Normal;

            //Hook into some engine callbacks
            //this.OnRenderSceneOpaque += new Action<FrameEventArgs>(RenderSceneOpaque);
            //this.OnRenderSceneTranslucent += new Action<FrameEventArgs>(RenderSceneTranslucent);

            //Make a furious attempt to change the window's icon
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.AppDomain.CurrentDomain.FriendlyName); }
            catch (Exception e) { Utilities.Print("Failed to load icon! {0}", Utilities.PrintCode.WARNING, e.Message); }

            //Hide the console if we want
            if (!engineSettings.ShowConsole)
                ConsoleManager.ShowWindow(ConsoleManager.GetConsoleWindow(), ConsoleManager.SW_HIDE);
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            //Base
            base.OnLoad(e);

            if (Utilities.EngineSettings == null) Utilities.EngineSettings = new Settings();

            //Print useful information about the card
            Console.WriteLine("==================================");
            Console.WriteLine("Vendor: {0}", GL.GetString(StringName.Vendor));
            Console.WriteLine("Renderer: {0}", GL.GetString(StringName.Renderer));
            Console.WriteLine("GLSL Version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
            string versionOpenGL = GL.GetString(StringName.Version);
            GLVersion.Major = (int)Char.GetNumericValue(versionOpenGL[0]);
            GLVersion.Minor = (int)Char.GetNumericValue(versionOpenGL[2]);
            Console.WriteLine("OpenGL version: {0}", versionOpenGL);

            if (GLVersion.Major < 3 || (GLVersion.Major == 3 && GLVersion.Minor < 2))
            {
                Utilities.Print("You graphics card is on an older version of OpenGL ({0}). This application uses OpenGL 3.2. Good luck!", Utilities.PrintCode.WARNING, GLVersion.SimpleString() );
            }
            Console.WriteLine("==================================");

            Utilities.Init(this);
            Audio.Init();

            float[] fogColor = { 0.18431f, 0.1764f, 0.22745f };//0.18431372549019607843137254901961
            //float[] fogColor = { 1.0f, 0.55f, 0.0f };
            GL.ClearColor(fogColor[0], fogColor[1], fogColor[2], 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.AlphaFunc(AlphaFunction.Greater, 0.1f);

            //Fog
            GL.Fog(FogParameter.FogStart, 30.0f);
            GL.Fog(FogParameter.FogEnd, 250.0f);
            GL.Fog(FogParameter.FogColor, fogColor);
            GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
            //GL.Enable(EnableCap.Fog);

            //Textures
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthClamp);

            //Initialize our shadow FBO
            shadowFBO = new FBO(Utilities.EngineSettings.ShadowMapSize, Utilities.EngineSettings.ShadowMapSize);
            //Change some specific texture parameters
            GL.BindTexture(TextureTarget.Texture2D, shadowFBO._RT);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            //Bind some textures to our default framebuffer
            //Utilities.ScreenDepthTex = FBO.BindTextureToFBO(0, this.WindowContext.Width, this.WindowContext.Height, PixelInternalFormat.DepthComponent, PixelFormat.DepthComponent, FramebufferAttachment.DepthAttachment);
            //Utilities.ScreenTex = FBO.BindTextureToFBO(0, this.WindowContext.Width, this.WindowContext.Height, PixelInternalFormat.Rgba, PixelFormat.Rgba, FramebufferAttachment.ColorAttachment1);
            
            //Initalize lighting
            LightingTechnique.Init();

            //Initialize our shadows
            ShadowTechnique.Init();
            ShadowTechnique.Enabled = Utilities.EngineSettings.EnableShadows;

            //Initialize skybox
            SkyboxTechnique.Init();

            //Initalize our gui system
            GUI.GUIManager.Init();

            //Create some debug stuff
            Graphics.Init();

            //Let's have some built in debug out stuff
            GUI.GUIManager.PostDrawHUD += new GUI.GUIManager.OnDrawHUD(GUIManager_PostDrawHUD);
        }

        void GUIManager_PostDrawHUD(EventArgs e)
        {

            if (Utilities.EngineSettings.ShowFPS)
            {
                AveragedFrametimes.Push(Utilities.Frametime);

                double frametime = 0;
                for (int i = 0; i < AveragedFrametimes.Count; i++)
                {
                    frametime += AveragedFrametimes.Value(i);
                }

                frametime = frametime / (double)AveragedFrametimes.Count;
                GUI.Surface.SetDrawColor(255, 255, 255);
                GUI.Surface.DrawSimpleText("debug", string.Format("FPS: {0,3:N0} ({1:0.000}ms)", 1 / frametime, frametime * 1000), 10, 10);
            }
        }


        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            //Base
            base.OnResize(e);

            View.UpdateViewOrthoMatrices();

            if (OnSceneResize != null)
                OnSceneResize();
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //Base
            base.OnUpdateFrame(e);

            Utilities.Think(e);

            Input.Think(this, e);
            Audio.Think(e);

            //Update the player's view so we know where to render
            View.Think(e); 

            //Levels.LevelManager.Think(e);
            Entity.EntManager.Think(e);

        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Base
            base.OnRenderFrame(e);

            //Slow the heck down when we're not in focus
            if (!this.Focused) System.Threading.Thread.Sleep(33);

            //Update timing information
            Utilities.Draw(e);

            //Reset the view matrix, just in case it's been altered
            Utilities.ViewMatrix = View.ViewMatrix;

            //Get the positions for all the light positions that'll cast fancyshadows
            ShadowTechnique.UpdateLightPositions();

            //Set them accordingly
            //TODO: make it support multiple points (and optimize the hell out of it)
            ShadowInfo info = ShadowTechnique.GetShadowInfo();
            shadowFBO.Enabled = ShadowTechnique.Enabled;
            ShadowTechnique.SetLightInfo(info);
            if (ShadowTechnique.Enabled && shadowFBO.Loaded && ShadowTechnique._lights.Count > 0 && Utilities.EngineSettings.EnableShadows)
            {
                Utilities.ProjectionMatrix = info.matrix;
                Utilities.ViewMatrix = View.ViewMatrix;

                //Pass 1, render in the view of the light
                Utilities.CurrentPass = 1;

                //Reverse the culling mode to make shadowmaps easier
                GL.CullFace(CullFaceMode.Front);

                //Insert a blank texture for where the shadowmap is bound just for rendering (fixes artifacts on AMD cards)
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, Utilities.White);

                shadowFBO.BindForWriting();
                GL.Clear(ClearBufferMask.DepthBufferBit);
                RenderSceneOpaque(e);
                RenderSceneTranslucent(e);

                //Change our renderer back to the default framebuffer/size
                FBO.ResetFramebuffer();
            }

            //Bind the shadow map into its texture slot
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, shadowFBO.RenderTexture);
            GL.ActiveTexture(TextureUnit.Texture0);

            //Set the view to the normal camera
            Utilities.ProjectionMatrix = View.CameraMatrix;
            Utilities.ViewMatrix = View.ViewMatrix;

            //Second pass, render normally
            Utilities.CurrentPass = 2;
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.CullFace(CullFaceMode.Back);

            LightingTechnique.Render();
            RenderSceneOpaque(e);

            //Draw the skybox
            SkyboxTechnique.Render();

            //Draw potentially-translucent renderables after all that other stuff
            RenderSceneTranslucent(e);

            //Draw surface stuff
            Utilities.ViewMatrix = View.OrthoMatrix;
            Utilities.ProjectionMatrix = Matrix4.Identity;

            GUI.GUIManager.Draw();
        }

        protected virtual void RenderSceneOpaque(FrameEventArgs e)
        {
            if (this.OnRenderSceneOpaque != null)
            {
                this.OnRenderSceneOpaque(e);
            }
        }

        protected virtual void RenderSceneTranslucent(FrameEventArgs e)
        {
            if (this.OnRenderSceneTranslucent != null)
            {
                this.OnRenderSceneTranslucent(e);
            }
        }
    }
}
