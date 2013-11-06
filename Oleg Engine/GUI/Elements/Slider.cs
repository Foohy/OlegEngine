using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
namespace OlegEngine.GUI
{
    public class Slider : Panel
    {
        public float Min { get; private set; }
        public float Max { get; private set; }
        public float Value { get; private set; }
        public int NumberOfDecimals { get; set; }

        /// <summary>
        /// Defines when the value for the slider has changed.
        /// <param name="oldvalue">The previous value of the slider</param>
        /// <param name="newvalue">The new value of the slider</param>
        /// </summary>
        public event Action<Panel, float> OnValueChanged;

        Button cursor;
        private bool isDragging = false;
        public override void Init()
        {
            base.Init();

            this.SetHeight(20);

            cursor = GUIManager.Create<Button>(this);
            cursor.SetWidthHeight(15, 15);
            cursor.SetImage(Resource.GetTexture("gui/slide_cursor.png"));
            cursor.SetColor(33*2, 36*2, 45*2);

            cursor.OnMouseDown += new Action<Panel, OpenTK.Input.MouseButtonEventArgs>(cursor_OnMouseDown);

            Utilities.engine.Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            Utilities.engine.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
            this.PreDraw += new Action<Panel, Vector2, DrawEventArgs>(Slider_PreDraw);
        }

        void Slider_PreDraw(Panel arg1, Vector2 arg2, DrawEventArgs e)
        {
            var realPos = this.GetScreenPos();

            Surface.SetDrawColor(33/3, 36/ 3, 45/ 3);
            Surface.DrawRect(realPos.X, realPos.Y + this.Height / 2, this.Width, 1);

            Surface.SetDrawColor(33 * 2, 36 * 2, 45 * 2);
            Surface.DrawRect(realPos.X, realPos.Y + this.Height / 2 + 1, this.Width, 2);

            Surface.SetDrawColor(33*3, 36*3, 45*3);
            Surface.DrawRect(realPos.X, realPos.Y + this.Height / 2 + 3, this.Width, 1);

            e.OverrideDraw = true;
        }

        void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (isDragging)
            {
                float relativeX = Utilities.engine.Mouse.X - this.GetScreenPos().X;
                float percent = (relativeX) / this.Width;
                this.SetValue( Utilities.Clamp( percent * (this.Max - this.Min) + this.Min, this.Max, this.Min));
            }
        }

        void cursor_OnMouseDown(Panel arg1, OpenTK.Input.MouseButtonEventArgs arg2)
        {
            isDragging = true;
        }

        protected override void ParentResized(float OldWidth, float OldHeight, float NewWidth, float NewHeight)
        {
            base.ParentResized(OldWidth, OldHeight, NewWidth, NewHeight);
            PerformLayout();
        }

        public override void Remove()
        {
            base.Remove();

            this.PreDraw -= new Action<Panel, Vector2, DrawEventArgs>(Slider_PreDraw);
            cursor.OnMouseDown -= new Action<Panel, OpenTK.Input.MouseButtonEventArgs>(cursor_OnMouseDown);
            Utilities.engine.Mouse.ButtonUp -= new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
            Utilities.engine.Mouse.Move -= new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
        }

        private void PerformLayout()
        {
            float perc = (Value - Min) / (Max - Min );

            cursor.CenterHeight();
            cursor.SetPos(perc * this.Width - cursor.Width/2, cursor.Position.Y);
        }

        public void SetValue(float value)
        {
            this.Value = (float)Math.Round(value, this.NumberOfDecimals);

            if (OnValueChanged != null)
                OnValueChanged(this, this.Value);
            
            PerformLayout();
        }

        public void SetMin(float min)
        {
            this.Min = min;
            PerformLayout();
        }

        public void SetMax(float max)
        {
            this.Max = max;
            PerformLayout();
        }

        public void SetMinMax(float min, float max)
        {
            this.Min = min;
            this.Max = max;
            PerformLayout();
        }
    }
}
