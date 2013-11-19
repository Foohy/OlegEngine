using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using OlegEngine;
using OpenTK;

namespace OlegEngine
{
    public class MeshGenerator
    {
        public static Mesh.BoundingBox CalculateBoundingBox(Vertex[] vertices, Vector3 scale)
        {
            Mesh.BoundingBox bbox = new Mesh.BoundingBox(Vector3.One * float.MaxValue, Vector3.One * float.MinValue);
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i].Position.Multiply(scale);

                //Update the size of the bounding box
                bbox.Negative = bbox.NegativeSet ? SmallestVec(bbox.Negative, vertex) : vertex;
                bbox.Positive = bbox.PositiveSet ? BiggestVec(bbox.Positive, vertex) : vertex;
            }

            return bbox;
        }

        public static void LoadOBJ(string filename, out Vertex[] lsVerts, out int[] lsElements, out Mesh.BoundingBox boundingBox)
        {
            filename = Resource.ModelDir + filename;

            string file = null;
            lsVerts = null;
            lsElements = null;
            boundingBox = new Mesh.BoundingBox();

            try
            {
                file = System.IO.File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                Utilities.Print("Failed to load model '{0}'. {1}", Utilities.PrintCode.ERROR, filename, ex.Message);
            }
            if (file == null || file.Length == 0)
            {
                Utilities.Print("Failed to load model '{0}'. File is empty!", Utilities.PrintCode.ERROR, filename);
                return;
            }

            LoadOBJFromString(file, out lsVerts, out lsElements, out boundingBox);
        }

        public static void LoadOBJFromString(string objString, out Vertex[] lsVerts, out int[] lsElements, out Mesh.BoundingBox boundingBox)
        {
            lsVerts = null;
            lsElements = null;
            boundingBox = new Mesh.BoundingBox();

            string[] file = objString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            //scan the file and look for the number of stuffs
            List<Vector3> verts_UNSORTED = new List<Vector3>();
            List<Vector3> normals_UNSORTED = new List<Vector3>();
            List<Vector2> uv_UNSORTED = new List<Vector2>();
            List<Vector3> tangents = new List<Vector3>();
            List<Vertex> vertices = new List<Vertex>();

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
                            verts_UNSORTED.Add(new Vector3(ParseFloatSafe(vert[2]), ParseFloatSafe(vert[3]), ParseFloatSafe(vert[4])));
                        }
                        else
                        {
                            verts_UNSORTED.Add(new Vector3(ParseFloatSafe(vert[1]), ParseFloatSafe(vert[2]), ParseFloatSafe(vert[3])));
                        }
                        break;

                    case "vn":
                        string[] norms = curline.Split(' ');
                        if (norms[1].Length == 0 && norms.Length > 4)
                        {
                            normals_UNSORTED.Add(new Vector3(ParseFloatSafe(norms[2]), ParseFloatSafe(norms[3]), ParseFloatSafe(norms[4])));
                        }
                        else
                        {
                            normals_UNSORTED.Add(new Vector3(ParseFloatSafe(norms[1]), ParseFloatSafe(norms[2]), ParseFloatSafe(norms[3])));
                        }
                        break;

                    case "vt":
                        string[] coords = curline.Split(' ');
                        if (coords[1].Length == 0 && coords.Length > 3)
                        {
                            uv_UNSORTED.Add(new Vector2(ParseFloatSafe(coords[2]), -ParseFloatSafe(coords[3])));
                        }
                        else
                        {
                            uv_UNSORTED.Add(new Vector2(ParseFloatSafe(coords[1]), -ParseFloatSafe(coords[2])));
                        }
                        break;

                    case "f":
                        string[] element = curline.Split(' ');

                        for (int n = 1; n < 4; n++)
                        {
                            string[] group = element[n].Split('/');

                            Vertex newVert = new Vertex();
                            newVert.Color = Vector3.One;
                            if (group.Length > 0 && group[0].Length > 0)
                            {
                                int vertNum = int.Parse(group[0]);
                                if (vertNum < verts_UNSORTED.Count + 1)
                                {
                                    newVert.Position = verts_UNSORTED[vertNum - 1];
                                    elements.Add(elements.Count);
                                }
                            }
                            if (group.Length > 1 && group[1].Length > 0)
                            {
                                int uvNum = int.Parse(group[1]);
                                if (uvNum < uv_UNSORTED.Count + 1)
                                {
                                    newVert.UV = uv_UNSORTED[uvNum - 1];
                                }
                            }
                            if (group.Length > 2 && group[2].Length > 0)
                            {
                                int normNum = int.Parse(group[2]);
                                if (normNum < normals_UNSORTED.Count + 1)
                                {
                                    newVert.Normal = normals_UNSORTED[normNum - 1];
                                }
                            }

                            vertices.Add(newVert);
                        }
                        break;

                    case "#":
                        //Console.WriteLine("Comment: {0}", curline );
                        break;
                    default:
                        break;
                }
            }

            //Slap them into normal arrays, we won't be changing it much now
            lsElements = elements.ToArray();
            lsVerts = vertices.ToArray();

            //Calculate the tangents
            CalculateTangents(ref lsVerts);

            //Calculate the bounding box
            boundingBox = CalculateBoundingBox(lsVerts, Vector3.One);
        }

        public static Mesh MeshFromData(Vertex[] lsVerts, int[] lsElements, Mesh.BoundingBox boundingBox, string Material)
        {
            //Try to load the material
            Material mat = Resource.GetMaterial(Material);

            //Create the model
            Mesh m = new Mesh(lsVerts, lsElements);
            m.mat = mat;

            return m;
        }

        public static Mesh MeshFromRawData(List<Vertex> verts, List<int> elements, string Material)
        {
            Vertex[] lsVerts = null;
            int[] lsElements = null;

            lsElements = elements.ToArray();
            lsVerts = verts.ToArray();

            //Calculate the tangents
            CalculateTangents(ref lsVerts);

            //Try to load the material
            Material mat = Resource.GetMaterial(Material);

            //Create the model
            Mesh m = new Mesh(lsVerts, lsElements);
            m.mat = mat;
            m.BBox = CalculateBoundingBox(verts.ToArray(), Vector3.One);

            return m;
        }

        public static MeshGroup LoadOBJMulti(string filename)
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
            List<Vector3> verts_UNSORTED = new List<Vector3>();
            List<Vector3> normals_UNSORTED = new List<Vector3>();
            List<Vector2> uv_UNSORTED = new List<Vector2>();
            List<Vertex> vertices = new List<Vertex>();
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
                            verts_UNSORTED.Add(new Vector3(ParseFloatSafe(vert[2]), ParseFloatSafe(vert[3]), ParseFloatSafe(vert[4])));
                        }
                        else
                        {
                            verts_UNSORTED.Add(new Vector3(ParseFloatSafe(vert[1]), ParseFloatSafe(vert[2]), ParseFloatSafe(vert[3])));
                        }
                        break;

                    case "vn":
                        string[] norms = curline.Split(' ');
                        if (norms[1].Length == 0 && norms.Length > 4)
                        {
                            normals_UNSORTED.Add(new Vector3(ParseFloatSafe(norms[2]), ParseFloatSafe(norms[3]), ParseFloatSafe(norms[4])));
                        }
                        else
                        {
                            normals_UNSORTED.Add(new Vector3(ParseFloatSafe(norms[1]), ParseFloatSafe(norms[2]), ParseFloatSafe(norms[3])));
                        }
                        break;

                    case "vt":
                        string[] coords = curline.Split(' ');
                        if (coords[1].Length == 0 && coords.Length > 3)
                        {
                            uv_UNSORTED.Add(new Vector2(ParseFloatSafe(coords[2]), -ParseFloatSafe(coords[3])));
                        }
                        else
                        {
                            uv_UNSORTED.Add(new Vector2(ParseFloatSafe(coords[1]), -ParseFloatSafe(coords[2])));
                        }
                        break;

                    case "f":
                        string[] element = curline.Split(' ');

                        for (int n = 1; n < 4; n++)
                        {
                            string[] group = element[n].Split('/');
                            Vertex vertex = new Vertex();
                            vertex.Color = Vector3.One;
                            if (group.Length > 0 && group[0].Length > 0)
                            {
                                int vertNum = int.Parse(group[0]);
                                if (vertNum < verts_UNSORTED.Count + 1)
                                {
                                    vertex.Position = verts_UNSORTED[vertNum - 1];
                                    elements.Add(elements.Count);
                                }
                            }
                            if (group.Length > 1 && group[1].Length > 0)
                            {
                                int uvNum = int.Parse(group[1]);
                                if (uvNum < uv_UNSORTED.Count + 1)
                                {
                                    vertex.UV = uv_UNSORTED[uvNum - 1];
                                }
                            }
                            if (group.Length > 2 && group[2].Length > 0)
                            {
                                int normNum = int.Parse(group[2]);
                                if (normNum < normals_UNSORTED.Count + 1)
                                {
                                    vertex.Normal = normals_UNSORTED[normNum - 1];
                                }
                            }

                            vertices.Add(vertex);
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
                    case "g":
                        //If there is info, compile it into a model
                        if (vertices.Count > 0)
                        {
                            meshList.Add(MeshFromRawData(vertices, elements, material));
                        }

                        // Reset everything for this new mesh
                        vertices = new List<Vertex>();
                        elements = new List<int>();

                        break;
                    case "#":
                        //Console.WriteLine("Comment: {0}", curline );
                        break;
                    default:
                        break;
                }
            }

            //If there is info, compile it into a model
            if (vertices.Count > 0)
            {
                meshList.Add(MeshFromRawData(vertices, elements, material));
            }

            return new MeshGroup(meshList);
        }

        /// <summary>
        /// Utility function to construct a quad from four vertices
        /// </summary>
        /// <param name="bottomleft">Bottom left vertex</param>
        /// <param name="topleft">Top left vertex</param>
        /// <param name="topright">Top right vertex</param>
        /// <param name="bottomright">Bottom right vertex</param>
        /// <returns>Array of vertices that form a quad</returns>
        public static Vertex[] AddQuad(Vertex bottomleft, Vertex topleft, Vertex topright, Vertex bottomright)
        {
            Vertex[] verts = new Vertex[6];
            verts[5] = bottomleft;
            verts[4] = bottomright;
            verts[3] = topright;

            verts[2] = topright;
            verts[1] = topleft;
            verts[0] = bottomleft;

            return verts;
        }

        /// <summary>
        /// Utility function to construct an array of three vertices
        /// </summary>
        /// <param name="first">The first vertex</param>
        /// <param name="second">The second vertex</param>
        /// <param name="third">The third vertex</param>
        /// <returns>Array of vertices that form a tri</returns>
        public static Vertex[] AddTri(Vertex first, Vertex second, Vertex third)
        {
            Vertex[] verts = new Vertex[3];
            verts[0] = first;
            verts[1] = second;
            verts[2] = third;

            return verts;
        }


        /// <summary>
        /// Calculate the tangents to the surface of a mesh constructed by an array of vertices
        /// </summary>
        /// <param name="verts">The array of vertices that make up the mesh with valid UV properties</param>
        public static void CalculateTangents(ref Vertex[] verts)
        {
            for (int i = 0; i < verts.Length; i += 3)
            {
                Vector3 v0 = verts[i].Position;
                Vector3 v1 = verts[i + 1].Position;
                Vector3 v2 = verts[i + 2].Position;

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;

                Vector2 uv0 = verts[i].UV;
                Vector2 uv1 = verts[i + 1].UV;
                Vector2 uv2 = verts[i + 2].UV;

                float DeltaU1 = uv1.X - uv0.X;
                float DeltaV1 = uv1.Y - uv0.Y;
                float DeltaU2 = uv2.X - uv0.X;
                float DeltaV2 = uv2.Y - uv0.Y;

                float f = 1.0f / (DeltaU1 * DeltaV2 - DeltaU2 * DeltaV1);
                Vector3 Tangent;
                Tangent = new Vector3()
                {
                    X = f * (DeltaV2 * edge1.X - DeltaV1 * edge2.X),
                    Y = f * (DeltaV2 * edge1.Y - DeltaV1 * edge2.Y),
                    Z = f * (DeltaV2 * edge1.Z - DeltaV1 * edge2.Z),
                };

                Tangent.Normalize();
                verts[i].Tangent = Tangent;
                verts[i + 1].Tangent = Tangent;
                verts[i + 2].Tangent = Tangent;
            }
        }

        private static CultureInfo ParseCultureInfo = CultureInfo.CreateSpecificCulture("en-US");
        private static float ParseFloatSafe(string num)
        {
            float fNum = 0;
            float.TryParse(num, NumberStyles.Float, ParseCultureInfo, out fNum);

            return fNum;
        }


        private static Vector3 SmallestVec(Vector3 currentSmallest, Vector3 contender)
        {
            if (contender.X < currentSmallest.X)
                currentSmallest.X = contender.X;

            if (contender.Y < currentSmallest.Y)
                currentSmallest.Y = contender.Y;
            
            if (contender.Z < currentSmallest.Z)
                currentSmallest.Z = contender.Z;
            
            return currentSmallest;
        }
        private static Vector3 BiggestVec(Vector3 currentBiggest, Vector3 contender)
        {
            if (contender.X > currentBiggest.X)
                currentBiggest.X = contender.X;
            
            if (contender.Y > currentBiggest.Y)
                currentBiggest.Y = contender.Y;
            
            if (contender.Z > currentBiggest.Z)
                currentBiggest.Z = contender.Z;
            
            return currentBiggest;
        }
    }
}
