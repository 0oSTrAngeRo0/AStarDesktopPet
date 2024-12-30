using System.Collections;
using System.IO;
using Assets.Scripts;
using SatorImaging.AppWindowUtility;
using UnityEngine;

namespace Game
{
    public class WindowSettings : MonoBehaviour
    {
        private void Start()
        {
            SetWindowFrameVisible(false);
            CreateNotifyIcon();
            SetTargetFps(30);
        }

        private void SetTargetFps(uint fps)
        {
            IEnumerator SetFps()
            {
                yield return null;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = (int)fps;
            }

            StartCoroutine(SetFps());
        }

        private void OnDestroy()
        {
            NotifyIconManager.Destroy(WinApi.GetUnityWindowHandle());
            Debug.Log("Try Destroy NotifyIcon");
        }

        private void SetWindowFrameVisible(bool visible)
        {
            AppWindowUtility.Transparent = !visible;
            AppWindowUtility.AlwaysOnTop = !visible;
            AppWindowUtility.ClickThrough = !visible;
        }

        private void CreateNotifyIcon()
        {
            bool success = NotifyIconManager.TryCreate(new NotifyIconItem.CreateInfo
            {
                HWnd = WinApi.GetUnityWindowHandle(),
                Items = new NotifyIconItem.CreateInfo.ContextMenuItem[]
                {
                    new NotifyIconItem.CreateInfo.ContextMenuItem()
                    {
                        Text = "Show Frame",
                        Callback = () => SetWindowFrameVisible(true)
                    },
                    new NotifyIconItem.CreateInfo.ContextMenuItem()
                    {
                        Text = "Hide Frame",
                        Callback = () => SetWindowFrameVisible(false)
                    },
                    new NotifyIconItem.CreateInfo.ContextMenuItem
                    {
                        Text = "Exit",
                        Callback = () =>
                        {
                            Debug.LogError("Start Quitting...");
                            Application.Quit();
                        }
                    }
                },
                AppPath = Path.GetFullPath($"./{Application.productName}.exe"),
                Tip = $"{Application.productName} Tip",
                Title = $"{Application.productName} Title",
                BoxText = $"{Application.productName} Box Text",
            });
        }
    }
}