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

        public enum EGWL : int
        {
            GWL_EXSTYLE = -20
        }

        [Flags]
        public enum EWSEX : uint
        {
            WS_EX_LAYERED = 0x00080000,
            WS_EX_TRANSPARENT = 0x00000020
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("Dwmapi.dll")]
        public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margin);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    }
}