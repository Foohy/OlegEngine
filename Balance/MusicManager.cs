using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using OlegEngine;

using Un4seen.Bass;

namespace Balance
{
    class MusicManager
    {
    }

    class MusicMix : ICollection<Audio>
    {
        //Private inner collection of meshes
        private List<Audio> Songs = new List<Audio>();
        public float Percent { get; private set; }
        public float Volume { get; private set; }

        public static MusicMix Create(List<Audio> musicList)
        {
            //Check if all of the contents are valid
            foreach( Audio song in musicList )
            {
                if (song == null || song.Handle == 0 )
                    return null;
            }

            return new MusicMix(musicList);
        }

        public static MusicMix Create(string location, string prefix, string filetype)
        {
            List<Audio> LoadedSongs = new List<Audio>();
            //Create the regex pattern
            string regpattern = prefix + @"(\d)" + filetype;

            //Load the files
            string[] files = System.IO.Directory.GetFiles(location);
            foreach (string file in files)
            {
                Match m = Regex.Match(file, regpattern);

                if (m.Success)
                {
                    int index = -1;
                    int.TryParse(m.Groups[1].Value, out index);

                    if (index >= 0)
                    {
                        Audio song = Audio.LoadSong( file, true);
                        LoadedSongs.Insert(index, song);
                    }
                }
            }

            return new MusicMix(LoadedSongs);
        }

        public MusicMix(List<Audio> musicList)
        {
            Songs = musicList;
            Percent = 0;
            Volume = 1.0f;

            SetPercent(this.Percent);
        }

        public void SetPercent( float newperc )
        {
            Percent = Utilities.Clamp(newperc, 1, 0);

            //Update the total volume if we're in the middle of two songs so there isn't a quiet zone
            float songInterval = 1 / (float)(Songs.Count - 1);
            float smallestPerc = Percent % songInterval;
            float fixedVol = 1 - Math.Abs(smallestPerc - (songInterval / 2));

            //for each track adjust the volume according to the distance from the selected percent
            for (int i = 0; i < Songs.Count; i++)
            {
                float placement = (float)i / (float)(Songs.Count - 1);
                float distancevolume = Utilities.Clamp(1 - Math.Abs((placement - Percent) * (Songs.Count - 1)), 1, 0);

                Bass.BASS_ChannelSetAttribute(Songs[i].Handle, BASSAttribute.BASS_ATTRIB_VOL, distancevolume * fixedVol * Volume);
            }
        }

        public void Play( bool FromBeginning )
        {
            for (int i = 0; i < Songs.Count; i++)
            {
                Songs[i].Play(FromBeginning);
            }
        }

        public void Pause()
        {
            for (int i = 0; i < Songs.Count; i++)
            {
                Songs[i].Pause();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < Songs.Count; i++)
            {
                Songs[i].Stop();
            }
        }

        public void SetVolume(float volume)
        {
            Volume = volume;
            SetPercent(this.Percent);
        }

        public IEnumerator<Audio> GetEnumerator()
        {
            return Songs.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool isRO = false;

        public bool Contains(Audio item)
        {
            return Songs.Contains(item);
        }

        public bool Contains(Audio item, EqualityComparer<Audio> comp)
        {
            return Songs.Contains(item, comp);
        }

        public void Add( Audio m)
        {
            Songs.Add(m);
        }

        public void Clear()
        {
            Songs.Clear();
        }

        public void CopyTo(Audio[] array, int arrayIndex)
        {
            Songs.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return Songs.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return isRO; }
        }

        public bool Remove(Audio item)
        {
            return Songs.Remove(item);
        }
    }
}
