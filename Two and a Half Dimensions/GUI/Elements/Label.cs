using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Two_and_a_Half_Dimensions.GUI
{
    class Label : Panel
    {

        public string Text { get; private set; }
        public bool AutoStretch { get; set; }

        private font DrawText;

        public Label()
        {
            this.Text = "";
            DrawText = new font("title", this.Text);
            this.ShouldPassInput = true;
        }

        public void SetText(string str)
        {
            this.Text = str;
            DrawText.SetText(this.Text);

            if (this.AutoStretch)
            {
                this.Width = DrawText.GetTextLength(this.Text);
            }
        }

        public float GetTextLength()
        {
            return this.DrawText.GetTextLength(this.Text);
        }

        public override void Draw()
        {
            Vector2 truePos = this.GetScreenPos();
            DrawText.SetPos(truePos.X, truePos.Y);//TODO: handle aligning to right/center
            DrawText.Color = this.Color;
            DrawText.Draw();
        }
    }
}
