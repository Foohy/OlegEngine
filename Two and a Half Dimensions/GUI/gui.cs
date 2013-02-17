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

        public static List<GUI.Panel> elements = new List<Panel>();
        private static EventArgs ev = new EventArgs();

        public static T Create<T>() where T : Panel, new()
        {
            T panel = new T();
            panel.ShouldDraw = true;
            panel.Init();

            elements.Add(panel);
            return panel;
        }

        public class FontProperties
        {
            public float fontSize = 12;
            public StyleSimulations style = StyleSimulations.None;
            public string Name = "myfont";

            public FontProperties( string uniqueName )
            {
                Name = uniqueName;
            }
        }

        public static void CreateFont(string font, FontProperties properties)
        {
            string url = Environment.CurrentDirectory + "\\" + font;
            GlyphTypeface face = new GlyphTypeface(new Uri(url), properties.style);
            GlyphRunDrawing p = new GlyphRunDrawing();

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawGlyphRun(Brushes.Red, new GlyphRun());
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap(256, 256, 30, 30, new System.Windows.Media.PixelFormat());
            bitmap.Render(dv);
        }

        public static void Init()
        {
            Utilities.window.Mouse.ButtonDown += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonDown);
            Utilities.window.Mouse.Move += new EventHandler<OpenTK.Input.MouseMoveEventArgs>(Mouse_Move);
            Utilities.window.Mouse.ButtonUp += new EventHandler<OpenTK.Input.MouseButtonEventArgs>(Mouse_ButtonUp);
        }

        static void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            foreach (Panel p in elements)
            {
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
            foreach (Panel p in elements)
            {
                if (p.IsMouseOver())
                {
                    p.MouseDown(e);
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
