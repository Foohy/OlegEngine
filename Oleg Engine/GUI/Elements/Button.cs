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
            this.SetDisabledColor(33/2, 36/2, 45/2);
            this.SetImage( Utilities.White);
            this.OnEnableChange += new Action<Panel, bool>(Button_OnEnableChange);

            TextLabel = GUIManager.Create<Label>();
            TextLabel.SetColor(255, 255, 255);
            TextLabel.SetDisabledColor(100, 100, 100);
            TextLabel.SetParent(this);
            TextLabel.Autosize = false;
            TextLabel.Dock(DockStyle.FILL);
            TextLabel.SetAlignment(Label.TextAlign.MiddleCenter);
        }

        void Button_OnEnableChange(Panel panel, bool enabled)
        {
            CheckButtonState();
              
            TextLabel.SetEnabled(this.Enabled);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            CheckButtonState();
        }

        public override void MouseDown(MouseButtonEventArgs e)
        {
            base.MouseDown(e);

            if (this.Enabled && this.IsMouseOver())
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

        private bool IsClickable()
        {
            return this.Enabled && this.IsMouseOver() && this.CurrentState != State.Pressed && !this.ShouldPassInput && !GUIManager.IsPanelAbovePoint(new Vector2(Utilities.engine.Mouse.X, Utilities.engine.Mouse.Y), this);
        }

        private void CheckButtonState()
        {
            if (this.IsClickable())
            {
                this.CurrentState = State.Hover;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
            }
            else if (this.CurrentState != State.Pressed)
            {
                this.CurrentState = State.Idle;
            }
        }

        public override void Resize(float oldWidth, float oldHeight, float newWidth, float newHeight)
        {
            base.Resize(oldWidth, oldHeight, newWidth, newHeight);

            //If we were resized but the mouse didn't move, check if we are still clickable
            CheckButtonState();
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
