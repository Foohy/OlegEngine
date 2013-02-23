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
}
