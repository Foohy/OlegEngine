using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

using OlegEngine.Entity;
using OlegEngine;

namespace Gravity_Car.Entity
{
    class ent_editor_build : BaseEntity 
    {
        public List<Vector3> Points = new List<Vector3>();
        public bool Built = false;
        private List<Vector3> _meshPoints = new List<Vector3>();
        private Mesh previewMesh;
        public override void Init()
        {
            previewMesh = new Mesh();
            this.Model = Resource.GetMesh("engine/ball.obj");
            this.Mat = Resource.GetMaterial("engine/white");
            //this.Mat.SetShader(Resource.GetProgram("default"));
            this.Scale = Vector3.One * 0.05f;
            this.Color = new Vector3(0, 1.0f, 0);

            previewMesh.DrawMode = BeginMode.Lines;
            previewMesh.mat = Resource.GetMaterial("engine/white_simple");
            previewMesh.UsageHint = BufferUsageHint.StreamDraw;
            previewMesh.Color = new Vector3(1.0f,0, 0);
            previewMesh.ShouldDrawDebugInfo = false;
        }

        public void AddPoint(Vector3 point)
        {
            Points.Add(point);
            //_meshPoints.Add(new Vector3(point.X, point.Y, this.Position.Z));
            Vector3[] verts = GenerateVerts();
            int[] elements = GenerateElements(verts);
            Vector3[] tangents = new Vector3[verts.Length];

            if (elements.Length > 1)
            {
                previewMesh.UpdateMesh(verts, elements, tangents);
            }
        }

        public void Build()
        {
            //previewMesh.DrawMode = BeginMode.Polygon;
            //this.SetModel(previewMesh);
            //Create a list of verts for the physics
            Vertices physverts = new Vertices();
            for (int i = 0; i < Points.Count; i++)
            {
                physverts.Add(new Microsoft.Xna.Framework.Vector2(Points[i].X, Points[i].Y));
            }
            //Create something that could be construed as physics
            Body bod = new Body(Utilities.PhysicsWorld);
            bod.BodyType = BodyType.Static;
            List<Vertices> moreverts = FarseerPhysics.Common.Decomposition.EarclipDecomposer.ConvexPartition(physverts);
            List<Fixture> fixt = FarseerPhysics.Factories.FixtureFactory.AttachCompoundPolygon(moreverts, 0.5f, bod);

            this.Built = true;
            previewMesh.Color = new Vector3(1.0f, 1.0f, 1.0f);
        }
        private Vector3[] GenerateVerts()
        {
            List<Vector3> verts = new List<Vector3>();
            for (int i = 0; i + 1 < Points.Count; i++)
            {
                verts.Add(new Vector3(Points[i].X, Points[i].Y, Points[i].Z));
                verts.Add(new Vector3(Points[i + 1].X, Points[i + 1].Y, Points[i+1].Z));
            }

            verts.Add(Points[Points.Count-1]);
            verts.Add(Points[0]);

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
            if (Built)
            {
                //base.Draw();
                previewMesh.Draw();
            }
            else
            {
                Vector3 oldPos = this.Position;
                GL.LineWidth(3.0f);
                this.previewMesh.Draw();
                for (int i = 0; i < Points.Count; i++)
                {
                    this.SetPos(Points[i]);
                    base.Draw();
                }
                this.SetPos(oldPos);
            }
        }
    }
}
