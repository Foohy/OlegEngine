using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using OlegEngine;
using OlegEngine.Entity;
using OlegEngine.GUI;

namespace Gravity_Car.Entity
{
    class ent_depthscreen : BaseEntity 
    {
        private bool shouldDraw = true;
        Panel DepthScreen;
        float Size = 8;
        public override void Init()
        {
            this.Mat = Resource.GetMaterial("engine/depth");
            this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.Model = Resource.GetMesh("engine/quad.obj");

            DepthScreen = GUIManager.Create<Panel>();
            DepthScreen.SetMaterial(Resource.GetMaterial("engine/depth"));
            DepthScreen.SetWidth(Utilities.engine.Width / Size);
            DepthScreen.SetHeight(Utilities.engine.Height / Size);
            DepthScreen.SetPos(new Vector2(0, Utilities.engine.Height - DepthScreen.Height));
            DepthScreen.AlphaBlendmode = false;

            Utilities.engine.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F1)
            {
                shouldDraw = !shouldDraw;
                DepthScreen.ShouldDraw = shouldDraw;
            }
        }

        public override void Think()
        {

        }

        public override void Draw()
        {

        }
    }
}
