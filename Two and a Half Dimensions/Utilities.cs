﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Newtonsoft.Json;

namespace Two_and_a_Half_Dimensions
{
    class Utilities
    {
        public static double Time = 0.0d;
        public static double Frametime = 0.0d;
        public static int ErrorTex { get; set; }
        public static Material ErrorMat { get; set; }
        public static Material NormalUp { get; private set; }
        public static Program window { get; private set; }
        public static Matrix4 ProjectionMatrix { get; set; }
        public static Matrix4 ViewMatrix { get; set; }
        public static int CurrentPass = 1; //What stage of rendering we're at

        #region timing stuff
        public static void Think(FrameEventArgs e)
        {
            Time += e.Time;
            Frametime = e.Time;
        }
        #endregion

        public static void Init(Program win)
        {
            window = win;

            //Make sure default error textures
            ErrorTex = LoadTexture("engine/error.png");
            ErrorMat = LoadMaterial("engine/error");
            NormalUp = LoadMaterial("engine/normal_up");
        }

        public static Material LoadMaterial(string filename)
        {
            filename = Resource.TextureDir + filename + Resource.MaterialExtension;
            if (String.IsNullOrEmpty(filename))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load material. Filename is empty!");
                Console.ResetColor();

                return null;
            }

            if (!System.IO.File.Exists(filename))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load material. Couldn't find: " + filename);
                Console.ResetColor();

                return null;
            }
            string jsonString = File.ReadAllText(filename);
            JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
            MaterialProperties properties = new MaterialProperties();
            string Name = "DYNAMIC";
            string lastVal = "";
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    switch (lastVal)
                    {
                        case "Shader":
                            properties.ShaderProgram = Resource.GetProgram( reader.Value.ToString() );
                            break;

                        case "Basetexture":
                            properties.BaseTexture = Resource.GetTexture(reader.Value.ToString());
                            Name = reader.Value.ToString();
                            break;

                        case "Normalmap":
                            properties.NormalMapTexture = Resource.GetTexture(reader.Value.ToString());
                            break;

                        case "SpecPower":
                            float pwr = 0.0f;
                            float.TryParse(reader.Value.ToString(), out pwr );
                            properties.SpecularPower = pwr;
                            break;

                        case "SpecIntensity":
                            float intensity = 0.0f;
                            float.TryParse(reader.Value.ToString(), out intensity);
                            properties.SpecularIntensity = intensity;
                            break;

                        case "Color":
                            string curline = reader.Value.ToString();
                            string[] components = curline.Split(' ');
                            if (components.Length > 2)
                            {
                                float r= 1.0f, g = 1.0f, b = 1.0f;
                                float.TryParse(components[0], out r);
                                float.TryParse(components[1], out g);
                                float.TryParse(components[2], out b);

                                properties.Color = new Vector3(r, g, b);
                            }
                            break;

                        default:
                            break;
                    }

                    lastVal = reader.Value.ToString();
                }
                else
                {
                    //Console.WriteLine(reader.TokenType);
                }

            }

            return new Material(properties, Name);
        }

        public static int LoadTexture(string filename)
        {
            filename = Resource.TextureDir + filename;
            if (String.IsNullOrEmpty(filename))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load texture. Filename is empty!");
                Console.ResetColor();
                return ErrorTex;
            }

            if (!System.IO.File.Exists(filename))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load texture. Couldn't find: " + filename);
                Console.ResetColor();
                return ErrorTex;
            }
            GL.ActiveTexture(TextureUnit.Texture6);//Something farish away
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap(filename);
            System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);

            // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.ActiveTexture(TextureUnit.Texture0);//Something farish away
            return id;
        }

        public static void LoadOBJ(string filename, out Vector3[] lsVerts, out int[] lsElements, out Vector3[] lsTangents, out Vector3[] lsNormals, out Vector2[] lsUV)
        {
            filename = Resource.ModelDir + filename;

            string[] file = null;
            lsVerts = null;
            lsElements = null;
            lsTangents = null;
            lsNormals = null;
            lsUV = null;
            try
            {
                file = System.IO.File.ReadAllLines(filename);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine("Failed to load model file: " + ex.Message);
            }
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("Failed to load model file");
                return;
            }

            //scan the file and look for the number of stuffs
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> verts_UNSORTED = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector3> normals_UNSORTED = new List<Vector3>();
            List<Vector3> tangents = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector2> uv_UNSORTED = new List<Vector2>();
            List<int> elements = new List<int>();
            for (int i = 0; i < file.Length; i++)
            {
                string curline = file[i];
                string seg1 = curline.Split(' ')[0];
                switch (seg1)
                {
                    case "v":
                        string[] vert = curline.Split(' ');
                        if (vert[1].Length == 0 && vert.Length > 4)
                        {
                            verts_UNSORTED.Add(new Vector3(float.Parse(vert[2]), float.Parse(vert[3]), float.Parse(vert[4])));
                        }
                        else
                        {
                            verts_UNSORTED.Add(new Vector3(float.Parse(vert[1]), float.Parse(vert[2]), float.Parse(vert[3])));
                        }
                        break;

                    case "vn":
                        string[] norms = curline.Split(' ');
                        if (norms[1].Length == 0 && norms.Length > 4)
                        {
                            normals_UNSORTED.Add(new Vector3(float.Parse(norms[2]), float.Parse(norms[3]), float.Parse(norms[4])));
                        }
                        else
                        {
                            normals_UNSORTED.Add(new Vector3(float.Parse(norms[1]), float.Parse(norms[2]), float.Parse(norms[3])));
                        }
                        break;

                    case "vt":
                        string[] coords = curline.Split(' ');
                        if (coords[1].Length == 0 && coords.Length > 3)
                        {
                            uv_UNSORTED.Add(new Vector2(float.Parse(coords[2]), -float.Parse(coords[3])));
                        }
                        else
                        {
                            uv_UNSORTED.Add(new Vector2(float.Parse(coords[1]), -float.Parse(coords[2])));
                        }
                        break;

                    case "f":
                        string[] element = curline.Split(' ');

                        for (int n = 1; n < 4; n++)
                        {
                            string[] group = element[n].Split('/');
                            //elements.Add(int.Parse(group[0]) - 1);
                            if (group.Length > 0 && group[0].Length > 0)
                            {
                                int vertNum = int.Parse(group[0]);
                                if (vertNum < verts_UNSORTED.Count + 1)
                                {
                                    verts.Add(verts_UNSORTED[vertNum - 1]);
                                    elements.Add(elements.Count);
                                }
                            }
                            if (group.Length > 1 && group[1].Length > 0)
                            {
                                int uvNum = int.Parse(group[1]);
                                if (uvNum < uv_UNSORTED.Count + 1)
                                {
                                    uv.Add(uv_UNSORTED[uvNum - 1]);
                                }
                            }
                            if (group.Length > 2 && group[2].Length > 0)
                            {
                                int normNum = int.Parse(group[2]);
                                if (normNum < normals_UNSORTED.Count)
                                {
                                    normals.Add(normals_UNSORTED[normNum - 1]);
                                }
                            }
                        }
                        break;

                    case "#":
                        //Console.WriteLine("Comment: {0}", curline );
                        break;
                    default:
                        if (!string.IsNullOrEmpty(curline))
                        {
                            #if DEBUG
                            //Console.WriteLine("Unknown line definition ({0}): {1}", i, curline);
                            #endif
                        }
                        break;
                }
            }
            lsElements = elements.ToArray();
            lsVerts = verts.ToArray();

            if (normals.Count > 0) lsNormals = normals.ToArray();
            if (uv.Count > 0) lsUV = uv.ToArray();
            //lsNormals = normals.ToArray();
            //lsUV = uv.ToArray();


            //lmao remove this
            /*
            Vector3[] Tverts = new Vector3[]
            {
                new Vector3( -5, 0, -5 ), new Vector3( 5, 0, -5 ),
                new Vector3( 5, 0, 5), new Vector3( -5, 0, 5 ),
            };
            Vector2[] Tuv = new Vector2[]
            {
                new Vector2( 0, 0 ), new Vector2( 5, 0 ),
                new Vector2( 5, 5 ), new Vector2( 0, 5 ),
            };
            int[] Telements = new int[]
            {
                0, 1, 2,
                2, 3, 0
            };
            lsVerts = Tverts;
            lsElements = Telements;
            //lsUV = Tuv;
            */
            //Go through all the vertices and calculate their tangents/bitangents
            if (uv.Count >= verts.Count)
            {
                for (int i = 0; i < verts.Count; i += 3)
                {
                    Vector3 v0 = verts[i];
                    Vector3 v1 = verts[i + 1];
                    Vector3 v2 = verts[i + 2];

                    Vector3 edge1 = v1 - v0;
                    Vector3 edge2 = v2 - v0;

                    Vector2 uv0 = uv[i];
                    Vector2 uv1 = uv[i + 1];
                    Vector2 uv2 = uv[i + 2];

                    float DeltaU1 = uv1.X - uv0.X;
                    float DeltaV1 = uv1.Y - uv0.Y;
                    float DeltaU2 = uv2.X - uv0.X;
                    float DeltaV2 = uv2.Y - uv0.Y;

                    float f = 1.0f / (DeltaU1 * DeltaV2 - DeltaU2 * DeltaV1);
                    Vector3 Tangent, Bitangent;
                    Tangent = new Vector3();
                    Bitangent = new Vector3();

                    Tangent.X = f * (DeltaV2 * edge1.X - DeltaV1 * edge2.X);
                    Tangent.Y = f * (DeltaV2 * edge1.Y - DeltaV1 * edge2.Y);
                    Tangent.Z = f * (DeltaV2 * edge1.Z - DeltaV1 * edge2.Z);

                    Bitangent.X = f * (-DeltaU2 * edge1.X - DeltaU1 * edge2.X);
                    Bitangent.X = f * (-DeltaU2 * edge1.Y - DeltaU1 * edge2.Y);
                    Bitangent.X = f * (-DeltaU2 * edge1.X - DeltaU1 * edge2.Z);

                    tangents.Add(Tangent);
                    tangents.Add(Tangent);
                    tangents.Add(Tangent);

                }
            }

            if (tangents.Count == 0)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    tangents.Add(new Vector3(0, 0, 0));
                }
            }
            else
            {
                for (int i = 0; i < tangents.Count; i++)
                {
                    tangents[i].Normalize();
                }
            }


            lsTangents = tangents.ToArray();
        }

        public static Vector3[] CalculateTangents(Vector3[] vertices, Vector2[] UV)
        {
            List<Vector3> Tangents = new List<Vector3>();
            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3 v0 = vertices[i];
                Vector3 v1 = vertices[i + 1];
                Vector3 v2 = vertices[i + 2];

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;

                Vector2 uv0 = UV[i];
                Vector2 uv1 = UV[i + 1];
                Vector2 uv2 = UV[i + 2];

                float DeltaU1 = uv1.X - uv0.X;
                float DeltaV1 = uv1.Y - uv0.Y;
                float DeltaU2 = uv2.X - uv0.X;
                float DeltaV2 = uv2.Y - uv0.Y;

                float f = 1.0f / (DeltaU1 * DeltaV2 - DeltaU2 * DeltaV1);
                Vector3 Tangent, Bitangent;
                Tangent = new Vector3();
                Bitangent = new Vector3();

                Tangent.X = f * (DeltaV2 * edge1.X - DeltaV1 * edge2.X);
                Tangent.Y = f * (DeltaV2 * edge1.Y - DeltaV1 * edge2.Y);
                Tangent.Z = f * (DeltaV2 * edge1.Z - DeltaV1 * edge2.Z);

                Bitangent.X = f * (-DeltaU2 * edge1.X - DeltaU1 * edge2.X);
                Bitangent.X = f * (-DeltaU2 * edge1.Y - DeltaU1 * edge2.Y);
                Bitangent.X = f * (-DeltaU2 * edge1.X - DeltaU1 * edge2.Z);

                Tangents.Add(Tangent);
                Tangents.Add(Tangent);
                Tangents.Add(Tangent);

            }

            for (int i = 0; i < Tangents.Count; i++)
            {
                Tangents[i].Normalize();
            }

            return Tangents.ToArray();
        }

        public static string LoadShaderSource(string shader)
        {
            shader = Resource.ShaderDir + shader;
            string src = "";

            if (!File.Exists(shader))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load texture. File \"{0}\" does not exist!", shader);
                Console.ResetColor();
            }

            try
            {
                src = System.IO.File.ReadAllText(shader);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load texture \"{0}\". {1}", shader, e.Message);
                Console.ResetColor();
            }

            return src;
        }

        public static int Clamp(int num, int high, int low)
        {
            if (num > high) return high;
            if (num < low) return low;
            return num;
        }
        public static float Clamp(float num, float high, float low)
        {
            if (num > high) return high;
            if (num < low) return low;
            return num;
        }
    }
}