using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions.GUI
{
    class Window : Panel
    {
        Panel Title;
        font TitleText;
        Vector2 Offset = Vector2.Zero;
        bool dragging = false;
        public override void Init()
        {
            this.SetMaterial(Resource.GetTexture("gui/window.png"));
            Title = new Panel();
            Title.Init();
            Title.SetMaterial(Resource.GetTexture("gui/title.png"));
            Title.Height = 20;

            Width = 600;
            Height = 400;

            this.Position = new Vector2(200, 480);
            TitleText = new font("title", "this is my favorite window");
        }

        public override void MouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            if (Title.IsMouseOver())
            {
                this.dragging = true;
                Offset = new Vector2(Utilities.window.Mouse.X - this.Position.X, Utilities.window.Mouse.Y - this.Position.Y);
            }
        }

        public override void MouseMove(OpenTK.Input.MouseMoveEventArgs e)
        {
            if (dragging)
            {
                this.Position = new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y) - Offset;
            }
        }

        public override void MouseUp(OpenTK.Input.MouseButtonEventArgs e)
        {
            if (dragging)
            {
                dragging = false;
                Offset = Vector2.Zero;
            }
        }

        public override void Draw()
        {
            Title.Position = this.Position - new Vector2(0, Title.Height);
            Title.Width = this.Width;

            TitleText.SetPos(Title.Position.X + 5, Title.Position.Y);

            base.Draw();
            Title.Draw();
            TitleText.Draw();
        }
    }
}
