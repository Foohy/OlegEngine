using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

using OlegEngine;
using OlegEngine.Entity;
using OlegEngine.GUI;

using Gravity_Car.Entity;

namespace OlegEngine
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
        public static Vector3 ViewPosition = new Vector3();

        private static Text CurrentModeText;
        private static float goalZoom = 5.0f;
        private static float multiplier = 8;
        private static float halfPI = -1.570796326794896f;
        private static float Multiplier = 0.001223f; //0.000924f;

        //Editor GUI
        private static Toolbar TopControl;

        private static List<Vector2> Points = new List<Vector2>();
        
        
        public static void Init()
        {
            ViewPosition = new Vector3(View.Player.Position);

            Cursor = EntManager.Create<ent_cursor>();
            Cursor.Spawn();
            Cursor.SetPos(new Vector3(0, 0, 0));

            Input.LockMouse = false;
            //Cursor.Scale = Vector3.One * 0.25f;


            //Slap some text on the screen
            CurrentModeText = new GUI.Text("debug", "Mode: " + CurrentMode.ToString());
            CurrentModeText.SetPos(Utilities.window.Width - 200, Utilities.window.Height - CurrentModeText.GetTextHeight() );
            GUI.GUIManager.PostDrawHUD += new GUI.GUIManager.OnDrawHUD(GUIManager_PostDrawHUD);

            Utilities.window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
            Utilities.window.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
            View.CalcView += new Action(View_CalcView);

            //Create our GUI stuff, if neccessary
            if (TopControl == null)
            {
                TopControl = GUIManager.Create<Toolbar>();
                TopControl.SetWidth(Utilities.window.Width);

                ButtonDropDown dd = TopControl.AddButtonDropDown("File");
                Button add = dd.AddButton("Load");
                Button save = dd.AddButton("Save");
                Button exit = dd.AddButton("Exit");
                exit.OnButtonPress += new Button.OnButtonPressDel(exit_OnButtonPress);

                TopControl.AddButton("Edit");
                Button help = TopControl.AddButton("Help...");
                help.OnButtonPress += new Button.OnButtonPressDel(help_OnButtonPress);

            }

            TopControl.ShouldDraw = true;
        }

        static Window exitMessageBox;
        static void exit_OnButtonPress(Panel sender)
        {
            exitMessageBox = GUIManager.Create<Window>();
            exitMessageBox.SetTitle("Hold the fucking phone");
            exitMessageBox.SetWidth(exitMessageBox.Width);
            exitMessageBox.ClipChildren = true;
            exitMessageBox.SetPos((Utilities.window.Width / 2) - (exitMessageBox.Width / 2), (Utilities.window.Height / 2) - (exitMessageBox.Height / 2));

            exitMessageBox.SetWidth(205);
            exitMessageBox.SetHeight(75);
            
            Label label = GUIManager.Create<Label>();
            label.Autosize = true;
            label.SetParent(exitMessageBox);
            label.SetText("Are you sure you'd like to leave?");
            label.SetPos(15, 10);

            Button button = GUIManager.Create<Button>();
            button.SetText("Yes");
            button.SizeToText(15);
            button.SetParent(exitMessageBox);
            button.DockPadding(20, 20, 20, 20);
            button.SetHeight(20);
            button.SetPos(20, 40);
            button.OnButtonPress += new Button.OnButtonPressDel(button_OnYesButtonPress);

            button = GUIManager.Create<Button>();
            button.SetText("Cancel");
            button.SizeToText(15);
            button.SetParent(exitMessageBox);
            button.DockPadding(20, 20, 20, 20);
            button.SetHeight(20);
            button.SetPos(exitMessageBox.Width - button.Width - 20, 40);
            button.SetAnchorStyle(Panel.Anchors.Right );
            button.OnButtonPress += new Button.OnButtonPressDel(button_OnNoButtonPress);
        }

        static void button_OnYesButtonPress(Panel sender)
        {
            Utilities.window.Exit();
        }

        static void button_OnNoButtonPress(Panel sender)
        {
            exitMessageBox.Remove();
        }

        static void help_OnButtonPress(Panel sender)
        {
            Window w = GUIManager.Create<Window>();
            w.SetTitle("So you need help?");
            w.ClipChildren = true;

            Button button = GUIManager.Create<Button>();
            button.SetText("Clip test!");
            button.SizeToText(15);
            //button.TexPressed = Resource.GetTexture("gui/toolbar_pressed.png");
            //button.TexIdle = Resource.GetTexture("gui/toolbar.png");
            //button.TexHovered = Resource.GetTexture("gui/toolbar_hover.png");
            button.SetParent(w);
            button.SetPos(new Vector2((w.Width / 2) - (button.Width / 2), w.Height - 50));
            button.DockPadding(20, 20, 20, 20);
            button.SetHeight(70);
            button.Dock(Panel.DockStyle.TOP);

            //Label label = GUIManager.Create<Label>();
            //label.SetText("Howdy!");
            //label.SetParent(w);
            //label.Position = new Vector2(10, 30);
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

        public static void Think()
        {
            Input.LockMouse = false;
            //curve dat zoom mmm girl u fine
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageDown]) goalZoom -= ((float)Utilities.ThinkTime * 100);
            if (Utilities.window.Keyboard[OpenTK.Input.Key.PageUp]) goalZoom += ((float)Utilities.ThinkTime * 100);


            goalZoom += Input.deltaZ;
            Zoom += (goalZoom - Zoom) / 4;
            ViewPosition = new Vector3(ViewPosition.X, ViewPosition.Y, Zoom);

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
                ViewPosition += new Vector3(0.0f, (float)Utilities.ThinkTime, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.A])
            {
                ViewPosition += new Vector3(-(float)Utilities.ThinkTime, 0.0f, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.S])
            {
                ViewPosition += new Vector3(0.0f, -(float)Utilities.ThinkTime, 0.0f) * multiplier;
            }
            if (Utilities.window.Keyboard[Key.D])
            {
                ViewPosition += new Vector3((float)Utilities.ThinkTime, 0.0f, 0.0f) * multiplier;
            }

            if (Cursor != null)
            {
                Vector3 mousePos = new Vector3((Utilities.window.Mouse.X - (Utilities.window.Width / 2)), -(Utilities.window.Mouse.Y - (Utilities.window.Height / 2)), 0);
                MousePos = ViewPosition + Utilities.Get2Dto3D(Utilities.window.Mouse.X, Utilities.window.Mouse.Y, -ViewPosition.Z);
                Cursor.SetPos( MousePos);
            }

            //Tell the current mode to think
            switch (CurrentMode)
            {
                case EditMode.CreateEnt:
                    CreateEnt.Think();
                    break;

                case EditMode.CreatePhys:
                    CreatePhys.Think();
                    break;

                case EditMode.Transform:
                    Transform.Think();
                    break;
            }
        }

        static void View_CalcView()
        {
            //Set the camera matrix itself
            View.SetAngles(new Vector3(halfPI, 0, 0));
            //Player.ply.CamAngle = new Vector2d(halfPI, 0.0f); //Clamp it because I can't math correctly

            //find the point where we'll be facing
            Vector3 point = new Vector3(0.0f, 0.0f, -1.0f);

            //Player.ply.ViewNormal = point;
            //Player.ply.ViewNormal.Normalize();
            //Player.ply.camMatrix = Matrix4.LookAt(Pos, (Pos + point), Vector3.UnitY);

            //Utilities.ProjectionMatrix = Player.ply.camMatrix;

            View.SetPos(ViewPosition);

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
            Utilities.window.Keyboard.KeyDown -= new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
            View.CalcView -= new Action(View_CalcView);


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

            public static void Think()
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
                    Vector3 hitPos = new Vector3();
                    if (ents[i].Model != null && dif.LengthSquared < curDist && !(ents[i] is ent_cursor) && ents[i].Model.LineIntersectsBox(Editor.MousePos - new Vector3(0, 0, -100), Editor.MousePos + new Vector3(0, 0, -100), ref hitPos))
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

            public static void Think()
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

            public static void Think()
            {
            }
        }

        #endregion
    }

}
