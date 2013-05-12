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
    }


    public class Engine
    {
        public GameWindow WindowContext;
        public FBO shadowFBO = new FBO();
        public Matrix4 camMat = Matrix4.Identity;
        public event Action<FrameEventArgs> OnRenderScene;
        public event Action OnSceneResize;

        private Matrix4 defaultViewMatrix = Matrix4.Identity;
        private Matrix4 defaultOrthoMatrix = Matrix4.Identity;

        public Engine(GameWindow window )
        {
            this.WindowContext = window;
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        public void OnLoad(EventArgs e)
        {
            //Print useful information about the card
            Console.WriteLine("==================================");
            Console.WriteLine("Vendor: {0}", GL.GetString(StringName.Vendor));
            Console.WriteLine("Renderer: {0}", GL.GetString(StringName.Renderer));
            Console.WriteLine(GL.GetString(StringName.ShadingLanguageVersion));
            string versionOpenGL = GL.GetString(StringName.Version);
            GLVersion.Major = versionOpenGL[0];
            GLVersion.Minor = versionOpenGL[2];
            Console.WriteLine("OpenGL version: {0}.{1}", GLVersion.Major, GLVersion.Minor);
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
            float ratio = this.WindowContext.Width / (float)this.WindowContext.Height;
            shadowFBO.Init(this.WindowContext.Width, this.WindowContext.Height);

            //Initalize lighting
            LightingTechnique.Init();
            LightingTechnique.SetShadowTexture(shadowFBO.shadowMap);

            //Initialize our shadows
            ShadowTechnique.Init();
            ShadowTechnique.Enable();

            //Initialize skybox
            SkyboxTechnique.Init();

            //Initalize our gui system
            GUI.GUIManager.Init();

            //Create a camera matrix
            Vector3 point = new Vector3((float)Math.Cos(-0.6f), (float)Math.Sin(-0.52) - 0.21f, (float)Math.Sin(-0.6));
            camMat = Matrix4.LookAt(new Vector3(88.94199f, 22.27345f, 5.085441f) + new Vector3(0, 1, 0), new Vector3(88.94199f, 22.27345f, 5.085441f) + point + new Vector3(0, 1, 0), Vector3.UnitY);

            //Create some debug stuff
            Graphics.Init();
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
            //Reset the view matrix, just in case it's been altered
            Utilities.ViewMatrix = defaultViewMatrix;

            //Get the positions for all the light positions that'll cast fancyshadows
            ShadowTechnique.UpdateLightPositions();

            //Set them accordingly
            //TODO: make it support multiple points (and optimize the hell out of it)
            ShadowInfo info = ShadowTechnique.GetShadowInfo();
            shadowFBO.Enabled = ShadowTechnique.Enabled;
            ShadowTechnique.SetLightInfo(info);
            if (ShadowTechnique.Enabled && ShadowTechnique._lights.Count > 0)
            {
                Utilities.ProjectionMatrix = info.matrix;

                //Pass 1, render in the view of the light
                Utilities.CurrentPass = 1;
                shadowFBO.BindForWriting();
                GL.Clear(ClearBufferMask.DepthBufferBit);
                RenderScene(e);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Reset it to the default framebuffer



            //Set the view to the normal camera
            //Utilities.ProjectionMatrix = ply.camMatrix;
            Utilities.ProjectionMatrix = View.CameraMatrix;

            //Second pass, render normally
            Utilities.CurrentPass = 2;
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shadowFBO.BindForReading();
            LightingTechnique.Render();
            RenderScene(e);

            //Draw the skybox
            SkyboxTechnique.Render();

            //Draw surface stuff
            Utilities.ViewMatrix = defaultOrthoMatrix;
            Utilities.ProjectionMatrix = Matrix4.Identity;

            GUI.GUIManager.Draw();
        }

        private void RenderScene(FrameEventArgs e)
        {
            if (this.OnRenderScene != null)
            {
                this.OnRenderScene(e);
            }
        }
    }
}
