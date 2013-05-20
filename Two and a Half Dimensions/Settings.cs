using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine
{
    public class Settings
    {
        //General settings
        public VSyncMode VSync = VSyncMode.Off;
        public WindowState WindowMode = WindowState.Normal;
        public bool NoBorder = false;
        public int Width = 1024;
        public int Height = 768;

        //Rendering settings
        public int ShadowMapSize = 1024;
        public int Samples = 8;

        //Debug settings
        public bool ShowFPS = false;

        public Settings()
        {
        }

        //Try loading settings from a file
        public Settings(string filename)
        {

        }

        //Try saving settings to a file
        public void Save()
        {

        }
    }
}
