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

        private static List<GUI.Panel> elements = new List<Panel>();
        private static EventArgs ev = new EventArgs();

        public static T Create<T>() where T : Panel, new()
        {
            T panel = new T();
            panel.ShouldDraw = true;
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

        static void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            for (int i = elements.Count-1; i >= 0; i--)
            {
                Panel p = elements[i];
                if (p.IsMouseOver())
                {
                    p.MouseUp(e);
                    //break; //Only trigger it for the top-clicked element
                }
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
                    break;
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

        private static void UpdateDrawOrder()
        {
            if (front_queue.Count < 1 && back_queue.Count < 1) return;

            foreach (Panel p in front_queue)
            {
                elements.Remove(p);
                elements.Add(p);
            }

            front_queue.Clear();

            foreach (Panel p in back_queue)
            {
                elements.Remove(p);
                elements.Insert(0, p);
            }

            back_queue.Clear();
        }

        public static void Draw()
        {
            GL.Disable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Always);
            GL.Enable(EnableCap.Blend);

            UpdateDrawOrder();

            foreach (Panel p in elements)
            {
                p.Draw();
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
