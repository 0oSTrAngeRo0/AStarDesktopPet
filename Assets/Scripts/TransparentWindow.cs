using System;
using UnityEngine;

namespace AStar.TransparentWindow
{
    public class TransparentWindow : MonoBehaviour
    {
        private void Awake()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            IntPtr hWnd = Native.GetActiveWindow();
            Native.MARGINS margins = new Native.MARGINS
            {
                cxLeftWidth = 0,
                cxRightWidth = 0,
                cyTopHeight = 0,
                cyBottomHeight = 0
            };
            Native.DwmExtendFrameIntoClientArea(hWnd, ref margins);
#endif
        }
    }
}