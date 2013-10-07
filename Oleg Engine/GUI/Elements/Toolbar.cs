using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OlegEngine.GUI
{
    public class Toolbar : Panel
    {
        public override void Init()
        {
            this.Width = Utilities.engine.Width;
            this.Height = 20;
            this.SetMaterial(Resource.GetTexture("gui/toolbar.png"));
            this.SetColor(40, 40, 40);
        }

        public void AddToolPanel(Panel p)
        {
            p.SetParent(this);
            p.SetHeight(this.Height);

            if (this.Children.Count > 1)
            {
                p.RightOf(this.Children[this.Children.Count-2]); //Align ourselves to the right of the last panel over
            }
        }

        public Button AddButton(string text)
        {
            Button button = GUIManager.Create<Button>();
            button.SetText(text);
            button.SizeToText(15);
            button.SetHeight(this.Height);

            this.AddToolPanel(button);

            return button;
        }

        public ContextMenu AddButtonDropDown(string text)
        {
            Button btn = this.AddButton(text);
            ContextMenu ctxt = GUIManager.Create<ContextMenu>();
            ctxt.Hide();

            btn.Userdata = ctxt;

            //When the button is clicked, show the context menu
            btn.OnButtonPress += (sender) =>
            {
                ContextMenu menu = sender.Userdata as ContextMenu;

                if (menu)
                {
                    menu.SetPos(sender.Position.X, sender.Position.Y + sender.Height);
                    menu.Toggle();
                }
            };


            return ctxt;
        }
        /*
        public ButtonDropDown AddButtonDropDown(string text)
        {
            ButtonDropDown button = GUIManager.Create<ButtonDropDown>();
            button.SetText(text);
            button.SizeToText(15);
            button.SetHeight(this.Height);

            this.AddToolPanel(button);

            return button;
        }
        */
        public override void Draw()
        {
            base.Draw();
        }
    }
}
