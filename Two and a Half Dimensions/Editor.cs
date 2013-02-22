using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;

using Two_and_a_Half_Dimensions.Entity;
using Two_and_a_Half_Dimensions.GUI;

namespace Two_and_a_Half_Dimensions
{
    enum EditMode
    {
        CreatePhys,
        CreateEnt,
        Transform
    }

    class Editor
    {
        public static bool Active = false;
        public static float Zoom = 1.0f;
        public static EditMode CurrentMode = EditMode.CreatePhys;
        public static Vector3 MousePos = new Vector3();
        public static BaseEntity SelectedEnt;
        public static ent_cursor Cursor;

        private static GUI.Font CurrentModeText;
        private static float goalZoom = 5.0f;
        private static Vector3 Pos = new Vector3();
        private static float multiplier = 8;
        private static float halfPI = -1.570796326794896f;
        private static float Multiplier = 0.000924f;

        //Editor GUI
        private static Toolbar TopControl;

        private static List<Vector2> Points = new List<Vector2>();
        
        
        public static void Init()
        {
            Pos = new Vector3(Player.ply.Pos.X, Player.ply.Pos.Y, Player.ply.Pos.Z);

            Cursor = EntManager.Create<ent_cursor>();
            Cursor.Spawn();
            Cursor.SetPos(new Vector3(0, 0, 0));
            //Cursor.Scale = Vector3.One * 0.25f;


            //Slap some text on the screen
            CurrentModeText = new GUI.Font("debug", "Mode: " + CurrentMode.ToString());
            CurrentModeText.SetPos(Utilities.window.Width - 200, 45 );
            GUI.GUIManager.PostDrawHUD += new GUI.GUIManager.OnDrawHUD(GUIManager_PostDrawHUD);

            Utilities.window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
            Utilities.window.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);

            //Create our GUI stuff, if neccessary
            if (TopControl == null)
            {
                TopControl = GUIManager.Create<Toolbar>();
                TopControl.SetWidth(Utilities.window.Width);

                ButtonDropDown dd = TopControl.AddButtonDropDown("File");
                dd.AddButton("Load");
                dd.AddButton("Save");
                dd.AddButton("Exit");

                TopControl.AddButton("Edit");
                Button help = TopControl.AddButton("Help...");
                help.OnButtonPress += new Button.OnButtonPressDel(help_OnButtonPress);

            }

            TopControl.ShouldDraw = true;
        }

        static void help_OnButtonPress()
        {
            Window w = GUIManager.Create<Window>();
            w.SetTitle("So you need help?");
            w.ClipChildren = true;

            Button button = GUIManager.Create<Button>();
            button.SetText("Clip test!");
            button.TextLabel.SetColor(0, 0, 0);
            button.SizeToText(15);
            button.TexPressed = Resource.GetTexture("gui/toolbar_pressed.png");
            button.TexIdle = Resource.GetTexture("gui/toolbar.png");
            button.TexHovered = Resource.GetTexture("gui/toolbar_hover.png");
            button.SetParent(w);
            button.Position = new Vector2((w.Width / 2) - (button.Width / 2), w.Height - 50);
            button.SetWidth(1000);
            button.SetHeight(40);

            Label label = GUIManager.Create<Label>();
            label.SetText("Howdy!");
            label.SetParent(w);
            label.Position = new Vector2(10, 30);
        }

        static void GUIManager_PostDrawHUD(EventArgs e)
        {
            CurrentModeText.Draw();
        }

        static void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.KeyDown(e);
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.KeyDown(e);
                    break;

                case EditMode.Transform:
                    Transform.KeyDown(e);
                    break;
            }

            if (e.Key == Key.Left)
            {
                CurrentMode--;
                if (CurrentMode < 0) CurrentMode = (EditMode)Enum.GetNames(typeof(EditMode)).Length-1;

                CurrentModeText.SetText("Mode: " + CurrentMode.ToString());
            }
            if (e.Key == Key.Right)
            {
                CurrentMode++;
                if ((int)CurrentMode > Enum.GetNames(typeof(EditMode)).Length-1) CurrentMode = EditMode.CreatePhys;

                CurrentModeText.SetText("Mode: " + CurrentMode.ToString());
            }
        }

        static void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.MouseUp(e);
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.MouseUp(e);
                    break;

                case EditMode.Transform:
                    Transform.MouseUp(e);
                    break;
            }
        }

        static void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GUIManager.IsMouseOverElement) return;

            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.MouseDown(e);
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.MouseDown(e);
                    break;

                case EditMode.Transform:
                    Transform.MouseDown(e);
                    break;
            }
        }

        public static void SetSelected(BaseEntity e)
        {
            //Reset the previous selected ents color
            if (SelectedEnt != null)
            {
                SelectedEnt.Color = Vector3.One;
            }
            SelectedEnt = e;

            //Set the new entity's color
            if (SelectedEnt != null)
            {
                SelectedEnt.Color = new Vector3(0, 1.0f, 0);
            }
        }

        public static void Think(FrameEventArgs e)
        {
            Input.LockMouse = false;
            //curve dat zoom mmm girl u fine
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageDown]) goalZoom -= ((float)Utilities.Frametime * 100);
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageUp]) goalZoom += ((float)Utilities.Frametime * 100);


            goalZoom += Input.deltaZ;
            Zoom += (goalZoom - Zoom) / 4;
            Pos = new Vector3(Pos.X, Pos.Y, Zoom);

            //How fast should we move
            multiplier = 7;
            if (Utilities.window.Keyboard[Key.LShift])
            {
                multiplier = 25;
            }
            else if (Utilities.window.Keyboard[Key.LControl])
            {
                multiplier = 1.0f;
            }
            

            //I SAID MOVE
            if (Utilities.window.Keyboard[Key.W])
            {
                Pos += new Vector3(0.0f, (float)e.Time, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.A])
            {
                Pos += new Vector3(-(float)e.Time, 0.0f, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.S])
            {
                Pos += new Vector3(0.0f, -(float)e.Time, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.D])
            {
                Pos += new Vector3((float)e.Time, 0.0f, 0.0f) * multiplier;
            }

            if (Cursor != null)
            {
                MouseState state = Mouse.GetState();
                if (state.IsButtonDown(MouseButton.Right ) )
                {
                    //Multiplier += (float)Input.deltaY / 100000.0f;
                    //Console.WriteLine(Multiplier);
                }
                //Console.WriteLine(LPCameraScreenToVector(0, 0, Utilities.window.Mouse.X, Utilities.window.Mouse.Y, (float)Math.PI / 4.0f ));
                Vector3 mousePos = new Vector3((Utilities.window.Mouse.X - (Utilities.window.Width / 2)), -(Utilities.window.Mouse.Y - (Utilities.window.Height / 2)), 0);
                //MousePos = Get2Dto3D(Utilities.window.Mouse.X, Utilities.window.Mouse.Y, -5.0f );
                //MousePos += new Vector3(MousePos.X, MousePos.Y, 0);
                MousePos = new Vector3(Pos.X, Pos.Y, 0);
                //MousePos += LPCameraScreenToVector(0, 0, Utilities.window.Mouse.X, Utilities.window.Mouse.Y, (float)Math.PI / 4.0f);
                MousePos += mousePos * Zoom * Multiplier; //lmao fuck
                Cursor.SetPos(MousePos);
            }

            //Tell the current mode to think
            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.Think(e);
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.Think(e);
                    break;

                case EditMode.Transform:
                    Transform.Think(e);
                    break;
            }
        }
        private static Vector3 Get2Dto3D(int x, int y, float z)
        {
            return UnProject(new Vector3(x, y, z), Utilities.ViewMatrix, Utilities.ProjectionMatrix);
        }
        /*
        private static Vector2 NormalizeMouse(float x, float y)
        {
            return new Vector2(((float)x - (Utilities.window.Width / 2)) / Utilities.window.Width, -((float)y - (Utilities.window.Height / 2)) / Utilities.window.Height);
        }
        */
        private static Vector2 NormalizeMouse(float x, float y)
        {
            return new Vector2(((float)x * 2.0f - Utilities.window.Width) / Utilities.window.Width, -((float)y * 2.0f - Utilities.window.Height) / Utilities.window.Height);
        }

        private static Vector3 LPCameraScreenToVector( float iScreenX, float iScreenY, float iScreenW, float iScreenH, float fFoV )
        {
            //This code works by basically treating the camera like a frustrum of a pyramid.
            //We slice this frustrum at a distance "d" from the camera, where the slice will be a rectangle whose width equals the "4:3" width corresponding to the given screen height.
            float d = (float)(4 * iScreenH / ( 6 * Math.Tan( 0.5 * fFoV ) ));
  
            //Forward, right, and up vectors (need these to convert from local to world coordinates
            Vector3 _lookAt = new Vector3(0, 0, 1.0f);
            Vector3 _right = new Vector3(0.0f, 1.0f, 0);
            Vector3 _up = Vector3.UnitY;
  
            //Then convert vec to proper world coordinates and return it
            Vector3 norm = (d * _lookAt + (iScreenX - 0.5f * iScreenW) * _right + (0.5f * iScreenH - iScreenY) * _up);
            norm.Normalize();
            return norm;
        }

        private static Vector3 GetDirection(float xclip, float yclip)
        {
            Vector3 _right = new Vector3(1.0f, 0, 0);
            Vector3 _lookAt = new Vector3(0, 0, -1.0f);


            float fov = (float)Math.PI / 4.0f;

            float ytotal = 1.0f * (float)Math.Tan(fov / 2.0f);
            float xtotal = ytotal * (Utilities.window.Width / Utilities.window.Height );

            //get positions relative to cam's forward vector
            float x = xclip * xtotal;
            float y = yclip * ytotal;

            Vector3 xoff = _right * x;
            Vector3 yoff = Vector3.UnitY * y;
            Vector3 zoff = _lookAt * 1.0f;

            Vector3 worldpos = Pos + xoff + yoff + zoff;
            Vector3 norm = (Pos - worldpos);
            norm.Normalize();
            return norm;
        }


        private static Vector3 UnProject(Vector3 screen, Matrix4 view, Matrix4 projection)
        {
            Vector4 pos = new Vector4();
            Vector2 Mousepos = NormalizeMouse(screen.X, screen.Y);
            // Map x and y from window coordinates, map to range -1 to 1 
            pos.X = Mousepos.X;
            pos.Y = Mousepos.Y;
            pos.Z = screen.Z * 2.0f - 1.0f;
            pos.W = 1.0f;

            Vector4 pos2 = Vector4.Transform(pos, Matrix4.Invert(Matrix4.Mult(view, projection)));
            Vector3 pos_out = new Vector3(pos2.X, pos2.Y, pos2.Z);

            return pos_out / pos2.W;
        }

        public static void Draw(FrameEventArgs e)
        {
            //Set the camera matrix itself
            Player.ply.CamAngle = new Vector2d(halfPI, 0.0f); //Clamp it because I can't math correctly

            //find the point where we'll be facing
            Vector3 point = new Vector3(0.0f, 0.0f, -1.0f);

            Player.ply.ViewNormal = point;
            Player.ply.ViewNormal.Normalize();
            Player.ply.camMatrix = Matrix4.LookAt(Pos, (Pos + point), Vector3.UnitY);

            Utilities.ProjectionMatrix = Player.ply.camMatrix;

            Player.ply.SetPos(Pos);

            if (Cursor != null)
            {
                Cursor.SetAngle((float)Utilities.Time);
            }
        }

        public static void Stop()
        {
            if (Cursor != null)
            {
                Cursor.Remove();
            }

            Utilities.window.Mouse.ButtonDown -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Mouse.ButtonUp -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);

            GUI.GUIManager.PostDrawHUD -= new GUI.GUIManager.OnDrawHUD(GUIManager_PostDrawHUD);

            TopControl.ShouldDraw = false;
        }



        #region SPECIFIC MODE FUNCTIONALITY

        private class CreatePhys
        {
            private static ent_editor_build TempEnt;

            public static void MouseDown(MouseButtonEventArgs e)
            {
                if (TempEnt == null)
                {
                    TempEnt = EntManager.Create<ent_editor_build>();
                    TempEnt.Spawn();
                }
                TempEnt.AddPoint(Editor.MousePos);
                //TempEnt.Points.Add(MousePos);
                //Points.Add(MousePos);
            }

            public static void MouseUp(MouseButtonEventArgs e)
            {

            }

            public static void KeyDown(KeyboardKeyEventArgs e)
            {
                if (e.Key == Key.Enter && TempEnt != null && TempEnt.Points.Count > 1)
                {
                    TempEnt.Build();

                    TempEnt = null;
                }
            }

            public static void KeyUp(KeyboardKeyEventArgs e)
            {
            }

            public static void Think(FrameEventArgs e)
            {

            }
        }
        private class Transform
        {
            static float dist = (float)Math.Pow(5, 2); //use the distance squared to save on sqrt calls
            static bool dragging = false;
            static Vector3 offset = Vector3.Zero;
            public static void MouseDown(MouseButtonEventArgs e)
            {
                float curDist = dist;
                BaseEntity closest = null;
                //Try to find an entity
                BaseEntity[] ents = EntManager.GetAll();
                for (int i = 0; i < ents.Length; i++)
                {
                    Vector3 dif = ents[i].Position - Editor.MousePos;
                    if (dif.LengthSquared < curDist && !(ents[i] is ent_cursor))
                    {
                        curDist = dif.LengthSquared;
                        closest = ents[i];
                    }
                }
                Editor.SetSelected(closest);

                if (closest != null)
                {
                    dragging = true;
                    offset = MousePos - closest.Position;
                }
                else
                {

                }
            }

            public static void MouseUp(MouseButtonEventArgs e)
            {
                if (dragging && Editor.SelectedEnt != null)
                {
                    Editor.SelectedEnt.SetPos(MousePos - offset);
                    dragging = false;
                }
            }

            public static void KeyDown(KeyboardKeyEventArgs e)
            {

            }

            public static void KeyUp(KeyboardKeyEventArgs e)
            {
            }

            public static void Think(FrameEventArgs e)
            {
                if (dragging && Editor.SelectedEnt != null)
                {
                    Editor.SelectedEnt.SetPos(MousePos - offset);

                    if (Editor.SelectedEnt.Physics != null)
                    {
                        Editor.SelectedEnt.Physics.Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Input.deltaX, -Input.deltaY) / 4.0f;
                    }
                }
            }
        }
        private class CreateEnt
        {
            public static void MouseDown(MouseButtonEventArgs e)
            {

            }

            public static void MouseUp(MouseButtonEventArgs e)
            {

            }

            public static void KeyDown(KeyboardKeyEventArgs e)
            {

            }

            public static void KeyUp(KeyboardKeyEventArgs e)
            {
            }

            public static void Think(FrameEventArgs e)
            {
            }
        }

        #endregion
    }

}
