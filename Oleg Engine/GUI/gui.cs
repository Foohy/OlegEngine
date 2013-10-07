using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine.GUI
{
    public class GUIManager
    {
        public delegate void OnDrawHUD(EventArgs e);
        public static event OnDrawHUD PostDrawHUD;
        public static bool IsMouseOverElement { get; private set; }

        private static List<GUI.Panel> elements = new List<Panel>();
        private static EventArgs ev = new EventArgs();

        public static T Create<T>() where T : Panel, new()
        {
            T panel = new T();
            panel.ShouldDraw = true;
            panel.ShouldDrawChildren = true;
            panel.Init();

            elements.Add(panel);
            return panel;
        }

        public static T Create<T>( Panel p) where T : Panel, new()
        {
            T panel = new T();
            panel.ShouldDraw = true;
            panel.ShouldDrawChildren = true;
            panel.SetParent(p);
            panel.Init();

            elements.Add(panel);
            return panel;
        }

        public static void Init()
        {
            Surface.Init();

            Utilities.engine.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.engine.Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            Utilities.engine.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
            Utilities.engine.KeyPress += new EventHandler<KeyPressEventArgs>(window_KeyPress);
        }

        private static void UpdateIsOverElement()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Parent == null && elements[i].IsMouseOver() && (!elements[i].ShouldPassInput))
                {
                    IsMouseOverElement = true;
                    break;
                }

                if (i == elements.Count - 1) IsMouseOverElement = false; //If we managed to get through everything in the list, the mouse isn't over anything
            }
        }

        static void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            for (int i = elements.Count-1; i >= 0; i--)
            {
                Panel p = elements[i];
                p.MouseUp(e);
            }
        }

        static void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            foreach (Panel p in elements)
            {
                p.MouseMove(e);
            }
        }

        static void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            //Going through each element in the list (from toplevel to bottom level)
            for (int i = elements.Count-1; i >= 0; i--)
            {
                Panel p = elements[i];
                //If the mouse is over the element
                if (p.Enabled &&  p.IsMouseOver())
                {
                    if (!p.Parent)
                        p.MouseDown(e);
                    else p.TopParent.MouseDown(e);

                    //If the element shouldn't let input pass through or we're not drawing the element stop now
                    if (!p.ShouldPassInput || !p.ShouldDraw)
                    {
                        break;
                    }
                }
            }
        }

        static void window_KeyPress(object sender, KeyPressEventArgs e)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                Panel p = elements[i];
                if (p.Enabled && elements[i].Parent == null) //Only call this on top level panels. the panels can control further input
                {
                    p.KeyPressed(e);
                }
            }
        }


        //Queue up sending things to the front
        private static List<Panel> front_queue = new List<Panel>();
        public static void SendToFront(Panel p)
        {
            front_queue.Insert(0, p);
        }

        private static List<Panel> back_queue = new List<Panel>();
        public static void SendToBack(Panel p)
        {
            back_queue.Insert(0, p);
        }

        public static bool IsPanelAbovePoint(Vector2 pos, Panel p)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                Panel panel = elements[i];
                if (panel.Parent == null && panel.IsPointOver(pos) && !panel.ShouldPassInput && panel.ShouldDraw)
                {
                    if (panel == p || p.IsParent(panel))
                    {
                        return false;
                    }
                    else return true;
                }
            }

            return false;
        }

        //Return which of the given panels is higher (closer to the user/drawn last)
        public static Panel GetHigherPanel(Panel p1, Panel p2)
        {
            return elements.IndexOf(p1) > elements.IndexOf(p2) ? p1 : p2;
        }

        /// <summary>
        /// Get the index of a given panel (Useful for comparison with other panels to get a 'height')
        /// </summary>
        /// <param name="p">The panel to get the index of</param>
        /// <returns>Index ('height')</returns>
        public static int GetPanelIndex( Panel p )
        {
            return elements.IndexOf(p);
        }

        private static void UpdatePanels()
        {
            if (front_queue.Count > 0 || back_queue.Count > 0)
            {
                //Send all the panels in the queue to the front
                foreach (Panel p in front_queue)
                {
                    elements.Remove(p);
                    elements.Add(p);
                }
                front_queue.Clear();

                //And these to the back
                foreach (Panel p in back_queue)
                {
                    elements.Remove(p);
                    elements.Insert(0, p);
                }
                back_queue.Clear();
            }

            //Clear panels marked to be removed
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i]._ToRemove)
                {
                    elements.RemoveAt(i);
                    i--;
                }

            }
        }

        public static void Draw()
        {
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Multisample);
            GL.DepthFunc(DepthFunction.Always);
            Graphics.EnableBlending(true);

            UpdatePanels();
            UpdateIsOverElement();

            foreach (Panel p in elements)
            {
                if (p.Parent == null)
                {
                    p.Draw();
                }
            }

            if (PostDrawHUD != null)
            {
                PostDrawHUD(ev);
            }


            GL.Enable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.Multisample);
            Graphics.EnableBlending(false);
        }
    }

    public class Surface
    {
        static Mesh Square;
        static Matrix4 TranslateMatrix = Matrix4.Identity;
        static Text genericText;

        public static void Init()
        {
            Square = EngineResources.CreateNewQuadMesh();
            Square.mat = new Material(Utilities.AlphaTex, "default"); ;
            Square.ShouldDrawDebugInfo = false;

            genericText = new Text("debug", "Untitled");
        }

        /// <summary>
        /// Set the color
        /// </summary>
        /// <param name="x">Red component, 0-1</param>
        /// <param name="y">Green component, 0-1</param>
        /// <param name="z">Blue component, 0-1</param>
        public static void SetDrawColorVector(float x, float y, float z)
        {
            Square.Color = new Vector3(x, y, z);
            genericText.Color = Square.Color;
        }
        /// <summary>
        /// Set the color
        /// </summary>
        /// <param name="r">Red component 0-255</param>
        /// <param name="g">Green component 0-255</param>
        /// <param name="b">Blue component 0-255</param>
        public static void SetDrawColor(float r, float g, float b)
        {
            SetDrawColorVector(r / 255, g / 255, b / 255);
        }
        /// <summary>
        /// Set the color
        /// </summary>
        /// <param name="color">Color</param>
        public static void SetDrawColor(System.Drawing.Color color)
        {
            SetDrawColorVector(color.R / 255, color.G / 255, color.B / 255);
        }

        public static void SetTexture(int texID)
        {
            Square.mat.Properties.BaseTexture = texID;
        }

        public static void SetNoTexture()
        {
            Square.mat.Properties.BaseTexture = Utilities.AlphaTex;
        }

        public static void DrawRect(float x, float y, float width, float height)
        {
            drawRect(x, y, width, height);
        }

        public static void DrawRect(Vector2 position, float width, float height)
        {
            drawRect(position.X, position.Y, width, height);
        }

        public static void DrawRect(Vector2 position, Vector2 dimensions)
        {
            drawRect(position.X, position.Y, dimensions.X, dimensions.Y);
        }

        public static void DrawWrappedText( string font, string text, float x, float y, float Width)
        {
            float textWidth, lineHeight, curX, curY;
            GetTextSize(font, text, out textWidth, out lineHeight);
            curX = curY = 0;
            string tempString = "";
            string curWord = "";
            float curWordLength = 0;
            for (int i = 0; i < text.Length; i++)
            {
                float charSize = GetTextLength(font, text[i].ToString());
                curWord += text[i].ToString();
                curWordLength += charSize;
                //Word seperator
                if (text[i] == ' ')
                {
                    tempString += curWord;
                    curX += curWordLength;
                    curWord = "";
                    curWordLength = 0;
                }


                if (curX + curWordLength > Width)
                {
                    DrawSimpleText(font, tempString, x, y + curY);

                    curX = 0;
                    curY += lineHeight;
                    tempString = "";
                }

                if (i == text.Length-1)
                    tempString += curWord;
            }

            DrawSimpleText(font, tempString, x, y + curY);
        }

        public static void DrawSimpleText(string font, string str, float x, float y)
        {
            Text.Charset ch = Resource.GetCharset( font );
            if (ch)
            {
                genericText.SetCharset(Resource.GetCharset(font));
            }
            genericText.SetPos( x, y );
            genericText.SetText(str);
            genericText.Draw();
        }

        public static float GetTextLength(string font, string str)
        {
            Text.Charset ch = Resource.GetCharset(font);
            if (ch)
            {
                genericText.SetCharset(Resource.GetCharset(font));
            }

            genericText.SetText(str);
            return genericText.GetTextLength();
        }

        public static float GetTextHeight(string font)
        {
            Text.Charset ch = Resource.GetCharset(font);
            if (ch)
            {
                genericText.SetCharset(Resource.GetCharset(font));
            }

            return genericText.GetTextHeight();
        }

        public static void GetTextSize(string font, string str, out float Width, out float Height)
        {
            Width = GetTextLength(font, str);
            Height = GetTextHeight(font);
        }


        private static void drawRect(float x, float y, float width, float height)
        {
            TranslateMatrix = Matrix4.CreateTranslation(Vector3.Zero);
            TranslateMatrix *= Matrix4.Scale(width, height, 1.0f);
            TranslateMatrix *= Matrix4.CreateTranslation(x, y, 3.0f);

            Square.DrawSimple(TranslateMatrix);
        }
    }
}
