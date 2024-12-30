using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Win32Api;

namespace Assets.Scripts
{
    public static class NotifyIconManager
    {
        private static ConcurrentDictionary<IntPtr, NotifyIconItem> m_NofifyIcons = new();

        public static bool TryCreate(NotifyIconItem.CreateInfo ci)
        {
            if (m_NofifyIcons.ContainsKey(ci.HWnd))
                return false;
            var item = new NotifyIconItem(ci, WndProc);
            return m_NofifyIcons.TryAdd(ci.HWnd, item);
        }

        public static void Destroy(IntPtr hWnd)
        {
            if (!m_NofifyIcons.TryRemove(hWnd, out var item)) return;
            item.Dispose();
        }

        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (!m_NofifyIcons.TryGetValue(hWnd, out var item)) return IntPtr.Zero;
            return item.WndProc(msg, wParam, lParam);
        }
    }

    public class NotifyIconItem
    {
        public struct CreateInfo
        {
            public class ContextMenuItem
            {
                public string Text { get; set; }
                public Action Callback { get; set; }
            }

            public IntPtr HWnd;
            public ContextMenuItem[] Items;
            public string AppPath;
            public string Tip;
            public string Title;
            public string BoxText;
        }

        private IntPtr m_HWnd;
        private Shell_NotifyIconEx m_NotifyIcon;
        private CreateInfo m_CreateInfo;

        public NotifyIconItem(CreateInfo ci, WndProcDelegate callback)
        {
            m_CreateInfo = ci;
            m_HWnd = ci.HWnd;
            InitWndProc(callback);
            CreateNotifyIcon();
        }

        public void Dispose()
        {
            TermWndProc();
        }

        // 创建任务栏窗体
        private void CreateNotifyIcon()
        {
            if (m_HWnd == IntPtr.Zero)
                return;
            DirectoryInfo assetData = new DirectoryInfo(Application.dataPath);
            if (assetData.Parent == null)
                return;
            StringBuilder exeFileSb = new StringBuilder(m_CreateInfo.AppPath);
            IntPtr iconPtr = Shell_NotifyIconEx.ExtractAssociatedIcon(m_HWnd, exeFileSb, out ushort uIcon);
            m_NotifyIcon = new Shell_NotifyIconEx(m_HWnd);
            int state = m_NotifyIcon.AddNotifyBox(iconPtr, m_CreateInfo.Tip, m_CreateInfo.Title, m_CreateInfo.BoxText);
            // if (state <= 0)
            // {
            //     Debug.Log("创建任务栏图标失败");
            // }
            // else
            // {
            //     Debug.Log("创建任务栏图标成功");
            // }
        }

        #region 监听窗体事件

        private HandleRef m_HMainWindow;
        private static IntPtr m_OldWndProcPtr;
        private IntPtr m_NewWndProcPtr;

        private void InitWndProc(WndProcDelegate callback)
        {
            m_HMainWindow = new HandleRef(null, m_HWnd);
            m_NewWndProcPtr = Marshal.GetFunctionPointerForDelegate(callback);
            m_OldWndProcPtr = WinUser32.SetWindowLongPtr(m_HMainWindow, WinUser32.GWLP_WNDPROC, m_NewWndProcPtr);
        }

        private void TermWndProc()
        {
            WinUser32.SetWindowLongPtr(m_HMainWindow, WinUser32.GWLP_WNDPROC, m_OldWndProcPtr);
            m_HMainWindow = new HandleRef(null, IntPtr.Zero);
            m_OldWndProcPtr = IntPtr.Zero;
            m_NewWndProcPtr = IntPtr.Zero;
        }

        // [MonoPInvokeCallback]
        public IntPtr WndProc(uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WinUser32.WM_SYSCOMMAND)
            {
                // 屏蔽窗口顶部关闭最小化事件
                switch ((int)wParam)
                {
                    case WinUser32.SC_CLOSE:
                        // 关闭
                        WinUser32.ShowWindowAsync(m_HWnd, WinUser32.SW_HIDE);
                        return IntPtr.Zero;
                    case WinUser32.SC_MAXIMIZE:
                        // 最大化
                        break;
                    case WinUser32.SC_MINIMIZE:
                        // 最小化
                        WinUser32.ShowWindowAsync(m_HWnd, WinUser32.SW_HIDE);
                        return IntPtr.Zero;
                }
            }
            else if (msg == Shell_NotifyIconEx.WM_NOTIFY_TRAY)
            {
                // 任务栏菜单图标事件
                if ((int)wParam == Shell_NotifyIconEx.uID)
                {
                    switch ((int)lParam)
                    {
                        case WinUser32.WM_LBUTTONDOWN:
                            // 左键点击
                            break;
                        case WinUser32.WM_RBUTTONDOWN:
                            // 右键点击
                            CreateNotifyIconMenu();
                            break;
                        case WinUser32.WM_MBUTTONDOWN:
                            // 中键点击
                            break;
                        case WinUser32.WM_LBUTTONDBLCLK:
                            // 左键双击
                            break;
                        case WinUser32.WM_RBUTTONDBLCLK:
                            // 右键双击
                            break;
                    }
                }
            }
            else if (msg == WinUser32.WM_COMMAND) // 任务栏菜单点击事件
            {
                // Debug.LogError("An item is clicked");
                // Debug.LogError($"{wParam}, {(int)wParam} is called");
                if (m_CreateInfo.Items.Length > (int)wParam)
                {
                    m_CreateInfo.Items[(int)wParam].Callback?.Invoke();
                }
            }
            else if (msg == WinUser32.WM_LBUTTONDOWN)
            {
                // 鼠标点击事件
                //var x = LOWORD(lParam);
                //var y = HIWORD(lParam);
            }

            //Debug.Log("WndProc msg:0x" + msg.ToString("x4") + " wParam:0x" + wParam.ToString("x4") + " lParam:0x" + lParam.ToString("x4"));
            return WinUser32.CallWindowProc(m_OldWndProcPtr, m_HWnd, msg, wParam, lParam);
        }

        // 创建任务栏菜单
        private void CreateNotifyIconMenu()
        {
            // 获取屏幕数量及宽高
            //int monitorCnt = WinUser32.GetSystemMetrics(WinUser32.SystemMetric.SM_CMONITORS);
            //var width = WinUser32.GetSystemMetrics(WinUser32.SystemMetric.SM_CXSCREEN);
            //var height = WinUser32.GetSystemMetrics(WinUser32.SystemMetric.SM_CYSCREEN);
            WinUser32.GetCursorPos(out var cursorPoint);
            IntPtr menuPtr = WinUser32.CreatePopupMenu();
            for (uint i = 0; i < m_CreateInfo.Items.Length; i++)
            {
                var item = m_CreateInfo.Items[i];
                WinUser32.AppendMenu(menuPtr, WinUser32.MenuFlags.MF_STRING, i, item.Text);
            }

            // 注意:调用SetForegroundWindow是为了鼠标点击别处时隐藏弹出的菜单，不能省略
            // https://stackoverflow.com/questions/4145561/system-tray-context-menu-doesnt-disappear
            WinUser32.SetForegroundWindow(m_HWnd);
            // 菜单点击时会发送WinUser32.WM_COMMAND消息,wParam为菜单的ID值
            WinUser32.TrackPopupMenuEx(
                menuPtr,
                2,
                cursorPoint.X,
                cursorPoint.Y,
                m_HWnd,
                IntPtr.Zero
            );
            WinUser32.DestroyMenu(menuPtr);
        }

        #endregion
    }
}