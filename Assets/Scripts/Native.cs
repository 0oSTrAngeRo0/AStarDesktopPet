using System;
using System.Runtime.InteropServices;

namespace AStar.TransparentWindow
{
    public static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }
        
        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("Dwmapi.dll")]
        public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margin);
    }
}