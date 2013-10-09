using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OlegEngine.GUI
{
    public class ButtonDropDown : Panel
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
        public int ElementSpacing = 1;

        public delegate void OnButtonPressDel();
        public event OnButtonPressDel OnButtonPress;

        public State CurrentState { get; private set; }

        public Label TextLabel;
        public Panel contextPanel;

        public override void Init()
        {
            base.Init();

            contextPanel = GUIManager.Create<Panel>();
            contextPanel.ShouldPassInput = true;
            contextPanel.SetWidth(150);
            contextPanel.SetPos(this.Position.X, this.Position.Y + this.Height);

            //Create our text label
            TextLabel = GUIManager.Create<Label>();
            TextLabel.SetParent(this);
            TextLabel.Autosize = false;
            TextLabel.Dock(DockStyle.FILL);
            TextLabel.SetAlignment(Label.TextAlign.MiddleCenter);

            this.SetButtonState(State.Idle);

            Utilities.engine.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
        }

        void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsMouseOver() && !this.contextPanel.IsMouseOver())
            {
                SetButtonState(State.Idle);
            }
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            if (this.IsMouseOver() && this.CurrentState != State.Pressed && !GUIManager.IsPanelAbovePoint(new Vector2(Utilities.engine.Mouse.X, Utilities.engine.Mouse.Y), this))
            {
                SetButtonState(State.Hover);
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
            }
            else if (this.CurrentState != State.Pressed )
            {
                SetButtonState(State.Idle);
            }
        }

        public override void MouseDown(MouseButtonEventArgs e)
        {
            base.MouseDown(e);

            if (this.IsMouseOver())
            {
                if (this.CurrentState == State.Pressed)
                {
                    SetButtonState(State.Idle);
                    this.SetPassInput(true); //Ignore any input given
                }
                else
                {
                    SetButtonState(State.Pressed);
                    this.OnPressed();
                    this.SetPassInput(false); //Let the context menu accept input
                }
            }
        }

        public override void MouseUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseOver() && this.CurrentState == State.Pressed)
            {
                
            }
        }

        public void SetPassInput(bool passinput)
        {
            this.contextPanel.ShouldPassInput = passinput;
            foreach (Panel p in contextPanel.Children)
            {
                p.ShouldPassInput = passinput;
            }
        }

        public void SetButtonState(State state)
        {
            this.CurrentState = state;

            if (state == State.Pressed) { this.SetPassInput(false); }
            else { this.SetPassInput(true); }
        }

        public void AddToolPanel(Panel p)
        {
            p.SetParent(this.contextPanel);
            p.ShouldPassInput = true;
            p.SetWidth(contextPanel.Width);

            if (this.contextPanel.Children.Count > 1)
            {
                p.Below(this.contextPanel.Children[this.contextPanel.Children.Count - 2], ElementSpacing );//Align ourselves below the last panel over
            }
        }

        public Button AddButton(string text)
        {
            Button button = GUIManager.Create<Button>();
            button.SetText(text);
            button.SizeToText(15);
            button.SetHeight(ElementHeight);
            //button.TexPressed = Resource.GetTexture("gui/toolbar_pressed.png");
            //button.TexIdle = Resource.GetTexture("gui/toolbar.png");
            //button.TexHovered = Resource.GetTexture("gui/toolbar_hover.png");
            button.TextLabel.SetAlignment(Label.TextAlign.MiddleLeft);
            this.AddToolPanel(button);

            UpdateContextContents();

            return button;
        }

        private void UpdateContextContents()
        {
            contextPanel.SetHeight(contextPanel.Children.Count * ElementHeight + (ElementSpacing * (contextPanel.Children.Count-1)) );
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

        protected override void Reposition()
        {
            base.Reposition();

            contextPanel.SetPos(this.Position.X, this.Position.Y + this.Height);
        }

        public override void Resize(float oldWidth, float oldHeight, float newWidth, float newHeight)
        {
            base.Resize(oldWidth, oldHeight, newWidth, newHeight);

            contextPanel.SetPos(this.Position.X, this.Position.Y + this.Height);
        }

        public override void Remove()
        {
            base.Remove();

            Utilities.engine.Mouse.ButtonDown -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
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
            contextPanel.IsVisible = this.CurrentState == State.Pressed;
        }
    }
}
