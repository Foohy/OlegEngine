using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            
        }

        public static void Draw()
        {
            GL.Disable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Always);

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
        }
    }
}
