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

        public string Text { get; set; }

        public delegate void OnButtonPressDel();
        public event OnButtonPressDel OnButtonPress;

        public State CurrentState = State.Idle;

        public font DrawText;
        public Panel contextPanel;

        public override void Init()
        {
            base.Init();

            contextPanel = GUIManager.Create<Panel>();
            contextPanel.SetParent(this);
            contextPanel.ShouldPassInput = true;
            contextPanel.Width = 150;
            this.ShouldDrawChildren = false;

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
            p.Width = contextPanel.Width;

            if (this.contextPanel.Children.Count > 1)
            {
                p.Below(this.contextPanel.Children[this.contextPanel.Children.Count - 2]);//Align ourselves below the last panel over
            }
        }

        public Button AddButton(string text)
        {
            Button button = GUIManager.Create<Button>();
            button.SetText(text);
            button.DrawText.SetColor(0, 0, 0);
            button.SizeToText(15);
            button.Height = ElementHeight;
            button.TexPressed = Resource.GetTexture("gui/toolbar_pressed.png");
            button.TexIdle = Resource.GetTexture("gui/toolbar.png");
            button.TexHovered = Resource.GetTexture("gui/toolbar_hover.png");
            this.AddToolPanel(button);

            UpdateContextContents();

            return button;
        }

        private void UpdateContextContents()
        {
            contextPanel.Height = contextPanel.Children.Count * ElementHeight;
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
            if (DrawText == null)
            {
                DrawText = new font("title", str);
            }

            this.Text = str;
            this.DrawText.SetText(str);
        }

        public void SizeToText(int offset = 0)
        {
            if (DrawText != null)
            {
                this.Width = DrawText.GetTextLength(this.Text) + offset;
            }
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

            if (DrawText != null)
            {
                DrawText.SetPos(this.Position.X + 5, this.Position.Y);
                DrawText.Draw();
            }

            //Draw the context panel
            if (this.CurrentState == State.Pressed)
            {
                contextPanel.Position = new Vector2(0, this.Height);
                contextPanel.Draw();
            }
        }
    }
}
