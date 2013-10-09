using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OlegEngine.GUI
{
    public class ContextMenu : Panel
    {
        public List<Panel> Items = new List<Panel>();
        public int ItemPadding { get; set; }
        public int DefaultItemHeight { get; set; }
        public bool IsHidden { get; set; }

        public ContextMenu()
        {
            DefaultItemHeight = 20;
            ItemPadding = 0;

            this.SetColor(33, 36, 45);
        }


        public override void MouseUp(OpenTK.Input.MouseButtonEventArgs e)
        {
            base.MouseUp(e);

            this.Hide();
        }

        public void AddPanel(Panel panel)
        {
            panel.SetParent(this);
            panel.SetWidth(this.Width);
            panel.SetAnchorStyle(Anchors.Left | Anchors.Top | Anchors.Right);
            Items.Add(panel);

            assemblePanel();
        }

        public Button AddButton(string text)
        {
            Button button = GUIManager.Create<Button>();
            button.SetText(text);
            button.SizeToText(15);
            button.SetHeight(DefaultItemHeight);

            button.TextLabel.SetAlignment(Label.TextAlign.MiddleLeft);
            this.AddPanel(button);

            return button;
        }

        public void Show()
        {
            this.IsHidden = false;
            setHiddenProps();
        }

        public void Hide()
        {
            this.IsHidden = true;
            setHiddenProps();
        }

        public void Toggle()
        {
            this.IsHidden = !this.IsHidden;
            setHiddenProps();
        }

        private void setHiddenProps()
        {
            this.SetEnabled(!this.IsHidden, false);
            this.IsVisible = !this.IsHidden;
            this.ShouldPassInput = this.IsHidden;
        }

        /// <summary>
        /// Perform some layouting of the panel to make sure everything is in order
        /// </summary>
        private void assemblePanel()
        {
            int curY = 0;
            foreach (Panel p in Items)
            {
                p.SetPos(0, curY);
                p.SetWidth(this.Width);

                //Only add the padding if it isn't the last element
                curY += (int)p.Height + (Items[Items.Count - 1] == p ? 0 : ItemPadding);
            }

            this.SetHeight(curY);

            //Double set our status on drawing and stuff
            setHiddenProps();
        }
    }
}
