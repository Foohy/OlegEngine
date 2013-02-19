using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;

using Two_and_a_Half_Dimensions.Entity;

namespace Two_and_a_Half_Dimensions.Levels
{
    class TestLevel : LevelBase
    {
        List<Vector2> Points = new List<Vector2>();
        List<Vector2> norms = new List<Vector2>();
        public int WideResolution = 700;
        public float Size = 5f;
        public float HeightRatio = 14f;
        public float WaveWidth = 10f;
        public float WaveAmp = 5;
        public Mesh geometry;
        public Mesh geometry_front;
        public VBO background;
        public Material bg_texture;
        private float Width = -100f;

        Entity.BaseEntity ply;

        public override void Preload()
        {
            base.Preload();

            //Create our 2 dimensional points, and calculate their normals while we're at it

            List<Vector2> PointsBottom = new List<Vector2>();
            for (int i = 0; i < WideResolution; i++)
            {
                float sin = (float)Math.Sin((float)i / HeightRatio) * WaveAmp;
                Points.Add(new Vector2(i * Size, sin));
                PointsBottom.Add(new Vector2(i * Size, sin - 100));
                norms.Add(new Vector2(1, sin));
            }

            //Generate the list of vertices
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (i + 1 < Points.Count)
                {
                    verts.Add(new Vector3(Points[i + 1].X, Points[i + 1].Y, Width));
                    verts.Add(new Vector3(Points[i].X, Points[i].Y, Width));
                    verts.Add(new Vector3(Points[i].X, Points[i].Y, 0));

                    verts.Add(new Vector3(Points[i].X, Points[i].Y, 0));
                    verts.Add(new Vector3(Points[i + 1].X, Points[i + 1].Y, 0));
                    verts.Add(new Vector3(Points[i + 1].X, Points[i + 1].Y, Width));

                    normals.Add(new Vector3(norms[i + 1].X, norms[i + 1].Y, 0));
                    normals.Add(new Vector3(norms[i].X, norms[i].Y, 0));
                    normals.Add(new Vector3(norms[i].X, norms[i].Y, 0));

                    normals.Add(new Vector3(norms[i].X, norms[i].Y, 0));
                    normals.Add(new Vector3(norms[i + 1].X, norms[i + 1].Y, 0));
                    normals.Add(new Vector3(norms[i + 1].X, norms[i + 1].Y, 0));

                    float widthM = Width / 5f;
                    uv.Add(new Vector2(widthM, 1));
                    uv.Add(new Vector2(widthM, 0));
                    uv.Add(new Vector2(0, 0));

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(widthM, 1));
                }
            }

            List<Vector3> front_verts = new List<Vector3>();
            List<Vector3> front_normals = new List<Vector3>();
            List<Vector2> front_UV = new List<Vector2>();


            PointsBottom.Add(new Vector2(WideResolution * Size, -100));
            PointsBottom.Add(new Vector2(0, -100));

            //front_normals.Add(new Vector3(1, 1, 0));
            //front_normals.Add(new Vector3(1, -1, 0));

            for (int i = 0; i < PointsBottom.Count; i++)
            {
                if (i + 1 < Points.Count && i + 1 < PointsBottom.Count)
                {
                    front_verts.Add(new Vector3(PointsBottom[i + 1].X, PointsBottom[i + 1].Y, 0)); // 1
                    front_verts.Add(new Vector3(Points[i + 1].X, Points[i + 1].Y, 0)); // 2
                    front_verts.Add(new Vector3(Points[i].X, Points[i].Y, 0)); //3

                    front_verts.Add(new Vector3(Points[i].X, Points[i].Y, 0)); //3
                    front_verts.Add(new Vector3(PointsBottom[i].X, PointsBottom[i].Y, 0)); //4
                    front_verts.Add(new Vector3(PointsBottom[i + 1].X, PointsBottom[i + 1].Y, 0)); // 1

                    front_normals.Add(new Vector3(0, 0, 1));
                    front_normals.Add(new Vector3(0, 0, 1));
                    front_normals.Add(new Vector3(0, 0, 1));
                    front_normals.Add(new Vector3(0, 0, 1));
                    front_normals.Add(new Vector3(0, 0, 1));
                    front_normals.Add(new Vector3(0, 0, 1));

                    front_UV.Add(new Vector2(1, 50));
                    front_UV.Add(new Vector2(1, 0));
                    front_UV.Add(new Vector2(0, 0));
                    front_UV.Add(new Vector2(0, 0));
                    front_UV.Add(new Vector2(0, 50));
                    front_UV.Add(new Vector2(1, 50));
                }
            }
            //Generate the indices
            List<int> front_indices = new List<int>();
            for (int i = 0; i < front_verts.Count; i++)
            {
                front_indices.Add(i);
            }

            //Generate the indices
            List<int> Indices = new List<int>();
            for (int i = 0; i < verts.Count; i++)
            {
                Indices.Add(i);
            }

            //Create the mesh
            Vector3[] tangents = Utilities.CalculateTangents(verts.ToArray(), uv.ToArray());
            geometry = new Mesh(verts.ToArray(), Indices.ToArray(), tangents, normals.ToArray(), uv.ToArray());
            geometry.mat = Resource.GetMaterial("levels/grass");
            //geometry.mat.Properties.NormalMapTexture = Resource.GetTexture("normalmap.png");
            //geometry.mat.SetShader(Utilities.window.effect.Program);

            tangents = Utilities.CalculateTangents(front_verts.ToArray(), front_UV.ToArray());
            geometry_front = new Mesh(front_verts.ToArray(), front_indices.ToArray(), tangents, front_normals.ToArray(), front_UV.ToArray());
            geometry_front.mat = Resource.GetMaterial("levels/dirt");
            //geometry_front.mat.Properties.NormalMapTexture = Resource.GetTexture("normalmap2.png");
            //geometry_front.mat.SetShader(Utilities.window.effect.Program);

            CreateBackgroundCubes(3000);
            CreateBackgroundSpeakers(50);

            //Create a list of verts for the physics
            Vertices physverts = new Vertices();
            for (int i = 0; i < Points.Count; i++ )
            {
                physverts.Add( new Microsoft.Xna.Framework.Vector2( Points[i].X, Points[i].Y ));
            }
            for (int i = 0; i < PointsBottom.Count; i++ )
            {
                physverts.Add( new Microsoft.Xna.Framework.Vector2( PointsBottom[i].X, PointsBottom[i].Y ));
            }

            //Create something that could be construed as physics
            Body bod = new Body(LevelManager.physWorld);
            bod.BodyType = BodyType.Static;
            List<Vertices> moreverts = FarseerPhysics.Common.Decomposition.EarclipDecomposer.ConvexPartition(physverts);
            List<Fixture> fixt = FarseerPhysics.Factories.FixtureFactory.AttachCompoundPolygon(moreverts, 0.5f, bod);
            //FarseerPhysics.Collision.Shapes.PolygonShape nerdshape = new FarseerPhysics.Collision.Shapes.PolygonShape(1.0f);
            //FarseerPhysics.Collision.Shapes.EdgeShape edgeShape = new FarseerPhysics.Collision.Shapes.EdgeShape( new Microsoft.Xna.Framework.Vector2( -100, 0 ), new Microsoft.Xna.Framework.Vector2( 100, 0.0f ) );
            //Fixture fix = bod.CreateFixture(edgeShape);

            //Create a player
            ply = Entity.EntManager.Create<Entity.ent_car>();
            ply.SetPos(new Vector3(190, 100, -2.0f));
            ply.Spawn();

           // Player.ply.SetPos(ply.Position);

            LevelManager.physWorld.Gravity = new Microsoft.Xna.Framework.Vector2(0.0f, -9f);
            Utilities.window.KeyPress += new EventHandler<KeyPressEventArgs>(window_KeyPress);
            Utilities.window.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            //teehee
            // Bitmap bmp = new Bitmap("Resources/Levels/level.png");
            // System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //uint[] data = new uint[bmp.Width * bmp.Height];

            //Vertices physverts = PolygonTools.CreatePolygon(bmp., bmp.Width);

            //gimme some depths
            Entity.BaseEntity ent = Entity.EntManager.Create<ent_depthscreen>();
            ent.Spawn();

        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Space)
            {
                Console.WriteLine("RESET");
                ply.SetPos(new Vector2(120, 120));
            }

            if (e.Key == OpenTK.Input.Key.Left)
            {
                LevelManager.PhysicsTimescale -= 0.10f;
                Console.WriteLine("Physics Timescale: x{0}", LevelManager.PhysicsTimescale);
            }
            if (e.Key == OpenTK.Input.Key.Right)
            {
                LevelManager.PhysicsTimescale += 0.10f;
                Console.WriteLine("Physics Timescale: x{0}", LevelManager.PhysicsTimescale);
            }
            if (e.Key == OpenTK.Input.Key.BackSpace)
            {
                Entity.BaseEntity[] ents = Entity.EntManager.GetAll();
                for (int i = 0; i < ents.Length; i++)
                {
                    if (!ents[i].WorldSpawn)
                    {
                        ents[i].Remove();
                    }
                }
            }

        }

        void window_KeyPress(object sender, KeyPressEventArgs e)
        {
            Random rand = new Random();
            if (e.KeyChar == 'q')
            {

                Entity.ent_testball ball = (Entity.ent_testball)Entity.EntManager.Create<Entity.ent_testball>();
                //ball.radius = rand.Next(0, 3000) / (float)1000;
                ball.Spawn();
                ball.SetPos(new Vector2(Player.ply.Pos.X, Player.ply.Pos.Y + 3.0f));
            }

            if (e.KeyChar == 'e')
            {
                Entity.ent_testnerd nerd = (Entity.ent_testnerd)Entity.EntManager.Create<Entity.ent_testnerd>();
                //ball.radius = rand.Next(0, 3000) / (float)1000;
                nerd.Spawn();
                nerd.SetPos(new Vector2(Player.ply.Pos.X, Player.ply.Pos.Y + 3.0f));
            }

            if (e.KeyChar == 'r')
            {
                LevelManager.PausePhysics = !LevelManager.PausePhysics;
            }
        }

        public float SinAt(float x)
        {
            return (float)Math.Sin((x / Size) / HeightRatio) * WaveAmp;
        }

        private void CreateBackgroundSpeakers(int amt = 2)
        {
             Random rnd = new Random();
            amt = Utilities.Clamp(amt, WideResolution-1, 0);

            for (int i = 0; i < amt; i+=20)
            {
                Entity.BaseEntity speaker = Entity.EntManager.Create<Entity.ent_speaker>();
                speaker.SetPos(new Vector3(Points[i].X - 1.0f, Points[i].Y, 1.0f) );
                speaker.Spawn();
            }
        }

        private void CreateBackgroundCubes(int amt = 40)
        {
            amt = Utilities.Clamp(amt, WideResolution-1, 0);
            Random rnd = new Random();
            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            for (int i = 0; i < amt; i++)
            {

                int point = i;//rnd.Next(1, Points.Count - 1);
                float z = rnd.Next(30, 90) - 100;
                float height = rnd.Next(50, 1300) / (float)1000;
                verts.Add(new Vector3(Points[point].X, Points[point].Y, z));
                verts.Add(new Vector3(Points[point].X, Points[point].Y - (z * height), z));
                verts.Add(new Vector3(Points[point + 1].X, Points[point + 1].Y - (z * height), z));
                verts.Add(new Vector3(Points[point + 1].X, Points[point + 1].Y, z));
                //Console.WriteLine("Num: {0}, Pos: {1}, sin: {2}", point, Points[point].X.ToString(), SinAt(Points[point].X));
                indices.Add(i * 4 + 3);
                indices.Add(i * 4 + 2);
                indices.Add(i * 4 + 1);
                indices.Add(i * 4 + 0);

                float color = 1.0f; // Points[point].X / (WideResolution * Size);
                // color = 1.0f;
                colors.Add(new Vector3(color, 0.0f, 0.0f));
                colors.Add(new Vector3(color, 0.0f, 0.0f));
                colors.Add(new Vector3(color, 0.0f, 0.0f));
                colors.Add(new Vector3(color, 0.0f, 0.0f));
                colors.Add(new Vector3(color, 0.0f, 0.0f));
                colors.Add(new Vector3(color, 0.0f, 0.0f));

                uv.Add(new Vector2(0, 0));
                uv.Add(new Vector2(0, Points[point].Y + HeightRatio * z * 0.01f));
                uv.Add(new Vector2(1, Points[point].Y + HeightRatio * z * 0.01f));
                uv.Add(new Vector2(1, 0));
            }

            background = new VBO(verts.ToArray(), indices.ToArray(), null, null, uv.ToArray());

            //Load up a texture
            bg_texture = Resource.GetMaterial("levels/building");
        }

        public override void Draw(FrameEventArgs e)
        {
            base.Draw(e);

            if (geometry != null)
            {
                geometry.Draw(Matrix4.Identity );
            }
            GL.Enable(EnableCap.CullFace);
            if (geometry_front != null)
            {
                geometry_front.Draw(Matrix4.Identity);
            }

            if (background != null && bg_texture != null)
            {
                bg_texture.BindMaterial();
                background.Draw(OpenTK.Graphics.OpenGL.BeginMode.Quads);
            }
            
        }

        /*
        private Vertices ReadLevel(string file)
        {
            string[] vertlines = File.ReadAllLines(file);
            Vertices verts = new Vertices();

            int num = 0;
            for (int i = 0; i < vertlines.Length; i++)
            {
                string[] vertPairs = vertlines[i].Split(' ');
                verts.Add( new Vector2( (uint)Convert.ToInt32(vertPairs[0]), (uint)Convert.ToInt32(vertPairs[1]) ));
                num += 2;
            }

            return verts;
        }
         * */
    }
}
