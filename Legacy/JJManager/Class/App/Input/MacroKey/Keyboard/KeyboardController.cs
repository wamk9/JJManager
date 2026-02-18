using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace JJManager.Class.App.Input.MacroKey.Keyboard
{
    public class KeyboardController
    {
        public enum VirtualKey
        {
            VK_LBUTTON = 0x01,
            VK_RBUTTON = 0x02,
            VK_CANCEL = 0x03,
            VK_MBUTTON = 0x04,
            VK_XBUTTON1 = 0x05,
            VK_XBUTTON2 = 0x06,
            // 0x07 is undefined
            VK_BACK = 0x08,
            VK_TAB = 0x09,
            // 0x0A - 0x0B are reserved
            VK_CLEAR = 0x0C,
            VK_RETURN = 0x0D,
            // 0x0E - 0x0F are undefined
            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_MENU = 0x12, // Alt
            VK_PAUSE = 0x13,
            VK_CAPITAL = 0x14,
            VK_KANA = 0x15,
            VK_HANGUL = 0x15,
            VK_IME_ON = 0x16,
            VK_JUNJA = 0x17,
            VK_FINAL = 0x18,
            VK_HANJA = 0x19,
            VK_KANJI = 0x19,
            VK_IME_OFF = 0x1A,
            VK_ESCAPE = 0x1B,
            VK_CONVERT = 0x1C,
            VK_NONCONVERT = 0x1D,
            VK_ACCEPT = 0x1E,
            VK_MODECHANGE = 0x1F,
            VK_SPACE = 0x20,
            VK_PRIOR = 0x21,
            VK_NEXT = 0x22,
            VK_END = 0x23,
            VK_HOME = 0x24,
            VK_LEFT = 0x25,
            VK_UP = 0x26,
            VK_RIGHT = 0x27,
            VK_DOWN = 0x28,
            VK_SELECT = 0x29,
            VK_PRINT = 0x2A,
            VK_EXECUTE = 0x2B,
            VK_SNAPSHOT = 0x2C, // Print Screen
            VK_INSERT = 0x2D,
            VK_DELETE = 0x2E,
            VK_HELP = 0x2F,
            VK_0 = 0x30,
            VK_1 = 0x31,
            VK_2 = 0x32,
            VK_3 = 0x33,
            VK_4 = 0x34,
            VK_5 = 0x35,
            VK_6 = 0x36,
            VK_7 = 0x37,
            VK_8 = 0x38,
            VK_9 = 0x39,
            // 0x3A - 0x40 are undefined
            VK_A = 0x41,
            VK_B = 0x42,
            VK_C = 0x43,
            VK_D = 0x44,
            VK_E = 0x45,
            VK_F = 0x46,
            VK_G = 0x47,
            VK_H = 0x48,
            VK_I = 0x49,
            VK_J = 0x4A,
            VK_K = 0x4B,
            VK_L = 0x4C,
            VK_M = 0x4D,
            VK_N = 0x4E,
            VK_O = 0x4F,
            VK_P = 0x50,
            VK_Q = 0x51,
            VK_R = 0x52,
            VK_S = 0x53,
            VK_T = 0x54,
            VK_U = 0x55,
            VK_V = 0x56,
            VK_W = 0x57,
            VK_X = 0x58,
            VK_Y = 0x59,
            VK_Z = 0x5A,
            VK_LWIN = 0x5B,
            VK_RWIN = 0x5C,
            VK_APPS = 0x5D,
            // 0x5E is reserved
            VK_SLEEP = 0x5F,
            VK_NUMPAD0 = 0x60,
            VK_NUMPAD1 = 0x61,
            VK_NUMPAD2 = 0x62,
            VK_NUMPAD3 = 0x63,
            VK_NUMPAD4 = 0x64,
            VK_NUMPAD5 = 0x65,
            VK_NUMPAD6 = 0x66,
            VK_NUMPAD7 = 0x67,
            VK_NUMPAD8 = 0x68,
            VK_NUMPAD9 = 0x69,
            VK_MULTIPLY = 0x6A,
            VK_ADD = 0x6B,
            VK_SEPARATOR = 0x6C,
            VK_SUBTRACT = 0x6D,
            VK_DECIMAL = 0x6E,
            VK_DIVIDE = 0x6F,
            VK_F1 = 0x70,
            VK_F2 = 0x71,
            VK_F3 = 0x72,
            VK_F4 = 0x73,
            VK_F5 = 0x74,
            VK_F6 = 0x75,
            VK_F7 = 0x76,
            VK_F8 = 0x77,
            VK_F9 = 0x78,
            VK_F10 = 0x79,
            VK_F11 = 0x7A,
            VK_F12 = 0x7B,
            VK_F13 = 0x7C,
            VK_F14 = 0x7D,
            VK_F15 = 0x7E,
            VK_F16 = 0x7F,
            VK_F17 = 0x80,
            VK_F18 = 0x81,
            VK_F19 = 0x82,
            VK_F20 = 0x83,
            VK_F21 = 0x84,
            VK_F22 = 0x85,
            VK_F23 = 0x86,
            VK_F24 = 0x87,
            // 0x88 - 0x8F are unassigned
            VK_NUMLOCK = 0x90,
            VK_SCROLL = 0x91,
            // 0x92 - 0x96 are OEM specific
            // 0x97 - 0x9F are unassigned
            VK_LSHIFT = 0xA0,
            VK_RSHIFT = 0xA1,
            VK_LCONTROL = 0xA2,
            VK_RCONTROL = 0xA3,
            VK_LMENU = 0xA4,
            VK_RMENU = 0xA5,
            VK_BROWSER_BACK = 0xA6,
            VK_BROWSER_FORWARD = 0xA7,
            VK_BROWSER_REFRESH = 0xA8,
            VK_BROWSER_STOP = 0xA9,
            VK_BROWSER_SEARCH = 0xAA,
            VK_BROWSER_FAVORITES = 0xAB,
            VK_BROWSER_HOME = 0xAC,
            VK_VOLUME_MUTE = 0xAD,
            VK_VOLUME_DOWN = 0xAE,
            VK_VOLUME_UP = 0xAF,
            VK_MEDIA_NEXT_TRACK = 0xB0,
            VK_MEDIA_PREV_TRACK = 0xB1,
            VK_MEDIA_STOP = 0xB2,
            VK_MEDIA_PLAY_PAUSE = 0xB3,
            VK_LAUNCH_MAIL = 0xB4,
            VK_LAUNCH_MEDIA_SELECT = 0xB5,
            VK_LAUNCH_APP1 = 0xB6,
            VK_LAUNCH_APP2 = 0xB7,
            // 0xB8 - 0xB9 are reserved
            VK_OEM_1 = 0xBA, // ';:' for US
            VK_OEM_PLUS = 0xBB, // '+' any country
            VK_OEM_COMMA = 0xBC, // ',' any country
            VK_OEM_MINUS = 0xBD, // '-' any country
            VK_OEM_PERIOD = 0xBE, // '.' any country
            VK_OEM_2 = 0xBF, // '/?' for US
            VK_OEM_3 = 0xC0, // '`~' for US
                             // 0xC1 - 0xD7 are reserved
                             // 0xD8 - 0xDA are unassigned
            VK_OEM_4 = 0xDB, // '[{' for US
            VK_OEM_5 = 0xDC, // '\|' for US
            VK_OEM_6 = 0xDD, // ']}' for US
            VK_OEM_7 = 0xDE, // ''"' for US
            VK_OEM_8 = 0xDF,
            // 0xE0 is reserved
            // 0xE1 is OEM specific
            VK_OEM_102 = 0xE2, // '<>' or '\|' on RT 102-key kbd.
                               // 0xE3 - 0xE4 are OEM specific
            VK_PROCESSKEY = 0xE5,
            // 0xE6 is OEM specific
            VK_PACKET = 0xE7,
            // 0xE8 is unassigned
            // 0xE9 - 0xF5 are OEM specific
            VK_ATTN = 0xF6,
            VK_CRSEL = 0xF7,
            VK_EXSEL = 0xF8,
            VK_EREOF = 0xF9,
            VK_PLAY = 0xFA,
            VK_ZOOM = 0xFB,
            VK_NONAME = 0xFC,
            VK_PA1 = 0xFD,
            VK_OEM_CLEAR = 0xFE,
        }


        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern int GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern int ToAscii(uint uVirtKey, uint uScanCode, byte[] lpKeyState, [Out] char[] lpChar, uint uFlags);

        private const uint MAPVK_VK_TO_VSC = 0x00;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
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

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_SCANCODE = 0x0008;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public void PressKey(ushort keyCode, bool hold = false)
        {
            INPUT downInput = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = keyCode,
                        dwFlags = KEYEVENTF_KEYDOWN | KEYEVENTF_SCANCODE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new INPUT[] { downInput }, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(50);

            if (!hold)
            {
                INPUT upInput = downInput;
                upInput.u.ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_SCANCODE;
                SendInput(1, new INPUT[] { upInput }, Marshal.SizeOf(typeof(INPUT)));
            }
        }
        
        public static ushort ConvertToScanMode(Keys key)
        {
            return (ushort) MapVirtualKey((ushort)key, MAPVK_VK_TO_VSC);
        }

        public void ReleaseKey(ushort keyCode)
        {
            INPUT releaseKey = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = keyCode,
                        dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_SCANCODE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new INPUT[] { releaseKey }, Marshal.SizeOf(typeof(INPUT)));
        }

        public void WriteText(string text)
        {
            foreach (char c in text)
            {
                SendUnicodeCharacter(c);
            }
        }

        private void SendUnicodeCharacter(char character)
        {
            INPUT[] inputs = new INPUT[2];

            // Send Unicode character
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wScan = (ushort)character;
            inputs[0].u.ki.dwFlags = KEYEVENTF_UNICODE;

            // Release key (optional, depending on application requirements)
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].u.ki.wScan = (ushort)character;
            inputs[1].u.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
        public static string GetFriendlyKeyNameFromScanCode(ushort scanCode)
        {
            // Convert scan code to virtual key code
            ushort vkCode = (ushort)MapVirtualKey(scanCode, 1); // MAPVK_VSC_TO_VK is 1

            // Use the method we created earlier to get the friendly name from the virtual key code
            return GetFriendlyKeyName(vkCode);
        }
        public static string GetFriendlyKeyName(ushort vk)
        {
            VirtualKey key = (VirtualKey)vk;

            switch (key)
            {
                case VirtualKey.VK_CONTROL:
                case VirtualKey.VK_LCONTROL:
                case VirtualKey.VK_RCONTROL:
                    return "Ctrl";
                case VirtualKey.VK_SHIFT:
                case VirtualKey.VK_LSHIFT:
                case VirtualKey.VK_RSHIFT:
                    return "Shift";
                case VirtualKey.VK_MENU:
                case VirtualKey.VK_LMENU:
                case VirtualKey.VK_RMENU:
                    return "Alt";
                case VirtualKey.VK_LWIN:
                case VirtualKey.VK_RWIN:
                    return "Win";
                case VirtualKey.VK_RETURN:
                    return "Enter";
                case VirtualKey.VK_SPACE:
                    return "Space";
                case VirtualKey.VK_BACK:
                    return "Backspace";
                case VirtualKey.VK_TAB:
                    return "Tab";
                case VirtualKey.VK_ESCAPE:
                    return "Esc";
                case VirtualKey.VK_CAPITAL:
                    return "Caps Lock";
                case VirtualKey.VK_DELETE:
                    return "Delete";
                case VirtualKey.VK_INSERT:
                    return "Insert";
                case VirtualKey.VK_HOME:
                    return "Home";
                case VirtualKey.VK_END:
                    return "End";
                case VirtualKey.VK_PRIOR:
                    return "Page Up";
                case VirtualKey.VK_NEXT:
                    return "Page Down";
                case VirtualKey.VK_LEFT:
                    return "Left Arrow";
                case VirtualKey.VK_RIGHT:
                    return "Right Arrow";
                case VirtualKey.VK_UP:
                    return "Up Arrow";
                case VirtualKey.VK_DOWN:
                    return "Down Arrow";
                case VirtualKey.VK_0:
                case VirtualKey.VK_1:
                case VirtualKey.VK_2:
                case VirtualKey.VK_3:
                case VirtualKey.VK_4:
                case VirtualKey.VK_5:
                case VirtualKey.VK_6:
                case VirtualKey.VK_7:
                case VirtualKey.VK_8:
                case VirtualKey.VK_9:
                    return ((int)vk - 0x30).ToString(); // Numbers 0-9
                case VirtualKey.VK_F1:
                case VirtualKey.VK_F2:
                case VirtualKey.VK_F3:
                case VirtualKey.VK_F4:
                case VirtualKey.VK_F5:
                case VirtualKey.VK_F6:
                case VirtualKey.VK_F7:
                case VirtualKey.VK_F8:
                case VirtualKey.VK_F9:
                case VirtualKey.VK_F10:
                case VirtualKey.VK_F11:
                case VirtualKey.VK_F12:
                    return "F" + ((int)vk - (int)VirtualKey.VK_F1 + 1).ToString(); // Function keys F1-F12
                default:
                    return key.ToString().Replace("VK_", "");
            }
        }
    }
}

