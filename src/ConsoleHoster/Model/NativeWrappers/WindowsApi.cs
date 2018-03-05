//-----------------------------------------------------------------------
// <copyright file="WindowsApi.cs" author="Artak Mkrtchyan">
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
    public static class WindowsApi
    {
        public const int ProcessBasicInformation = 0;
        public const int ProcessWow64Information = 26;

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr hProcess, int pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, ref int pSize);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr hProcess, int pic, ref IntPtr pi, int cb, ref int pSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr dwSize, ref IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, IntPtr dwSize, ref IntPtr lpNumberOfBytesRead);

        public const int PAGE_NOACCESS = 0x01;
        public const int PAGE_EXECUTE = 0x10;

        [DllImport("kernel32")]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, ref MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        [DllImport("kernel32.dll")]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetStdHandle(int whichHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);


        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi, EntryPoint = "DebugActiveProcess", ExactSpelling = true, SetLastError = true)]
        public static extern bool DebugActiveProcess(int ProcessHandle);

        [DllImport("kernel32", CallingConvention = CallingConvention.Winapi, EntryPoint = "WaitForDebugEvent", ExactSpelling = true, SetLastError = true)]
        public static extern bool WaitForDebugEvent([Out, MarshalAs(UnmanagedType.LPStruct)] DEBUG_EVENT DebugEvent, int dwMilliseconds);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("kernel32.dll")]
        internal static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hHandle);


        #region Type definitions
        [Flags]
        internal enum HANDLE_FLAGS
        {
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEBUG_EVENT
        {
            public DebugEventType dwDebugEventCode;
            public int dwProcessId;
            public int dwThreadId;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 86, ArraySubType = UnmanagedType.U1)]
            byte[] debugInfo;

            public EXCEPTION_DEBUG_INFO Exception
            {
                get
                {
                    return GetDebugInfo<EXCEPTION_DEBUG_INFO>();
                }
            }

            public CREATE_THREAD_DEBUG_INFO CreateThread
            {
                get
                {
                    return GetDebugInfo<CREATE_THREAD_DEBUG_INFO>();
                }
            }

            public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo
            {
                get
                {
                    return GetDebugInfo<CREATE_PROCESS_DEBUG_INFO>();
                }
            }

            public EXIT_THREAD_DEBUG_INFO ExitThread
            {
                get
                {
                    return GetDebugInfo<EXIT_THREAD_DEBUG_INFO>();
                }
            }

            public EXIT_PROCESS_DEBUG_INFO ExitProcess
            {
                get
                {
                    return GetDebugInfo<EXIT_PROCESS_DEBUG_INFO>();
                }
            }

            public LOAD_DLL_DEBUG_INFO LoadDll
            {
                get
                {
                    return GetDebugInfo<LOAD_DLL_DEBUG_INFO>();
                }
            }

            public UNLOAD_DLL_DEBUG_INFO UnloadDll
            {
                get
                {
                    return GetDebugInfo<UNLOAD_DLL_DEBUG_INFO>();
                }
            }

            public OUTPUT_DEBUG_STRING_INFO DebugString
            {
                get
                {
                    return GetDebugInfo<OUTPUT_DEBUG_STRING_INFO>();
                }
            }

            public RIP_INFO RipInfo
            {
                get
                {
                    return GetDebugInfo<RIP_INFO>();
                }
            }

            private T GetDebugInfo<T>() where T : struct
            {
                var structSize = Marshal.SizeOf(typeof(T));
                var pointer = Marshal.AllocHGlobal(structSize);
                Marshal.Copy(debugInfo, 0, pointer, structSize);

                var result = Marshal.PtrToStructure(pointer, typeof(T));
                Marshal.FreeHGlobal(pointer);
                return (T)result;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct EXCEPTION_DEBUG_INFO
            {
                public EXCEPTION_RECORD ExceptionRecord;
                public uint dwFirstChance;
            }

            public delegate uint PTHREAD_START_ROUTINE(IntPtr lpThreadParameter);

            [StructLayout(LayoutKind.Sequential)]
            public struct CREATE_THREAD_DEBUG_INFO
            {
                public IntPtr hThread;
                public IntPtr lpThreadLocalBase;
                public PTHREAD_START_ROUTINE lpStartAddress;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CREATE_PROCESS_DEBUG_INFO
            {
                public IntPtr hFile;
                public IntPtr hProcess;
                public IntPtr hThread;
                public IntPtr lpBaseOfImage;
                public uint dwDebugInfoFileOffset;
                public uint nDebugInfoSize;
                public IntPtr lpThreadLocalBase;
                public PTHREAD_START_ROUTINE lpStartAddress;
                public IntPtr lpImageName;
                public ushort fUnicode;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct EXIT_THREAD_DEBUG_INFO
            {
                public uint dwExitCode;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct EXIT_PROCESS_DEBUG_INFO
            {
                public uint dwExitCode;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct LOAD_DLL_DEBUG_INFO
            {
                public IntPtr hFile;
                public IntPtr lpBaseOfDll;
                public uint dwDebugInfoFileOffset;
                public uint nDebugInfoSize;
                public IntPtr lpImageName;
                public ushort fUnicode;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct UNLOAD_DLL_DEBUG_INFO
            {
                public IntPtr lpBaseOfDll;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct OUTPUT_DEBUG_STRING_INFO
            {
                [MarshalAs(UnmanagedType.LPStr)]
                public string lpDebugStringData;
                public ushort fUnicode;
                public ushort nDebugStringLength;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RIP_INFO
            {
                public uint dwError;
                public uint dwType;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct EXCEPTION_RECORD
            {
                public uint ExceptionCode;
                public uint ExceptionFlags;
                public IntPtr ExceptionRecord;
                public IntPtr ExceptionAddress;
                public uint NumberParameters;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15, ArraySubType = UnmanagedType.U4)]
                public uint[] ExceptionInformation;
            }

            public enum DebugEventType : int
            {
                CREATE_PROCESS_DEBUG_EVENT = 3, //Reports a create-process debugging event. The value of u.CreateProcessInfo specifies a CREATE_PROCESS_DEBUG_INFO structure.
                CREATE_THREAD_DEBUG_EVENT = 2, //Reports a create-thread debugging event. The value of u.CreateThread specifies a CREATE_THREAD_DEBUG_INFO structure.
                EXCEPTION_DEBUG_EVENT = 1, //Reports an exception debugging event. The value of u.Exception specifies an EXCEPTION_DEBUG_INFO structure.
                EXIT_PROCESS_DEBUG_EVENT = 5, //Reports an exit-process debugging event. The value of u.ExitProcess specifies an EXIT_PROCESS_DEBUG_INFO structure.
                EXIT_THREAD_DEBUG_EVENT = 4, //Reports an exit-thread debugging event. The value of u.ExitThread specifies an EXIT_THREAD_DEBUG_INFO structure.
                LOAD_DLL_DEBUG_EVENT = 6, //Reports a load-dynamic-link-library (DLL) debugging event. The value of u.LoadDll specifies a LOAD_DLL_DEBUG_INFO structure.
                OUTPUT_DEBUG_STRING_EVENT = 8, //Reports an output-debugging-string debugging event. The value of u.DebugString specifies an OUTPUT_DEBUG_STRING_INFO structure.
                RIP_EVENT = 9, //Reports a RIP-debugging event (system debugging error). The value of u.RipInfo specifies a RIP_INFO structure.
                UNLOAD_DLL_DEBUG_EVENT = 7, //Reports an unload-DLL debugging event. The value of u.UnloadDll specifies an UNLOAD_DLL_DEBUG_INFO structure.
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public int AllocationProtect;
            public IntPtr RegionSize;
            public int State;
            public int Protect;
            public int Type;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public IntPtr[] Reserved2;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }
        #endregion
    }
}