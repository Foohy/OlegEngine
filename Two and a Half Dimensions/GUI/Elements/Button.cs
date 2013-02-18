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

        public override void Draw()
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

            base.Draw();
        }
    }
}
