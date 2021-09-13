using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace motion_stone_mouse
{
    public class Input
    {
        [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
        static public extern uint SendInput(
           uint nInputs,
           INPUT[] pInputs,
           int cbSize);

        [DllImport("user32.dll", EntryPoint = "GetMessageExtraInfo", SetLastError = true)]
        static public extern IntPtr GetMessageExtraInfo();

        public enum InputType
        {
            INPUT_MOUSE = 0
        }

        [Flags()]
        public enum MOUSEEVENTF
        {
            MOVE = 0x0001,  // mouse move
            ABSOLUTE = 0x8000,  // absolute move
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int dwData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(4)]
            public MOUSEINPUT mi; 
        }
        public static void MouseEvent(int dwFlags, int dx, int dy, int dwData)
        {
            Input.INPUT input = new Input.INPUT();
            input.mi.dwFlags = dwFlags; 
            input.mi.dx = dx;
            input.mi.dy = dy;
            input.mi.dwData = dwData;
            Input.INPUT[] inputArr = { input };
            Input.SendInput(1, inputArr, Marshal.SizeOf(input));
        } 
    }
}