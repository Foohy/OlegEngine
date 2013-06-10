using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Newtonsoft.Json;

namespace OlegEngine
{
    public class Utilities
    {
        /// <summary>
        /// The current time
        /// </summary>
        public static double Time { get; private set; }
        /// <summary>
        /// The amount of time the last frame took to render
        /// </summary>
        public static double Frametime { get; private set; }
        /// <summary>
        /// The amount of time since the last 'think' event happened
        /// </summary>
        public static double ThinkTime { get; private set; }

        /// <summary>
        /// The current engine settings that are active
        /// </summary>
        public static Settings EngineSettings { get; set; }

        public const double D_RAD2DEG = 180d / Math.PI;
        public const double D_DEG2RAD = Math.PI / 180d;
        public const float F_RAD2DEG = (float)(180d / Math.PI);
        public const float F_DEG2RAD = (float)(Math.PI / 180d);

        public static int ErrorTex { get; set; }
        public static int White { get; set; }
        public static int NormalTex { get; set; }
        public static int AlphaTex { get; set; }
        public static int SpecTex { get; set; }
        public static Material ErrorMat { get; set; }
        public static Material NormalMat { get; set; }
        public static GameWindow window { get; set; }
        public static Matrix4 ProjectionMatrix { get; set; }
        public static Matrix4 ViewMatrix { get; set; }
        public static int CurrentPass = 1; //What stage of rendering we're at
        public static Random Rand;
        public static Engine engine;

        public static FarseerPhysics.Dynamics.World PhysicsWorld { get; set; }

        /// <summary>
        /// Units to the near clipping plane
        /// </summary>
        public const float NearClip = 1.0f;
        /// <summary>
        /// Units to the far clipping plane
        /// </summary>
        public const float FarClip = 256f;

        #region Timing information
        public static void Think(FrameEventArgs e)
        {
            Time += e.Time;
            ThinkTime = e.Time;
        }

        public static void Draw(FrameEventArgs e)
        {
            Frametime = e.Time;
        }
        #endregion

        public static void Init(GameWindow win, Engine eng)
        {
            window = win;
            engine = eng;

            //Create engine-specific resources (debug models/materials, etc.)
            EngineResources.CreateResources();

            //Create a global random variable that's easily accessable
            Rand = new Random();
        }

        public enum PrintCode
        {
            NONE,
            WARNING,
            ERROR,
            INFO
        }
        public static void Print(string str, PrintCode code = PrintCode.NONE, params string[] parameters)
        {
            switch (code)
            {
                case PrintCode.WARNING:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case PrintCode.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case PrintCode.INFO:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.WriteLine(str, parameters);
            Console.ResetColor();
        }


        //Strings for each component of a material
        private const string MAT_SHADER         = "shader";
        private const string MAT_BASETEXTURE    = "basetexture";
        private const string MAT_NORMALMAP      = "normalmap";
        private const string MAT_SPECMAP        = "specmap";
        private const string MAT_ALPHAMAP       = "alphamap";
        private const string MAT_NOCULL         = "nocull";
        private const string MAT_ALPHATEST      = "alphatest";
        private const string MAT_SPECPOWER      = "specpower";
        private const string MAT_SPECINTENSITY  = "specintensity";
        private const string MAT_COLOR          = "color";
        private const string MAT_ISANIMATED     = "isanimated";
        private const string MAT_FRAMELENGTH    = "framelength";

        public static Material LoadMaterial(string filename)
        {
            string Name = filename;
            filename = Resource.TextureDir + filename + Resource.MaterialExtension;
            if (String.IsNullOrEmpty(filename))
            {
                Utilities.Print("Failed to load material. Filename is empty!", PrintCode.ERROR);

                return null;
            }

            if (!System.IO.File.Exists(filename))
            {
                Utilities.Print("Failed to load material. Couldn't find '{0}'", PrintCode.ERROR, filename);
                return null;
            }

            string jsonString = File.ReadAllText(filename);
            MaterialProperties properties = new MaterialProperties();

            Newtonsoft.Json.Linq.JObject infObj = null;
            try
            {
                infObj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
            }
            catch (Exception e)
            {
                Utilities.Print("Failed to load material. {0}", PrintCode.ERROR, e.Message);
                return ErrorMat;
            }

            if (infObj[MAT_SHADER] != null)
                properties.ShaderProgram = Resource.GetProgram(infObj[MAT_SHADER].ToString());

            if (infObj[MAT_BASETEXTURE] != null)
            {
                properties.BaseTexture = Resource.GetTexture(infObj[MAT_BASETEXTURE].ToString());
                if (Path.GetExtension(infObj[MAT_BASETEXTURE].ToString()) == ".gif")
                {
                    //Load it with multiple frames in mind
                    properties.BaseTextures = LoadAnimatedTexture(infObj[MAT_BASETEXTURE].ToString()); //TODO: Add these images to the resource manager
                }

            }

            if (infObj[MAT_NORMALMAP] != null)
                properties.NormalMapTexture = Resource.GetTexture(infObj[MAT_NORMALMAP].ToString());

            if (infObj[MAT_SPECMAP] != null)
                properties.SpecMapTexture = Resource.GetTexture(infObj[MAT_SPECMAP].ToString());

            if (infObj[MAT_ALPHAMAP] != null)
                properties.AlphaMapTexture = Resource.GetTexture(infObj[MAT_ALPHAMAP].ToString());

            if (infObj[MAT_NOCULL] != null)
                properties.NoCull = infObj[MAT_NOCULL].ToString().ToLower() == bool.TrueString.ToLower();

            if (infObj[MAT_ALPHATEST] != null)
                properties.AlphaTest = infObj[MAT_ALPHATEST].ToString().ToLower() == bool.TrueString.ToLower();

            if (infObj[MAT_ISANIMATED] != null)
                properties.IsAnimated = infObj[MAT_ISANIMATED].ToString().ToLower() == bool.TrueString.ToLower();

            if (infObj[MAT_SPECPOWER] != null)
                properties.SpecularPower = float.TryParse( infObj[MAT_SPECPOWER].ToString(), out properties.SpecularPower) ? properties.SpecularPower : 0.0f;

            if (infObj[MAT_SPECINTENSITY] != null)
                properties.SpecularIntensity = float.TryParse(infObj[MAT_SPECINTENSITY].ToString(), out properties.SpecularIntensity) ? properties.SpecularIntensity : 0.0f;

            if (infObj[MAT_FRAMELENGTH] != null)
                properties.Framelength = double.TryParse(infObj[MAT_FRAMELENGTH].ToString(), out properties.Framelength) ? properties.Framelength : 1.0;

            if (infObj[MAT_COLOR] != null)
            {
                string curline = infObj[MAT_COLOR].ToString();
                string[] components = curline.Split(' ');
                if (components.Length > 2)
                {
                    float r = 1.0f, g = 1.0f, b = 1.0f;
                    float.TryParse(components[0], out r);
                    float.TryParse(components[1], out g);
                    float.TryParse(components[2], out b);

                    properties.Color = new Vector3(r, g, b);
                }
            }


            return new Material(properties, Name);
        }

        public static int LoadTexture(Bitmap bmp)
        {
            GL.ActiveTexture(TextureUnit.Texture6);//Something farish away
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bool hasAlpha = Image.IsAlphaPixelFormat(bmp.PixelFormat);

            PixelFormat pixFormat = hasAlpha ? PixelFormat.Bgra : PixelFormat.Bgr;
            PixelInternalFormat internalPixFormat = hasAlpha ? PixelInternalFormat.Rgba : PixelInternalFormat.Rgb;

            GL.TexImage2D(TextureTarget.Texture2D, 0, internalPixFormat, bmp_data.Width, bmp_data.Height, 0,
                pixFormat, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);


            if (GLVersion.Major <= 2)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
            }
            else
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            //TODO: Settings system that'll only set supported modes
            float maxAniso;
            GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out maxAniso);
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, maxAniso);

            GL.ActiveTexture(TextureUnit.Texture0);
            return id;
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

            Bitmap bmp;
            if (Path.GetExtension(filename) == ".tga")
            {
                bmp = Paloma.TargaImage.LoadTargaImage(filename);
            }
            else
            {
                bmp = new Bitmap(filename);
            }

            int Tex = LoadTexture(bmp);
            bmp.Dispose();
            return Tex;
        }

        public static int[] LoadAnimatedTexture(string filename)
        {
            filename = Resource.TextureDir + filename;
            if (String.IsNullOrEmpty(filename))
            {
                Utilities.Print("Failed to load texture. Filename is empty!", PrintCode.ERROR);
                return new int[0];
            }

            if (!System.IO.File.Exists(filename))
            {
                Utilities.Print("Failed to load texture. Couldn't find '{0}'", PrintCode.ERROR, filename );
                return new int[0];
            }

            
            Bitmap bmp = new Bitmap(filename);
            var Dimension = new System.Drawing.Imaging.FrameDimension(bmp.FrameDimensionsList[0]);
            int NumFrames = bmp.GetFrameCount(Dimension);

            int[] Textures = new int[NumFrames];
            for (int i = 0; i < NumFrames; i++)
            {
                bmp.SelectActiveFrame(Dimension, i);
                Textures[i] = LoadTexture(bmp);
            }
            bmp.Dispose();

            return Textures;
        }

        public static Mesh.BoundingBox CalculateBoundingBox(Vector3[] vertices, Vector3 scale)
        {
            Mesh.BoundingBox bbox = new Mesh.BoundingBox();
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i].Multiply( scale);

                //Update the size of the bounding box
                bbox.Negative = bbox.NegativeSet ? SmallestVec(bbox.Negative, vertex) : vertex;
                bbox.Positive = bbox.PositiveSet ? BiggestVec(bbox.Positive, vertex) : vertex;
            }

            return bbox;
        }

        public static void LoadOBJ(string filename, out Vector3[] lsVerts, out int[] lsElements, out Vector3[] lsTangents, out Vector3[] lsNormals, out Vector2[] lsUV, out Mesh.BoundingBox boundingBox)
        {
            filename = Resource.ModelDir + filename;

            string file = null;
            lsVerts = null;
            lsElements = null;
            lsTangents = null;
            lsNormals = null;
            lsUV = null;
            boundingBox = new Mesh.BoundingBox();

            try
            {
                file = System.IO.File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                Utilities.Print("Failed to load model '{0}'. {1}", PrintCode.ERROR, filename, ex.Message);
            }
            if (file == null || file.Length == 0)
            {
                Utilities.Print("Failed to load model '{0}'. File is empty!", PrintCode.ERROR, filename);
                return;
            }

            LoadOBJFromString(file, out lsVerts, out lsElements, out lsTangents, out lsNormals, out lsUV, out boundingBox);
        }

        public static void LoadOBJFromString(string objString, out Vector3[] lsVerts, out int[] lsElements, out Vector3[] lsTangents, out Vector3[] lsNormals, out Vector2[] lsUV, out Mesh.BoundingBox boundingBox)
        {
            lsVerts = null;
            lsElements = null;
            lsTangents = null;
            lsNormals = null;
            lsUV = null;
            boundingBox = new Mesh.BoundingBox();

            string[] file = objString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

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
                                if (normNum < normals_UNSORTED.Count + 1)
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

            //Calculate the bounding box
            boundingBox = CalculateBoundingBox(lsVerts, Vector3.One);

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

        public static Mesh MeshFromData(Vector3[] lsVerts, int[] lsElements, Vector3[] lsTangents, Vector3[] lsNormals, Vector2[] lsUV, Mesh.BoundingBox boundingBox, string Material)
        {
            //Try to load the material
            Material mat = Resource.GetMaterial(Material);

            //Create the model
            Mesh m = new Mesh(lsVerts, lsElements, lsTangents, lsNormals, lsUV);
            m.mat = mat;

            return m;
        }

        public static Mesh MeshFromRawData(List<Vector3> verts, List<int> elements, List<Vector3> normals, List<Vector2> uv, string Material)
        {
            Vector3[] lsVerts = null;
            int[] lsElements = null;
            Vector3[] lsTangents = null;
            Vector3[] lsNormals = null;
            Vector2[] lsUV = null;

            lsElements = elements.ToArray();
            lsVerts = verts.ToArray();

            if (normals.Count > 0) lsNormals = normals.ToArray();
            if (uv.Count > 0) lsUV = uv.ToArray();

            //Calculate bitangents and tangents of model
            if (uv.Count >= verts.Count)
            {
                lsTangents = CalculateTangents(lsVerts, lsUV);
            }
            else
            {
                lsTangents = new Vector3[lsVerts.Length];
            }

            //Try to load the material
            Material mat = Resource.GetMaterial(Material);

            //Create the model
            Mesh m = new Mesh(lsVerts, lsElements, lsTangents, lsNormals, lsUV);
            m.mat = mat;
            m.BBox = CalculateBoundingBox(verts.ToArray(), Vector3.One);

            return m;
        }

        public static MeshGroup LoadOBJMulti( string filename )
        {
            List<Mesh> meshList = new List<Mesh>();

            filename = Resource.ModelDir + filename;

            string material = "";
            string[] file = null;

            try
            {
                file = System.IO.File.ReadAllLines(filename);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine("Failed to load level file: " + ex.Message);
            }
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("Failed to load level file: An unknown error occurred!");
                return null;
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
                                if (normNum < normals_UNSORTED.Count + 1)
                                {
                                    normals.Add(normals_UNSORTED[normNum - 1]);
                                }
                            }
                        }
                        break;

                    //Specify what material this object should use
                    case "usemtl":
                        string[] mtlline = curline.Split(' ');
                        if (mtlline.Length > 0)
                        {
                            material = mtlline[1];
                        }

                        break;
                    case "o":
                        //If there is info, compile it into a model
                        if (verts.Count > 0)
                        {
                            meshList.Add(MeshFromRawData(verts, elements, normals, uv, material));
                        }

                        // Reset everything for this new mesh
                        verts = new List<Vector3>();
                        normals = new List<Vector3>();
                        tangents = new List<Vector3>();
                        uv = new List<Vector2>();
                        elements = new List<int>();

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

            //If there is info, compile it into a model
            if (verts.Count > 0)
            {
                meshList.Add(MeshFromRawData(verts, elements, normals, uv, material));
            }

            return new MeshGroup(meshList);
        }

        public static Vector3 Get2Dto3D(int x, int y)
        {
            int[] viewport = new int[4];
            Matrix4 modelviewMatrix, projectionMatrix;
            GL.GetFloat(GetPName.ModelviewMatrix, out modelviewMatrix);
            GL.GetFloat(GetPName.ProjectionMatrix, out projectionMatrix);
            GL.GetInteger(GetPName.Viewport, viewport);

            // get depth of clicked pixel
            float[] t = new float[1];
            GL.ReadPixels(x, Utilities.window.Height - y, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, t);

            return UnProject(new Vector3(x, viewport[3] - y, t[0]), modelviewMatrix, projectionMatrix, viewport);
        }

        public static Vector3 Get2Dto3D(int x, int y, float z)
        {
            int[] viewport = new int[4];
            Matrix4 modelviewMatrix, projectionMatrix;
            GL.GetFloat(GetPName.ModelviewMatrix, out modelviewMatrix);
            GL.GetFloat(GetPName.ProjectionMatrix, out projectionMatrix);
            GL.GetInteger(GetPName.Viewport, viewport);

            // convert z to be in terms of 0 - 1 of the clip planes
            float A = -(Utilities.FarClip + Utilities.NearClip) / (Utilities.FarClip - Utilities.NearClip);
            float B = -2 * Utilities.FarClip * Utilities.NearClip / (Utilities.FarClip - Utilities.NearClip);
            z = -(A * z + B) / z;
            z = 0.5f * z + 0.5f; // convert it to be from 0 to 1


            return UnProject(new Vector3(x, viewport[3] - y, z), modelviewMatrix, projectionMatrix, viewport);
        }

        public static Vector3 UnProject(Vector3 screen, Matrix4 view, Matrix4 projection, int[] view_port)
        {
            Vector4 pos = new Vector4();

            // Map x and y from window coordinates, map to range -1 to 1 
            pos.X = (screen.X - (float)view_port[0]) / (float)view_port[2] * 2.0f - 1.0f;
            pos.Y = (screen.Y - (float)view_port[1]) / (float)view_port[3] * 2.0f - 1.0f;
            pos.Z = screen.Z * 2.0f - 1.0f;
            pos.W = 1.0f;

            Vector4 pos2 = Vector4.Transform(pos, Matrix4.Invert(Matrix4.Mult(view, projection)));
            Vector3 pos_out = new Vector3(pos2.X, pos2.Y, pos2.Z);

            return pos_out / pos2.W;
        }

        private static Vector3 SmallestVec(Vector3 currentSmallest, Vector3 contender)
        {
            if (contender.X < currentSmallest.X)
            {
                currentSmallest.X = contender.X;
            }
            if (contender.Y < currentSmallest.Y)
            {
                currentSmallest.Y = contender.Y;
            }
            if (contender.Z < currentSmallest.Z)
            {
                currentSmallest.Z = contender.Z;
            }

            return currentSmallest;
        }
        private static Vector3 BiggestVec(Vector3 currentBiggest, Vector3 contender)
        {
            if (contender.X > currentBiggest.X)
            {
                currentBiggest.X = contender.X;
            }
            if (contender.Y > currentBiggest.Y)
            {
                currentBiggest.Y = contender.Y;
            }
            if (contender.Z > currentBiggest.Z)
            {
                currentBiggest.Z = contender.Z;
            }

            return currentBiggest;
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
        public static double Clamp(double num, double high, double low)
        {
            if (num > high) return high;
            if (num < low) return low;
            return num;
        }
        public static long Clamp(long num, long high, long low)
        {
            if (num > high) return high;
            if (num < low) return low;
            return num;
        }

        public static float Lerp( float start, float end, float percent)
        {
            return start + percent * (end - start);
        }
    }

    class DropOutStack<T>
    {
        private T[] items;
        private int top = 0;

        public int Count
        {
            get
            {
                return items.Length;
            }

        }

        public DropOutStack(int capacity)
        {
            items = new T[capacity];
        }

        public void Push(T item)
        {
            items[top] = item;
            top = (top + 1) % items.Length;
        }
        public T Pop()
        {
            top = (items.Length + top - 1) % items.Length;
            return items[top];
        }

        public T Value(int index)
        {
            return items[index < items.Length ? index : 0];
        }
    }
}