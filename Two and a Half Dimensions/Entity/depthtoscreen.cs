using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions.Entity
{
    class DepthScreen : BaseEntity 
    {
        private bool shouldDraw = true;
        public override void Init()
        {
            this.Mat = Resource.GetMaterial(Utilities.window.shadowFBO.shadowMap, "depthtest");
            this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.Model = Resource.GetMesh("debug/quad.obj");

            Utilities.window.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F1)
            {
                shouldDraw = !shouldDraw;
            }
        }

        public override void Think()
        {
            //this.SetPos(Utilities.window.ply.Pos);
            //this.SetAngle(new Vector3((float)Utilities.Time, (float)Utilities.Time, (float)Utilities.Time ));
        }

        public override void Draw()
        {
            if (!shouldDraw) { return; }
            if (Utilities.CurrentPass == 2)
            {
                GL.Disable(EnableCap.CullFace);
                GL.DepthFunc(DepthFunction.Always);
                base.Draw();
                GL.Enable(EnableCap.CullFace);
                GL.DepthFunc(DepthFunction.Less);
            }
        }
    }
}
