using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine.GUI
{
    public class Window : Panel
    {
        private enum ResizeMode
        {
            None,
            Top,
            Left,
            Right,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
        public string WindowTitle { get; private set; }
        public bool Resizable { get; set; } //Should the mouse be able to size it?
        public Vector2 MinimumSize { get; set; } //Minimum size of the window
        public Vector2 MaximumSize { get; set; } //Maximum size of the window

        Panel Title;
        Label TitleText;
        Button closeButton;
        Vector2 Offset = Vector2.Zero;
        ResizeMode CurrentResizeMode = ResizeMode.None;
        bool dragging = false;

        public Window()
        {

        }

        public override void Init()
        {
            this.Position = new Vector2(200, 480);
            this.WindowTitle = "Untitled";
            this.Width = 200;
            this.Height = 150;
            this.SetMaterial(Resource.GetTexture("gui/window.png"));
            this.SetColor(20, 24, 33);

            Title = GUIManager.Create<Panel>();
            Title.SetMaterial(Resource.GetTexture("gui/title.png"));
            Title.SetHeight(25);
            Title.SetWidth(this.Width);
            Title.SetPos(this.Position - new Vector2(0, Title.Height));
            Title.OnMouseDown += new Action<Panel,OpenTK.Input.MouseButtonEventArgs>(Title_OnMouseDown);
            Title.OnMouseMove += new Action<Panel,OpenTK.Input.MouseMoveEventArgs>(Title_OnMouseMove);
            Title.OnMouseUp += new Action<Panel,OpenTK.Input.MouseButtonEventArgs>(Title_OnMouseUp);
            Title.SetColor(135, 36, 31);

            TitleText = GUIManager.Create<Label>();
            TitleText.SetFont("windowtitle");
            TitleText.SetParent(Title);
            TitleText.SetPos(0, 0);
            TitleText.SetColor(255, 255, 255);
            TitleText.SetText(this.WindowTitle);
            TitleText.Dock(DockStyle.LEFT);
            TitleText.SetAlignment(Label.TextAlign.MiddleLeft);
            TitleText.DockPadding(10, 10, 0, 0);

            //Create the close button
            closeButton = GUIManager.Create<Button>();
            closeButton.SetImage(Resource.GetTexture("gui/close.png"));
            closeButton.SetWidth(25);
            closeButton.SetHeight(25);
            closeButton.SetColor(26, 30, 38);
            closeButton.Dock(DockStyle.RIGHT);
            closeButton.SetParent(Title);
            closeButton.AlignRight();
            closeButton.OnButtonPress += new Button.OnButtonPressDel(closeButton_OnButtonPress);
            closeButton.PreDraw += new Action<Panel, Vector2>(closeButton_PreDraw);
        }

        void closeButton_PreDraw(Panel btn, Vector2 ScreenPos)
        {
            Surface.SetDrawColor(48, 55, 71);
            Surface.DrawRect(ScreenPos.X + 2, ScreenPos.Y + 2, btn.Width - 4, btn.Height - 4);
        }

        void closeButton_OnButtonPress(Panel sender)
        {
            this.Remove();
        }

        void Title_OnMouseUp(Panel sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (dragging)
            {
                dragging = false;
                Offset = Vector2.Zero;
            }
        }

        void Title_OnMouseMove(Panel sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (dragging)
            {
                this.Title.SetPos( new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y - Title.Height) - Offset);
                this.SetPos( new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y) - Offset );
            }
        }

        void Title_OnMouseDown(Panel sender, OpenTK.Input.MouseButtonEventArgs e)
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

            if (this.MouseWithinBottom())
            {
                this.CurrentResizeMode = ResizeMode.Bottom;
            }

            if (this.MouseWithinRight())
            {
                this.CurrentResizeMode = ResizeMode.Right;
            }

            if (this.MouseWithinLeft())
            {
                this.CurrentResizeMode = ResizeMode.Left;
            }


            if (this.MouseWithinRightCorner())
            {
                this.CurrentResizeMode = ResizeMode.BottomRight;
            }
            if (this.MouseWithinLeftCorner())
            {
                this.CurrentResizeMode = ResizeMode.BottomLeft;
            } 
        }

        public override void MouseMove(OpenTK.Input.MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            if (this.MouseWithinBottom() )
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNS;
            }

            if (this.MouseWithinRight() || this.MouseWithinLeft())
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeWE;
            }

            if (this.MouseWithinRightCorner())
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNWSE;
            }

            if (this.MouseWithinLeftCorner())
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeNESW;
            }
            
            if (CurrentResizeMode != ResizeMode.None )
            {
                ResizeThink();

                Title.SetWidth(this.Width);
            }
        }

        public override void MouseUp(OpenTK.Input.MouseButtonEventArgs e)
        {
            if (CurrentResizeMode != ResizeMode.None)
            {
                ResizeThink();
                this.CurrentResizeMode = ResizeMode.None;

                Title.SetWidth(this.Width);
            }
        }

        protected override void Reposition()
        {
            base.Reposition();

            this.Title.SetPos(this.Position - new Vector2(0, this.Title.Height));
        }

        public override void Resize(float oldWidth, float oldHeight, float newWidth, float newHeight)
        {
            //Make sure our size is within our minimum and maximum
            this.Width = Utilities.Clamp(this.Width, (this.MaximumSize.X <= 0) ? float.PositiveInfinity : this.MaximumSize.X, (this.MinimumSize.X < this.closeButton.Width) ? this.closeButton.Width : this.MinimumSize.X);
            this.Height = Utilities.Clamp(this.Height, (this.MaximumSize.Y <= 0) ? float.PositiveInfinity : this.MaximumSize.Y, this.MinimumSize.Y);

            base.Resize(oldWidth, oldHeight, this.Width, this.Height);
            Title.SetWidth(this.Width);
        }

        public override void Remove()
        {
            base.Remove();

            closeButton.Remove();
            TitleText.Remove();
            Title.Remove();
        }

        public void SetTitle(string str)
        {
            this.WindowTitle = str;
            this.TitleText.SetText(str);
        }

        public void SetEnableCloseButton(bool enabled)
        {
            this.closeButton.Enabled = enabled;
            this.closeButton.ShouldDraw = enabled;
        }

        private static int size = 10; //must be within a box of this many pixels wide
        private bool MouseWithinRightCorner()
        {
            return (MouseWithinBottom() && MouseWithinRight());
        }

        private bool MouseWithinLeftCorner()
        {
            return (MouseWithinBottom() && MouseWithinLeft());
        }

        private bool MouseWithinBottom()
        {
            if (this.IsMouseOver())
            {
                if ((this.GetScreenPos().Y + this.Height) - Utilities.window.Mouse.Y < size)
                {
                    return true;
                }
            }

            return false;
        }

        private bool MouseWithinRight()
        {
            if (this.IsMouseOver())
            {
                if ((this.GetScreenPos().X + this.Width) - Utilities.window.Mouse.X < size)
                {
                    return true;
                }
            }

            return false;
        }

        private bool MouseWithinTop()
        {
            if (this.IsMouseOver())
            {
                if ((Utilities.window.Mouse.Y - this.GetScreenPos().Y)  < size)
                {
                    return true;
                }
            }

            return false;
        }

        private bool MouseWithinLeft()
        {
            if (this.IsMouseOver())
            {
                if ((Utilities.window.Mouse.X - this.GetScreenPos().X) < size)
                {
                    return true;
                }
            }

            return false;
        }

        //Handle resizing by mouse
        private void ResizeThink()
        {
            if (CurrentResizeMode == ResizeMode.None) return;
            Vector2 Screenpos = this.GetScreenPos();

            if (CurrentResizeMode == ResizeMode.TopLeft ||
            CurrentResizeMode == ResizeMode.Left ||
            CurrentResizeMode == ResizeMode.BottomLeft)
            {
                this.SetWidth((this.Position.X + this.Width) - Utilities.window.Mouse.X );
                this.SetPos(Utilities.window.Mouse.X, this.Position.Y);
            }

            if (CurrentResizeMode == ResizeMode.TopRight ||
            CurrentResizeMode == ResizeMode.Right ||
            CurrentResizeMode == ResizeMode.BottomRight)
            {
                this.SetWidth(Utilities.window.Mouse.X - Screenpos.X);
            }

            if (CurrentResizeMode == ResizeMode.TopLeft ||
            CurrentResizeMode == ResizeMode.Top ||
            CurrentResizeMode == ResizeMode.TopRight)
            {
                this.SetPos(this.Position.Y, Utilities.window.Mouse.Y);
            }

            if (CurrentResizeMode == ResizeMode.BottomLeft ||
            CurrentResizeMode == ResizeMode.Bottom ||
            CurrentResizeMode == ResizeMode.BottomRight)
            {
                this.SetHeight(Utilities.window.Mouse.Y - Screenpos.Y);
            }
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
