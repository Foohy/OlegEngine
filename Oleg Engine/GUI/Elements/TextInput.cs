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
        public int CaretPos = 0;

        private bool blinkOn = false;
        private double nextBlink = 0;

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
            TextLabel.SetColor(73, 73, 73);
            TextLabel.SetParent(this);
            TextLabel.DockPadding(3, 3, 0, 0);
            TextLabel.Dock(DockStyle.FILL);
            TextLabel.SetAlignment(Label.TextAlign.MiddleLeft);

            Utilities.window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                //Set the caret position
                int newCaretPos = e.Key == Key.Right ? CaretPos + 1 : CaretPos - 1;
                CaretPos = Utilities.Clamp(newCaretPos, this.TextLabel.Text.Length, 0);

                //Reset caret blinking
                blinkOn = true;
                nextBlink = System.Windows.Forms.SystemInformation.CaretBlinkTime / 1000f + Utilities.Time;
            }
            else if (e.Key == Key.Delete)
            {
                //Delete characters in front of the caret
                delete();
            }
            //Paste
            else if (e.Key == Key.V && (Utilities.window.Keyboard[Key.ControlLeft] || Utilities.window.Keyboard[Key.ControlRight]))
            {
                if (System.Windows.Clipboard.ContainsText())
                {
                    string paste = System.Windows.Clipboard.GetText();

                    this.typeKey(paste);
                }
            }
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

            if (this.Enabled && this.IsMouseOver() && !this.ShouldPassInput && this.ShouldDraw && !GUIManager.IsPanelAbovePoint(new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y), this) )
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

                //Set the caret position based on mouse click position
                this.CaretPos = this.TextLabel.GetIndexFromPosition(e.X);
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
                        backspace();
                        break;

                    case '\n': case '\t': case '\r':
                        break;

                    default:
                        //Only type the key if it's a valid character
                        if (TextLabel.IsValidCharacter(e.KeyChar))
                        {
                            this.typeKey(e.KeyChar.ToString());
                        }
                        break;
                }
            }
        }

        private void backspace()
        {
            if (CaretPos > 1)
            {
                this.TextLabel.SetText(this.TextLabel.Text.Remove(CaretPos-1, 1));
                CaretPos--;
            }
            else if (this.TextLabel.Text.Length == 1)
            {
                this.TextLabel.SetText("");
                CaretPos = 0;
            }
        }

        private void delete()
        {
            if (this.TextLabel.Text.Length > 1 && CaretPos != this.TextLabel.Text.Length)
            {
                this.TextLabel.SetText(this.TextLabel.Text.Remove(CaretPos, 1));
            }
        }

        private void typeKey(string Key)
        {
            this.TextLabel.SetText(this.TextLabel.Text.Insert(CaretPos, Key));
            CaretPos += Key.Length;
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

        public override void Draw()
        {
            base.Draw();
            
            //Draw the caret
            if (this.Selected)
            {
                //think about how we should be blinking
                if (Utilities.Time > nextBlink)
                {
                    blinkOn = !blinkOn;
                    nextBlink = System.Windows.Forms.SystemInformation.CaretBlinkTime/1000f + Utilities.Time;
                }

                if (blinkOn)
                {
                    Vector2 ScreenPos = TextLabel.GetScreenPos();

                    float caretX = Surface.GetTextLength(this.TextLabel.GetFont(), this.TextLabel.Text.Substring(0, CaretPos));

                    Surface.SetDrawColor(0, 0, 0);
                    Surface.SetNoTexture();
                    Surface.DrawRect(ScreenPos.X + caretX, ScreenPos.Y + 2, 1, this.Height - 4);
                }
            }
        }

        public override void Remove()
        {
            base.Remove();
            Utilities.window.Mouse.ButtonDown -= new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
        }
    }
}
