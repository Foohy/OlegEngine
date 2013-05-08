using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine.GUI
{
    public class Text
    {
        public struct CharDescriptor
        {
            public ushort x, y;
            public ushort Width, Height;
            public ushort XOffset, YOffset;
            public ushort XAdvance;
        }

        public class Charset
        {
            public ushort LineHeight;
            public ushort Base;
            public ushort Width, Height;
            public CharDescriptor[] Chars;

            public Charset()
            {
                Chars = new CharDescriptor[256];
            }
        }

        public const string FontPath = "Resources/Fonts/";
        public const string FontmapPath = "gui/fontmaps/"; //Relative to the directory for materials
        public string CurrentText;

        public static Charset CreateFrontFromBitmap(string font)
        {
            return ParseFont(font);

        }

        public static void ConstructVertexArray(Charset ch, string str, out Vector3[] verts, out Vector2[] UV, out int[] elements )
        {
            ushort CharX;
            ushort CharY;
            ushort Width;
            ushort Height;
            ushort OffsetX;
            ushort OffsetY;
            int CurX = 0;

            //vert stuff
            verts = new Vector3[str.Length * 4];
            UV = new Vector2[str.Length * 4];
            elements = new int[str.Length * 4];

            for (int i = 0; i < str.Length; i++)
            {
                char curChar = str[i];
                CharX = ch.Chars[curChar].x;
                CharY = ch.Chars[curChar].y;
                Width = ch.Chars[curChar].Width;
                Height = ch.Chars[curChar].Height;
                OffsetX = ch.Chars[curChar].XOffset;
                OffsetY = ch.Chars[curChar].YOffset;

                //upper left
                UV[i * 4].X = (float)CharX / (float)ch.Width;
                UV[i * 4].Y = (float)CharY / (float)ch.Height;
                verts[i * 4].X = (float)CurX + OffsetX;
                verts[i * 4].Y = (float)OffsetY;
                elements[i*4] = i*4;

                //upper right
                UV[i * 4 + 1].X = (float)(CharX + Width) / (float)ch.Width;
                UV[i * 4 + 1].Y = (float)CharY / (float)ch.Height;
                verts[i * 4 + 1].X = (float)Width + CurX + OffsetX;
                verts[i * 4 + 1].Y = (float)OffsetY;
                elements[i*4 + 1] = i*4 + 1;

                //lower right
                UV[i * 4 + 2].X = (float)(CharX + Width) / (float)ch.Width;
                UV[i * 4 + 2].Y = (float)(CharY + Height) / (float)ch.Height;
                verts[i * 4 + 2].X = (float)Width + CurX + OffsetX;
                verts[i * 4 + 2].Y = (float)Height + OffsetY;
                elements[i*4 + 2] = i*4 + 2;

                //lower left
                UV[i * 4 + 3].X = (float)CharX / (float)ch.Width;
                UV[i * 4 + 3].Y = (float)(CharY + Height) / (float)ch.Height;
                verts[i * 4 + 3].X = (float)CurX + OffsetX;
                verts[i * 4 + 3].Y = (float)Height + OffsetY;
                elements[i*4 + 3] = i*4 + 3;



                CurX += ch.Chars[curChar].XAdvance;
            }
        }

        static Charset ParseFont(string FNT)
        {
            Charset charset = new Charset();
            if (!File.Exists(FNT))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load font: " + FNT);
                Console.ResetColor();
                return charset;
            }

            StreamReader sr = new StreamReader(FNT);

            string Read, Key, Value;
            string FirstWord;

            while (!sr.EndOfStream)
            {
                Read = sr.ReadLine();
                FirstWord = GetFirstWord(Read);
                switch (FirstWord)
                {
                    case "common":
                        string[] sections = Read.Split(' ');
                        for (int i = 0; i < sections.Length; i++)
                        {
                            Key = GetFirstWord(sections[i]);
                            Value = sections[i].Substring(sections[i].IndexOf('=') + 1);
                            switch (Key)
                            {
                                case "lineHeight":
                                    charset.LineHeight = StringToShort(Value);
                                    break;

                                case "base":
                                    charset.Base = StringToShort(Value);
                                    break;

                                case "scaleW":
                                    charset.Width = StringToShort(Value);
                                    break;

                                case "scaleH":
                                    charset.Height = StringToShort(Value);
                                    break;
                            }
                        }

                        break;

                    case "char":
                        ushort CharID = 0;
                        string[] keys = Read.Split(' ');
                        for (int i = 0; i < keys.Length; i++)
                        {
                            Key = GetFirstWord(keys[i]);
                            Value = keys[i].Substring(keys[i].IndexOf('=') + 1);
                            if (CharID > charset.Chars.Length) continue;
                            switch (Key)
                            {
                                case "id":
                                    CharID = StringToShort(Value);
                                    break;

                                case "x":
                                    charset.Chars[CharID].x = StringToShort(Value);
                                    break;

                                case "y":
                                    charset.Chars[CharID].y = StringToShort(Value);
                                    break;

                                case "width":
                                    charset.Chars[CharID].Width = StringToShort(Value);
                                    break;

                                case "height":
                                    charset.Chars[CharID].Height = StringToShort(Value);
                                    break;

                                case "xoffset":
                                    charset.Chars[CharID].XOffset = StringToShort(Value);
                                    break;

                                case "yoffset":
                                    charset.Chars[CharID].YOffset = StringToShort(Value);
                                    break;

                                case "xadvance":
                                    charset.Chars[CharID].XAdvance = StringToShort(Value);
                                    break;

                            }

                        }
                        break;
                }

            }

            return charset;
        }

        private static ushort StringToShort(string str, ushort def = 1)
        {
            ushort.TryParse(str, out def);
            return def;
        }

        private static string GetFirstWord( string str )
        {
            char[] limit = {' ', '='};
            if (str.IndexOfAny(limit) <= 0) return "";
            return str.Substring(0, str.IndexOfAny(limit));
        }


        //instanced class
        public Charset charset { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float ScaleW { get; private set; }
        public float ScaleH { get; private set; }
        public Vector3 Color = Vector3.One;

        private Matrix4 view;
        private Mesh textMesh;
        public Text( string font, string text )
        {
            this.charset = ParseFont(FontPath + font + ".fnt");


            //Create a blank vertex buffer which we'll update later with our text info
            textMesh = new Mesh(new Vector3[0], new int[0], new Vector3[0], null, new Vector2[0]);
            textMesh.DrawMode = BeginMode.Quads;
            textMesh.ShouldDrawDebugInfo = false;

            this.SetText(text);
            this.textMesh.mat = new Material(Resource.GetTexture(FontmapPath + font + ".png"), Resource.GetProgram("hud"));

            this.ScaleH = 1;
            this.ScaleW = 1;

            this.UpdateMatrix(); //Update the model matrix
        }

        public void SetPos(float x, float y)
        {
            this.X = x;
            this.Y = y;

            this.UpdateMatrix();
        }

        public void SetScale(float Width, float Height)
        {
            this.ScaleW = Width;
            this.ScaleH = Height;

            this.UpdateMatrix();
        }

        public void SetColor(Vector3 colorvec)
        {
            this.Color = colorvec;
            this.textMesh.Color = this.Color;
        }

        public void SetColor(float R, float G, float B)
        {
            this.Color = new Vector3(R, G, B);
            this.textMesh.Color = this.Color;
        }

        public void UpdateMatrix()
        {
            view = Matrix4.CreateTranslation(Vector3.Zero);
            view *= Matrix4.Scale(this.ScaleW, this.ScaleH, 1.0f);
            view *= Matrix4.CreateTranslation(this.X, this.Y, 3.0f);
        }

        /// <summary>
        /// Get the length, in pixels, of the given text string/charset combo
        /// </summary>
        /// <returns>The length in pixels of the string</returns>
        public float GetTextLength( string str )
        {
            if (string.IsNullOrEmpty(str)) return 0;

            float CurX = 0;
            float StartX = 0;
            float EndX = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (i == 0) StartX = this.charset.Chars[str[i]].XOffset;
                if (i == str.Length - 1) EndX = this.charset.Chars[str[i]].Width + CurX + this.charset.Chars[str[i]].XOffset;

                CurX += this.charset.Chars[str[i]].XAdvance;
            }

            return (EndX - StartX) *ScaleW;
        }

        /// <summary>
        /// Get the length, in pixels, of the given text string/charset combo
        /// </summary>
        /// <returns>The length in pixels of the string</returns>
        public float GetTextLength()
        {
            return GetTextLength(this.CurrentText);
        }

        public float GetTextHeight()
        {
            return this.charset.LineHeight * ScaleH;
        }

        public void SetText(string text)
        {
            if (string.IsNullOrEmpty(text) || text == this.CurrentText) return;
            this.CurrentText = text;

            Vector3[] verts;
            Vector2[] UV;
            int[] elements;
            ConstructVertexArray(this.charset, text, out verts, out UV, out elements);
            if (textMesh == null)
            {
                textMesh = new Mesh(verts, elements, new Vector3[verts.Length], null, UV);
                textMesh.DrawMode = BeginMode.Quads;
            }
            else
            {
                textMesh.UpdateMesh(verts, elements, new Vector3[verts.Length], null, UV);
            }
        }

        public void Draw()
        {
            if (this.textMesh == null) return;

            this.SetColor(this.Color); //HACK HACK: make some sort of render.SetColor
            this.textMesh.DrawSimple(view);
        }
    }
}
