using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Newtonsoft.Json;

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
        public bool EnableShadows = true;
        public int ShadowMapSize = 1024;
        public int Samples = 4;
        public int AnisotropicFiltering = 4;

        //Audio settings
        public float GlobalVolume = 1.0f;

        //Debug settings
        public bool ShowFPS = false;

        public Settings()
        {
        }

        //Try loading settings from a file
        public Settings(string filename)
        {
            if (!File.Exists(filename))
            {
                Save(filename); //Create the default settings file
            }
            else
            {
                try
                {
                    Settings s = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(filename));
                    System.Reflection.FieldInfo[] fields = s.GetType().GetFields();

                    foreach (var field in fields)
                    {
                        this.GetType().GetField(field.Name).SetValue(this, field.GetValue(s));
                    }
                }
                catch (Exception e)
                {
                    Utilities.Print("Failed to load {0}!\n\t {1}", Utilities.PrintCode.ERROR, filename, e.Message);
                }
            }
        }

        //Try saving settings to a file
        public void Save( string filename )
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filename, json);
            }
            catch (Exception e)
            {
                Utilities.Print("Failed to save {0}!\n\t {1}", Utilities.PrintCode.ERROR, filename, e.Message);
            }

        }
    }
}
