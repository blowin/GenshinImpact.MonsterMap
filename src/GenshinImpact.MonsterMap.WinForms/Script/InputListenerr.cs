using System;
using System.Runtime.InteropServices;

namespace GenshinImpact.MonsterMap.Script
{
    /// <summary>
    /// Global detection class
    /// </summary>
    class InputListenerr
    {
        static bool isCtrlDown = false;
        ///////////////////////////////////////////The following is the win API area that I can't understand///////////////////////////////////////////////////
        ///The role of this class is to detect keyboard input even in the background
        static WindowsHookCallBack k_callback;
        ///The role of this class is to detect mouse input even in the background
        static WindowsHookCallBack m_callback;
        public static void GetKeyDownEvent(Action<string> response)
        {
            k_callback = CreateCallBack((status, data) =>
            {
                if (data.vkCode == 162)//Determine if ctrl is pressed
                {
                    isCtrlDown = status== KeyBoredHookStatus.WM_KEYDOWN; 
                }
                else if (status == KeyBoredHookStatus.WM_KEYDOWN)
                {
                    //the code
                    if (data.vkCode==27)
                    {
                        response("esc");

                    }
                    else
                    {
                        string key = (isCtrlDown ? "Ctrl" : "") + (((char)data.vkCode).ToString().ToUpper());
                        response(key);
                    }
                
                }
            });
            IntPtr intPtr = SetWindowsHookEx(WindowsHookType.WH_KEYBOARD_LL, k_callback, IntPtr.Zero, 0);
        }
        public static void GetMouseEvent(Action<string> response)
        {
            m_callback = CreateCallBack((status, data) =>
            {
                if ((int)status!=512)
                {
                    response(status.ToString());
                }

            });
            IntPtr intPtr = SetWindowsHookEx(WindowsHookType.WH_MOUSE_LL, m_callback, IntPtr.Zero, 0);
        }

        public enum KeyBoredHookStatus
        {
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class KeyBoredHookData
        {
            //virtual code
            public int vkCode;

            //scan code
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }


        enum WindowsHookType
        {
            //global keyboard hook
            WH_KEYBOARD_LL = 13,

            //global mouse hook
            WH_MOUSE_LL = 14,
        }
        ///////////////////////////////////////////The following is the win API area that I can't understand///////////////////////////////////////////////////

        //The parameters of all hook functions are the same, the problem is how to interpret the parameters
        delegate IntPtr WindowsHookCallBack(int nCode, int wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true)]
        extern static IntPtr SetWindowsHookEx(WindowsHookType hookType, WindowsHookCallBack lpfn, IntPtr hmod, int dwThreadId);


        [DllImport("User32.dll", SetLastError = true)]
        extern static IntPtr CallNextHookEx(int hhk, int nCode, int wParam, IntPtr lParam);
        //These two combination keys, you can change
        static WindowsHookCallBack CreateCallBack(Action<KeyBoredHookStatus, KeyBoredHookData> action)
        {
            return (int nCode, int wParam, IntPtr lParam) =>
            {
                if (nCode < 0)
                {
                    return CallNextHookEx(default, nCode, wParam, lParam);
                }
                else
                {
                    KeyBoredHookData data = Marshal.PtrToStructure<KeyBoredHookData>(lParam);
                    action((KeyBoredHookStatus)wParam, data);
                    return CallNextHookEx(default, nCode, wParam, lParam);
                }
            };
        }
    }
}
