using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions
{
    class Graphics
    {
        public static bool DrawBoundingBoxes = false;

        private static Mesh box;

        public static void DrawBox(Vector3 Position, Vector3 BottomLeft, Vector3 TopRight)
        {
            if (Utilities.CurrentPass == 1) return;

            if (box == null)
            {
                box = Resource.GetMesh("debug/box.obj");
                box.mat = new Material("engine/white.png", "default");
            }
            
            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale((TopRight - BottomLeft ) / 2 );
            modelview *= Matrix4.CreateTranslation(Position);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            DrawBoundingBoxes = false;
            box.DrawSimple(modelview);
            DrawBoundingBoxes = true;
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public static void DrawLine(Matrix4 matrix, Vector3 Point1, Vector3 Point2)
        {
            GL.UseProgram(0);
            //Teehee deprecated functions
            GL.LineWidth(1.0f);
            GL.Color3(0.0f, 0.0f, 1.0f);
            GL.PushMatrix();
                GL.MultMatrix(ref matrix);
                GL.Color3(0.0f, 1.0f, 1.0f);
                GL.Begin(BeginMode.Lines);
                GL.LineWidth(9.0f);
                
                GL.Vertex3(Point1);
                GL.Vertex3(Point2);
                GL.End();
            GL.PopMatrix();
        }
    }

    class VBO
    {
        public int VertexBufferID;
        public int ColorBufferID;
        public int TexCoordBufferID;
        public int NormalBufferID;
        public int ElementBufferID;
        public int NumElements;

        /// <summary>
        /// Generate a VertexBuffer for each of Color, Normal, TextureCoordinate, Vertex, and Indices
        /// </summary>
        /// 
        public VBO(Vector3[] Vertices, int[] Indices, Vector3[] Colors = null, Vector3[] Normals = null, Vector2[] Texcoords = null)
        {
            InitializeVBO(Vertices, Indices, Colors, Normals, Texcoords);
        }

        private void InitializeVBO(Vector3[] Vertices, int[] Indices, Vector3[] Colors = null, Vector3[] Normals = null, Vector2[] Texcoords = null)
        {
            int bufferSize;

            // Color Array Buffer
            if (Colors != null)
            {
                // Generate Array Buffer Id
                GL.GenBuffers(1, out ColorBufferID);

                // Bind current context to Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBufferID);

                // Send data to buffer
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Colors.Length * sizeof(int)), Colors, BufferUsageHint.StaticDraw);

                // Validate that the buffer is the correct size
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (Colors.Length * sizeof(int) != bufferSize)
                    throw new ApplicationException("Vertex array not uploaded correctly");

                // Clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

            // Normal Array Buffer
            if (Normals != null)
            {
                // Generate Array Buffer Id
                GL.GenBuffers(1, out NormalBufferID);

                // Bind current context to Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferID);

                // Send data to buffer
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Normals.Length * Vector3.SizeInBytes), Normals, BufferUsageHint.StaticDraw);

                // Validate that the buffer is the correct size
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (Normals.Length * Vector3.SizeInBytes != bufferSize)
                    throw new ApplicationException("Normal array not uploaded correctly");

                // Clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

            // TexCoord Array Buffer
            if (Texcoords != null)
            {
                // Generate Array Buffer Id
                GL.GenBuffers(1, out TexCoordBufferID);

                // Bind current context to Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, TexCoordBufferID);

                // Send data to buffer
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Texcoords.Length * 8), Texcoords, BufferUsageHint.StaticDraw);

                // Validate that the buffer is the correct size
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (Texcoords.Length * 8 != bufferSize)
                    throw new ApplicationException("TexCoord array not uploaded correctly");

                // Clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

            // Vertex Array Buffer
            {
                // Generate Array Buffer Id
                GL.GenBuffers(1, out VertexBufferID);

                // Bind current context to Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);

                // Send data to buffer
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * Vector3.SizeInBytes), Vertices, BufferUsageHint.DynamicDraw);

                // Validate that the buffer is the correct size
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (Vertices.Length * Vector3.SizeInBytes != bufferSize)
                    throw new ApplicationException("Vertex array not uploaded correctly");

                // Clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

            // Element Array Buffer
            {
                // Generate Array Buffer Id
                GL.GenBuffers(1, out ElementBufferID);

                // Bind current context to Array Buffer ID
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferID);

                // Send data to buffer
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Length * sizeof(int)), Indices, BufferUsageHint.StaticDraw);

                // Validate that the buffer is the correct size
                GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (Indices.Length * sizeof(int) != bufferSize)
                    throw new ApplicationException("Element array not uploaded correctly");

                // Clear the buffer Binding
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }

            // Store the number of elements for the DrawElements call
            NumElements = Indices.Length;
        }

        public void Draw(BeginMode mode = BeginMode.LineLoop)
        {
            // Push current Array Buffer state so we can restore it later
            GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);

            if (VertexBufferID == 0) return;
            if (ElementBufferID == 0) return;

            if (GL.IsEnabled(EnableCap.Lighting))
            {
                // Normal Array Buffer
                if (NormalBufferID != 0)
                {
                    // Bind to the Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferID);

                    // Set the Pointer to the current bound array describing how the data ia stored
                    GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

                    // Enable the client state so it will use this array buffer pointer
                    GL.EnableClientState(ArrayCap.NormalArray);
                }
            }
            else
            {
                // Color Array Buffer (Colors not used when lighting is enabled)
                if (ColorBufferID != 0)
                {
                    // Bind to the Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBufferID);

                    // Set the Pointer to the current bound array describing how the data ia stored
                    GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(int), IntPtr.Zero);

                    // Enable the client state so it will use this array buffer pointer
                    GL.EnableClientState(ArrayCap.ColorArray);
                }
            }

            // Texture Array Buffer
            if (GL.IsEnabled(EnableCap.Texture2D))
            {
                if (TexCoordBufferID != 0)
                {
                    // Bind to the Array Buffer ID
                    GL.BindBuffer(BufferTarget.ArrayBuffer, TexCoordBufferID);

                    // Set the Pointer to the current bound array describing how the data ia stored
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 8, IntPtr.Zero);

                    // Enable the client state so it will use this array buffer pointer
                    GL.EnableClientState(ArrayCap.TextureCoordArray);
                }
            }

            // Vertex Array Buffer
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferID);

                // Set the Pointer to the current bound array describing how the data ia stored
                GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(ArrayCap.VertexArray);
            }

            // Element Array Buffer
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferID);

                // Draw the elements in the element array buffer
                // Draws up items in the Color, Vertex, TexCoordinate, and Normal Buffers using indices in the ElementArrayBuffer
                GL.DrawElements(mode, NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);

                // Could also call GL.DrawArrays which would ignore the ElementArrayBuffer and just use primitives
                // Of course we would have to reorder our data to be in the correct primitive order
            }

            // Restore the state
            GL.PopClientAttrib();
        }

        public void Remove()
        {
            if (VertexBufferID != 0) GL.DeleteBuffers(1, ref VertexBufferID);
            if (ElementBufferID != 0) GL.DeleteBuffers(1, ref ElementBufferID);

            if (TexCoordBufferID != 0) GL.DeleteBuffers(1, ref TexCoordBufferID);
            if (NormalBufferID != 0) GL.DeleteBuffers(1, ref NormalBufferID);
            if (ColorBufferID != 0) GL.DeleteBuffers(1, ref ColorBufferID);

        }
    }

    class Mesh
    {
        public BeginMode DrawMode = BeginMode.Triangles;
        public BufferUsageHint UsageHint = BufferUsageHint.StaticDraw;
        public Vector3 Color = Vector3.One;
        public BoundingBox BBox = new BoundingBox();

        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Angle { get; set; }

        public const int INDEX_BUFFER  = 0;
        public const int POS_VB        = 1;
        public const int NORMAL_VB     = 2;
        public const int TEXCOORD_VB   = 3;
        public const int TANGENT_VB    = 4;

        int VAO = 0;
        public int[] buffers = new int[5];
        public Material mat;

        int[] BaseIndex;
        int NumIndices = -1;

        public class BoundingBox
        {
            public Vector3 BottomLeft = -Vector3.One * 0.1f;
            public Vector3 TopRight = Vector3.One * 0.1f;
        }

        public Mesh()
        {
            this.Scale = Vector3.One;
        }
        public Mesh(string filename)
        {
            this.LoadMesh(filename);
            this.Scale = Vector3.One;
        }
        public Mesh(Vector3[] verts, int[] elements, Vector3[] tangents, Vector3[] normals = null, Vector2[] texcoords = null)
        {
            loadMesh(verts, elements, tangents, normals, texcoords);
            this.Scale = Vector3.One;
        }
        public Mesh(Vector3[] verts, int[] elements, Vector3[] tangents)
        {
            loadMesh(verts, elements, tangents);
            this.Scale = Vector3.One;
        }

        public void LoadMesh(string filename)
        {
            Vector3[] verts;
            Vector3[] tangents;
            Vector3[] normals;
            Vector2[] lsUV;
            int[] elements;
            Mesh.BoundingBox boundingbox;

            Utilities.LoadOBJ(filename, out verts, out elements, out tangents, out normals, out lsUV, out boundingbox );
            this.BBox = boundingbox;

            loadMesh(verts, elements, tangents, normals, lsUV);
        }

        public void UpdateMesh(string filename)
        {
            Vector3[] verts;
            Vector3[] tangents;
            Vector3[] normals;
            Vector2[] lsUV;
            int[] elements;
            Mesh.BoundingBox boundingbox;

            Utilities.LoadOBJ(filename, out verts, out elements, out tangents, out normals, out lsUV, out boundingbox);
            this.BBox = boundingbox;

            UpdateMesh(verts, elements, tangents, normals, lsUV);
        }

        public void UpdateMesh(Vector3[] verts, int[] elements, Vector3[] tangents, Vector3[] normals = null, Vector2[] lsUV = null)
        {
            if (NumIndices < 0) //If we've never set this, we need to create our array buffer
            {
                GL.GenVertexArrays(1, out VAO);
                GL.BindVertexArray(VAO);

                //Create the buffers for the vertices attributes
                GL.GenBuffers(5, buffers);
            }

            NumIndices = elements.Length;
            BaseIndex = elements;

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[POS_VB]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * Vector3.SizeInBytes), verts, this.UsageHint);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            if (lsUV != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[TEXCOORD_VB]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(lsUV.Length * Vector2.SizeInBytes), lsUV, this.UsageHint);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            }

            if (normals != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[NORMAL_VB]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * Vector3.SizeInBytes), normals, this.UsageHint);
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            }

            //tangent buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[TANGENT_VB]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tangents.Length * Vector3.SizeInBytes), tangents, this.UsageHint);
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers[INDEX_BUFFER]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(int)), elements, this.UsageHint);


            GL.BindVertexArray(0);
        }

        public bool PointWithinBox(Vector3 point)
        {
            Vector3 boxPosL = BBox.BottomLeft + this.Position;
            Vector3 boxPosR = BBox.TopRight + this.Position;

            return (point.X > boxPosL.X && point.X < boxPosR.X &&
                    point.Y > boxPosL.Y && point.Y < boxPosR.Y &&
                    point.Z > boxPosL.Z && point.Y < boxPosR.Z  );
        }

        public bool LineIntersectsBox(Vector3 L1, Vector3 L2, ref Vector3 Hit)
        {
            Vector3 B1 = this.BBox.BottomLeft + this.Position; ;
            Vector3 B2 = this.BBox.TopRight + this.Position; ;
            if (L2.X < B1.X && L1.X < B1.X) return false;
            if (L2.X > B2.X && L1.X > B2.X) return false;
            if (L2.Y < B1.Y && L1.Y < B1.Y) return false;
            if (L2.Y > B2.Y && L1.Y > B2.Y) return false;
            if (L2.Z < B1.Z && L1.Z < B1.Z) return false;
            if (L2.Z > B2.Z && L1.Z > B2.Z) return false;
            if (L1.X > B1.X && L1.X < B2.X &&
                L1.Y > B1.Y && L1.Y < B2.Y &&
                L1.Z > B1.Z && L1.Z < B2.Z)
            {
                Hit = L1;
                return true;
            }
            if ((GetIntersection(L1.X - B1.X, L2.X - B1.X, L1, L2, ref Hit) && InBox(Hit, B1, B2, 1))
              || (GetIntersection(L1.Y - B1.Y, L2.Y - B1.Y, L1, L2, ref Hit) && InBox(Hit, B1, B2, 2))
              || (GetIntersection(L1.Z - B1.Z, L2.Z - B1.Z, L1, L2, ref Hit) && InBox(Hit, B1, B2, 3))
              || (GetIntersection(L1.X - B2.X, L2.X - B2.X, L1, L2, ref Hit) && InBox(Hit, B1, B2, 1))
              || (GetIntersection(L1.Y - B2.Y, L2.Y - B2.Y, L1, L2, ref Hit) && InBox(Hit, B1, B2, 2))
              || (GetIntersection(L1.Z - B2.Z, L2.Z - B2.Z, L1, L2, ref Hit) && InBox(Hit, B1, B2, 3)))
                return true;

            return false;
        }

        bool GetIntersection(float fDst1, float fDst2, Vector3 P1, Vector3 P2, ref Vector3 Hit)
        {
            if ((fDst1 * fDst2) >= 0.0f) return false;
            if (fDst1 == fDst2) return false;
            Hit = P1 + (P2 - P1) * (-fDst1 / (fDst2 - fDst1));
            return true;
        }

        bool InBox(Vector3 Hit, Vector3 B1, Vector3 B2, int Axis)
        {
            if (Axis == 1 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            if (Axis == 2 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.X > B1.X && Hit.X < B2.X) return true;
            if (Axis == 3 && Hit.X > B1.X && Hit.X < B2.X && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            return false;
        }

        private void loadMesh(Vector3[] verts, int[] elements, Vector3[] tangents, Vector3[] normals = null, Vector2[] lsUV = null )
        {
            GL.GenVertexArrays(1, out VAO);
            GL.BindVertexArray(VAO);

            //Create the buffers for the vertices attributes
            GL.GenBuffers(5, buffers);

            NumIndices = elements.Length;
            BaseIndex = elements;

            //Create their buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[POS_VB]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * Vector3.SizeInBytes), verts, this.UsageHint);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            if (lsUV != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[TEXCOORD_VB]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(lsUV.Length * Vector2.SizeInBytes), lsUV, this.UsageHint);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            }

            if (normals != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[NORMAL_VB]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * Vector3.SizeInBytes), normals, this.UsageHint);
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            }

            //tangent buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[TANGENT_VB]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tangents.Length * Vector3.SizeInBytes), tangents, this.UsageHint);
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers[INDEX_BUFFER]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(int)), elements, this.UsageHint);


            GL.BindVertexArray(0);
        }

        public void DrawSimple(Matrix4 vmatrix)
        {
            render(Utilities.ViewMatrix, vmatrix, Utilities.ProjectionMatrix);
        }

        public void Draw()
        {
            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale(Scale);
            modelview *= Matrix4.CreateRotationZ(this.Angle.Z);
            modelview *= Matrix4.CreateRotationX(this.Angle.X);
            modelview *= Matrix4.CreateRotationY(this.Angle.Y);

            modelview *= Matrix4.CreateTranslation(Position);

            render(Utilities.ViewMatrix, modelview, Utilities.ProjectionMatrix);
        }

        private void render(Matrix4 mmatrix, Matrix4 vmatrix, Matrix4 pmatrix)
        {
            GL.BindVertexArray(this.VAO);
            if (mat != null)
            {
                mat.Properties.Color = this.Color;
                mat.BindMaterial();

                //Set the matrix to the shader
                GL.UniformMatrix4(mat.locMMatrix, false, ref mmatrix);
                GL.UniformMatrix4(mat.locVMatrix, false, ref vmatrix);
                GL.UniformMatrix4(mat.locPMatrix, false, ref pmatrix);

                GL.Uniform1(mat.locTime, (float)Utilities.Time);

            }
            
            //GL.DrawElementsBaseVertex(BeginMode.Triangles, NumIndices, DrawElementsType.UnsignedInt, BaseIndex, 0);
            GL.DrawElements(DrawMode, NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero);
            //Matrix4 newmatlmao = vmatrix * Matrix4.CreateTranslation(new Vector3(10.0f, 10.0f, 0));
            //GL.UniformMatrix4(mat.locVMatrix, false, ref newmatlmao);
            //GL.DrawElements(BeginMode.Triangles, NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);

            //Draw the bounding box
            if (Graphics.DrawBoundingBoxes)
            {
                Graphics.DrawBox(this.Position , this.BBox.BottomLeft, this.BBox.TopRight);

            }
        }

        public void Remove()
        {
            GL.DeleteVertexArrays(1, ref this.VAO);
        }
    }

    class Material
    {
        public MaterialProperties Properties = new MaterialProperties();
        public string Name { get; private set; }

        public int locMMatrix = -1;
        public int locVMatrix = -1;
        public int locPMatrix = -1;
        public int locTime = -1;
        public int locBaseTexture = -1;
        public int locNormalMap = -1;
        public int locShadowMap = -1;
        public int locShadowMapTexture = -1;
        public int locColor = -1;
        public int locSpecularIntensity = -1;
        public int locSpecularPower = -1;

        /// <summary>
        /// Initialize a new material object - all by yourself!
        /// </summary>
        public Material( MaterialProperties properties, string name = "DYNAMIC" )
        {
            Properties = properties;
            setShader(Properties.ShaderProgram);
            Name = name;
        }

        public Material(string filename)
        {
            Properties.BaseTexture = Resource.GetTexture(filename);
            Name = filename;
        }

        public Material(string filename, string shaderfile)
        {
            Properties.BaseTexture = Resource.GetTexture(filename);
            Name = filename;
            createShader(shaderfile);
        }

        public Material(int texturebuffer, string shaderfile)
        {
            Properties.BaseTexture = texturebuffer;
            Name = "DYNAMIC";
            createShader(shaderfile);
        }

        public Material(int texturebuffer, int Program )
        {
            Properties.BaseTexture = texturebuffer;
            Name = "DYNAMIC";
            setShader(Program);
        }

        public Material(string filename, int Program)
        {
            Properties.BaseTexture = Resource.GetTexture(filename);
            Name = filename;
            setShader(Program);
        }

        public void SetProperties(MaterialProperties properties)
        {
            Properties = properties;
            setShader(Properties.ShaderProgram);
        }

        public void SetShader(string shaderfile)
        {
            createShader(shaderfile);
        }

        public void SetShader(int Program)
        {
            setShader(Program);
        }

        private void setShader(int Program)
        {
            if (GL.IsProgram(Program))
            {
                Properties.ShaderProgram = Program;
                GL.UseProgram(Program);

                //Cache some uniform locations
                locMMatrix = GL.GetUniformLocation(Program, "_mmatrix");
                locVMatrix = GL.GetUniformLocation(Program, "_vmatrix");
                locPMatrix = GL.GetUniformLocation(Program, "_pmatrix");
                locTime = GL.GetUniformLocation(Program, "_time");
                locBaseTexture = GL.GetUniformLocation(Program, "sampler");
                locNormalMap = GL.GetUniformLocation(Program, "sampler_normal");
                locShadowMap = GL.GetUniformLocation(Program, "sampler_shadow");
                locShadowMapTexture = GL.GetUniformLocation(Program, "sampler_shadow_tex");
                locColor = GL.GetUniformLocation(Program, "_color");
                locSpecularIntensity = GL.GetUniformLocation(Program, "gMatSpecularIntensity");
                locSpecularPower = GL.GetUniformLocation(Program, "gSpecularPower");

                //Bind relevant sampler locations
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.Uniform1(locBaseTexture, 0);

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.Uniform1(locNormalMap, 1);

                GL.ActiveTexture(TextureUnit.Texture2);
                GL.Uniform1(locShadowMap, 2);

                GL.ActiveTexture(TextureUnit.Texture3);
                GL.Uniform1(locShadowMapTexture, 3);

                GL.ActiveTexture(TextureUnit.Texture0);
            }
        }

        private void createShader(string shaderfile)
        {
            if (Resource.ProgramExists(shaderfile) && GL.IsProgram(Properties.ShaderProgram)) return;
            removeShaders();

            Properties.ShaderProgram = Resource.GetProgram(shaderfile);

            if (Properties.ShaderProgram != -1)
            {
                //GL.LinkProgram(Program);
                GL.UseProgram(Properties.ShaderProgram);

                //Cache some uniform locations
                locMMatrix = GL.GetUniformLocation(Properties.ShaderProgram, "_mmatrix");
                locVMatrix = GL.GetUniformLocation(Properties.ShaderProgram, "_vmatrix");
                locPMatrix = GL.GetUniformLocation(Properties.ShaderProgram, "_pmatrix");
                locTime = GL.GetUniformLocation(Properties.ShaderProgram, "_time");
                locBaseTexture = GL.GetUniformLocation(Properties.ShaderProgram, "sampler");
                locNormalMap = GL.GetUniformLocation(Properties.ShaderProgram, "sampler_normal");
                locShadowMap = GL.GetUniformLocation(Properties.ShaderProgram, "sampler_shadow");
                locShadowMapTexture = GL.GetUniformLocation(Properties.ShaderProgram, "sampler_shadow_tex");
                locColor = GL.GetUniformLocation(Properties.ShaderProgram, "_color");
                locSpecularIntensity = GL.GetUniformLocation(Properties.ShaderProgram, "gMatSpecularIntensity");
                locSpecularPower = GL.GetUniformLocation(Properties.ShaderProgram, "gSpecularPower");

                //Bind relevant sampler locations
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.Uniform1(locBaseTexture, 0);

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.Uniform1(locNormalMap, 1);

                GL.ActiveTexture(TextureUnit.Texture2);
                GL.Uniform1(locShadowMap, 2);

                GL.ActiveTexture(TextureUnit.Texture3);
                GL.Uniform1(locShadowMapTexture, 3);

                GL.ActiveTexture(TextureUnit.Texture0);

                //Console.WriteLine("VMatrix: {0}, PMatrix: {1}, MMatrixL {2}, Time: {3}", locVMatrix, locPMatrix, locMMatrix, locTime);
            }
        }

        private void removeShaders()
        {
            GL.DeleteProgram(Properties.ShaderProgram);
        }

        public void BindMaterial()
        {
            //Bind the base texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, Properties.BaseTexture);

            //Bind the normal map
            GL.ActiveTexture(TextureUnit.Texture1);
            if (Properties.NormalMapTexture > 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, Properties.NormalMapTexture );             
            }
            else
            {
                 GL.BindTexture(TextureTarget.Texture2D, Utilities.NormalUp.Properties.BaseTexture);
            }

            //The shadow map is bound to texture unit three in Techniques.cs. It only needs to be bound once.

            //Bind the program and set some parameters to the lighting
            GL.UseProgram(Properties.ShaderProgram);

            GL.Uniform1(locSpecularIntensity, Properties.SpecularIntensity);
            GL.Uniform1(locSpecularPower, Properties.SpecularPower);

            GL.Uniform3(this.locColor, this.Properties.Color);
        }
    }

    class MaterialProperties
    {
        public int ShaderProgram;
        public int BaseTexture;
        public int NormalMapTexture;
        public float SpecularPower;
        public float SpecularIntensity;
        public Vector3 Color;

        public MaterialProperties()
        {
            Color = Vector3.One;
        }
    }

    class FBO
    {
        private int fbo = 0;
        public int shadowMap = 0;

        public FBO()
        {

        }
        ~FBO()
        {
            if (fbo != 0)
            {
                //GL.DeleteFramebuffers(1, ref fbo);
            }

            if (shadowMap != 0)
            {
               // GL.DeleteTexture(shadowMap);
            }
        }

        public bool Init(int Width, int Height)
        {
            //Create our shadow framebuffer
            GL.GenFramebuffers(1, out fbo);

            GL.GenTextures(1, out shadowMap);
            GL.BindTexture(TextureTarget.Texture2D, shadowMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.None);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            //Attach the depth texture to the framebuffer
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, fbo);
            GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadowMap, 0);

            //Tell opengl we are not going to render into the color buffer
            GL.DrawBuffer(DrawBufferMode.None);

            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Framebuffer error!! Status: " + status.ToString());
                Console.ResetColor();

                return false;
            }

            return true;
        }

        public void BindForWriting()
        {
            GL.CullFace(CullFaceMode.Front);
            //GL.ActiveTexture(TextureUnit.Texture2);
            //GL.Disable(EnableCap.Texture2D);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, fbo);
        }

        public void BindForReading()
        {
            GL.CullFace(CullFaceMode.Back);
            //GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, shadowMap);
            GL.ActiveTexture(TextureUnit.Texture0);
        }
    }
}
