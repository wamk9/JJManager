using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JJManager.Class.App.Input.MacroKey.Mouse
{
    public class MouseController
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const int INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public void MoveMouseBy(int deltaX, int deltaY)
        {
            // Get current mouse position
            GetCursorPos(out POINT currentPos);

            // Calculate new position
            int newX = currentPos.X + deltaX;
            int newY = currentPos.Y + (deltaY * -1); // To up with positive values

            // Set mouse position to the new coordinates
            SetMousePosition(newX, newY);
        }

        private void SetMousePosition(int x, int y, int screenNumber = 0)
        {
            INPUT input = new INPUT();
            input.type = INPUT_MOUSE;
            input.mi.dx = x * (65536 / GetScreenWidth());
            input.mi.dy = y * (65536 / GetScreenHeight());
            input.mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;

            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public void ClickLeftButton()
        {
            ClickMouse(MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP);
        }

        public void ClickMiddleButton()
        {
            ClickMouse(MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP);
        }

        public void ClickRightButton()
        {
            ClickMouse(MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP);
        }

        private void ClickMouse(uint downFlag, uint upFlag)
        {
            INPUT[] inputs = new INPUT[2];

            inputs[0].type = INPUT_MOUSE;
            inputs[0].mi.dwFlags = downFlag;

            inputs[1].type = INPUT_MOUSE;
            inputs[1].mi.dwFlags = upFlag;

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private int GetScreenWidth()
        {
            return Screen.PrimaryScreen.Bounds.Width;
        }

        private int GetScreenHeight()
        {
            return Screen.PrimaryScreen.Bounds.Height;
        }
    }
}
