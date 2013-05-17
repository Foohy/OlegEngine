using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OlegEngine.GUI
{
    public class TextInput : Panel
    {
        //TODO: Text selection/cursor
        public bool Selected { get; private set; }
        public Label TextLabel;
        public event Action<Panel, bool> OnSelectedChange;

        public TextInput()
        {
        }

        public override void Init()
        {
            base.Init();

            this.SetColor(255, 255, 255);
            this.SetHeight(20);
            this.SetWidth(40);

            TextLabel = GUIManager.Create<Label>();
            TextLabel.SetColor(0, 0, 0);
            TextLabel.SetParent(this);
            TextLabel.Autosize = true;
            TextLabel.Dock(DockStyle.FILL);
            TextLabel.SetAlignment(Label.TextAlign.TopLeft);

            Utilities.window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
        }

        void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsMouseOver())
            {
                this.SetIsSelected(false);
            }
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            if (this.Enabled && this.IsMouseOver() && !this.ShouldPassInput && !GUIManager.IsPanelAbovePoint(new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y), this) )
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.IBeam;
            }
        }

        public override void MouseDown(MouseButtonEventArgs e)
        {
            base.MouseDown(e);

            if (this.Enabled && this.IsMouseOver())
            {
                this.SetIsSelected(true);
            }
            else { this.SetIsSelected(false); }
        }

        public override void KeyPressed(KeyPressEventArgs e)
        {
            if (this.Selected && this.Enabled)
            {
                switch (e.KeyChar)
                {
                    case '\b':
                        if (this.TextLabel.Text.Length > 1 )
                            this.TextLabel.SetText(this.TextLabel.Text.Remove(this.TextLabel.Text.Length - 1));
                        else if (this.TextLabel.Text.Length == 1)
                            this.TextLabel.SetText("");

                        break;

                    default:
                        this.TextLabel.SetText(this.TextLabel.Text + e.KeyChar);
                        break;
                }
            }
        }

        /// <summary>
        /// Set the label text of the button
        /// </summary>
        /// <param name="str"></param>
        public void SetText(string str)
        {
            this.TextLabel.SetText(str);
        }

        public void SizeToText(int offset = 0)
        {
            this.Width = this.TextLabel.GetTextLength() + offset;
        }

        public void SetIsSelected(bool selected)
        {
            if (this.Selected != selected)
            {
                this.Selected = selected;
                if (this.OnSelectedChange != null)
                    this.OnSelectedChange(this, this.Selected);
            }
        }

        public override void Resize(float oldWidth, float oldHeight, float newWidth, float newHeight)
        {
            base.Resize(oldWidth, oldHeight, newWidth, newHeight);
        }

        public override void Remove()
        {
            base.Remove();
            Utilities.window.Mouse.ButtonDown -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
        }
    }
}
