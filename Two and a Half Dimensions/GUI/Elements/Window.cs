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

        public override void Init()
        {
            this.SetMaterial(Resource.GetTexture("gui/window.png"));
            Title = new Panel();
            Title.Init();
            Title.SetMaterial(Resource.GetTexture("gui/title.png"));
            Title.Height = 20;

            Width = 600;
            Height = 400;

            this.Position = new Vector2(500, 380);
        }

        public override void Draw()
        {
            //this.Position = new Vector2(500 + (float)Math.Sin(Utilities.Time) * 200f, 500 );
            this.Width = 150 + (float)Math.Cos(Utilities.Time) * 100;
            this.Height = 150 + (float)Math.Sin(Utilities.Time) * 100;


            Title.Position = this.Position - new Vector2(0, Title.Height);
            Title.Width = this.Width;

            base.Draw();
            Title.Draw();
            
        }
    }
}
