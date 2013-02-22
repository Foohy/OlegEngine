using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Two_and_a_Half_Dimensions.GUI
{
    class Button : Panel
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

        public delegate void OnButtonPressDel();
        public event OnButtonPressDel OnButtonPress;

        public State CurrentState = State.Idle;

        //public font DrawText;
        public Label TextLabel;

        public Button()
        {
            TextLabel = GUIManager.Create<Label>();
            TextLabel.SetColor(0, 0, 0);
            TextLabel.SetParent(this);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            if (this.IsMouseOver() && this.CurrentState != State.Pressed)
            {
                this.CurrentState = State.Hover;
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
                OnButtonPress();
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

            //TextLabel.Draw();
        }
    }
}
