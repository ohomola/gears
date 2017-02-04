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

        /// <summary>
        /// Converts screen coordinates to zero-based coordinates of the pixel buffer.
        /// Input - point returned by ClientToScreen native method (this can be negative for multiple screen layout- Zero is on primary monitor regardless of the layout.)
        /// Output - point in a space where top-left is 0,0
        /// </summary>
        /// <param name="p"></param>
        public static void GraphicsToScreen(ref Point p)
        {
            p.X += VirtualScreenLeft;
            p.Y += VirtualScreenTop;
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

        public static void PressWithControl(IntPtr wndHandle, ushort c)
        {
            var inputs = new List<UserBindings.INPUT>();

            inputs.Add(CreateInput(KeyPressInputType.Down, keyCode: 0x11));
            inputs.Add(CreateInput(KeyPressInputType.Down, keyCode: c));
            inputs.Add(CreateInput(KeyPressInputType.Up, keyCode: c));
            inputs.Add(CreateInput(KeyPressInputType.Up, keyCode: 0x11));
            UserBindings.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(UserBindings.INPUT)));
        }

        private static UserBindings.INPUT[] GetCharacterInputs(char character)
        {
            var down = CreateInput(KeyPressInputType.Down, scanCode:character);
            var up = CreateInput(KeyPressInputType.Up, scanCode: character);

            if ((character & 0xFF00) == 0xE000)
            {
                down.Data.Keyboard.Flags |= 0x0001;
                up.Data.Keyboard.Flags |= 0x0001;
            }

            return new [] {down, up};
        }

        private static UserBindings.INPUT CreateInput(KeyPressInputType upOrDown, ushort scanCode=0, ushort keyCode=0)
        {
            uint flags = 0x0000; // Flag input as 'sending basic key'
            if (keyCode == 0x11)//Control
            {
                flags = 0x0001; // Flag input as 'sending extended key'
            }
            if (keyCode == 0 && scanCode != 0)
            {
                flags = 0x0004; // Flag input as 'sending Unicode character'
            }

            if (upOrDown == KeyPressInputType.Up)
            {
                flags = flags | 0x0002; // add 'sending UP' flag
            }
            return new UserBindings.INPUT
            {
                Type = 1,
                Data =
                {
                    Keyboard =
                        new UserBindings.KEYBDINPUT
                        {
                            KeyCode = keyCode,
                            Scan = scanCode,
                            Flags =flags,
                            Time = 0,
                            ExtraInfo = IntPtr.Zero
                        }
                }
            };
        }

        public enum KeyPressInputType
        {
            Up,
            Down
        }
    }
}