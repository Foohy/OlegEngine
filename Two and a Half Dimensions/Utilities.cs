using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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

        public struct VertexP3N3T2
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TexCoord;
        }

        #region cube constants
        private static Vector3[] vecCubeVerts = new Vector3[]
        {
            new Vector3( -1.0f, -1.0f, -1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(1.0f,1.0f,-1.0f), new Vector3(1.0f,-1.0f,-1.0f), //Front face
            new Vector3( -1.0f, -1.0f, 1.0f), new Vector3(-1.0f,1.0f,1.0f), new Vector3(1.0f,1.0f,1.0f), new Vector3(1.0f,-1.0f,1.0f), //back face
            new Vector3(-1.0f,-1.0f,1.0f), new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(-1.0f,1.0f,1.0f), //Left face
            new Vector3(1.0f,-1.0f,-1.0f), new Vector3(1.0f,1.0f,-1.0f), new Vector3(1.0f,1.0f,1.0f), new Vector3(1.0f,-1.0f,1.0f), //Right face
            new Vector3(1.0f,1.0f,-1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(-1.0f,1.0f,1.0f), new Vector3(1.0f,1.0f,1.0f), //Top face
            new Vector3(1.0f,-1.0f,-1.0f), new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(-1.0f,-1.0f,1.0f), new Vector3(1.0f,-1.0f,1.0f), //Bottom face
        };
        private static Vector3[] CubeNorms = new Vector3[]
        {
            new Vector3( 0, 0, -1.0f), new Vector3( 0, 0, -1.0f), new Vector3( 0, 0, -1.0f), new Vector3( 0, 0, -1.0f), //Front face
            new Vector3( 0, 0, 1.0f), new Vector3( 0, 0, 1.0f), new Vector3( 0, 0, 1.0f), new Vector3( 0, 0, 1.0f), //back face
            new Vector3(-1.0f,0,0), new Vector3(-1.0f,0,0), new Vector3(-1.0f,0,0), new Vector3(-1.0f,0,0), //Left face
            new Vector3(1.0f,0,0), new Vector3(1.0f,0,0), new Vector3(1.0f,0,0), new Vector3(1.0f,0,0), //Right face
            new Vector3(0,1.0f,0), new Vector3(0,1.0f,0), new Vector3(0,1.0f,0), new Vector3(0,1.0f,0), //Top face
            new Vector3(0,-1.0f,0), new Vector3(0,-1.0f,0), new Vector3(0,-1.0f,0), new Vector3(0,-1.0f,0), //Bottom face
        };
        private static Vector2[] TexCoords = new Vector2[]
        {
            new Vector2( 0, 0 ),new Vector2( 0.0f,1.0f),new Vector2(1.0f, 1.0f),new Vector2( 1.0f, 0.0f ),
        };
        private static int[] CubeElements = new int[]
        {
            1,2,3,4,
            5,6,7,8,
            9,10,11,12,
            13,14,15,16,
            17,18,19,20,
            21,22,23,24
        };
        #endregion

        public static void Init(Program win)
        {
            window = win;

            //Make sure default error textures
            ErrorTex = LoadTexture("engine/error.png");
            ErrorMat = Resource.GetMaterial("engine/error.png", "default");
            Console.WriteLine(GL.GetError());
            NormalUp = Resource.GetMaterial("engine/normal_up.jpg");
        }

        #region Debug

        public static void CheckShader( int shader )
        {

        }

        #endregion

        public static VertexP3N3T2[] CalculateVertices(float radius, float height, byte segments, byte rings)
        {
            var data = new VertexP3N3T2[segments * rings];

            int i = 0;

            for (double y = 0; y < rings; y++)
            {
                double phi = (y / (rings - 1)) * Math.PI / 2;
                for (double x = 0; x < segments; x++)
                {
                    double theta = (x / (segments - 1)) * 2 * Math.PI;

                    Vector3 v = new Vector3()
                    {
                        X = (float)(radius * Math.Sin(phi) * Math.Cos(theta)),
                        Y = (float)(height * Math.Cos(phi)),
                        Z = (float)(radius * Math.Sin(phi) * Math.Sin(theta)),
                    };
                    Vector3 n = Vector3.Normalize(v);
                    Vector2 uv = new Vector2()
                    {
                        X = (float)(x / (segments - 1)),
                        Y = (float)(y / (rings - 1))
                    };
                    // Using data[i++] causes i to be incremented multiple times in Mono 2.2 (bug #479506).
                    data[i] = new VertexP3N3T2() { Position = v, Normal = n, TexCoord = uv };
                    i++;
                }

            }

            return data;
        }

        public static int[] CalculateElements( byte segments, byte rings)
        {
            var num_vertices = segments * rings;
            var data = new int[num_vertices * 6];

            ushort i = 0;

            for (byte y = 0; y < rings - 1; y++)
            {
                for (byte x = 0; x < segments - 1; x++)
                {
                    data[i++] = (ushort)((y + 0) * segments + x);
                    data[i++] = (ushort)((y + 1) * segments + x);
                    data[i++] = (ushort)((y + 1) * segments + x + 1);

                    data[i++] = (ushort)((y + 1) * segments + x + 1);
                    data[i++] = (ushort)((y + 0) * segments + x + 1);
                    data[i++] = (ushort)((y + 0) * segments + x);
                }
            }

            // Verify that we don't access any vertices out of bounds:
            foreach (int index in data)
                if (index >= segments * rings)
                    throw new IndexOutOfRangeException();

            return data;
        }

        public static VertexP3N3T2[] GenerateCube(float width, float height, float depth, float texSize = 1)
        {
            VertexP3N3T2[] data = new VertexP3N3T2[24];

            for (int i = 0; i < data.Length; i++)
            {
                Vector3 vert = new Vector3(vecCubeVerts[i].X * width, vecCubeVerts[i].Y * height, vecCubeVerts[i].Z * depth);
                Vector3 norm = CubeNorms[i];
                Vector2 uv = TexCoords[i % 4] * texSize;

                data[i] = new VertexP3N3T2() { Normal = norm, Position = vert, TexCoord = uv };
            }

            return data;
        }
        public static int[] CalcCubeElements()
        {
            return CubeElements;
        }

        public static int LoadTexture(string filename )
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
                Console.WriteLine("Failed to load texture. Couldn't find: " + filename );
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

                        for (int n = 1; n < 4; n++ )
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
                                    uv.Add(uv_UNSORTED[uvNum-1]);
                                }
                            }
                            if (group.Length > 2 && group[2].Length > 0)
                            {
                                int normNum = int.Parse(group[2]);
                                if (normNum < normals_UNSORTED.Count)
                                {
                                    normals.Add(normals_UNSORTED[normNum-1]);
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
                            Console.WriteLine("Unknown line definition ({0}): {1}", i, curline);
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

            for (int i = 0; i < tangents.Count; i++)
            {
                tangents[i].Normalize();
            }

            lsTangents = tangents.ToArray();

            Console.WriteLine("Done loading model! " + lsVerts.Length );
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
            catch ( Exception e )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load texture \"{0}\". {1}", shader, e.Message );
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

    class ObjLoader
    {
        private static string cfm(string str)
        {
            return str.Replace('.', ',');
        }

        public static VBO LoadStream(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            List<Vector3> points = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            List<Tri> tris = new List<Tri>();
            string line;
            char[] splitChars = { ' ' };
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(splitChars);
                line = line.Replace("  ", " ");

                string[] parameters = line.Split(splitChars);

                switch (parameters[0])
                {
                    case "p": // Point
                        break;

                    case "v": // Vertex
                        float x = float.Parse(cfm(parameters[1]));
                        float y = float.Parse(cfm(parameters[2]));
                        float z = float.Parse(cfm(parameters[3]));
                        points.Add(new Vector3(x, y, z));
                        break;

                    case "vt": // TexCoord
                        float u = float.Parse(cfm(parameters[1]));
                        float v = float.Parse(cfm(parameters[2]));
                        texCoords.Add(new Vector2(u, v));
                        break;

                    case "vn": // Normal
                        float nx = float.Parse(cfm(parameters[1]));
                        float ny = float.Parse(cfm(parameters[2]));
                        float nz = float.Parse(cfm(parameters[3]));
                        normals.Add(new Vector3(nx, ny, nz));
                        break;

                    case "f": // Face
                        tris.AddRange(parseFace(parameters));
                        break;
                }
            }

            Vector3[] p = points.ToArray();
            Vector2[] tc = texCoords.ToArray();
            Vector3[] n = normals.ToArray();
            Tri[] f = tris.ToArray();

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector3> uv = new List<Vector3>();
            List<Utilities.VertexP3N3T2> compiledVerts = new List<Utilities.VertexP3N3T2>();

            int[] elements = new int[ tris.Count * 3];
            for (int i = 0; i < tris.Count; i++)
            {
                Tri tri = tris[i];
                compiledVerts.AddRange(AddTriangle(tri, p, tc, n));

                elements[i*3] = i*3;
                elements[i*3+1] = i*3+1;
                elements[i*3+2] = i*3+2;
            }
            Console.WriteLine("leedle");

            return new VBO(compiledVerts.ToArray(), elements);

            //return new MeshData(p, n, tc, f);
        }

        private static List<Utilities.VertexP3N3T2> AddTriangle(Tri tri, Vector3[] p, Vector2[] tc, Vector3[] n)
        {
            List<Utilities.VertexP3N3T2> compiledVerts = new List<Utilities.VertexP3N3T2>();
            Utilities.VertexP3N3T2 vert = new Utilities.VertexP3N3T2();
            vert.Normal = n[tri.First.norm];
            vert.TexCoord = tc[tri.First.tex];
            vert.Position = p[tri.First.vert];
            compiledVerts.Add(vert);

            vert.Normal = n[tri.Second.norm];
            vert.TexCoord = tc[tri.Second.tex];
            vert.Position = p[tri.Second.vert];
            compiledVerts.Add(vert);

            vert.Normal = n[tri.Third.norm];
            vert.TexCoord = tc[tri.Third.tex];
            vert.Position = p[tri.Third.vert];
            compiledVerts.Add(vert);

            return compiledVerts;
        }

        public static VBO LoadFile(string file)
        {
            // Silly me, using() closes the file automatically.
            using (FileStream s = File.Open(file, FileMode.Open))
            {
                return LoadStream(s);
            }
        }

        private static Tri[] parseFace(string[] indices)
        {
            GPoint[] p = new GPoint[indices.Length - 1];
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = parsePoint(indices[i + 1]);
            }
            return Triangulate(p);
            //return new Face(p);
        }

        // Takes an array of points and returns an array of triangles.
        // The points form an arbitrary polygon.
        private static Tri[] Triangulate(GPoint[] ps)
        {
            List<Tri> ts = new List<Tri>();
            if (ps.Length < 3)
            {
                throw new Exception("Invalid shape!  Must have >2 points");
            }

            GPoint lastButOne = ps[1];
            GPoint lastButTwo = ps[0];
            for (int i = 2; i < ps.Length; i++)
            {
                Tri t = new Tri();
                t.First = lastButTwo;
                t.Second = lastButOne;
                t.Third = ps[i];

                lastButOne = ps[i];
                lastButTwo = ps[i - 1];
                ts.Add(t);
            }
            return ts.ToArray();
        }

        private static GPoint parsePoint(string s)
        {
            char[] splitChars = { '/' };
            string[] parameters = s.Split(splitChars);
            int vert = int.Parse(parameters[0]) - 1;
            int tex = int.Parse(parameters[1]) - 1;
            int norm = int.Parse(parameters[2]) - 1;

            GPoint p = new GPoint();
            p.vert = vert;
            p.norm = norm;
            p.tex = tex;

            return p;
        }
    }
    struct GPoint
    {
        public int vert;
        public int norm;
        public int tex;
    }

    struct Tri
    {
        public GPoint First;
        public GPoint Second;
        public GPoint Third;
    }
}
