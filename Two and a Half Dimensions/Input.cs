using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;

namespace Two_and_a_Half_Dimensions
{
    class Input
    {
        public static bool LockMouse { get; set; }
        public static int deltaX { get; set; }
        public static int deltaY { get; set; }
        public static int deltaZ { get; set; }
        private static MouseState current, previous;

        /// <summary>
        /// Update input, including getting mouse deltas/etc.
        /// </summary>
        public static void Think(Program window, FrameEventArgs e)
        {
            current = Mouse.GetState();

            window.CursorVisible = !LockMouse;

            if (current != previous && window.Focused)
            {
                // Mouse state has changed
                deltaX = current.X - previous.X;
                deltaY = current.Y - previous.Y;
                deltaZ = current.Wheel - previous.Wheel;

                if (LockMouse)
                {
                    Mouse.SetPosition(window.X + window.Width / 2, window.Y + window.Height / 2);
                }
            }
            else
            {
                deltaX = 0;
                deltaY = 0;
                deltaZ = 0;
            }
            previous = current;
        }
    }
}
