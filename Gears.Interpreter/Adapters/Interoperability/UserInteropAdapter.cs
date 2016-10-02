using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;

namespace Gears.Interpreter.Adapters.Interoperability
{
    static class UserInteropAdapter
    {
        /// <summary>
        /// Converts screen coordinates to zero-based coordinates of the pixel buffer.
        /// Input - point returned by ClientToScreen native method (this can be negative for multiple screen layout- Zero is on primary monitor regardless of the layout.)
        /// Output - point in a space where top-left is 0,0
        /// </summary>
        /// <param name="p"></param>
        public static void ScreenToGraphics(ref Point p)
        {
            p.X -= VirtualScreenLeft;
            p.Y -= VirtualScreenTop;
        }


        public static int VirtualScreenLeft
        {
            get { return UserBindings.GetSystemMetrics(76); }
        }

        public static int VirtualScreenTop
        {
            get { return UserBindings.GetSystemMetrics(77); }
        }


        public static int VirtualScreenWidth
        {
            get { return UserBindings.GetSystemMetrics(78); }
        }

        public static int VirtualScreenHeight
        {
            get { return UserBindings.GetSystemMetrics(79); }
        }


        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            var oldPosition = Cursor.Position;
            UserBindings.ClientToScreen(wndHandle, ref clientPoint);
            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            var inputMouseDown = new UserBindings.INPUT {Type = 0};
            inputMouseDown.Data.Mouse.Flags = 0x0002;

            var inputMouseUp = new UserBindings.INPUT {Type = 0};
            inputMouseUp.Data.Mouse.Flags = 0x0004;

            var inputs = new [] { inputMouseDown, inputMouseUp };
            UserBindings.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(UserBindings.INPUT)));

            Cursor.Position = oldPosition;
        }

        public static void SendText(IntPtr wndHandle,string s, Point clientPoint)
        {
            UserBindings.ClientToScreen(wndHandle, ref clientPoint);

            var inputs = new List<UserBindings.INPUT>();
            foreach (var c in s.ToCharArray())
            {
                inputs.AddRange(GetCharacterInputs(c));
            }
            UserBindings.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(UserBindings.INPUT)));
        }

        private static UserBindings.INPUT[] GetCharacterInputs(char character)
        {
            UInt16 scanCode = character;

            var down = new UserBindings.INPUT
            {
                Type = (UInt32)1,
                Data =
                {
                    Keyboard =
                        new UserBindings.KEYBDINPUT
                        {
                            KeyCode = 0,
                            Scan = scanCode,
                            Flags = (UInt32)0x0004,
                            Time = 0,
                            ExtraInfo = IntPtr.Zero
                        }
                }
            };

            var up = new UserBindings.INPUT
            {
                Type = (UInt32)1,
                Data =
                {
                    Keyboard =
                        new UserBindings.KEYBDINPUT
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