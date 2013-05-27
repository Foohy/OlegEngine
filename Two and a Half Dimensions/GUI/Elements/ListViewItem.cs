using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OlegEngine.GUI
{
    public class ListViewItem : Panel 
    {
        public object Userdata { get; set; }
        public string[] Labels { get; protected set; }
        public bool Selected { get; private set; }
        public event Action<ListViewItem, bool> OnSelectedChange;

        private Label[] labelPanels;
        public override void Init()
        {
            base.Init();

            this.SetColor(33, 36, 45);
            this.SetHeight(20);
        }

        public override void MouseMove(OpenTK.Input.MouseMoveEventArgs e)
        {
            if (!this.Selected)
            {
                if (this.IsMouseOver() && this.Enabled)
                    this.SetColor(33 * 2, 36 * 2, 45 * 2);
                else
                    this.SetColor(33, 36, 45);
            }
        }

        public override void MouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            base.MouseDown(e);

            if (this.Enabled && this.IsMouseOver())
            {
                this.SetIsSelected(true);
            }
            else { this.SetIsSelected(false); }
        }

        public override void Remove()
        {
            base.Remove();
        }

        public void SetLabels(params string[] labels)
        {
            Labels = labels;

            labelPanels = new Label[Labels.Length];
            for (int i = 0; i < Labels.Length; i++)
            {
                string label = Labels[i];
                Label l = GUIManager.Create<Label>(this);
                l.SetText(label);
                l.SizeToText();

                //TODO: Set position according to listview categories
                if (i > 0) l.SetPos(labelPanels.Length * 70 + 3, 0);
                
                labelPanels[i] = l;  
            }
        }

        public void SetIsSelected(bool selected)
        {
            if (this.Selected != selected)
            {
                this.Selected = selected;
                this.SetColor( this.Selected ? System.Drawing.Color.FromArgb( 33*3, 36*3, 45*3) : System.Drawing.Color.FromArgb(33, 36, 45) );
                if (this.OnSelectedChange != null)
                    this.OnSelectedChange(this, this.Selected);
            }
        }
    }
}
