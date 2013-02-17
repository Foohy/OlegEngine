﻿using System;
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

        public override void Draw()
        {
            this.Width = 250 + (float)Math.Cos(Utilities.Time) * 100;
            this.Height = 250 + (float)Math.Sin(Utilities.Time) * 100;


            Title.Position = this.Position - new Vector2(0, Title.Height);
            Title.Width = this.Width;

            TitleText.SetPos(Title.Position.X + 5, Title.Position.Y);

            base.Draw();
            Title.Draw();
            TitleText.Draw();
        }
    }
}
