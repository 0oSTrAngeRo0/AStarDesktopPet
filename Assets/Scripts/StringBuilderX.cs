using System;
using System.Text;
using TMPro;

namespace Game
{
    public struct StringBuilderTMPSetter : IDisposable
    {
        public readonly StringBuilder Builder;
        private readonly TMP_Text m_Ui;

        public StringBuilderTMPSetter(TMP_Text ui, StringBuilder builder)
        {
            Builder = builder;
            m_Ui = ui;
        }

        public StringBuilderTMPSetter(TMP_Text ui) : this(ui, StringBuilderX.MainThreadStringBuilder) { }


        public void Dispose()
        {
            m_Ui.SetText(Builder);
            Builder.Clear();
        }
    }

    public static class StringBuilderX
    {
        public static readonly StringBuilder MainThreadStringBuilder = new StringBuilder();
    }
}