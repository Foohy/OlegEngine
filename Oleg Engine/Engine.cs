﻿using System;
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


    public class Engine
    {
        public GameWindow WindowContext;
        public FBO shadowFBO;
        public event Action<FrameEventArgs> OnRenderSceneOpaque;
        public event Action<FrameEventArgs> OnRenderSceneTranslucent;
        public event Action OnSceneResize;

        public Matrix4 defaultViewMatrix = Matrix4.Identity;
        public Matrix4 defaultOrthoMatrix = Matrix4.Identity;

        private DropOutStack<double> AveragedFrametimes = new DropOutStack<double>( 30 );

        public Engine(GameWindow window)
        {
            this.WindowContext = window;
            if (Utilities.EngineSettings == null) Utilities.EngineSettings = new Settings();
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        public void OnLoad(EventArgs e)
        {
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

            Utilities.Init(this.WindowContext, this);
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

            //Bind some textures to our default framebuffer
            //Utilities.ScreenDepthTex = FBO.BindTextureToFBO(0, this.WindowContext.Width, this.WindowContext.Height, PixelInternalFormat.DepthComponent, PixelFormat.DepthComponent, FramebufferAttachment.DepthAttachment);
            //Utilities.ScreenTex = FBO.BindTextureToFBO(0, this.WindowContext.Width, this.WindowContext.Height, PixelInternalFormat.Rgba, PixelFormat.Rgba, FramebufferAttachment.ColorAttachment1);
            
            //Initalize lighting
            LightingTechnique.Init();
            LightingTechnique.SetShadowTexture(shadowFBO.RenderTexture);

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
        public void OnResize(EventArgs e)
        {
            
            float FOV = (float)Math.PI / 4;
            float Ratio = this.WindowContext.Width / (float)this.WindowContext.Height;

            GL.Viewport(this.WindowContext.ClientRectangle.X, this.WindowContext.ClientRectangle.Y, this.WindowContext.ClientRectangle.Width, this.WindowContext.ClientRectangle.Height);
            defaultViewMatrix = Matrix4.CreatePerspectiveFieldOfView(FOV, Ratio, Utilities.NearClip, Utilities.FarClip);
            //defaultOrthoMatrix = Matrix4.CreateOrthographic(Width, Height, 1.0f, 256.0f);
            defaultOrthoMatrix = Matrix4.CreateOrthographicOffCenter(0, this.WindowContext.Width, this.WindowContext.Height, 0, Utilities.NearClip, Utilities.FarClip);
            Utilities.ViewMatrix = defaultViewMatrix;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref defaultViewMatrix);

            Graphics.ViewFrustum.SetCamInternals(FOV, Ratio, Utilities.NearClip, Utilities.FarClip);

            if (OnSceneResize != null)
                OnSceneResize();
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        public void OnUpdateFrame(FrameEventArgs e)
        {
            Utilities.Think(e);

            Input.Think(this.WindowContext, e);
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
        public void OnRenderFrame(FrameEventArgs e)
        {
            //Slow the heck down when we're not in focus
            if (!WindowContext.Focused) System.Threading.Thread.Sleep(33);

            //Update timing information
            Utilities.Draw(e);

            //Reset the view matrix, just in case it's been altered
            Utilities.ViewMatrix = defaultViewMatrix;

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
                Utilities.ViewMatrix = defaultViewMatrix;

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
            Utilities.ViewMatrix = defaultViewMatrix;

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
            Utilities.ViewMatrix = defaultOrthoMatrix;
            Utilities.ProjectionMatrix = Matrix4.Identity;

            GUI.GUIManager.Draw();
        }

        private void RenderSceneOpaque(FrameEventArgs e)
        {
            if (this.OnRenderSceneOpaque != null)
            {
                this.OnRenderSceneOpaque(e);
            }
        }

        private void RenderSceneTranslucent(FrameEventArgs e)
        {
            if (this.OnRenderSceneTranslucent != null)
            {
                this.OnRenderSceneTranslucent(e);
            }
        }
    }
}
