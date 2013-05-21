using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using OlegEngine.GUI;
namespace OlegEngine
{
    public class Resource
    {
        public const string ModelDir = "Resources/Models/";
        public const string TextureDir = "Resources/Materials/";
        public const string ShaderDir = "Resources/Shaders/";
        public const string FontDir = "Resources/Fonts/";
        public const string MaterialExtension = ".fmf";

        static Dictionary<string, int> Textures = new Dictionary<string, int>();
        static Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        static Dictionary<string, VBO> Models = new Dictionary<string, VBO>();
        static Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
        static Dictionary<string, int> Programs = new Dictionary<string, int>();
        static Dictionary<string, Text.Charset> Charsets = new Dictionary<string, Text.Charset>();

        public static void InsertMaterial(string filename, Material m)
        {
            Materials[filename] = m;
        }
        public static Material GetMaterial(string filename)
        {
            if (!Materials.ContainsKey(filename))
            {
                Materials[filename] = Utilities.LoadMaterial(filename);
            }

            if (Materials[filename] != null) return Materials[filename];

            return Utilities.ErrorMat;
        }

        public static void InsertTexture(string filename, int texture)
        {
            Textures[filename] = texture;
        }
        public static int GetTexture(string filename)
        {
            if (!Textures.ContainsKey(filename))
            {
                Textures[filename] = Utilities.LoadTexture(filename);
            }

            return Textures[filename];
        }

        public static Text.Charset GetCharset(string name)
        {
            if (!Charsets.ContainsKey(name))
            {
                Charsets[name] = Text.ParseFont(name);
            }

            return Charsets[name];
        }

        public static void InsertMesh(string name, Mesh m)
        {
            Meshes[name] = m;
        }
        public static Mesh GetMesh(string filename, bool newInstance = false)
        {
            if (!Meshes.ContainsKey(filename) || newInstance )
            {
                Vector3[] verts;
                Vector3[] tangents;
                Vector3[] normals;
                Vector2[] lsUV;
                int[] elements;
                Mesh.BoundingBox boundingbox;

                Utilities.LoadOBJ(filename, out verts, out elements, out tangents, out normals, out lsUV, out boundingbox);
                Mesh m = new Mesh(verts, elements, tangents, normals, lsUV);
                m.BBox = boundingbox;
                if (newInstance) return m;
                else Meshes[filename] = m;
            }

            return Meshes[filename];
        }

        public static void InsertProgram(string filename)
        {
            int prog = CreateProgramPair(filename);
            if (prog == -1) return;

            Programs[filename] = prog;
        }
        public static int GetProgram(string filename)
        {
            string filenameV = filename + ".vert";
            string filenameF = filename + ".frag";
            if (!Programs.ContainsKey(filename))
            {
                int prog = CreateProgramPair(filename);
                if (prog == -1) return -1;

                Programs[filename] = prog;
            }


            return Programs[filename];
        }

        public static bool ProgramExists(string filename)
        {
            string filenameV = filename + ".vert";
            string filenameF = filename + ".frag";
            return Programs.ContainsKey(filename);
        }

        private static int CreateProgramPair(string name)
        {
            string filenameV = name + ".vert";
            string filenameF = name + ".frag";

            string vert = Utilities.LoadShaderSource(filenameV);
            string frag = Utilities.LoadShaderSource(filenameF);
            if (!string.IsNullOrEmpty(vert) && !string.IsNullOrEmpty(frag))
            {
                int compileStatus = -1;

                //Create our vertex shader
                int VertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(VertexShader, vert);
                GL.CompileShader(VertexShader);

                GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out compileStatus);

                //Create our fragment shader
                int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(FragmentShader, frag);
                GL.CompileShader(FragmentShader);

                //Create our shader program and attach our fragment and vertex shaders
                int program = GL.CreateProgram();
                GL.AttachShader(program, VertexShader);
                GL.AttachShader(program, FragmentShader);

                //Create some locations for our variables
                GL.BindAttribLocation(program, 0, "_Position");
                GL.BindAttribLocation(program, 1, "_UV");
                GL.BindAttribLocation(program, 2, "_Normal");
                GL.BindAttribLocation(program, 3, "_Tangent");

                //Check for errors
                int statV = 0;
                int statF = 0;
                GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out statV);
                GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out statF);

                if (statV == 0)
                {
                    Utilities.Print("{0} failed to compile!", Utilities.PrintCode.ERROR, filenameV);
                    Utilities.Print(GL.GetShaderInfoLog(VertexShader), Utilities.PrintCode.ERROR);
                }
                if (statF == 0)
                {
                    Utilities.Print("{0} failed to compile!", Utilities.PrintCode.ERROR, filenameF);
                    Utilities.Print(GL.GetShaderInfoLog(FragmentShader), Utilities.PrintCode.ERROR);
                }

                //Link them up
                GL.LinkProgram(program);

                //Once we're done with creating the program, we don't need the shader objects anymore (they'll persist until the program is deleted)
                GL.DeleteShader(VertexShader);
                GL.DeleteShader(FragmentShader);

                return program;
            }
            else
            {
                return -1;
            } 
        }
    }
}
