using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OlegEngine.GUI
{
    public class Button : Panel
    {
        public enum State
        {
            Idle,
            Pressed,
            Hover
        }

        public int TexIdle = -1;
        public int TexPressed = -1;
        public int TexHovered = -1;

        public delegate void OnButtonPressDel(Panel sender);
        public event OnButtonPressDel OnButtonPress;

        public State CurrentState = State.Idle;

        //public font DrawText;
        public Label TextLabel;

        public Button()
        {
            this.SetColor(33, 36, 45);
            this.SetImage(Resource.GetTexture("engine/white.png"));

            TextLabel = GUIManager.Create<Label>();
            TextLabel.SetColor(255, 255, 255);
            TextLabel.SetParent(this);
            TextLabel.Autosize = false;
            TextLabel.Dock(DockStyle.FILL);
            TextLabel.SetAlignment(Label.TextAlign.MiddleCenter);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            if (this.IsMouseOver() && this.CurrentState != State.Pressed && !this.ShouldPassInput && !GUIManager.IsPanelAbovePoint(new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y), this) )
            {
                this.CurrentState = State.Hover;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
                GUIManager.IsPanelAbovePoint(new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y), this);
            }
            else if (this.CurrentState != State.Pressed )
            {
                this.CurrentState = State.Idle;
            }
        }

        public override void MouseDown(MouseButtonEventArgs e)
        {
            base.MouseDown(e);

            if (this.IsMouseOver())
            {
                this.CurrentState = State.Pressed;
            }

        }

        public override void MouseUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseOver() && this.CurrentState == State.Pressed)
            {
                this.OnPressed();
            }
            this.CurrentState = State.Idle;
        }

        public void OnPressed()
        {
            if (OnButtonPress != null)
            {
                OnButtonPress(this);
            }
        }

        /// <summary>
        /// Set the label text of the button
        /// </summary>
        /// <param name="str"></param>
        public void SetText(string str)
        {
            this.TextLabel.SetText(str);

            //center text
            //this.TextLabel.SetPos((this.Width / 2) - (this.TextLabel.Width / 2), (this.Height / 2) - (this.TextLabel.Height / 2));
        }

        public void SizeToText(int offset = 0)
        {
            this.Width = this.TextLabel.GetTextLength() + offset;
        }

        public void SetImage(int image)
        {
            this.TexIdle = image;
            this.TexHovered = image;
            this.TexPressed = image;
        }

        public void SetImage(int idle, int hover, int pressed)
        {
            this.TexIdle = idle;
            this.TexHovered = hover;
            this.TexPressed = pressed;
        }

        public override void Resize(float oldWidth, float oldHeight, float newWidth, float newHeight)
        {
            base.Resize(oldWidth, oldHeight, newWidth, newHeight);

            //center text
            //this.TextLabel.SetPos((this.Width / 2) - (this.TextLabel.Width / 2), (this.Height / 2) - (this.TextLabel.Height / 2));
        }

        public override void Draw()
        {
            if (this.TexHovered > 0 && this.TexIdle > 0 && this.TexPressed > 0)
            {
                switch (this.CurrentState)
                {
                    case State.Pressed:
                        this.SetMaterial(this.TexPressed);
                        break;

                    case State.Idle:
                        this.SetMaterial(this.TexIdle);
                        break;

                    case State.Hover:
                        this.SetMaterial(this.TexHovered);
                        break;
                }
            }

            base.Draw();
        }
    }
}
