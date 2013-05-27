using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine.GUI
{
    public class Label : Panel
    {
        public enum TextAlign
        {
            TopLeft,
            MiddleLeft,
            LowerLeft,
            TopCenter,
            MiddleCenter,
            LowerCenter,
            TopRight,
            MiddleRight,
            LowerRight
        }

        public string Text { get; private set; }
        public bool Autosize { get; set; }
        public TextAlign Alignment { get; private set; }

        private Text DrawText;
        private Vector2 PosOffset;

        public Label()
        {
            this.Text = "";
            DrawText = new Text("title", this.Text);
            SizeToText();
            this.ShouldPassInput = true;
            this.Autosize = false;
            this.SetColor(255, 255, 255);
        }

        public void SetText(string str)
        {
            this.Text = str;
            DrawText.SetText(this.Text);
            if (this.Autosize) { SizeToText(); }
            PositionText();
        }

        public void SetAlignment(TextAlign align)
        {
            this.Alignment = align;
            if (this.Autosize) { SizeToText(); }
            this.PositionText();
        }

        protected override void ParentResized( float oldWidth, float oldHeight, float newWidth, float newHeight)
        {
            base.ParentResized(oldWidth, oldHeight, newWidth, newHeight);
            if (this.Autosize) { SizeToText(); }
            PositionText();
        }

        public void SizeToText()
        {
            this.SetWidth(DrawText.GetTextLength(this.Text));
            this.SetHeight(DrawText.GetTextHeight());
        }

        public void PositionText()
        {
            if (Alignment == TextAlign.LowerLeft ||
                Alignment == TextAlign.MiddleLeft ||
                Alignment == TextAlign.TopLeft )
            {
                PosOffset = new Vector2(0, 0);
            }
            if (Alignment == TextAlign.LowerCenter ||
                Alignment == TextAlign.MiddleCenter ||
                Alignment == TextAlign.TopCenter)
            {
                PosOffset = new Vector2((this.Width/2) - (this.GetTextLength()/2), 0);
            }
            if (Alignment == TextAlign.LowerRight ||
                Alignment == TextAlign.MiddleRight ||
                Alignment == TextAlign.TopRight)
            {
                PosOffset = new Vector2(this.Width-this.GetTextLength(), 0);
            }

            if (Alignment == TextAlign.LowerLeft ||
                Alignment == TextAlign.LowerCenter ||
                Alignment == TextAlign.LowerRight)
            {
                PosOffset += new Vector2(0, this.Height - this.GetTextHeight());
            }
            if (Alignment == TextAlign.MiddleLeft ||
                Alignment == TextAlign.MiddleCenter ||
                Alignment == TextAlign.MiddleRight)
            {
                PosOffset += new Vector2(0, (this.Height / 2) - (this.GetTextHeight() / 2));
            }
            if (Alignment == TextAlign.TopLeft ||
                Alignment == TextAlign.TopCenter ||
                Alignment == TextAlign.TopRight)
            {
                PosOffset += new Vector2(0, 0);
            }
        }

        public void SetTextSize(float Width, float Height)
        {
            this.DrawText.SetScale(Width, Height);
        }

        public void SetFont(string font)
        {
            this.DrawText = new Text(font, this.Text);
        }

        protected override void Reposition()
        {
            base.Reposition();
            PositionText();
        }

        public float GetTextLength()
        {
            return this.DrawText.GetTextLength(this.Text);
        }

        public float GetTextHeight()
        {
            return this.DrawText.GetTextHeight();
        }

        public override void Draw()
        {
            Vector2 truePos = this.GetScreenPos();
            DrawText.SetPos(truePos.X + PosOffset.X, truePos.Y + PosOffset.Y);//TODO: handle aligning to right/center
            DrawText.Color = this.Color;

            int X, Y, W, H;
            bool clipping = ConstructClip(truePos, out X, out Y, out W, out H);
            if (clipping)
            {
                GL.Enable(EnableCap.ScissorTest);
                GL.Scissor(X, Y, W, H);
            }

            DrawText.Draw();


            if (clipping)
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }
    }
}
