using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Two_and_a_Half_Dimensions
{

    struct GLVersion
    {
        static public int Major;
        static public int Minor;
    }


    class Program : GameWindow 
    {
        public Player ply;
        public FBO shadowFBO = new FBO();
        public Matrix4 camMat = Matrix4.Identity;
        public LightingTechnique effect = new LightingTechnique();
        public SkyboxTechnique skybox = new SkyboxTechnique();
        public ShadowTechnique shadows = new ShadowTechnique();

        private Matrix4 defaultViewMatrix = Matrix4.Identity;
        private Matrix4 defaultOrthoMatrix = Matrix4.Identity;
        private GUI.font counter;
        public Program()
            : base(1900, 900, new GraphicsMode(32, 24, 0, 4), "BY NO MEANS.", GameWindowFlags.Default) //GraphicsMode(32, 24, 0, 4)
        {
            VSync = VSyncMode.Adaptive;
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Print useful information about the card
            Console.WriteLine("==================================");
            Console.WriteLine("Vendor: {0}", GL.GetString(StringName.Vendor));
            Console.WriteLine(", Renderer: {0}", GL.GetString(StringName.Renderer));
            Console.WriteLine(GL.GetString(StringName.ShadingLanguageVersion));
            string versionOpenGL = GL.GetString(StringName.Version);
            GLVersion.Major = versionOpenGL[0];
            GLVersion.Minor = versionOpenGL[2];
            Console.WriteLine("OpenGL version: {0}.{1}", GLVersion.Major, GLVersion.Minor);
            Console.WriteLine("==================================");

            Utilities.Init(this);
            Audio.Init();

            float[] fogColor = { 0.18431f, 0.1764f, 0.22745f };//0.18431372549019607843137254901961
            //float[] fogColor = { 1.0f, 0.55f, 0.0f };
            GL.ClearColor(fogColor[0], fogColor[1], fogColor[2], 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

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
            float ratio = this.Width / (float)this.Height;
            shadowFBO.Init(this.Width, this.Height);

            //Initalize lighting
            effect.Init();
            effect.SetShadowTexture(shadowFBO.shadowMap);

            //Initialize our shadows
            shadows.Init();
            shadows.Enable();

            //Initialize skybox
            skybox.Init();

            ply = new Player(this, new Vector3(88.94199f, 23.27345f, 5.085441f));
            Levels.LevelManager.InitalizeLevel(new Levels.Level1());

            //Create a camera matrix
            Vector3 point = new Vector3((float)Math.Cos(-0.6f), (float)Math.Sin(-0.52) - 0.21f, (float)Math.Sin(-0.6));
            camMat = Matrix4.LookAt(new Vector3(88.94199f, 22.27345f, 5.085441f) + new Vector3(0, 1, 0), new Vector3(88.94199f, 22.27345f, 5.085441f) + point + new Vector3(0, 1, 0), Vector3.UnitY);

            //Create our little FPS counter
            counter = new GUI.font("debug", "frick off");
            GUI.GUIManager.PostDrawHUD += new GUI.GUIManager.OnDrawHUD(GUIManager_PostDrawHUD);
        }

        double last = 0.0d;
        string fps = "frasd";
        
        void GUIManager_PostDrawHUD(EventArgs e)
        {        
            if (last < Utilities.Time)
            {
                last = Utilities.Time + 0.5;
                int FPS = (int)(1 / this.RenderPeriod);

                fps = (this.RenderPeriod * 1000).ToString() + " ms (" + FPS+ " FPS)";
                counter.SetText(fps);

                counter.SetColor(0, 1, 0);
                if (FPS < 45)
                {
                    counter.SetColor(1, 1, 0);
                }
                if (FPS < 30)
                {
                    counter.SetColor(1, 0, 0);
                }
            }
            

            counter.Draw();
        }

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            defaultViewMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 256.0f);
            //defaultOrthoMatrix = Matrix4.CreateOrthographic(Width, Height, 1.0f, 256.0f);
            defaultOrthoMatrix = Matrix4.CreateOrthographicOffCenter(0, this.Width, this.Height, 0, 1.0f, 256f);
            Utilities.ViewMatrix = defaultViewMatrix;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref defaultViewMatrix);
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            Utilities.Think(e);

            if (Keyboard[Key.Escape])
                Exit();
            Input.Think(this, e);
            Audio.Think(e);
            ply.Think(e);
            Levels.LevelManager.Think(e);
            Entity.EntManager.Think(e);

        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        Matrix4 plyView = Matrix4.Identity;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //Reset the view matrix, just in case it's been altered
            Utilities.ViewMatrix = defaultViewMatrix;

            //Get the positions for all the light positions that'll cast fancyshadows
            shadows.UpdateLightPositions();

            //Set them accordingly
            //TODO: make it support multiple points (and optimize the hell out of it)
            ShadowInfo info = shadows.GetShadowInfo();

            if (shadows.Enabled)
            {
                shadows.SetLightInfo(info);
                plyView = info.matrix;
                Utilities.ProjectionMatrix = info.matrix;

                //Pass 1, render in the view of the light
                Utilities.CurrentPass = 1;
                shadowFBO.BindForWriting();
                GL.Clear(ClearBufferMask.DepthBufferBit);
                RenderScene(e);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Reset it to the default framebuffer
            }




            //Set the view to the normal camera
            plyView = ply.camMatrix;
            Utilities.ProjectionMatrix = ply.camMatrix;

            //Second pass, render normally
            Utilities.CurrentPass = 2;
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shadowFBO.BindForReading();
            Player.ply.Draw(e);
            effect.Render();
            RenderScene(e);

            //Draw the skybox
            skybox.Render();

            //Draw surface stuff
            Utilities.ViewMatrix = defaultOrthoMatrix;
            Utilities.ProjectionMatrix = Matrix4.Identity;
            GUI.GUIManager.Draw();
            SwapBuffers();
        }

        private void RenderScene(FrameEventArgs e)
        {
            //Draw stuff
            Levels.LevelManager.Draw(e);
            Entity.EntManager.Draw(e);
            ply.Draw(e);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (Program game = new Program())
            {
                game.Run(60.0);
            }
        }
    }
}
