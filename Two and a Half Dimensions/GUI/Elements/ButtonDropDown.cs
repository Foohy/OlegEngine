using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Two_and_a_Half_Dimensions.GUI
{
    class ButtonDropDown : Panel
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

        public float ElementHeight = 20;

        public delegate void OnButtonPressDel();
        public event OnButtonPressDel OnButtonPress;

        public State CurrentState = State.Idle;

        public Label TextLabel;
        public Panel contextPanel;

        public override void Init()
        {
            base.Init();

            contextPanel = GUIManager.Create<Panel>();
            contextPanel.ShouldPassInput = true;
            contextPanel.SetWidth(150);

            this.ShouldDrawChildren = false;

            //Create our text label
            TextLabel = GUIManager.Create<Label>();
            TextLabel.SetParent(this);

            Utilities.window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);

        }

        void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsMouseOver() && !this.contextPanel.IsMouseOver())
            {
                this.CurrentState = State.Idle;
            }
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
                if (this.CurrentState == State.Pressed)
                {
                    this.CurrentState = State.Idle;
                }
                else
                {
                    this.CurrentState = State.Pressed;
                    this.OnPressed();
                }
            }
        }

        public override void MouseUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseOver() && this.CurrentState == State.Pressed)
            {
                
            }
        }

        public void AddToolPanel(Panel p)
        {
            p.SetParent(this.contextPanel);
            p.ShouldPassInput = true;
            p.SetWidth(contextPanel.Width);

            if (this.contextPanel.Children.Count > 1)
            {
                p.Below(this.contextPanel.Children[this.contextPanel.Children.Count - 2]);//Align ourselves below the last panel over
            }
        }

        public Button AddButton(string text)
        {
            Button button = GUIManager.Create<Button>();
            button.SetText(text);
            button.TextLabel.SetColor(0, 0, 0);
            button.SizeToText(15);
            button.SetHeight(ElementHeight);
            button.TexPressed = Resource.GetTexture("gui/toolbar_pressed.png");
            button.TexIdle = Resource.GetTexture("gui/toolbar.png");
            button.TexHovered = Resource.GetTexture("gui/toolbar_hover.png");
            this.AddToolPanel(button);

            UpdateContextContents();

            return button;
        }

        private void UpdateContextContents()
        {
            contextPanel.SetHeight(contextPanel.Children.Count * ElementHeight);
        }


        private void OnPressed()
        {
            if (OnButtonPress != null)
            {
                OnButtonPress();
            }

            this.SendToFront();
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
            this.SetWidth(TextLabel.GetTextLength() + offset);
        }

        public override void Remove()
        {
            base.Remove();

            Utilities.window.Mouse.ButtonDown -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
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

            //Because we're set to not draw our children, draw the label manually
            this.TextLabel.Draw();

            //Draw the context panel
            if (this.CurrentState == State.Pressed)
            {
                contextPanel.Position = new Vector2(this.Position.X, this.Position.Y + this.Height);
                contextPanel.ShouldDraw = true;
                contextPanel.ShouldPassInput = false;
            }
            else
            {
                contextPanel.ShouldDraw = false;
                contextPanel.ShouldPassInput = true;
            }
        }
    }
}
