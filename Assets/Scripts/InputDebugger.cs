using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Game
{
    public class InputDebugger : MonoBehaviour
    {
        private Dictionary<InputDevice, StringBuilder> m_DeviceLogs;
        private StringBuilder m_GeneralLog;
        [SerializeField] private TMP_Text m_Text;

        private void OnEnable()
        {
            m_DeviceLogs = new Dictionary<InputDevice, StringBuilder>();
            UpdateGeneralLog();
            InputSystem.runInBackground = true;
            InputSystem.onDeviceChange += (device, change) =>
            {
                UpdateGeneralLog();
                if (change == InputDeviceChange.Removed)
                    m_DeviceLogs.Remove(device);
                else if (change == InputDeviceChange.Added)
                    m_DeviceLogs.Add(device, new StringBuilder());
            };
        }

        private void Update()
        {
            foreach (var pair in m_DeviceLogs) { pair.Value.Clear(); }
            foreach (var device in InputSystem.devices)
            {
                StringBuilder builder = GetStringBuilder(device);
                foreach (var control in device.allControls)
                {
                    Type type = control.GetType();
                    if (control.noisy || control.synthetic) continue;
                    if (type != typeof(ButtonControl) && type != typeof(KeyControl)) continue;
                    if (!control.IsPressed()) continue;
                    builder.Append(control.name);
                    builder.Append(" + ");
                }

                if (builder.Length >= 3) { builder.Remove(builder.Length - 3, 3); }
            }

            using (var s = new StringBuilderTMPSetter(m_Text))
            {
                s.Builder.Append(m_GeneralLog);
                s.Builder.AppendLine();
                foreach (var pair in m_DeviceLogs)
                {
                    s.Builder.Append(pair.Key.displayName);
                    s.Builder.Append(": ");
                    s.Builder.Append(pair.Value);
                    s.Builder.AppendLine();
                }
            }
        }

        private StringBuilder GetStringBuilder(InputDevice device)
        {
            if (m_DeviceLogs.TryGetValue(device, out var builder))
                return builder;
            builder = new StringBuilder();
            m_DeviceLogs.TryAdd(device, builder);
            return builder;
        }

        private void UpdateGeneralLog()
        {
            if (m_GeneralLog == null) m_GeneralLog = new StringBuilder();
            m_GeneralLog.Clear();
            foreach (InputDevice device in InputSystem.devices)
            {
                m_GeneralLog.Append(device.displayName);
                m_GeneralLog.Append(": ");
                m_GeneralLog.Append(device.canRunInBackground);
                m_GeneralLog.AppendLine();
            }
        }
    }
}