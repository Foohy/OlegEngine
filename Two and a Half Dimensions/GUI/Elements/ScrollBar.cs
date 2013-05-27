using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OlegEngine.GUI
{
    public class ScrollBar : Panel
    {
        public Panel ScrollPanel { get; protected set; }

        private Panel Grip;
        private int Scroll = 0; //Amount of pixels we're set to scroll at

        public override void Init()
        {
            base.Init();

            ScrollPanel = GUIManager.Create<Panel>(this);
            ScrollPanel.SetMaterial(Utilities.White);
            ScrollPanel.SetWidth(this.Width);
            ScrollPanel.SetHeight(this.Height);
            ScrollPanel.SetAnchorStyle(Anchors.Left | Anchors.Right);

            Grip = GUIManager.Create<Panel>(this);
            Grip.SetColor(200, 200, 200);
            Grip.SetWidth(20);
            Grip.Dock(DockStyle.RIGHT );
            Grip.OnMouseMove += new Action<Panel, OpenTK.Input.MouseMoveEventArgs>(ScrollBar_OnMouseMove);
        }

        void ScrollBar_OnMouseMove(Panel sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (this.Enabled && sender.IsMouseOver())
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
            }
        }

        public override void Resize(float OldWidth, float OldHeight, float NewWidth, float NewHeight)
        {
            base.Resize(OldWidth, OldHeight, NewWidth, NewHeight);
            ScrollPanel.SetWidth(this.Width);
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

        public void Rebuild()
        {

        }
    }
}
