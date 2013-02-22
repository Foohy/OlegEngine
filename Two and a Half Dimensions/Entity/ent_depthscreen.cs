using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions.Entity
{
    class ent_depthscreen : BaseEntity 
    {
        private bool shouldDraw = true;
        GUI.Panel DepthScreen;
        float Size = 8;
        public override void Init()
        {
            this.Mat = Resource.GetMaterial("engine/depth");
            this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.Model = Resource.GetMesh("debug/quad.obj");

            DepthScreen = GUI.GUIManager.Create<GUI.Panel>();
            DepthScreen.SetMaterial(Resource.GetMaterial("engine/depth"));
            DepthScreen.SetWidth(Utilities.window.Width / Size);
            DepthScreen.SetHeight(Utilities.window.Height / Size);
            DepthScreen.SetPos(new Vector2(0, Utilities.window.Height - DepthScreen.Height));
            DepthScreen.AlphaBlendmode = false;

            Utilities.window.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
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
