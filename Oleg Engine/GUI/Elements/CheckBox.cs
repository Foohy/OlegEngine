using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OlegEngine.GUI
{
    public class CheckBox : Panel
    {

        public event Action<Panel, bool> OnChange;
        public bool IsChecked { get; private set; }
        public Label TextLabel;

        private int CheckedTex = 0;
        private int UncheckedTex = 0;

        public CheckBox()
        {

        }

        public override void Init()
        {
            base.Init();

            this.SetColor(255, 255, 255);

            this.SetImages(Resource.GetTexture("gui/checkbox_checked.png"), Resource.GetTexture("gui/checkbox_unchecked.png"));
            this.SetHeight(16);
            this.SetWidth(16);
            this.ClipChildren = false;

            TextLabel = GUIManager.Create<Label>();
            TextLabel.SetColor(255, 255, 255);
            TextLabel.SetParent(this);
            TextLabel.Autosize = false;
            TextLabel.SetHeight(this.Height);
            TextLabel.SetPos(new Vector2(this.Position.X + this.Width, 0));
            TextLabel.SetAlignment(Label.TextAlign.MiddleLeft);
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            base.MouseMove(e);

            if (this.IsMouseOver() && !this.ShouldPassInput && !GUIManager.IsPanelAbovePoint(new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y), this) )
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
            }
        }

        public override void MouseDown(MouseButtonEventArgs e)
        {
            base.MouseDown(e);

            if (this.IsMouseOver())
            {
                SetChecked(!this.IsChecked);
            }

        }

        public override void Resize(float OldWidth, float OldHeight, float NewWidth, float NewHeight)
        {
            base.Resize(OldWidth, OldHeight, NewWidth, NewHeight);
            if (TextLabel)
                TextLabel.SetPos(new Vector2(this.Position.X + this.Width, 0));
        }

        protected override void Reposition()
        {
            base.Reposition();
            if (TextLabel)
                TextLabel.SetPos(new Vector2(this.Position.X + this.Width, 0));
        }

        /// <summary>
        /// Set the label text of the button
        /// </summary>
        /// <param name="str"></param>
        public void SetText(string str)
        {
            this.TextLabel.SetText(str);
        }

        public void SetImages(int checkTex, int uncheckTex)
        {
            this.CheckedTex = checkTex;
            this.UncheckedTex = uncheckTex;
            this.SetMaterial(this.IsChecked ? this.CheckedTex : this.UncheckedTex);
        }

        public void SetChecked(bool Checked)
        {
            this.IsChecked = Checked;
            this.SetMaterial(this.IsChecked ? this.CheckedTex : this.UncheckedTex);
            if (OnChange != null)
                OnChange(this, this.IsChecked);
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}
