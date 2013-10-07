using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using OlegEngine;

namespace Gravity_Car
{
    class Program : Engine
    {
        public const string SettingsFile = "settings.cfg";

        [STAThread]
        static void Main(string[] args)
        {
            using (Program game = new Program(new Settings(SettingsFile)))
            {
                game.Run(60.0);
            }
        }

        public Program()
            : base()
        {
        }

        public Program(Settings settings)
            : base( settings )
        {
        }

        public Program(Settings settings, string title)
            : base(settings, title)
        {
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Levels.LevelManager.InitalizeLevel(new Levels.Level1());
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            Levels.LevelManager.Think(e);
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            SwapBuffers();
        }

        protected override void RenderSceneOpaque(FrameEventArgs e)
        {
            //Draw opaque geometry
            Levels.LevelManager.Draw(e);
            OlegEngine.Entity.EntManager.DrawOpaque(e);

            //Call into the engine so it can do it's thing
            base.RenderSceneOpaque(e);

            //Draw debug stuff
            Graphics.DrawDebug();
        }

        protected override void RenderSceneTranslucent(FrameEventArgs e)
        {
            //Now draw geometry that is potentially transcluent
            Graphics.EnableBlending(true);
            OlegEngine.Entity.EntManager.DrawTranslucent(e);

            //Call into the engine so it can do it's thing
            base.RenderSceneTranslucent(e);

            Graphics.EnableBlending(false);
        }
    }
}
