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
        Label TitleText;
        Button closeButton;
        Vector2 Offset = Vector2.Zero;
        bool dragging = false;
        bool resizing = false;

        public Window()
        {

        }

        public override void Init()
        {
            this.Position = new Vector2(200, 480);
            this.WindowTitle = "Untitled";
            this.Width = 150;
            this.Height = 150;
            this.SetMaterial(Resource.GetTexture("gui/window.png"));

            Title = GUIManager.Create<Panel>();
            Title.SetMaterial(Resource.GetTexture("gui/title.png"));
            Title.SetHeight(20);
            Title.SetWidth(this.Width);
            Title.SetPos(this.Position - new Vector2(0, Title.Height));
            Title.OnMouseDown += new OnMouseDownDel(Title_OnMouseDown);
            Title.OnMouseMove += new OnMouseMoveDel(Title_OnMouseMove);
            Title.OnMouseUp += new OnMouseUpDel(Title_OnMouseUp);

            TitleText = GUIManager.Create<Label>();
            TitleText.SetParent(Title);
            TitleText.SetPos(0, 0);
            TitleText.SetColor(1, 1, 1);
            TitleText.SetText(this.WindowTitle);
            TitleText.Dock(DockStyle.LEFT);
            TitleText.SetAlignment(Label.TextAlign.MiddleLeft);
            TitleText.DockPadding(10, 10, 0, 0);

            //Create the close button
            closeButton = GUIManager.Create<Button>();
            closeButton.TexIdle = Resource.GetTexture("gui/x_idle.png");
            closeButton.TexPressed = Resource.GetTexture("gui/x_pressed.png");
            closeButton.TexHovered = Resource.GetTexture("gui/x_hover.png");
            closeButton.SetWidth(20);
            closeButton.SetHeight(20);
            closeButton.Dock(DockStyle.RIGHT);
            closeButton.SetParent(Title);
            closeButton.AlignRight();
            closeButton.OnButtonPress += new Button.OnButtonPressDel(closeButton_OnButtonPress);
            //TODO: align left
        }

        void closeButton_OnButtonPress()
        {
            this.Remove();
            closeButton.Remove();
            TitleText.Remove();
            Title.Remove();
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
                this.Title.SetPos( new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y - Title.Height) - Offset);
                this.SetPos( new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y) - Offset );
            }
        }

        void Title_OnMouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            this.dragging = true;
            Offset = new Vector2(Utilities.window.Mouse.X - this.Position.X, Utilities.window.Mouse.Y - this.Position.Y);
            this.SendToFront();
            Title.SendToFront();
        }

        public override void MouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            base.MouseDown(e);
            this.SendToFront();
            Title.SendToFront();
            if (this.MouseWithinCorner())
            {
                this.resizing = true;
            } 
        }

        public override void MouseMove(OpenTK.Input.MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            if (this.resizing)
            {
                Vector2 Screenpos = this.GetScreenPos();
                this.SetWidth(Utilities.Clamp(Utilities.window.Mouse.X - Screenpos.X, 10000, this.TitleText.GetTextLength() + closeButton.Width));
                this.SetHeight( Utilities.Clamp( Utilities.window.Mouse.Y - Screenpos.Y, 10000, 30 ));

                Title.SetWidth(this.Width);
            }
        }

        public override void MouseUp(OpenTK.Input.MouseButtonEventArgs e)
        {
            if (this.resizing)
            {
                this.resizing = false;

                Title.SetWidth(this.Width);
            }
        }

        protected override void Reposition()
        {
            base.Reposition();

            this.Title.SetPos(this.Position - new Vector2(0, this.Title.Height));
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
            //Title.ShouldDraw = true;
            //closeButton.ShouldDraw = true;
            base.Draw();
            //Title.Draw();
            //TitleText.Draw();
            //closeButton.Draw();
            //Title.ShouldDraw = false; //override it's drawing with our own
            //closeButton.ShouldDraw = false;
        }
    }
}
