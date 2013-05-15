using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OlegEngine.GUI
{
    public class Panel
    {
        public enum DockStyle
        {
            NODOCK,
            LEFT,
            RIGHT,
            TOP,
            BOTTOM,
            FILL
        }

        [Flags]
        public enum Anchors
        {
            None    = 0x0,
            Bottom  = 0x1,
            Left    = 0x2,
            Right   = 0x4,
            Top     = 0x8
        }

        public float Width { get; protected set; }
        public float Height { get; protected set; }
        public bool ShouldDraw { get; set; }
        public bool AlphaBlendmode { get; set; }
        public bool ShouldDrawChildren { get; set; }
        public bool ShouldPassInput { get; set; } //Clicks should pass 'through' this panel to underlying panels
        public bool ClipChildren { get; set; } //Should the panel clip child panels when they go off the edge?
        public bool Enabled { get; set; }
        public bool ShouldAnchor { get; set; } //Control which style should be used: Docking, or anchor style
        public DockStyle DockingStyle { get; protected set; }
        public Anchors AnchorStyle { get; protected set; }
        public Vector3 Color { get; set; }
        public Vector2 Position { get; protected set; }
        public string Name { get; set; }

        public float PaddingLeft { get; protected set; }
        public float PaddingRight { get; protected set; }
        public float PaddingTop { get; protected set; }
        public float PaddingBottom { get; protected set; }

        public Panel Parent { get; set; }
        public Panel TopParent { get; set; }
        public List<Panel> Children = new List<Panel>();
        public bool _ToRemove = false;

        //Events
        public event Action<Panel, MouseButtonEventArgs> OnMouseDown;
        public event Action<Panel, MouseButtonEventArgs> OnMouseUp;
        public event Action<Panel, MouseMoveEventArgs> OnMouseMove;
        public event Action<Panel, ResizeEventArgs> OnResize;
        public class ResizeEventArgs : EventArgs { float OldWidth; float OldHeight; float NewWidth; float NewHeight; public ResizeEventArgs(float oldWidth, float oldHeight, float newWidth, float newHeight) { OldWidth = oldWidth; oldHeight = OldHeight; newWidth = NewWidth; newHeight = NewHeight; } }
        public event Action<Panel, Vector2> PreDraw;
        public event Action<Panel, Vector2> PostDraw;

        Material Mat;
        Mesh panelMesh = Resource.GetMesh("debug/quad.obj");
        public Matrix4 modelview;

        public Panel()
        {
            this.ClipChildren = true;
        }

        public static implicit operator bool(Panel p)
        {
            return p != null;
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
            UpdateTopParent();
        }

        public bool IsParent(Panel parent)
        {
            Panel p = this.Parent;
            while (p != null)
            {
                if (p == parent) return true;
                p = p.Parent;
            }

            return false;
        }

        public void UpdateTopParent()
        {
            Panel p = this.Parent;
            while (p != null)
            {
                p = p.Parent;
            }
            TopParent = p;
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

        public void SetPos(Vector2 pos)
        {
            this.Position = pos;

            this.Reposition();
        }

        public void SetPos(float x, float y)
        {
            this.Position = new Vector2(x, y);

            this.Reposition();
        }

        protected virtual void Reposition()
        {
        }

        #endregion

        #region Sizing Functions
        public void SetWidth(float width)
        {
            float oldW = this.Width;
            this.Width = width;
            this.Resize(oldW, this.Height, this.Width, this.Height);
        }

        public void SetHeight(float height)
        {
            float oldH = this.Height;
            this.Height = height;
            this.Resize(this.Width, oldH, this.Width, this.Height);
        }

        public void Dock(DockStyle style)
        {
            this.DockingStyle = style;
            this.ParentResized(this.Width, this.Height, this.Width, this.Height);
            this.ShouldAnchor = false;
        }

        public void DockPadding(float left, float right, float top, float bottom)
        {
            this.PaddingLeft = left;
            this.PaddingRight = right;
            this.PaddingTop = top;
            this.PaddingBottom = bottom;

            this.ParentResized(this.Width, this.Height, this.Width, this.Height);
        }

        public void SetAnchorStyle(Anchors anchors)
        {
            this.AnchorStyle = anchors;
            this.ShouldAnchor = true;
        }

        private void HandleDocking()
        {
            //Handle docking
            if (this.DockingStyle == DockStyle.NODOCK) return;
            if (this.DockingStyle == DockStyle.FILL)
            {
                this.Width = this.Parent.Width - (PaddingLeft + PaddingRight);
                this.Height = this.Parent.Height - (PaddingTop + PaddingBottom);
                this.Position = new Vector2(PaddingLeft, PaddingTop);
            }


            if (this.DockingStyle == DockStyle.TOP)
            {
                this.Position = new Vector2(PaddingLeft, PaddingTop);
                this.Width = this.Parent.Width - (PaddingLeft + PaddingRight);
            }
            if (this.DockingStyle == DockStyle.BOTTOM)
            {
                this.Position = new Vector2(PaddingLeft, this.Parent.Height - this.Height - PaddingBottom);
                this.Width = this.Parent.Width - (PaddingLeft + PaddingRight);
            }
            if (this.DockingStyle == DockStyle.LEFT)
            {
                this.Position = new Vector2(PaddingLeft, PaddingTop);
                this.Height = this.Parent.Height - (PaddingTop + PaddingBottom);
            }
            if (this.DockingStyle == DockStyle.RIGHT)
            {
                this.Position = new Vector2(this.Parent.Width - (this.Width + PaddingRight), PaddingTop);
                this.Height = this.Parent.Height - (PaddingTop + PaddingBottom);
            }
        }

        private void HandleAnchors( float DeltaW, float DeltaH )
        {
            /////////////////////////////////////
            if (this.AnchorStyle == Anchors.None ) return;

            if ((Anchors.Bottom & this.AnchorStyle) != 0)
            {
                if ((Anchors.Top & this.AnchorStyle ) != 0)
                {
                    //Set the height
                    this.Height += DeltaH; 
                }
                else
                {
                    //Move the entire panel
                    this.Position = new Vector2(this.Position.X, this.Position.Y + DeltaH);
                }
            }

            if ((Anchors.Right & this.AnchorStyle) != 0)
            {
                if ((Anchors.Left & this.AnchorStyle ) != 0)
                {
                    //Set the width
                    this.Width += DeltaW;
                }
                else
                {
                    //Move the entire panel
                    this.Position = new Vector2(this.Position.X + DeltaW, this.Position.Y);
                }
            }
            ///////////////////////////////////
        }

        protected virtual void ParentResized( float OldWidth, float OldHeight, float NewWidth, float NewHeight )
        {
            if (this.Parent == null) return;

            if (this.ShouldAnchor) HandleAnchors(NewWidth - OldWidth, NewHeight - OldHeight);
            else HandleDocking();

            //Alert that we've just resized ourselves
            this.Resize(OldWidth, OldHeight, NewWidth, NewHeight);
        }

        public virtual void Resize(float OldWidth, float OldHeight, float NewWidth, float NewHeight)
        {
            foreach (Panel child in Children)
            {
                child.ParentResized(OldWidth, OldHeight,NewWidth, NewHeight);
            }

            if (OnResize != null)
            {
                OnResize(this, new ResizeEventArgs(OldWidth, OldHeight, NewWidth, NewHeight));
            }
        }
        #endregion

        /// <summary>
        /// Whether the mouse is currently hovering over the box of the panel
        /// </summary>
        /// <returns>True - Mouse is over the panel, False - the mouse is not</returns>
        public bool IsMouseOver()
        {
            return IsPointOver( new Vector2(Utilities.window.Mouse.X, Utilities.window.Mouse.Y ));
        }

        public bool IsPointOver(Vector2 point)
        {
            Vector2 PanelPos = this.GetScreenPos();

            if (point.X < PanelPos.X) return false;
            if (point.X > PanelPos.X + this.Width) return false;
            if (point.Y < PanelPos.Y) return false;
            if (point.Y > PanelPos.Y + this.Height) return false;

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

        /// <summary>
        /// Set the color of the panel
        /// </summary>
        /// <param name="x">Red component, 0-1</param>
        /// <param name="y">Green component, 0-1</param>
        /// <param name="z">Blue component, 0-1</param>
        public void SetColorVector(float x, float y, float z)
        {
            this.Color = new Vector3(x, y, z);
        }
        /// <summary>
        /// Set the color of the panel
        /// </summary>
        /// <param name="r">Red component 0-255</param>
        /// <param name="g">Green component 0-255</param>
        /// <param name="b">Blue component 0-255</param>
        public void SetColor(float r, float g, float b)
        {
            this.Color = new Vector3(r / 255, g / 255, b / 255);
        }
        /// <summary>
        /// Set the color of a panel
        /// </summary>
        /// <param name="color">Color</param>
        public void SetColor(System.Drawing.Color color)
        {
            this.Color = new Vector3(color.R / 255, color.G / 255, color.B / 255);
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
                OnMouseDown(this, e);
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
                OnMouseUp(this, e);
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
                OnMouseMove(this, e);
            }
        }

        #endregion

        public virtual void Init()
        {
            Mat = new Material(Resource.GetTexture("gui/window.png"), Resource.GetProgram("hud"));
            panelMesh.mat = Mat;
            panelMesh.ShouldDrawDebugInfo = false;

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

            if (this.PreDraw != null) { this.PreDraw(this, posOffset); }
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


            this.panelMesh.Color = this.Color;
            panelMesh.DrawSimple(modelview);
            if (this.PostDraw != null) { this.PostDraw(this, posOffset); }


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
