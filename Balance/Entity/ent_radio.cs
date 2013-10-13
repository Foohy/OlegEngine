using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OlegEngine;
using OlegEngine.Entity;

namespace Balance.Entity
{
    class ent_radio : BaseEntity
    {
        Audio Song;
        public bool IsCorrupted = false;
        public override void Init()
        {
            this.SetModel(Resource.GetMesh("radio.obj"));
            this.Material = Resource.GetMaterial("engine/white");

            Song = Audio.LoadSong("Resources/Audio/radio.mp3", true, true, this);
            //Song.Play(true);
        }

        public void Corrupt()
        {
            Song.SetFrequency((float)Utilities.Rand.NextDouble( 22000, 44100));

            if (!this.IsCorrupted)
            {
                this.IsCorrupted = true;

                Audio.PlaySound("Resources/Audio/rift_bg.wav");
            }
        }
    }
}
