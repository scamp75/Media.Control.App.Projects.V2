using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace Mpv.Player.App
{
    public class CustomHwndHost : HwndHost
    {
        private nint hwndHost;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            hwndHost = CreateWindowEx(
                0,
                "STATIC",
                "",
                WS_CHILD | WS_VISIBLE,
                0,
                0,
                (int)ActualWidth,
                (int)ActualHeight,
                hwndParent.Handle,
                nint.Zero,
                nint.Zero,
                nint.Zero
            );

            return new HandleRef(this, hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint CreateWindowEx(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            nint hWndParent,
            nint hMenu,
            nint hInstance,
            nint lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(nint hWnd);

        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;

    }

}



