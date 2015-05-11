using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KinectUsabilityTest
{
    public enum InputFlags
    {
        INPUT_MOUSE = 0,
        INPUT_KEYBOARD = 1,
        INPUT_HARDWARE = 2
    }

    public enum MouseFlags
    {
        MOUSEEVENTF_MOVE = 0x0001,
        MOUSEEVENTF_ABSOLUTE = 0x8000,
        MOUSEEVENTF_LEFTDOWN = 0x0002,
        MOUSEEVENTF_LEFTUP = 0x0004,
        MOUSEEVENTF_WHEEL = 0x0800,
        MOUSEEVENTF_MIDDLEDOWN = 0x0020,
        MOUSEEVENTF_MIDDLEUP = 0x0040
    }

    class MouseInput
    {

        struct INPUT
        {
            public InputFlags type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public MouseFlags dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public uint SendMouseInput(int deltaX, int deltaY, int data, MouseFlags flags)
        {
            INPUT[] inputs = new INPUT[]
            {
                new INPUT
                {
                    type = InputFlags.INPUT_MOUSE,
                    u = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            dwFlags = flags,
                            dx = (int)deltaX,
                            dy = (int)deltaY,
                            mouseData = data
                        }
                    }
                }
            };

            return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
