using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions.Levels
{
    class LevelEditor : LevelBase 
    {
        public int DrawMode = 0;
        public int SelectedMat = 0;

        private List<Material> materials = new List<Material>();
        private bool dragging = false;
        private Entity.DragEntity dragEnt;
        private Vector3 ViewPos = new Vector3();

        public override void Preload()
        {
            base.Preload();

            //Create some event handlers
            Utilities.window.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
            Utilities.window.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);

            //Load up all of the materials in the resources directory
            string[] files = Directory.GetFiles("Resources/Materials/"); //TODO: handle subdirectories
            for (int i = 0; i < files.Length; i++)
            {
                string strMat = files[i];
                Material mat = new Material(strMat);
                materials.Add(mat);
            }
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Left)
            {
                SelectedMat++;
                if (SelectedMat > materials.Count - 1)
                {
                    SelectedMat = 0;
                }

                Console.WriteLine(materials[SelectedMat].Name);
            }
            else if (e.Key == OpenTK.Input.Key.Right)
            {
                SelectedMat--;
                if (SelectedMat < 0)
                {
                    SelectedMat = materials.Count - 1;
                }

                Console.WriteLine(materials[SelectedMat].Name);

            }
        }

        void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            dragging = true;


            dragEnt = (Entity.DragEntity)Entity.EntManager.Create<Entity.DragEntity>();
            dragEnt.SetPos(new Vector3( this.ViewPos.X, this.ViewPos.Y, this.ViewPos.Z + 3.0f ));
            dragEnt.Spawn();
        }

        void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            dragging = false;

            Vector3 StartPos = dragEnt.Position;
            Vector3 EndPos = new Vector3(this.ViewPos.X, this.ViewPos.Y, this.ViewPos.Z + 3.0f);

            //Create an entity to do stuffity stuffs
            Entity.Editor_Rectangle rec = (Entity.Editor_Rectangle)Entity.EntManager.Create<Entity.Editor_Rectangle>();
            rec.TopLeft = StartPos;
            rec.BottomRight = EndPos;
            Mesh model = Resource.GetMesh("ball.obj");
            rec.SetModel(model);
            rec.Mat = materials[SelectedMat];

            dragEnt.Remove();
        }

        public override void Think(OpenTK.FrameEventArgs e)
        {
            base.Think(e);

            if (Utilities.window.Keyboard[OpenTK.Input.Key.D])
            {
                ViewPos += new Vector3((float)Utilities.Frametime * 10.0f, 0, 0);
            }
            if (Utilities.window.Keyboard[OpenTK.Input.Key.A])
            {
                ViewPos += new Vector3((float)Utilities.Frametime * -10.0f, 0, 0);
            }
        }

        public override void Draw(OpenTK.FrameEventArgs e)
        {
            base.Draw(e);
        }
    }
}
