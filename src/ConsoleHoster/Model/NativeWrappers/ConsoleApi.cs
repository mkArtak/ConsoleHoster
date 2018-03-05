//-----------------------------------------------------------------------
// <copyright file="ConsoleApi.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleHoster.Model.NativeWrappers
{
    public class ConsoleApi
    {
        private int hConsoleHandle;
        private CONSOLE_SCREEN_BUFFER_INFO ConsoleInfo = new CONSOLE_SCREEN_BUFFER_INFO();
        private int originalColors;

        private const int STD_OUTPUT_HANDLE = -11;

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", EntryPoint = "GetConsoleScreenBufferInfo", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetConsoleScreenBufferInfo(int hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

        [DllImport("kernel32.dll", EntryPoint = "SetConsoleTextAttribute", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetConsoleTextAttribute(int hConsoleOutput, int wAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        public enum Foreground
        {
            Blue = 0x00000001,
            Green = 0x00000002,
            Red = 0x00000004,
            Intensity = 0x00000008
        }

        public enum Background
        {
            Blue = 0x00000010,
            Green = 0x00000020,
            Red = 0x00000040,
            Intensity = 0x00000080
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            short X;
            short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SMALL_RECT
        {
            short Left;
            short Top;
            short Right;
            short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public int wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

        public ConsoleApi()
            : this(GetStdHandle(STD_OUTPUT_HANDLE))
        {
        }

        public ConsoleApi(int argConsoleProcessHandle)
        {
            hConsoleHandle = argConsoleProcessHandle;
        }

        public void TextColor(int color)
        {
            SetConsoleTextAttribute(hConsoleHandle, color);
        }

        public void ResetColor()
        {
            SetConsoleTextAttribute(hConsoleHandle, OriginalColors);
        }

        public int? CurrentColor
        {
            get
            {
                if (GetConsoleScreenBufferInfo(hConsoleHandle, ref this.ConsoleInfo) != 0)
                {
                    return this.ConsoleInfo.wAttributes;
                }
                else
                {
                    return null;
                }
            }
        }

        public int OriginalColors
        {
            get
            {
                return this.originalColors;
            }
            private set
            {
                this.originalColors = value;
            }
        }
    }
}