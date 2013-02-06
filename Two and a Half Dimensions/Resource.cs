using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions
{
    class Resource
    {
        public const string ModelDir = "Resources/Models/";
        public const string TextureDir = "Resources/Materials/";
        public const string ShaderDir = "Resources/Shaders/";

        static Dictionary<string, int> Textures = new Dictionary<string, int>();
        static Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        static Dictionary<string, VBO> Models = new Dictionary<string, VBO>();
        static Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
        static Dictionary<string, int> Programs = new Dictionary<string, int>();

        public static Material GetMaterial(string filename)
        {
            if (!Materials.ContainsKey(filename))
            {
                Materials[filename] = new Material(filename);
            }

            return Materials[filename];
        }
        public static Material GetMaterial(string filename, string shader)
        {
            if (!Materials.ContainsKey(filename))
            {
                Materials[filename] = new Material(filename,shader);
            }

            return Materials[filename];
        }
        public static Material GetMaterial(int textureid, string shader)
        {
            if (!Materials.ContainsKey(textureid.ToString()))
            {
                Materials[textureid.ToString()] = new Material(textureid, shader);
            }

            return Materials[textureid.ToString()];
        }
        public static int GetTexture(string filename)
        {
            if (!Textures.ContainsKey(filename))
            {
                Textures[filename] = Utilities.LoadTexture(filename);
            }

            return Textures[filename];
        }
        public static VBO GetModel(string filename)
        {
            if (!Models.ContainsKey(filename))
            {
                Vector3[] verts;
                Vector3[] tangents;
                Vector3[] normals;
                Vector2[] lsUV;
                int[] elements;

                Utilities.LoadOBJ(filename, out verts, out elements, out tangents,out normals, out lsUV );
                Models[filename] = new VBO(verts, elements, null, normals, lsUV);
            }

            return Models[filename];
        }
        public static Mesh GetMesh(string filename)
        {
            if (!Meshes.ContainsKey(filename) )
            {
                Vector3[] verts;
                Vector3[] tangents;
                Vector3[] normals;
                Vector2[] lsUV;
                int[] elements;

                Utilities.LoadOBJ(filename, out verts, out elements, out tangents, out normals, out lsUV);
                Meshes[filename] = new Mesh(verts, elements,tangents, normals, lsUV);
            }

            return Meshes[filename];
        }
        public static bool ProgramExists(string filename)
        {
            string filenameV = filename + ".vert";
            string filenameF = filename + ".frag";
            return Programs.ContainsKey(filename);
        }
        public static int GetProgram(string filename)
        {
            string filenameV = filename + ".vert";
            string filenameF = filename + ".frag";
            if (!Programs.ContainsKey(filename))
            {
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
                    bool over = false;
                    GL.GetShader( VertexShader, ShaderParameter.CompileStatus, out statV );
                    GL.GetShader( FragmentShader, ShaderParameter.CompileStatus, out statF );

                    #if DEBUG
                    over = true;
                    #endif

                    Console.ForegroundColor = ConsoleColor.Red;
                    if (statV == 0 || over)
                    {
                        Console.WriteLine(GL.GetShaderInfoLog(VertexShader));
                    }
                    if (statF == 0 || over)
                    {
                        Console.WriteLine(GL.GetShaderInfoLog(FragmentShader));
                    }
                    Console.ResetColor();

                    //Link them up
                    GL.LinkProgram(program);

                    //Once we're done with creating the program, we don't need the shader objects anymore (they'll persist until the program is deleted)
                    GL.DeleteShader(VertexShader);
                    GL.DeleteShader(FragmentShader);

                    Programs[filename] = program;
                }
                else
                {
                    return -1;
                } 
            }


            return Programs[filename];
        }
    }
}
