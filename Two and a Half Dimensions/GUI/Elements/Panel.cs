using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions.GUI
{
    class Panel
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public bool ShouldDraw { get; set; }
        public Vector2 Position;

        Material Mat;
        Mesh panelMesh = Resource.GetMesh("debug/quad.obj");
        public Matrix4 modelview;

        public void SetMaterial(int TextureBuffer)
        {
            if (Mat == null) Mat = new Material(TextureBuffer, Resource.GetProgram("hud"));
            else Mat.Properties.BaseTexture = TextureBuffer;
        }

        public void SetMaterial(Material mat)
        {
            if (Mat == null) Mat = new Material(mat.Properties);
            else Mat.SetProperties(mat.Properties);
        }

        public void SetShader(int Program)
        {
            if (Mat == null) Mat = new Material(Resource.GetTexture("gui/window.png"), Program);
            else Mat.SetShader(Program);
        }

        public virtual void Init()
        {
            Mat = new Material(Resource.GetTexture("gui/window.png"), Resource.GetProgram("hud"));
            panelMesh.mat = Mat;

            Width = 200;
            Height = 200;

            this.Position = new Vector2(500, 400);

            ShouldDraw = true;
        }

        public virtual void Draw()
        {
            if (!ShouldDraw) { return; }

            panelMesh.mat = Mat;
            modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale(Width, Height, 1.0f);
            //modelview *= Matrix4.CreateRotationY((float)Utilities.Time);
            //modelview *= Matrix4.CreateRotationZ((float)Utilities.Time -1);
            modelview *= Matrix4.CreateTranslation(Position.X, Position.Y, 3.0f);

            panelMesh.Render(modelview);
        }
    }
}
