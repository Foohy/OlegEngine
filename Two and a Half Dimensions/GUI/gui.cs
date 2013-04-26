using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Two_and_a_Half_Dimensions.GUI
{
    class GUIManager
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

        public static void Init()
        {
            Surface.Init();

            Utilities.window.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            Utilities.window.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
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
            for (int i = elements.Count-1; i >= 0; i--)
            {
                Panel p = elements[i];
                if (p.IsMouseOver())
                {
                    p.MouseDown(e);

                    if (!p.ShouldPassInput || !p.ShouldDraw)
                    {
                        break;
                    }
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
                if (panel.Parent == null && panel.IsMouseOver() && !panel.ShouldPassInput && panel.ShouldDraw)
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
            GL.DepthFunc(DepthFunction.Always);
            GL.Enable(EnableCap.Blend);

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
            GL.Disable(EnableCap.Blend);
        }
    }

    class Surface
    {
        static Mesh Square;
        static Matrix4 TranslateMatrix = Matrix4.Identity;

        public static void Init()
        {
            Square = new Mesh("debug/quad.obj");
            Square.mat = Utilities.LoadMaterial("engine/white");
            Square.mat.SetShader(Resource.GetProgram("default"));
            Square.ShouldDrawDebugInfo = false;
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
        }
        /// <summary>
        /// Set the color
        /// </summary>
        /// <param name="r">Red component 0-255</param>
        /// <param name="g">Green component 0-255</param>
        /// <param name="b">Blue component 0-255</param>
        public static void SetDrawColor(float r, float g, float b)
        {
            Square.Color = new Vector3(r / 255, g / 255, b / 255);
        }
        /// <summary>
        /// Set the color
        /// </summary>
        /// <param name="color">Color</param>
        public static void SetDrawColor(System.Drawing.Color color)
        {
            Square.Color = new Vector3(color.R / 255, color.G / 255, color.B / 255);
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

        private static void drawRect(float x, float y, float width, float height)
        {
            TranslateMatrix = Matrix4.CreateTranslation(Vector3.Zero);
            TranslateMatrix *= Matrix4.Scale(width, height, 1.0f);
            TranslateMatrix *= Matrix4.CreateTranslation(x, y, 3.0f);

            Square.DrawSimple(TranslateMatrix);
        }
    }
}
