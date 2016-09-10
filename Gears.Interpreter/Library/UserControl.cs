using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gears.Interpreter.Library
{
    public class UserControl
    {
        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
            int cbSize);

#pragma warning disable 649
        internal struct INPUT
        {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)] public MOUSEINPUT Mouse;
            [FieldOffset(0)]public KEYBDINPUT Keyboard;
        }
    
        internal struct KEYBDINPUT
        {
            public UInt16 KeyCode;
            public UInt16 Scan;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

        internal struct MOUSEINPUT
        {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }
#pragma warning restore 649
        
        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            var oldPosition = Cursor.Position;
            ClientToScreen(wndHandle, ref clientPoint);
            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            var inputMouseDown = new INPUT {Type = 0};
            inputMouseDown.Data.Mouse.Flags = 0x0002;

            var inputMouseUp = new INPUT {Type = 0};
            inputMouseUp.Data.Mouse.Flags = 0x0004;

            var inputs = new [] { inputMouseDown, inputMouseUp };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            Cursor.Position = oldPosition;
        }

        public static void SendText(IntPtr wndHandle,string s, Point clientPoint)
        {
            ClientToScreen(wndHandle, ref clientPoint);

            var inputs = new List<INPUT>();
            foreach (var c in s.ToCharArray())
            {
                inputs.AddRange(GetCharacterInputs(c));
            }
            SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(INPUT)));
        }



        private static INPUT[] GetCharacterInputs(char character)
        {
            UInt16 scanCode = character;

            var down = new INPUT
            {
                Type = (UInt32)1,
                Data =
                {
                    Keyboard =
                        new KEYBDINPUT
                        {
                            KeyCode = 0,
                            Scan = scanCode,
                            Flags = (UInt32)0x0004,
                            Time = 0,
                            ExtraInfo = IntPtr.Zero
                        }
                }
            };

            var up = new INPUT
            {
                Type = (UInt32)1,
                Data =
                {
                    Keyboard =
                        new KEYBDINPUT
                        {
                            KeyCode = 0,
                            Scan = scanCode,
                            Flags =(UInt32)(0x0002 | 0x0004),
                            Time = 0,
                            ExtraInfo = IntPtr.Zero
                        }
                }
            };
       
            if ((scanCode & 0xFF00) == 0xE000)
            {
                down.Data.Keyboard.Flags |= (UInt32)0x0001;
                up.Data.Keyboard.Flags |= (UInt32)0x0001;
            }

            return new [] {down, up};
        }
    }
}