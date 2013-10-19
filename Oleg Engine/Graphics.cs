using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine
{
    public class Graphics
    {
        public static Frustum ViewFrustum;

        public static bool ShouldDrawBoundingBoxes = false;
        public static bool ShouldDrawBoundingSpheres = false;
        public static bool ShouldDrawNormals = false;
        public static bool ShouldDrawFrustum = false;

        private static Mesh box;
        private static Mesh sphere;
        private static Material dbgWhite;

        #region Debug drawing functions

        public static void Init()
        {
            dbgWhite = Resource.GetMaterial("engine/white_simple");
            box = Resource.GetMesh("engine/box.obj");
            box.mat = dbgWhite;
            box.ShouldDrawDebugInfo = false;

            sphere = Resource.GetMesh("engine/ball.obj");
            sphere.mat = dbgWhite;
            sphere.ShouldDrawDebugInfo = false;

            ViewFrustum = new Frustum();
        }

        public static void DrawDebug()
        {
            if (ViewFrustum != null && ShouldDrawFrustum)
            {
                dbgWhite.BindMaterial();
                GL.UniformMatrix4(dbgWhite.locVMatrix, false, ref Matrix4.Identity);
                GL.LineWidth(1.5f);

                ViewFrustum.DrawLines();
                //ViewFrustum.DrawPlanes();
            }
        }

        public static void DrawLine(Vector3 Position1, Vector3 Position2, bool SetMaterial = true )
        {
            if (SetMaterial)
            {
                dbgWhite.BindMaterial();
                GL.UniformMatrix4(dbgWhite.locVMatrix, false, ref Matrix4.Identity);
                GL.LineWidth(1.5f);
            }

            GL.Begin(BeginMode.Lines);
            GL.Vertex3(Position1);
            GL.Vertex3(Position2);
            GL.End();
        }

        public static void DrawBox(Vector3 Position, Vector3 BottomLeft, Vector3 TopRight)
        {
            if (Utilities.CurrentPass == 1) return;
            
            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale((TopRight - BottomLeft ) / 2 );
            modelview *= Matrix4.CreateTranslation(Position + (BottomLeft + TopRight) / 2);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            box.DrawSimple(modelview);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public static void DrawSphere(Vector3 Position, float Radius)
        {
            if (Utilities.CurrentPass == 1) return;

            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale(Radius / 2);
            modelview *= Matrix4.CreateTranslation(Position);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            sphere.DrawSimple(modelview);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        public static void DrawNormals(Mesh m )
        {
            if (m.DBG_Vertices == null || m.DBG_Elements == null) return;

            dbgWhite.BindMaterial();
            GL.UniformMatrix4(dbgWhite.locVMatrix, false, ref Matrix4.Identity);
            GL.LineWidth(1.5f);

            for (int i = 0; i < m.DBG_Elements.Length; i++)
            {
                int element = m.DBG_Elements[i];
                if (element > m.DBG_Vertices.Length) continue;

                DrawLine(m.Position + m.DBG_Vertices[element].Position, m.Position + m.DBG_Vertices[element].Position + m.DBG_Vertices[element].Normal, false);
            }
        }

        #endregion

        #region Default graphics library function wrappers
        public static void EnableBlending(bool enabled)
        {
            if (enabled) GL.Enable(EnableCap.Blend);
            else GL.Disable(EnableCap.Blend);
        }
        #endregion
    }

    public class Mesh
    {
        public BeginMode DrawMode = BeginMode.Triangles;
        public BufferUsageHint UsageHint = BufferUsageHint.StaticDraw;
        public Vector3 Color = Vector3.One;
        public float Alpha = 1.0f;
        public BoundingBox BBox = new BoundingBox();
        public bool ShouldDrawDebugInfo = true;

        public Vector3 Position { get; set; }
        public Vector3 PositionOffset { get; set; }
        public Vector3 Scale { get; set; }
        public Angle Angles { get; set; }

        public const int INDEX_BUFFER  = 0;
        public const int VERTEX_VB     = 1;

        int VAO = -1;
        public int[] buffers = new int[2];
        public Material mat;

        int[] BaseIndex;
        int NumIndices = -1;

        //Debug properties
        public Vertex[] DBG_Vertices;
        public int[] DBG_Elements;

        public static int MeshesDrawn = 0;
        public static int MeshesTotal = 0;
        private static double LastDrawTime = 0;

        public class BoundingBox
        {
            private Vector3 vecNegative = -Vector3.One * 0.1f;
            private Vector3 vecPositive = Vector3.One * 0.1f;
            private float fRadius = 1.0f;
            private Vector3 vecScale = Vector3.One;

            public Vector3 Negative {
                get
                {
                    return vecNegative;
                }
                set
                {
                    if (!NegativeSet)
                    {
                        NegativeSet = true;
                    }
                    vecNegative = value;
                    fRadius = RadiusFromBoundingBox(this);
                }
            }
            public Vector3 Positive { 
                get
                {
                    return vecPositive;   
                }
                set
                {
                    if (!PositiveSet)
                    {
                        PositiveSet = true;
                    }
                    vecPositive = value;
                    fRadius = RadiusFromBoundingBox(this);
                }
            }
            public Vector3 Scale
            {
                get
                {
                    return vecScale;
                }
                set
                {
                    vecScale = value;
                    fRadius = RadiusFromBoundingBox(this);
                }
            }

            public float Radius
            {
                get
                {
                    return fRadius;
                }
                set
                {
                    fRadius = value;
                }
            }

            public bool NegativeSet { get; private set; }
            public bool PositiveSet { get; private set; }

            public BoundingBox()
            {
                NegativeSet = false;
                PositiveSet = false;
            }

            public BoundingBox(Vector3 Negative, Vector3 Positive)
            {
                this.Negative = Negative;
                this.Positive = Positive;
            }

            public Vector3 GetVertexP(Vector3 Normal)
            {
                Vector3 res = this.Negative;
                if (Normal.X > 0)
                    res.X += this.Positive.X * Scale.X;

                if (Normal.Y > 0)
                    res.Y += this.Positive.Y * Scale.Y;

                if (Normal.Z > 0)
                    res.Z += this.Positive.Z * Scale.Z;

                return res;
            }

            public Vector3 GetVertexN(Vector3 Normal)
            {
                Vector3 res = this.Negative;
                if (Normal.X < 0)
                    res.X += this.Positive.X * Scale.X;

                if (Normal.Y < 0)
                    res.Y += this.Positive.Y * Scale.Y;

                if (Normal.Z < 0)
                    res.Z += this.Positive.Z * Scale.Z;

                return res;
            }

            private static Vector3 absVec(Vector3 vec)
            {
                return new Vector3(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));
            }

            private static float Distance(Vector3 a, Vector3 b)
            {
                return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
            }

            public static float RadiusFromBoundingBox(BoundingBox b)
            {
                return Math.Abs(Distance(b.Negative.Multiply(b.Scale), b.Positive.Multiply(b.Scale)));
            }

            /// <summary>
            /// Create a bounding box from a known radius
            /// </summary>
            /// <param name="radius">The radius of the bounding sphere</param>
            /// <returns>A newly created bounding box from the maxiumum size of the sphere. Note this is not a perfect reconstruction for a proper box.</returns>
            public static BoundingBox BoundingBoxFromRadius(float radius)
            {
                return new BoundingBox(new Vector3(radius), new Vector3(-radius));
            }
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
        public Mesh(Vertex[] verts, int[] elements)
        {
            UpdateMesh(verts, elements);
            this.Scale = Vector3.One;
        }

        public void LoadMesh(string filename)
        {
            Vertex[] verts;
            int[] elements;
            Mesh.BoundingBox boundingbox;

            MeshGenerator.LoadOBJ(filename, out verts, out elements, out boundingbox);
            this.BBox = boundingbox;

            UpdateMesh(verts, elements);
        }

        /// <summary>
        /// Update an existing VAO with new vertex data
        /// Depending on what you've set your <code>UsageHint</code> to, this may potentially be a performance hit.
        /// </summary>
        /// <param name="filename">The filename to load from the disk</param>
        public void UpdateMesh(string filename)
        {
            Vertex[] verts;
            int[] elements;
            Mesh.BoundingBox boundingbox;

            MeshGenerator.LoadOBJ(filename, out verts, out elements, out boundingbox);
            this.BBox = boundingbox;

            UpdateMesh(verts, elements);
        }

        /// <summary>
        /// Update an existing VAO with new vertex data
        /// Depending on what you've set your <code>UsageHint</code> to, this may potentially be a performance hit.
        /// </summary>
        /// <param name="verts">The array of vertices to update the VAO with</param>
        /// <param name="elements">The array of elements to update the VAO with</param>
        public void UpdateMesh(Vertex[] verts, int[] elements)
        {
            if (verts == null || elements == null) return;
            if (VAO < 0) //If we've never set this, we need to create our array buffer
            {
                GL.GenVertexArrays(1, out VAO);
                GL.BindVertexArray(VAO);

                //Create the buffers for the vertices attributes
                GL.GenBuffers(2, buffers);
            }

            NumIndices = elements.Length;
            BaseIndex = elements;

            //Create their buffers
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[VERTEX_VB]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * Vertex.SizeInBytes), verts, this.UsageHint);

            //Positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);
            //UVs
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, true, Vertex.SizeInBytes, 12);
            //Normals
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, Vertex.SizeInBytes, 20);
            //Tangents
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, true, Vertex.SizeInBytes, 32);
            //Color
            //GL.EnableVertexAttribArray(4);
            //GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, true, vertexStride, 11);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers[INDEX_BUFFER]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(int)), elements, this.UsageHint);

            //Reset it back
            GL.BindVertexArray(0);

            DBG_Elements = elements;
            DBG_Vertices = verts;
        }

        #region Point Testing

        public bool PointWithinBox(Vector3 point)
        {
            Vector3 boxPosL = BBox.Negative + this.Position;
            Vector3 boxPosR = BBox.Positive + this.Position;

            return (point.X > boxPosL.X && point.X < boxPosR.X &&
                    point.Y > boxPosL.Y && point.Y < boxPosR.Y &&
                    point.Z > boxPosL.Z && point.Y < boxPosR.Z  );
        }

        public bool LineIntersectsBox(Vector3 L1, Vector3 L2, ref Vector3 Hit)
        {
            Vector3 B1 = this.BBox.Negative + this.Position; ;
            Vector3 B2 = this.BBox.Positive + this.Position; ;
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



        #endregion

        public void DrawSimple(Matrix4 vmatrix)
        {
            render(Utilities.ViewMatrix, vmatrix, Utilities.ProjectionMatrix);
        }

        public void Draw()
        {
            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale(Scale);

            modelview *= Matrix4.CreateTranslation(PositionOffset);
            
            modelview *= Matrix4.CreateRotationZ(this.Angles.Roll * Utilities.F_DEG2RAD);
            modelview *= Matrix4.CreateRotationX(this.Angles.Pitch * Utilities.F_DEG2RAD);
            modelview *= Matrix4.CreateRotationY(this.Angles.Yaw * Utilities.F_DEG2RAD);

            modelview *= Matrix4.CreateTranslation(Position);
            

            this.BBox.Scale = this.Scale;
            render(Utilities.ViewMatrix, modelview, Utilities.ProjectionMatrix);
        }

        private void render(Matrix4 mmatrix, Matrix4 vmatrix, Matrix4 pmatrix)
        {
            if (LastDrawTime != Utilities.Time)
            {
                LastDrawTime = Utilities.Time;
                MeshesTotal = 0;
                MeshesDrawn = 0;
            }


            if (Utilities.CurrentPass > 1 && (ShouldDrawDebugInfo))
            {
                MeshesTotal++;

                //this.Color = Vector3.UnitY;
                if (Graphics.ViewFrustum.SphereInFrustum(this.Position + this.PositionOffset + (this.BBox.Positive + this.BBox.Negative) / 2, BBox.Radius) == Frustum.FrustumState.OUTSIDE) { return; }
                //if (Graphics.ViewFrustum.BoxInFrustum(this.BBox, this.Position) == Frustum.FrustumState.OUTSIDE) { this.Color = Vector3.UnitX; MeshesDrawn--; }
                //if (Graphics.ViewFrustum.PointInFrustum((this.BBox.BottomLeft + this.BBox.TopRight) / 2 + this.Position) == Frustum.FrustumState.OUTSIDE) { this.Color = Vector3.UnitX; MeshesDrawn--;  }

                MeshesDrawn++;
            }

            GL.BindVertexArray(this.VAO);
            if (mat == null) mat = Utilities.ErrorMat;

            //Automatically update the material properties based on mesh settings
            mat.Properties.Color = this.Color;
            mat.Properties.Alpha = this.Alpha;
            mat.BindMaterial();

            //Set the matrix to the shader
            GL.UniformMatrix4(mat.locMMatrix, false, ref mmatrix);
            GL.UniformMatrix4(mat.locVMatrix, false, ref vmatrix);
            GL.UniformMatrix4(mat.locPMatrix, false, ref pmatrix);

            GL.Uniform1(mat.locTime, (float)Utilities.Time);

            if (mat.Properties.NoCull) { GL.Disable(EnableCap.CullFace); }
            if (mat.Properties.AlphaTest) { GL.Enable(EnableCap.AlphaTest); }
            
            //Draw it
            GL.DrawElements(DrawMode, NumIndices, DrawElementsType.UnsignedInt, IntPtr.Zero);

            if (mat.Properties.AlphaTest) { GL.Disable(EnableCap.AlphaTest); }
            if (mat.Properties.NoCull) { GL.Enable(EnableCap.CullFace); }

            GL.BindVertexArray(0);

            //Draw optional debug stuff
            if (this.ShouldDrawDebugInfo)
            {
                //Draw the bounding box
                if (Graphics.ShouldDrawBoundingBoxes)
                {
                    Graphics.DrawBox(this.Position, this.BBox.Negative, this.BBox.Positive);
                }

                if (Graphics.ShouldDrawBoundingSpheres)
                {
                    Graphics.DrawSphere(this.Position + (this.BBox.Positive + this.BBox.Negative ) / 2, this.BBox.Radius);
                }

                if (Graphics.ShouldDrawNormals)
                {
                    Graphics.DrawNormals(this);
                }
            }
        }

        public void Remove()
        {
            GL.DeleteVertexArrays(1, ref this.VAO);
        }
    }

    public class MeshGroup : ICollection<Mesh>
    {
        public Vector3 Scale { get; set; }
        public Vector3 Angle { get; set; }
        public Vector3 Position { get; set; }

        //Private inner collection of meshes
        private List<Mesh> meshes = new List<Mesh>();

        public MeshGroup()
        {
            this.Scale = Vector3.One;
        }

        public MeshGroup(Mesh[] meshArray)
        {
            meshes = meshArray.ToList<Mesh>();

            this.Scale = Vector3.One;
        }

        public MeshGroup(List<Mesh> meshList)
        {
            meshes = meshList;

            this.Scale = Vector3.One;
        }

        /// <summary>
        /// Draw the group of meshes
        /// </summary>
        public void Draw()
        {
            Matrix4 modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale(Scale);
            modelview *= Matrix4.CreateRotationZ(this.Angle.Z);
            modelview *= Matrix4.CreateRotationX(this.Angle.X);
            modelview *= Matrix4.CreateRotationY(this.Angle.Y);

            modelview *= Matrix4.CreateTranslation(Position);

            foreach (Mesh m in this)
            {
                m.DrawSimple(modelview);
            }
        }

        public IEnumerator<Mesh> GetEnumerator()
        {
            return meshes.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool isRO = false;

        public bool Contains(Mesh item)
        {
            return meshes.Contains(item);
        }

        public bool Contains(Mesh item, EqualityComparer<Mesh> comp)
        {
            return meshes.Contains(item, comp);
        }

        public void Add( Mesh m)
        {
            meshes.Add(m);
        }

        public void Clear()
        {
            meshes.Clear();
        }

        public void CopyTo(Mesh[] array, int arrayIndex)
        {
            meshes.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return meshes.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return isRO; }
        }

        public bool Remove(Mesh item)
        {
            return meshes.Remove(item);
        }
    }

    public class Material
    {
        private static int LastShader = -1;

        public MaterialProperties Properties = new MaterialProperties();
        public string Name { get; private set; }

        public int locMMatrix = -1;
        public int locVMatrix = -1;
        public int locPMatrix = -1;
        public int locTime = -1;
        public int locBaseTexture = -1;
        public int locNormalMap = -1;
        public int locSpecMap = -1;
        public int locAlphaMap = -1;
        public int locShadowMap = -1;
        public int locShadowMapTexture = -1;
        public int locColor = -1;
        public int locSpecularIntensity = -1;
        public int locSpecularPower = -1;
        public int locAlpha = -1;
        public int locCheap = -1;

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
                locSpecMap = GL.GetUniformLocation(Program, "sampler_spec");
                locAlphaMap = GL.GetUniformLocation(Program, "sampler_alpha");
                locShadowMapTexture = GL.GetUniformLocation(Program, "sampler_shadow_tex");
                locColor = GL.GetUniformLocation(Program, "_color");
                locSpecularIntensity = GL.GetUniformLocation(Program, "gMatSpecularIntensity");
                locSpecularPower = GL.GetUniformLocation(Program, "gSpecularPower");
                locAlpha = GL.GetUniformLocation(Program, "gAlpha");
                locCheap = GL.GetUniformLocation(Program, "gCheap");

                //Bind relevant sampler locations
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.Uniform1(locBaseTexture, 0);

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.Uniform1(locNormalMap, 1);

                GL.ActiveTexture(TextureUnit.Texture2);
                GL.Uniform1(locShadowMap, 2);

                GL.ActiveTexture(TextureUnit.Texture3);
                GL.Uniform1(locShadowMapTexture, 3);

                GL.ActiveTexture(TextureUnit.Texture4);
                GL.Uniform1(locSpecMap, 4);

                GL.ActiveTexture(TextureUnit.Texture5);
                GL.Uniform1(locAlphaMap, 5);

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
                locSpecMap = GL.GetUniformLocation(Properties.ShaderProgram, "sampler_spec");
                locAlphaMap = GL.GetUniformLocation(Properties.ShaderProgram, "sampler_alpha");
                locShadowMapTexture = GL.GetUniformLocation(Properties.ShaderProgram, "sampler_shadow_tex");
                locColor = GL.GetUniformLocation(Properties.ShaderProgram, "_color");
                locSpecularIntensity = GL.GetUniformLocation(Properties.ShaderProgram, "gMatSpecularIntensity");
                locSpecularPower = GL.GetUniformLocation(Properties.ShaderProgram, "gSpecularPower");
                locAlpha = GL.GetUniformLocation(Properties.ShaderProgram, "gAlpha");
                locCheap = GL.GetUniformLocation(Properties.ShaderProgram, "gCheap");

                //BASETEXTURE
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.Uniform1(locBaseTexture, 0);

                //NORMAL MAP
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.Uniform1(locNormalMap, 1);

                //SHADOW MAP
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.Uniform1(locShadowMap, 2);

                //SHADOWMAP OVERLAY TEXTURE
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.Uniform1(locShadowMapTexture, 3);

                //SPECULARITY MAP
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.Uniform1(locSpecMap, 4);

                //ALPHA MAP
                GL.ActiveTexture(TextureUnit.Texture5);
                GL.Uniform1(locAlphaMap, 5);

                GL.ActiveTexture(TextureUnit.Texture0);
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
            GL.BindTexture(this.Properties.TextureType, this.GetCurrentTexture());

            //Bind the normal map, if it exists
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, (Properties.NormalMapTexture > 0) ? Properties.NormalMapTexture : Utilities.NormalTex);

            //Bind the specularity map, if it exists
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, (Properties.SpecMapTexture > 0) ? Properties.SpecMapTexture : Utilities.SpecTex);

            //Bind the alpha map, if it exists
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, (Properties.AlphaMapTexture > 0) ? Properties.AlphaMapTexture : Utilities.AlphaTex);

            //The shadow map is bound to texture unit three in Techniques.cs. It only needs to be bound once.

            //Bind the program and set some parameters to the lighting
            if (LastShader != Properties.ShaderProgram)
            {
                GL.UseProgram(Properties.ShaderProgram);
                LastShader = Properties.ShaderProgram;
            }

            GL.Uniform1(locSpecularIntensity, Properties.SpecularIntensity);
            GL.Uniform1(locSpecularPower, Properties.SpecularPower);

            GL.Uniform3(this.locColor, this.Properties.Color);
            GL.Uniform1(locAlpha, this.Properties.Alpha);

            //Bind the parameters for fog
            FogTechnique.UpdateUniforms(Properties.ShaderProgram);
        }

        public int GetCurrentTexture()
        {
            if (this.Properties.IsAnimated && this.Properties.BaseTextures.Length > 0 && Utilities.Time > this.Properties._NextFrameChange)
            {
                int index = Properties._CurrentFrame + 1;
                index = index < this.Properties.BaseTextures.Length ? index : 0;
                Properties._CurrentFrame = index;
                this.Properties.BaseTexture = this.Properties.BaseTextures[index];
                this.Properties._NextFrameChange = Utilities.Time + this.Properties.Framelength;
            }


            return this.Properties.BaseTexture;
        }
    }

    public class MaterialProperties
    {
        public TextureTarget TextureType;
        public int ShaderProgram;
        public int BaseTexture;
        public int[] BaseTextures; //For animated textures
        public int NormalMapTexture;
        public int SpecMapTexture;
        public int AlphaMapTexture;
        public int _CurrentFrame;
        public float SpecularPower;
        public float SpecularIntensity;
        public float Alpha;
        public double Framelength; //How long each frame is for an animated texture
        public double _NextFrameChange;
        public Vector3 Color;
        public bool NoCull;
        public bool AlphaTest;
        public bool IsAnimated;

        public MaterialProperties()
        {
            Color = Vector3.One;
            Alpha = 1.0f;
            AlphaTest = false;
            TextureType = TextureTarget.Texture2D;
        }
    }

    public class FBO
    {
        /// <summary>
        /// Whether the current FBO is enabled and should return its rendertexture
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// The width of the FBO
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the FBO
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Whether the FBO has been loaded successfully
        /// </summary>
        public bool Loaded { get; private set; }

        /// <summary>
        /// The automatically-generated RenderTexture attached to the framebuffer
        /// </summary>
        public int RenderTexture
        {
            get
            {
                return (this.Enabled && this.Loaded ) ? _RT : Utilities.White;
            }
        }

        /// <summary>
        /// The actual render texture, without having a fallback texture.
        /// Use this if you want to set specific texture parameters
        /// </summary>
        public int _RT = 0;
        private int fbo = 0;

        /// <summary>
        /// Create a new framebuffer
        /// </summary>
        /// <param name="width">Width of the framebuffer/the RenderTexture</param>
        /// <param name="height">Height of the framebuffer/the RenderTexture</param>
        /// <param name="InternalFormat">The internal format for the RenderTexture</param>
        /// <param name="Format">The format for the RenderTexture</param>
        /// <param name="Attachment">The attachment point of the framebuffer</param>
        /// <param name="Mode">The buffer mode that OpenGL will write with</param>
        public FBO(int width, int height, PixelInternalFormat InternalFormat, PixelFormat Format, FramebufferAttachment Attachment, DrawBufferMode Mode)
        {
            this.Width = width;
            this.Height = height;

            this.Loaded = createFBO(width, height, InternalFormat, Format, Attachment, Mode);
        }

        /// <summary>
        /// Create a new framebuffer
        /// </summary>
        /// <param name="width">Width of the framebuffer/the RenderTexture</param>
        /// <param name="height">Height of the framebuffer/the RenderTexture</param>
        /// <param name="ShadowMap">Automatically use the settings for a shadowmap-dedicated framebuffer</param>
        public FBO(int width, int height, bool ShadowMap = true)
        {
            this.Width = width;
            this.Height = height;

            //Modify the parameters based on if we're gonna be about depth or not
            PixelInternalFormat InternalFormat  = ShadowMap ? PixelInternalFormat.DepthComponent    : PixelInternalFormat.Rgba;
            PixelFormat Format                  = ShadowMap ? PixelFormat.DepthComponent            : PixelFormat.Rgba;
            FramebufferAttachment Attachment    = ShadowMap ? FramebufferAttachment.DepthAttachment : FramebufferAttachment.ColorAttachment0;
            DrawBufferMode Mode                 = ShadowMap ? DrawBufferMode.None                   : DrawBufferMode.FrontAndBack;

            this.Loaded = createFBO(width, height, InternalFormat, Format, Attachment, Mode);
        }

        private bool createFBO(int width, int height, PixelInternalFormat InternalFormat, PixelFormat Format, FramebufferAttachment Attachment, DrawBufferMode Mode)
        {
            //Create the render texture
            GL.GenTextures(1, out _RT);
            GL.BindTexture(TextureTarget.Texture2D, _RT);

            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, Format, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.None);

            //Create our shadow framebuffer
            GL.GenFramebuffers(1, out fbo);

            //Attach the depth texture to the framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, _RT, 0);

            //Tell opengl we are not going to render into the color buffer if we're a shadow map
            GL.DrawBuffer(Mode);

            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Utilities.Print("Framebuffer error!! Status: {0}", Utilities.PrintCode.ERROR, status.ToString());
                return false;
            }

            //Set it back to our default framebuffer
            FBO.ResetFramebuffer();

            //We have successfully created a framebuffer for framebuffing
            return true;
        }

        /// <summary>
        /// Bind this framebuffer, setting it as the currently active framebuffer.
        /// </summary>
        /// <param name="SetViewPort">Whether the viewport size should be modified to the bounds of the framebuffer</param>
        public void BindForWriting( bool SetViewPort = true)
        {
            //Change the viewport to fit the size of our framebuffer
            if (SetViewPort) GL.Viewport(Utilities.engine.ClientRectangle.X, Utilities.engine.ClientRectangle.Y, this.Width, this.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        }

        /// <summary>
        /// Reset the framebuffer back to the one provided by the windowing system.
        /// </summary>
        /// <param name="SetViewPort">Whether the viewport size should be modified to the bounds of the framebuffer</param>
        public static void ResetFramebuffer(bool SetViewPort = true)
        {
            ResetFramebuffer(FramebufferTarget.Framebuffer, SetViewPort);
        }

        /// <summary>
        /// Reset the framebuffer back to the one provided by the windowing system.
        /// </summary>
        /// <param name="target">The specified framebuffer target type</param>
        /// <param name="SetViewPort">Whether the viewport size should be modified to the bounds of the framebuffer</param>
        public static void ResetFramebuffer(FramebufferTarget target, bool SetViewPort )
        {
            //Change the viewport back to the size of the window
            if (SetViewPort) GL.Viewport(Utilities.engine.ClientRectangle.X, Utilities.engine.ClientRectangle.Y, Utilities.engine.ClientRectangle.Width, Utilities.engine.ClientRectangle.Height);
            GL.BindFramebuffer(target, 0);
        }

        public static int BindTextureToFBO(int FBO, int Width, int Height, PixelInternalFormat InternalFormat, PixelFormat Format, FramebufferAttachment Attachment)
        {
            int tex = 0;
            //Create the render texture
            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, Format, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.None);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, tex, 0);

            return tex;
        }
    }
}
