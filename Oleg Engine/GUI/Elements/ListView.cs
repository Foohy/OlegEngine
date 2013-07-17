using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OlegEngine.GUI
{
    public class ListView : ScrollBar 
    {
        private List<Panel> Panels = new List<Panel>();
        public ListViewItem SelectedPanel { get; set; }

        public override void Init()
        {
            base.Init();

            this.SetMaterial(Utilities.White);
            this.SetColor(16, 19, 26);
            this.ScrollPanel.SetColor(16, 19, 26);
        }

        public override void MouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            base.MouseDown(e);

            Panel HighestItem = this.ScrollPanel.GetHighestChildAtPos(new OpenTK.Vector2(e.X, e.Y));
            if (!HighestItem)
            {
                foreach (var item in Panels)
                {
                    if (item is ListViewItem)
                    {
                        ListViewItem listItem = item as ListViewItem;
                        listItem.SetIsSelected(false);
                    }
                }

                this.SelectedPanel = null;
            }
        }

        public void AddPanel(Panel p)
        {
            p.SetParent(this.ScrollPanel);

            if (this.Panels.Count > 0 && this.Panels[this.Panels.Count - 1])
            {
                p.Below(this.Panels[this.Panels.Count - 1], 3);
            }

            Panels.Add(p);

            p.SetWidth(this.ScrollPanel.Width);
            p.SetAnchorStyle(Anchors.Left | Anchors.Top | Anchors.Right);

            this.ScrollPanel.SetHeight(p.Position.Y + p.Height);
        }

        public void AddListItem(object Userdata, params string[] Labels)
        {
            ListViewItem item = GUIManager.Create<ListViewItem>();
            item.SetLabels(Labels);
            item.Userdata = Userdata;
            item.OnSelectedChange += new Action<ListViewItem, bool>(item_OnSelectedChange);

            this.AddPanel(item);
        }

        void item_OnSelectedChange(ListViewItem sender, bool isSelected)
        {
            if (isSelected && sender != SelectedPanel)
            {
                SelectedPanel = sender;

                foreach (Panel item in this.Panels)
                {
                    if (!(item is ListViewItem)) continue;
                    ListViewItem listItem = item as ListViewItem;

                    if (listItem != SelectedPanel) listItem.SetIsSelected(false);
                }
            }
        }
    }
}
