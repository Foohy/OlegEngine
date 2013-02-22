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
        public float Width { get; protected set; }
        public float Height { get; protected set; }
        public bool ShouldDraw { get; set; }
        public bool AlphaBlendmode { get; set; }
        public bool ShouldDrawChildren { get; set; }
        public bool ShouldPassInput { get; set; } //Clicks should pass 'through' this panel to underlying panels
        public bool ClipChildren { get; set; } //Should the panel clip child panels when they go off the edge?
        public bool Enabled { get; set; }
        public Vector3 Color { get; set; }
        public Vector2 Position;

        public Panel Parent { get; set; }
        public List<Panel> Children = new List<Panel>();
        public bool _ToRemove = false;

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

        public Panel()
        {
            this.ClipChildren = true;
        }

        /// <summary>
        /// Set the material of the panel
        /// </summary>
        /// <param name="TextureBuffer"></param>
        public void SetMaterial(int TextureBuffer)
        {
            if (Mat == null) Mat = new Material(TextureBuffer, Resource.GetProgram("hud"));
            else Mat.Properties.BaseTexture = TextureBuffer;
        }

        /// <summary>
        /// Set the material of the panel
        /// </summary>
        /// <param name="Material"></param>
        public void SetMaterial(Material mat)
        {
            if (Mat == null) Mat = new Material(mat.Properties);
            else Mat.SetProperties(mat.Properties);
        }

        /// <summary>
        /// Set the shader of the panel
        /// </summary>
        /// <param name="Shader Program"></param>
        public void SetShader(int Program)
        {
            if (Mat == null) Mat = new Material(Resource.GetTexture("gui/window.png"), Program);
            else Mat.SetShader(Program);
        }

        #region Positioning Functions

        /// <summary>
        /// Parent this panel to the given panel. Position will be relative to the parent's location.
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(Panel parent)
        {
            this.Parent = parent;
            parent.Children.Add(this);
        }

        /// <summary>
        /// Send the panel to the top of the rendering stack, so it'll be drawn first.
        /// </summary>
        public void SendToFront()
        {
            if (this.Parent != null)
            {
                for (int i = 0; i < this.Parent.Children.Count; i++)
                {
                    if (this.Parent.Children[i] == this)
                    {
                        this.Parent.Children.RemoveAt(i);
                        this.Parent.Children.Add(this);
                    }
                }
            }
            else
            {
                GUIManager.SendToFront(this);
            }
        }

        public void SendToBack()
        {
            if (this.Parent != null)
            {
                for (int i = 0; i < this.Parent.Children.Count; i++)
                {
                    if (this.Parent.Children[i] == this)
                    {
                        this.Parent.Children.RemoveAt(i);
                        this.Parent.Children.Insert(0, this);
                    }
                }
            }
            else
            {
                GUIManager.SendToBack(this);
            }
        }

        /// <summary>
        /// Align the panel to the rightmost edge of the parent panel
        /// </summary>
        /// <param name="offset">Distance away from the edge</param>
        public void AlignRight(int offset = 0)
        {
            if (this.Parent != null)
            {
                this.Position = new Vector2(this.Parent.Width - (this.Width + offset), this.Position.Y);
            }
            else
            {
                this.Position = new Vector2(Utilities.window.X - (this.Width + offset), this.Position.Y);
            }
        }

        /// <summary>
        /// Align the panel to the leftmost edge of the parent panel
        /// </summary>
        /// <param name="offset">Distance away from the edge</param>
        public void AlignLeft( int offset = 0 )
        {
            this.Position = new Vector2(offset, this.Position.Y);
        }

        public void RightOf(Panel p, int offset = 0)
        {
            this.Position = new Vector2(p.Position.X + p.Width + offset, this.Position.Y);
        }

        public void LeftOf(Panel p, int offset = 0)
        {
            this.Position = new Vector2(p.Position.X - offset, this.Position.Y);
        }

        public void Below(Panel p, int offset = 0)
        {
            this.Position = new Vector2(this.Position.X, p.Position.Y + p.Height + offset);
        }

        public void Above(Panel p, int offset = 0)
        {
            this.Position = new Vector2(this.Position.X, p.Position.Y - offset);
        }

        #endregion

        #region Sizing Functions
        public void SetWidth(float width)
        {
            this.Width = width;
            this.Resize();
        }

        public void SetHeight(float height)
        {
            this.Height = height;
            this.Resize();
        }
        #endregion

        /// <summary>
        /// Whether the mouse is currently hovering over the box of the panel
        /// </summary>
        /// <returns>True - Mouse is over the panel, False - the mouse is not</returns>
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

        /// <summary>
        /// Get the 'true' position of a panel (one relative to the screen, not the parent panel)
        /// </summary>
        /// <returns>Screen position of the panel</returns>
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

        public void SetColor(float x, float y, float z)
        {
            this.Color = new Vector3(x, y, z);
        }

        public virtual void Resize()
        {

        }

        /// <summary>
        /// Remove the panel and all of its children
        /// </summary>
        public virtual void Remove()
        {
            foreach (Panel p in this.Children)
            {
                p.Remove();
            }

            this._ToRemove = true;
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

            this.Position = new Vector2(0, 0);

            ShouldDraw = true;
            AlphaBlendmode = true;
        }

        public virtual void Draw()
        {
            if (!ShouldDraw) { return; }

            Vector2 posOffset = this.GetScreenPos();
  
            if (!AlphaBlendmode) { GL.Disable(EnableCap.Blend); }
            bool clipping = (this.ClipChildren && ((this.Parent != null && !this.Parent.ClipChildren ) || this.Parent == null)); //Clip if clipping is enabled, our parent isn't clipping, or our parent is null
            if (clipping)
            {
                GL.Enable(EnableCap.ScissorTest);
                GL.Scissor( (int)posOffset.X, (int)(Utilities.window.Height - posOffset.Y - this.Height), (int)this.Width, (int)this.Height );
            }

            panelMesh.mat = Mat;
            modelview = Matrix4.CreateTranslation(Vector3.Zero);
            modelview *= Matrix4.Scale(Width, Height, 1.0f);
            modelview *= Matrix4.CreateTranslation(posOffset.X, posOffset.Y, 3.0f);

            this.panelMesh.mat.Properties.Color = this.Color;
            panelMesh.Draw(modelview);
            if (!AlphaBlendmode) { GL.Enable(EnableCap.Blend); }

            //Draw our children
            if (ShouldDrawChildren)
            {
                DrawChildren();
            }

            if (clipping)
            {
                GL.Disable(EnableCap.ScissorTest);
            }

        }

        private void DrawChildren()
        {
            foreach (Panel p in this.Children)
            {
                p.Draw();
            }
        }
    }
}
