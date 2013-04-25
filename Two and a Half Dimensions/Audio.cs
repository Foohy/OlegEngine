using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Un4seen.Bass;

namespace Two_and_a_Half_Dimensions
{
    class Audio
    {
        public int Handle { get; private set; }
        public string Filename { get; private set; }
        public Entity.BaseEntity attachedEnt = null;
        public bool Positional = false;
        public Vector3 Position { get; private set; }
        public float Volume { get; private set; }
        public bool Looped { get; private set; }

        private static List<Audio> _lsAudio = new List<Audio>();
        private static Dictionary<string, CachedAudio> _lsPrecached = new Dictionary<string, CachedAudio>();
        //private static Dictionary<Audio, int> musics = new Dictionary<Audio, int>();
        public static bool init = false;
        public Audio( int handle, string filename )
        {
            Handle = handle;
            Filename = filename;
        }

        public static void Init()
        {
            try
            {
                if (init) return;
                //This is so you don't see the dumb HERPADERP BASS IS STARTING spash screen
                BassNet.Registration("swkauker@yahoo.com", "2X2832371834322");
                Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_3D, IntPtr.Zero);

                init = true;

                //Set the distance for 3D sounds
                Bass.BASS_Set3DFactors(1.0f, 0.6f, -1f);
                Bass.BASS_Apply3D(); // apply the change
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("COULD NOT INITIALIZE AUDIO CONTEXT: " + ex.Message);
                Console.ResetColor();
            }
        }

        public static Audio LoadSong(string filename, bool Loop = false, bool Positional = false, Entity.BaseEntity ent = null)
        {
            int handle = 0;
            BASSFlag flags = BASSFlag.BASS_MUSIC_PRESCAN;
            if (Loop) flags = flags | BASSFlag.BASS_SAMPLE_LOOP;
            if (Positional || ent != null) flags = flags | BASSFlag.BASS_SAMPLE_3D | BASSFlag.BASS_SAMPLE_MONO;

            handle = Bass.BASS_StreamCreateFile(filename, 0, 0, flags);

            if (handle != 0)
            {
                Audio audio = new Audio(handle, filename);
                audio.Positional = (Positional || ent != null);
                audio.attachedEnt = ent;
                audio.Looped = Loop;

                _lsAudio.Add(audio);

                return audio;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to create music stream '" + filename + "'!");
                Console.WriteLine(Bass.BASS_ErrorGetCode());
                Console.ResetColor();
                if (Bass.BASS_ErrorGetCode() == BASSError.BASS_ERROR_NO3D) Console.WriteLine("Are you sure the audio file is mono?");
            }

            return new Audio(-1, "rude");
        }

        public static unsafe void Precache(string filename )
        {
            byte[] bytes = null;
            try
            {
                bytes = System.IO.File.ReadAllBytes(filename);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine("Failed to load audio file: " + ex.Message);
                return;
            }
            int length = bytes.Length;
            fixed (byte* p = &bytes[0])
            {
                CachedAudio memAudio = new CachedAudio(filename, new IntPtr(p), length);
                memAudio.Filename = filename; //may as well store this

                if (!_lsPrecached.ContainsKey(filename))
                {
                    _lsPrecached.Add(filename, memAudio);
                }
            }
        }

        public static void PlaySound( string name, float volume = 1.0f, int frequency = 44100 )
        {
            if (!_lsPrecached.ContainsKey(name))
            {
                Precache(name);
                Console.WriteLine("Sound not precached: {0}", name);
            }

            int handle = Bass.BASS_StreamCreateFile(_lsPrecached[name].bufferPointer, 0, _lsPrecached[name].bufferLength, BASSFlag.BASS_DEFAULT); //Bass.BASS_StreamCreatePush( 44100, 1, BASSFlag.BASS_DEFAULT, IntPtr.Zero );
            if (handle == 0) Console.WriteLine("Failed to play precached sound! " + Bass.BASS_ErrorGetCode());
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_VOL, volume);
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_FREQ, frequency);
            Bass.BASS_ChannelPlay(handle, true);
        }

        public static void Think( FrameEventArgs e )
        {
            if (View.Player != null)
            {
                BASS_3DVECTOR pos = new BASS_3DVECTOR(View.Player.Position.X, View.Player.Position.Y, View.Player.Position.Z);
                Bass.BASS_Set3DPosition(pos, null, null, null );
                Bass.BASS_Apply3D();
            }

            for (int i = 0; i < _lsAudio.Count; i++)
            {
                if (_lsAudio[i].attachedEnt != null)
                {
                    _lsAudio[i].Set3DPosition(_lsAudio[i].attachedEnt.Position);
                }

                if (_lsAudio[i].Handle != 0 && !_lsAudio[i].Looped)
                {
                    long seconds = Bass.BASS_ChannelGetPosition(_lsAudio[i].Handle);
                    long length = Bass.BASS_ChannelGetLength(_lsAudio[i].Handle);

                    if (seconds >= length)
                    {
                        Bass.BASS_ChannelStop( _lsAudio[i].Handle );
                        _lsAudio.RemoveAt(i);
                    }
                    
                }
            }
        }

        #region non-static methods
        public void SetVolume(float volume)
        {
            if (_lsAudio.Contains(this) && this.Handle != 0)
            {
                Volume = volume;
                Bass.BASS_ChannelSetAttribute(this.Handle, BASSAttribute.BASS_ATTRIB_VOL, volume);
            }
        }
        public void SetFrequency(float freq)
        {
            if (_lsAudio.Contains(this) && this.Handle != 0)
            {
                Bass.BASS_ChannelSetAttribute(this.Handle, BASSAttribute.BASS_ATTRIB_FREQ, freq);
            }
        }
        public void Play(bool fromBeginning)
        {
            if (_lsAudio.Contains(this) && this.Handle != 0)
            {
                Bass.BASS_ChannelPlay(this.Handle, fromBeginning);
            }
        }
        public void Stop()
        {
            if (_lsAudio.Contains(this) && this.Handle != 0)
            {
                Bass.BASS_ChannelStop(this.Handle);
            }
        }
        public void Pause()
        {
            if (_lsAudio.Contains(this) && this.Handle != 0)
            {
                Bass.BASS_ChannelPause(this.Handle);
            }
        }
        public bool IsPlaying()
        {
            if (_lsAudio.Contains(this) && this.Handle != 0)
            {
                Un4seen.Bass.BASSActive active = Bass.BASS_ChannelIsActive(this.Handle);
                return active == BASSActive.BASS_ACTIVE_STOPPED || active == BASSActive.BASS_ACTIVE_PAUSED;
            }
            
            return false;
        }
        public BASSActive GetStreamState()
        {
            if (_lsAudio.Contains(this) && this.Handle != 0)
            {
                return Bass.BASS_ChannelIsActive(this.Handle);
            }
            else return BASSActive.BASS_ACTIVE_STALLED;
        }
        public void Set3DPosition( float x, float y, float z )
        {
            if (_lsAudio.Contains(this) && this.Handle != 0 && this.Positional )
            {
                BASS_3DVECTOR vec = new BASS_3DVECTOR(x, y, z );
                Bass.BASS_ChannelSet3DPosition(this.Handle, vec, null, null);
                Bass.BASS_Apply3D();
            }
        }
        public void Set3DPosition(Vector3 vecpos)
        {
            if (_lsAudio.Contains(this) && this.Handle != 0 && this.Positional)
            {
                BASS_3DVECTOR vec = new BASS_3DVECTOR(vecpos.X, vecpos.Y, vecpos.Z);
                Bass.BASS_ChannelSet3DPosition(this.Handle, vec, null, null);
                Bass.BASS_Apply3D();
            }
        }

        #endregion
    }

    class CachedAudio
    {
        public string Name { get; set; }
        public IntPtr bufferPointer { get; set; }
        public int bufferLength { get; set; }
        public string Filename { get; set; }

        public CachedAudio(string name, IntPtr ptr, int length)
        {
            Name = name;
            bufferPointer = ptr;
            bufferLength = length;
        }
    }
}
