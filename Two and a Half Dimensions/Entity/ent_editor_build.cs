using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

namespace Two_and_a_Half_Dimensions.Entity
{
    class ent_editor_build : BaseEntity 
    {
        public List<Vector2> Points = new List<Vector2>();
        private List<Vector3> _meshPoints = new List<Vector3>();
        private Mesh previewMesh = new Mesh();
        public override void Init()
        {
            this.Model = Resource.GetMesh("ball.obj");
            this.Mat = Resource.GetMaterial("engine/white");
            this.Mat.Properties.ShaderProgram = Resource.GetProgram("default");
            this.Scale = Vector3.One * 0.25f;

            previewMesh.DrawMode = BeginMode.Lines;
            previewMesh.mat = Resource.GetMaterial("engine/white");
            previewMesh.mat.SetShader("default");
            previewMesh.UsageHint = BufferUsageHint.StreamDraw;
            previewMesh.LoadMesh("cow.obj");
        }

        public void AddPoint(Vector2 point)
        {
            Points.Add(point);
            _meshPoints.Add(new Vector3(point.X, point.Y, this.Position.Z));
            Vector3[] verts = GenerateVerts();
            int[] elements = GenerateElements(verts);
            Vector3[] tangents = new Vector3[verts.Length];

            if (elements.Length > 1)
            {
                /*
                //Update the mesh
                GL.BindBuffer(BufferTarget.ArrayBuffer, previewMesh.buffers[Mesh.POS_VB]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * Vector3.SizeInBytes), verts, previewMesh.UsageHint);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, previewMesh.buffers[Mesh.INDEX_BUFFER]);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(int)), elements, previewMesh.UsageHint);
                 * */
                previewMesh.UpdateMesh(verts, elements, tangents);
                //previewMesh.UpdateMesh("vehicles/van.obj");
            }
            if (elements.Length > 3)
            {
                //previewMesh.UpdateMesh("monkey.obj");
            }
        }
        private Vector3[] GenerateVerts()
        {
            List<Vector3> verts = new List<Vector3>();
            for (int i = 0; i + 1 < Points.Count; i++)
            {
                verts.Add(new Vector3(Points[i].X, Points[i].Y, this.Position.Z));
                verts.Add(new Vector3(Points[i+1].X, Points[i+1].Y, this.Position.Z));
            }

            return verts.ToArray();
        }

        private int[] GenerateElements(Vector3[] verts)
        {
            int[] elements = new int[verts.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = i;
            }

            return elements;
        }
        public override void Draw()
        {
            Vector3 oldPos = this.Position;
            this.previewMesh.Render(Matrix4.Identity);
            for (int i = 0; i < Points.Count; i++)
            {
                //if (i + 1 < Points.Count)
                //{
                //    Graphics.DrawLine(oldPos, new Vector3(Points[i].X, Points[i].Y, this.Position.Z), new Vector3(Points[i + 1].X, Points[i + 1].Y, this.Position.Z));
                //}
                this.SetPos(Points[i]);
                base.Draw();
            }
            this.SetPos(oldPos);       
        }
    }
}
