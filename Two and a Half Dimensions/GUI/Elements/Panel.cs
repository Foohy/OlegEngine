using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Two_and_a_Half_Dimensions.GUI
{
    class Panel
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public bool ShouldDraw { get; set; }
        public bool AlphaBlendmode { get; set; }
        public Vector2 Position;

        public Panel Parent { get; set; }
        public List<Panel> Children = new List<Panel>();

        //Events
        public delegate void OnMouseDownDel(MouseButtonEventArgs e);
        public event OnMouseDownDel OnMouseDown;

        public delegate void OnMouseUpDel(MouseButtonEventArgs e);
        public event OnMouseUpDel OnMouseUp;

        public delegate void OnMouseMoveDel(MouseMoveEventArgs e);
        public event OnMouseMoveDel OnMouseMove;

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

        public void SetParent(Panel parent)
        {
            this.Parent = parent;
            parent.Children.Add(this);
        }

        public bool IsMouseOver()
        {
            Vector2 MousePos = new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y);
            Vector2 PanelPos = this.GetScreenPos();

            if (MousePos.X < PanelPos.X) return false;
            if (MousePos.X > PanelPos.X + this.Width) return false;
            if (MousePos.Y < PanelPos.Y) return false;
            if (MousePos.Y > PanelPos.Y + this.Height) return false;

            return true;   
        }

        public Vector2 GetScreenPos()
        {
            Panel cur = this;
            Vector2 Pos = new Vector2();

            while (cur != null)
            {
                Pos += cur.Position;
                cur = cur.Parent;
            }

            return Pos;
        }

        #region inputs

        public virtual void MouseDown(MouseButtonEventArgs e)
        {
            foreach (Panel child in this.Children)
            {
                if (child.IsMouseOver())
                {
                    child.MouseDown(e);
                }
            }

            if (OnMouseDown != null)
            {
                OnMouseDown(e);
            }
        }

        public virtual void MouseUp(MouseButtonEventArgs e)
        {
            foreach (Panel child in this.Children)
            {
                if (child.IsMouseOver())
                {
                    child.MouseUp(e);
                }
            }


            if (OnMouseUp != null)
            {
                OnMouseUp(e);
            }
        }

        public virtual void MouseMove(MouseMoveEventArgs e)
        {
            foreach (Panel child in this.Children)
            {
                child.MouseMove(e);
            }

            if (OnMouseMove != null)
            {
                OnMouseMove(e);
            }
        }

        #endregion

        public virtual void Init()
        {
            Mat = new Material(Resource.GetTexture("gui/window.png"), Resource.GetProgram("hud"));
            panelMesh.mat = Mat;

            Width = 200;
            Height = 200;

            this.Position = new Vector2(500, 400);

            ShouldDraw = true;
            AlphaBlendmode = true;
        }

        public virtual void Draw()
        {
            if (!ShouldDraw) { return; }

            if (!AlphaBlendmode) { GL.Disable(EnableCap.Blend); }
            Vector2 posOffset = Vector2.Zero;

            if (this.Parent != null)
            {
                posOffset = this.Parent.Position;
            }

            panelMesh.mat = Mat;
            modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale(Width, Height, 1.0f);
            modelview *= Matrix4.CreateTranslation(Position.X + posOffset.X, Position.Y + posOffset.Y, 3.0f);

            panelMesh.Render(modelview);
            if (!AlphaBlendmode) { GL.Enable(EnableCap.Blend); }
        }
    }
}
