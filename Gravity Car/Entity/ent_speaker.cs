using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using OlegEngine;
using OlegEngine.Entity;

namespace Gravity_Car.Entity
{
    class ent_speaker : BaseEntity 
    {
        float startTime = 0;
        float syncedTime = 0;
        bool started = false;
        Audio song;
  
        public override void Init()
        {
            this.Mat = Resource.GetMaterial("models/props/speaker");
            this.Mat.Properties.SpecularPower = 32.0f;
            this.Mat.Properties.SpecularIntensity = 1.0f;
            this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.Model = Resource.GetMesh("speaker.obj");
            //this.Model = ObjLoader.LoadFile("Resources/Models/cow.obj");            
            song = Audio.LoadSong("Resources/Audio/Brodyquest.mp3", true, true, this );

            startTime = (float)Utilities.Time + 1.0f;
            //this.DisableLighting = true;
        }

        public override void Think()
        {
            if (Utilities.Time > startTime && !started)
            {
                started = true;
                song.Play(true);
                Console.WriteLine("BOOM");
                //syncedTime = 1f;
            }
            if (started)
            {
                syncedTime += (float)Utilities.ThinkTime;
            }
            //Thomas the tank engine: 10.04
            //Brodyquest: 10.7
            float scale = (float)Math.Sin(syncedTime * 10.28);
            scale = Utilities.Clamp(scale, 1, 0.90f) - .3f;
            this.Scale = new Vector3(scale, scale, scale);
        }
    }
}
