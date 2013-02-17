using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions.GUI
{
    class Window : Panel
    {
        public string WindowTitle { get; private set; }

        Panel Title;
        font TitleText;
        Vector2 Offset = Vector2.Zero;
        bool dragging = false;
        bool resizing = false;
        public override void Init()
        {
            this.WindowTitle = "Untitled";

            this.SetMaterial(Resource.GetTexture("gui/window.png"));
            Title = GUIManager.Create<Panel>();
            Title.SetMaterial(Resource.GetTexture("gui/title.png"));
            Title.Height = 20;
            Title.OnMouseDown += new OnMouseDownDel(Title_OnMouseDown);
            Title.OnMouseMove += new OnMouseMoveDel(Title_OnMouseMove);
            Title.OnMouseUp += new OnMouseUpDel(Title_OnMouseUp);
            Title.SetParent(this);

            Width = 150;
            Height = 150;

            this.Position = new Vector2(200, 480);
            TitleText = new font("title", WindowTitle);
        }

        void Title_OnMouseUp(OpenTK.Input.MouseButtonEventArgs e)
        {
            if (dragging)
            {
                dragging = false;
                Offset = Vector2.Zero;
            }
        }

        void Title_OnMouseMove(OpenTK.Input.MouseMoveEventArgs e)
        {
            if (dragging)
            {
                this.Position = new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y) - Offset;
            }
        }

        void Title_OnMouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            this.dragging = true;
            Offset = new Vector2(Utilities.window.Mouse.X - this.Position.X, Utilities.window.Mouse.Y - this.Position.Y);
            this.SendToFront();
        }

        public override void MouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            base.MouseDown(e);
            GUIManager.SendToFront(this);
            if (this.MouseWithinCorner())
            {
                this.resizing = true;
                this.SendToFront();
            } 
        }

        public override void MouseMove(OpenTK.Input.MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            if (this.resizing)
            {
                Vector2 Screenpos = this.GetScreenPos();
                this.Width = Utilities.Clamp(Utilities.window.Mouse.X - Screenpos.X, 10000, this.TitleText.GetTextLength(WindowTitle) + 10);
                this.Height = Utilities.Clamp( Utilities.window.Mouse.Y - Screenpos.Y, 10000, 30 );
            }
        }

        public override void MouseUp(OpenTK.Input.MouseButtonEventArgs e)
        {
            if (this.resizing)
            {
                this.resizing = false;
            }
        }

        public void SetTitle(string str)
        {
            this.WindowTitle = str;
            this.TitleText.SetText(str);
        }

        private static int size = 10; //must be within a box of this many pixels wide
        private bool MouseWithinCorner()
        {
            Vector2 MousePos = new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y);
            Vector2 CornerPos = this.GetScreenPos() + new Vector2( this.Width, this.Height );
            if (MousePos.X < CornerPos.X - size) return false;
            if (MousePos.X > CornerPos.X + size) return false;
            if (MousePos.Y < CornerPos.Y - size) return false;
            if (MousePos.Y > CornerPos.Y + size) return false;

            return true;
        }

        public override void Draw()
        {
            Title.Width = this.Width;

            TitleText.SetPos(this.Position.X + 5, this.Position.Y);

            Title.ShouldDraw = true;
            base.Draw();
            Title.Draw();
            TitleText.Draw();
            Title.ShouldDraw = false; //override it's drawing with our own
        }
    }
}
