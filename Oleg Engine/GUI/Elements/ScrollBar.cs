using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OlegEngine.GUI
{
    public class ScrollBar : Panel
    {
        public Panel ScrollPanel { get; protected set; }
        // HI SCOTT YOU ARE MY FRIEND
        private Panel Grip;
        private int Scroll = 0; //Amount of pixels we're set to scroll at
        private bool IsMouseDownGrabber = false;
        private Vector2 GrabOffset = Vector2.Zero;
        private bool ShouldDrawBar;

        public override void Init()
        {
            base.Init();

            ScrollPanel = GUIManager.Create<Panel>(this);
            ScrollPanel.SetMaterial(Utilities.White);
            ScrollPanel.SetWidth(this.Width);
            ScrollPanel.SetHeight(this.Height);
            ScrollPanel.SetAnchorStyle(Anchors.Left | Anchors.Right);

            Grip = GUIManager.Create<Panel>(this);
            Grip.SetColor(200, 201, 200);
            Grip.SetWidth(20);
            Grip.Dock(DockStyle.RIGHT);
            Grip.ShouldDraw = false; //We'll be drawing it manually
            Grip.OnMouseDown += new Action<Panel,OpenTK.Input.MouseButtonEventArgs>(Grip_OnMouseDown);
            this.OnMouseMove += new Action<Panel,OpenTK.Input.MouseMoveEventArgs>(ScrollBar_OnMouseMove);

            Utilities.engine.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
        }

        void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            IsMouseDownGrabber = false;
        }

        void Grip_OnMouseDown(Panel arg1, OpenTK.Input.MouseButtonEventArgs e)
        {
            if (Grip.IsMouseOver())
            {
                IsMouseDownGrabber = true;
                GrabOffset = new Vector2(e.Position.X, e.Position.Y - Grip.Position.Y );
            }
        }

        void ScrollBar_OnMouseMove(Panel sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (this.Enabled && sender.IsMouseOver() && IsMouseDownGrabber)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
                Grip.SetPos(new Vector2(Grip.Position.X, Utilities.Clamp(e.Y - GrabOffset.Y, this.Height - Grip.Height, 0 )));
                SetScroll((int)((Grip.Position.Y) / (this.Height / this.ScrollPanel.Height)));
            }
        }

        public override void Resize(float OldWidth, float OldHeight, float NewWidth, float NewHeight)
        {
            base.Resize(OldWidth, OldHeight, NewWidth, NewHeight);
            ScrollPanel.SetWidth(this.Width);

            int GrabHeight = (int)Utilities.Clamp((int)((float)this.Height * ((float)this.Height / (float)this.ScrollPanel.Height)), this.ScrollPanel.Height, 10 );

            this.ShouldDrawBar = GrabHeight < this.ScrollPanel.Height;
            Grip.SetHeight(GrabHeight);
        }

        public Panel GetScrollPanel()
        {
            return this.ScrollPanel;
        }

        public void SetScroll(int scroll)
        {
            this.Scroll = Utilities.Clamp(scroll, (int)this.ScrollPanel.Height, 0);
            ScrollPanel.SetPos(0, -this.Scroll);
        }

        public override void Draw()
        {
            base.Draw();

            Vector2 realPos = this.GetScreenPos();

            if (ShouldDrawBar)
            {
                Surface.SetNoTexture();
                Surface.SetDrawColor(33, 36, 45);
                Surface.DrawRect(realPos.X + Grip.Position.X, realPos.Y, Grip.Width, this.Height);

                Grip.ShouldDraw = true;
                Grip.Draw();
                Grip.ShouldDraw = false;
            }
        }

    }
}
