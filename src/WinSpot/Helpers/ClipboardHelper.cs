using System.Windows;

namespace WinSpot.Helpers;

public static class ClipboardHelper
{
    public static void SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        Clipboard.SetText(text);
    }
}
