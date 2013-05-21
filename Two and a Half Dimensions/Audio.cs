using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using OpenTK;

using Un4seen.Bass;

namespace OlegEngine
{
    public class Audio
    {
        public int Handle { get; private set; }
        public string Filename { get; private set; }
        public Entity.BaseEntity attachedEnt = null;
        public bool Positional = false;
        public Vector3 Position { get; private set; }
        public float Volume { get; private set; }
        public bool Looped { get; private set; }

        public Int32 StartLoopPosition { get; private set; }
        public Int32 EndLoopPosition { get; private set; }

        private static List<Audio> _lsAudio = new List<Audio>();
        private static Dictionary<string, CachedAudio> _lsPrecached = new Dictionary<string, CachedAudio>();
        private static SYNCPROC _SyncDel;

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
                Bass.BASS_Set3DFactors(1.0f, 0.0f, 1.0f);
                Bass.BASS_Apply3D(); // apply the change
                _SyncDel = new SYNCPROC(SyncThink);
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

                if (audio.Looped)
                {
                    //Try to load the loop positions from wav files
                    LoadCuePoints(audio, filename);
                }

                _lsAudio.Add(audio);
                //Create a callback so we can control playback looping and stuff
                Bass.BASS_ChannelSetSync(audio.Handle, BASSSync.BASS_SYNC_END, 0, _SyncDel, IntPtr.Zero);

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

        public static void Precache(string filename )
        {
            byte[] bytes = null;
            try
            {
                bytes = System.IO.File.ReadAllBytes(filename);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load audio file: " + ex.Message);
                return;
            }
            int length = bytes.Length;
            GCHandle rawDataHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            CachedAudio memAudio = new CachedAudio(filename, rawDataHandle.AddrOfPinnedObject(), length, rawDataHandle);
            memAudio.Filename = filename; //may as well store this

            if (!_lsPrecached.ContainsKey(filename))
            {
                _lsPrecached.Add(filename, memAudio);
            }
        }

        public static void PlaySound( string name, float volume = 1.0f, int frequency = 44100 )
        {
            if (!_lsPrecached.ContainsKey(name))
            {
                Precache(name);
                Console.WriteLine("Sound not precached: {0}", name);
            }

            int handle = Bass.BASS_StreamCreateFile(_lsPrecached[name].bufferPointer, 0, _lsPrecached[name].bufferLength, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_STREAM_AUTOFREE); //Bass.BASS_StreamCreatePush( 44100, 1, BASSFlag.BASS_DEFAULT, IntPtr.Zero );
            if (handle == 0) Console.WriteLine("Failed to play precached sound! " + Bass.BASS_ErrorGetCode());
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_VOL, volume);
            Bass.BASS_ChannelSetAttribute(handle, BASSAttribute.BASS_ATTRIB_FREQ, frequency);
            Bass.BASS_ChannelPlay(handle, true);
        }

        public static void Think( FrameEventArgs e )
        {
            if (View.Player != null)
            {
                BASS_3DVECTOR pos = new BASS_3DVECTOR(View.Position.X, View.Position.Y, View.Position.Z);
                BASS_3DVECTOR fwd = new BASS_3DVECTOR(View.ViewNormal.X, View.ViewNormal.Y, View.ViewNormal.Z);
                BASS_3DVECTOR up = new BASS_3DVECTOR(0, -1, 0);
                Bass.BASS_Set3DPosition(pos, null, fwd, up );
                Bass.BASS_Apply3D();
            }

            for (int i = 0; i < _lsAudio.Count; i++)
            {
                Audio audio = _lsAudio[i]; 
                if (audio.attachedEnt != null)
                {
                   audio.Set3DPosition(audio.attachedEnt.Position);
                }

                long seconds = Bass.BASS_ChannelGetPosition(audio.Handle, BASSMode.BASS_POS_BYTES);
                long length = Bass.BASS_ChannelGetLength(audio.Handle, BASSMode.BASS_POS_BYTES);

                if (audio.Handle != 0 && !audio.Looped)
                {
                    if (seconds >= length)
                    {
                        audio.Remove();
                        i--;
                    }
                }
                else if (audio.StartLoopPosition > 0 && audio.EndLoopPosition > 0 )
                {
                    if (seconds > audio.EndLoopPosition)
                    {
                        Bass.BASS_ChannelSetPosition(audio.Handle, audio.StartLoopPosition, BASSMode.BASS_POS_BYTES);
                    }
                }
            }
        }

        private static Audio GetAudioFromHandle(int handle)
        {
            for (int i = 0; i < _lsAudio.Count; i++)
            {
                if (_lsAudio[i].Handle == handle) return _lsAudio[i];
            }

            //Nothing found with that handle
            return null;
        }

        //Called when a stream reaches its end
        private static void SyncThink(int handle, int channel, int data, IntPtr user)
        {
            Audio audio = GetAudioFromHandle(channel);
            if (audio == null) { Bass.BASS_StreamFree(channel); return; }

            //If the end cue point wasn't caught in the above think function, set it to the start cue here
            if (audio.Looped && audio.StartLoopPosition > 0 && audio.EndLoopPosition > 0)
            {
                long seconds = Bass.BASS_ChannelGetPosition(audio.Handle, BASSMode.BASS_POS_BYTES);
                Bass.BASS_ChannelSetPosition(audio.Handle, audio.StartLoopPosition, BASSMode.BASS_POS_BYTES);
            }
            else
            {
                //Remove the channel
                Bass.BASS_StreamFree(channel);
            }
        }

        private static void LoadCuePoints(Audio audio, string filename)
        {
            AudioCueLoader.CueStruct[] cues = AudioCueLoader.LoadCues(filename);
            if (cues != null)
            {
                audio.StartLoopPosition = cues[0].SamplePosition * 2;
                audio.EndLoopPosition = cues[1].SamplePosition * 2;
                audio.EndLoopPosition = (int)Utilities.Clamp(audio.EndLoopPosition, Bass.BASS_ChannelGetLength(audio.Handle, BASSMode.BASS_POS_BYTES) - 200, 0L);
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
                return active != BASSActive.BASS_ACTIVE_STOPPED && active != BASSActive.BASS_ACTIVE_PAUSED;
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

        public void Remove()
        {
            //Remove the channel
            Bass.BASS_ChannelStop(this.Handle);
            Bass.BASS_StreamFree(this.Handle);
            _lsAudio.Remove(this);
        }

        #endregion
    }

    class AudioCueLoader
    {
        public static string StartCueText = "start_cue";
        public static string EndCueText = "end_cue";

        private static long DataOffset = 0;
        public struct CueStruct
        {
            public string Text;
            public Int32 SamplePosition;
            public Int32 SampleOffset;

            public CueStruct(string text, Int32 samplePos, Int32 sampleOffset)
            {
                Text = text;
                SamplePosition = samplePos;
                SampleOffset = sampleOffset;
            }
        }
        static Dictionary<Int32, CueStruct> lsCuePointIDs;
        public static CueStruct[] LoadCues(string filename)
        {
            lsCuePointIDs = new Dictionary<Int32, CueStruct>();
            DataOffset = 0;
            FileStream fs;

            try
            {
                fs = new FileStream(filename, FileMode.Open);
            }
            catch (Exception e) { Console.WriteLine(e.Message); return null; }

            using (BinaryReader br = new BinaryReader(fs))
            {
                string Chunk = GetString(br.ReadBytes(4));
                Int32 ChunkSize = br.ReadInt32();
                string Type = GetString(br.ReadBytes(4));

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    ReadSubChunk(br);
                }
            }

            CueStruct[] cues = new CueStruct[2];
            int CueCount = 0;
            //Given a list of keys, get the sample positions for start_cue and end_cue
            foreach (CueStruct cue in lsCuePointIDs.Values)
            {

                if (cue.Text == StartCueText )
                {
                    cues[0] = cue;
                    CueCount++;
                }

                if (cue.Text == EndCueText)
                {
                    cues[1] = cue;
                    CueCount++;
                }

                if (CueCount == 2)
                {
                    Console.WriteLine("Valid cues in {0}!", filename);
                    return cues;
                }
            }

            return null;
        }

        public static void ReadSubChunk(BinaryReader br)
        {
            byte[] somebytes = br.ReadBytes(4);
            string Chunk = GetString(somebytes);
            Int32 ChunkSize = br.ReadInt32();

            switch (Chunk.ToLower())
            {
                case "cue":
                case "cue ":
                    ReadCueChunk(br);
                    break;

                case "labl":
                    ReadTextLabelChunk(br);
                    break;

                case "list":
                    ReadDataListChunk(br);
                    break;

                case "data":
                    ReadDataChunk(br, ChunkSize);
                    break;

                default:
                    br.ReadBytes(ChunkSize);
                    break;
            }
        }

        public static void ReadCueChunk(BinaryReader br)
        {
            Int32 NumPoints = br.ReadInt32();
            Int32 Size = 4 + (NumPoints * 24);

            for (int i = 0; i < NumPoints; i++)
            {
                Int32 ID = br.ReadInt32();
                Int32 SamplePosition = br.ReadInt32();
                Int32 DataChunkID = br.ReadInt32();
                Int32 ChunkStart = br.ReadInt32();
                Int32 BlockStart = br.ReadInt32();
                Int32 SampleOffset = br.ReadInt32();

                lsCuePointIDs.Add(ID, new CueStruct("", SamplePosition + 100, SampleOffset + 100));
            }
        }

        public static void ReadTextLabelChunk(BinaryReader br)
        {
            Int32 CueID = br.ReadInt32();
            string Text = ReadAscii(br);

            if (lsCuePointIDs.ContainsKey(CueID ))
            {
                CueStruct cuestruct= lsCuePointIDs[CueID];
                cuestruct.Text = Text;
                lsCuePointIDs[CueID] = cuestruct;
            }
        }

        public static void ReadDataChunk(BinaryReader br, Int32 ChunkSize )
        {
            DataOffset = br.BaseStream.Position;
            //We don't actually need to read any data, just take note of when it starts
            br.ReadBytes(ChunkSize);
        }

        public static void ReadDataListChunk(BinaryReader br)
        {
            string TypeID = GetString(br.ReadBytes(4));
            ReadSubChunk(br);
        }

        static string GetString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        static string ReadAscii(BinaryReader input)
        {
            List<byte> strBytes = new List<byte>();
            int b;
            while ((b = input.ReadByte()) != 0x00)
                strBytes.Add((byte)b);
            return System.Text.Encoding.ASCII.GetString(strBytes.ToArray());
        }
    }

    public class CachedAudio
    {
        public string Name { get; set; }
        public IntPtr bufferPointer { get; set; }
        public int bufferLength { get; set; }
        public string Filename { get; set; }
        public GCHandle DataHandle;

        public CachedAudio(string name, IntPtr ptr, int length, GCHandle handle)
        {

            DataHandle = handle;
            Name = name;
            bufferPointer = ptr;
            bufferLength = length;
        }
    }
}
