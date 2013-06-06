﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using OlegEngine;

namespace Balance
{
    class Program : GameWindow
    {
        public Engine engine;
        public Program()
            : base(1350, 680, new GraphicsMode(32, 24, 0, 4), "Balance the ball! (Wowzers)", GameWindowFlags.Default)
        {
            VSync = VSyncMode.Adaptive;
            engine = new Engine(this); //Create the engine class that'll do all the heavy lifting
            engine.OnRenderSceneOpaque += new Action<FrameEventArgs>(RenderSceneOpaque);
            engine.OnRenderSceneTranslucent += new Action<FrameEventArgs>(RenderSceneTranslucent);
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            engine.OnLoad(e);

            BalanceLevel.Initialize();
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
            engine.OnResize(e);
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            engine.OnUpdateFrame(e);
            BalanceLevel.Think();

            if (this.Keyboard[Key.Escape]) this.Exit();
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            engine.OnRenderFrame(e);

            SwapBuffers();
        }

        /// <summary>
        /// Called by the engine when it is time to render opaque renderables
        /// </summary>
        /// <param name="e"></param>
        private void RenderSceneOpaque(FrameEventArgs e)
        {
            //Draw opaque geometry
            BalanceLevel.Draw();
            OlegEngine.Entity.EntManager.DrawOpaque(e);

            //Draw debug stuff
            Graphics.DrawDebug();
        }

        /// <summary>
        /// Called by the engine when it is time to render potentially-translucent renderables
        /// </summary>
        /// <param name="e"></param>
        void RenderSceneTranslucent(FrameEventArgs e)
        {
            //Now draw geometry that is potentially transcluent
            GL.Enable(EnableCap.Blend);
            OlegEngine.Entity.EntManager.DrawTranslucent(e);
            GL.Disable(EnableCap.Blend);
        }

        [STAThread]
        static void Main(string[] args)
        {
            using (Program game = new Program())
            {
                game.Run(60.0);
            }
        }
    }
}
