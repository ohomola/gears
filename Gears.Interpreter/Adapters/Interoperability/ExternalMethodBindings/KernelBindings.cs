﻿using System;
using System.Runtime.InteropServices;

namespace Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings
{
    class KernelBindings
    {
        public const int ConsoleBufferSize = 10000;
        public const int STD_OUTPUT_HANDLE = -11;
        public const int MY_CODE_PAGE = 437;

        [DllImport("kernel32.dll",
             EntryPoint = "GetStdHandle",
             SetLastError = true,
             CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll",
             EntryPoint = "AllocConsole",
             SetLastError = true,
             CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();
    }
}
