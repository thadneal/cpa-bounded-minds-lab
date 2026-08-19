using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Cpa.BoundedMindsLab.Desktop.Services;

public static partial class WindowsDarkMode
{
    private const int DwmwaUseImmersiveDarkMode = 20;

    public static void Apply(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var enabled = 1;
        _ = DwmSetWindowAttribute(
            handle,
            DwmwaUseImmersiveDarkMode,
            ref enabled,
            Marshal.SizeOf<int>());
    }

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int attributeValue,
        int attributeSize);
}
