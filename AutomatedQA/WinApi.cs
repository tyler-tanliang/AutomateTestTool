using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;

namespace AutomatedQA
{
    public class WinApi
    {
        #region ScreenShot
        
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt
        (
            IntPtr hdcDest,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            Int32 dwRo
        );

        [DllImport("user32.dll")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll")]
        public extern static IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public extern static int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        public static Bitmap GetWindowPic(IntPtr hwnd)
        {
            if (!hwnd.Equals(IntPtr.Zero))
            {
                SetFocus(hwnd);
                Thread.Sleep(500);
                Rectangle rect;
                GetWindowRect(hwnd, out rect);
                int width = rect.Width - rect.X;
                int height = rect.Height - rect.Y;
                Bitmap pic = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(pic);
                IntPtr hdc1 = GetDC(hwnd);
                IntPtr hdc2 = g.GetHdc();
                BitBlt(hdc2, 0, 0, width, height, hdc1, 0, 0, 13369376);
                g.ReleaseHdc(hdc2);
                return pic;
            }
            return null;
        }

        public static Bitmap[] GetWindowPic(IntPtr[] hwnds)
        {
            Bitmap[] bitmaps = new Bitmap[hwnds.Length];
            for (int i = 0; i < hwnds.Length; i++)
                bitmaps[i] = GetWindowPic(hwnds[i]);
            return bitmaps;
        }

        public static Bitmap GetWindowPic(int hwndId)
        {
            IntPtr hwnd = new IntPtr(hwndId);
            return GetWindowPic(hwnd);
        }

        #endregion ScreenShot

        #region MountEvents

        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        
        [DllImport("user32.dll")]
        extern static bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        extern static void mouse_event(int mouseEventFlag, int incrementX, int incrementY, uint data, UIntPtr extraInfo);

        public static void ClickLeftMouse(AutomationElement element)
        {
            Rect rect = element.Current.BoundingRectangle;
            int IncrementX = (int)(rect.Left + rect.Width / 2);
            int IncrementY = (int)(rect.Top + rect.Height / 2);

            //Make the cursor position to the element.
            SetCursorPos(IncrementX, IncrementY);

            //Make the left mouse down and up.
            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
        }

        public static void ClickRightMouse(AutomationElement element)
        {
            Rect rect = element.Current.BoundingRectangle;
            int incrementX = (int)(rect.Left + rect.Width / 2);
            int incrementY = (int)(rect.Top + rect.Height / 2);

            //Make the cursor position to the element.
            SetCursorPos(incrementX, incrementY);
            //Make the right mouse down and up.
            mouse_event(MOUSEEVENTF_RIGHTDOWN, incrementX, incrementY, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_RIGHTUP, incrementX, incrementY, 0, UIntPtr.Zero);
        }

        #endregion MountEvents

        #region Windows

        private const int SW_SHOW = 5;
        [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void SetFocus(IntPtr handle)
        {
            ShowWindow(handle, SW_SHOW);
            SetForegroundWindow(handle);
        }

        #endregion Windows
    }
}
