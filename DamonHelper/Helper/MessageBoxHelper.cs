﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MessageBoxHelper
{
    private static IWin32Window s_owner;
    private static HookProc s_hookProc;
    private static IntPtr s_hHook;

    public static DialogResult Show(string text)
    {
        Initialize();
        return MessageBox.Show(text);
    }

    public static DialogResult Show(string text, string caption)
    {
        Initialize();
        return MessageBox.Show(text, caption);
    }

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
    {
        Initialize();
        return MessageBox.Show(text, caption, buttons);
    }

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        Initialize();
        return MessageBox.Show(text, caption, buttons, icon);
    }

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
    {
        Initialize();
        return MessageBox.Show(text, caption, buttons, icon, defButton);
    }

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options)
    {
        Initialize();
        return MessageBox.Show(text, caption, buttons, icon, defButton, options);
    }

    public static DialogResult Show(IWin32Window owner, string text)
    {
        s_owner = owner;
        Initialize();
        return MessageBox.Show(owner, text);
    }

    public static DialogResult Show(IWin32Window owner, string text, string caption)
    {
        s_owner = owner;
        Initialize();
        return MessageBox.Show(owner, text, caption);
    }

    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
    {
        s_owner = owner;
        Initialize();
        return MessageBox.Show(owner, text, caption, buttons);
    }

    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        s_owner = owner;
        Initialize();
        return MessageBox.Show(owner, text, caption, buttons, icon);
    }

    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
    {
        s_owner = owner;
        Initialize();
        return MessageBox.Show(owner, text, caption, buttons, icon, defButton);
    }

    public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options)
    {
        s_owner = owner;
        Initialize();
        return MessageBox.Show(owner, text, caption, buttons, icon,
                               defButton, options);
    }

    public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    public delegate void TimerProc(IntPtr hWnd, uint uMsg, UIntPtr nIDEvent, uint dwTime);

    public const int WH_CALLWNDPROCRET = 12;

    public enum CbtHookAction
    {
        HCBT_MOVESIZE = 0,
        HCBT_MINMAX = 1,
        HCBT_QS = 2,
        HCBT_CREATEWND = 3,
        HCBT_DESTROYWND = 4,
        HCBT_ACTIVATE = 5,
        HCBT_CLICKSKIPPED = 6,
        HCBT_KEYSKIPPED = 7,
        HCBT_SYSCOMMAND = 8,
        HCBT_SETFOCUS = 9
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

    [DllImport("user32.dll")]
    private static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("User32.dll")]
    public static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, TimerProc lpTimerFunc);

    [DllImport("User32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

    [DllImport("user32.dll")]
    public static extern int UnhookWindowsHookEx(IntPtr idHook);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

    [DllImport("user32.dll")]
    public static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

    [StructLayout(LayoutKind.Sequential)]
    public struct CWPRETSTRUCT
    {
        public IntPtr lResult;
        public IntPtr lParam;
        public IntPtr wParam;
        public uint message;
        public IntPtr hwnd;
    }

    static MessageBoxHelper()
    {
        s_hookProc = MessageBoxHookProc;
        s_hHook = IntPtr.Zero;
    }

    private static void Initialize()
    {
        if (s_hHook != IntPtr.Zero)
        {
            throw new NotSupportedException("multiple calls are not supported");
        }

        if (s_owner != null)
        {
#pragma warning disable 618
            s_hHook = SetWindowsHookEx(WH_CALLWNDPROCRET, s_hookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
#pragma warning restore 618
        }
    }

    private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return CallNextHookEx(s_hHook, nCode, wParam, lParam);
        }

        CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
        IntPtr hook = s_hHook;

        if (msg.message == (int)CbtHookAction.HCBT_ACTIVATE)
        {
            try
            {
                CenterWindow(msg.hwnd);
            }
            finally
            {
                UnhookWindowsHookEx(s_hHook);
                s_hHook = IntPtr.Zero;
            }
        }

        return CallNextHookEx(hook, nCode, wParam, lParam);
    }

    private static void CenterWindow(IntPtr hChildWnd)
    {
        Rectangle recChild = new Rectangle(0, 0, 0, 0);
        GetWindowRect(hChildWnd, ref recChild);

        int width = recChild.Width - recChild.X;
        int height = recChild.Height - recChild.Y;

        Rectangle recParent = new Rectangle(0, 0, 0, 0);
        GetWindowRect(s_owner.Handle, ref recParent);

        Point ptCenter = new Point(0, 0)
        {
            X = recParent.X + ((recParent.Width - recParent.X) / 2),
            Y = recParent.Y + ((recParent.Height - recParent.Y) / 2)
        };


        Point ptStart = new Point(0, 0)
        {
            X = (ptCenter.X - (width / 2)),
            Y = (ptCenter.Y - (height / 2))
        };

        ptStart.X = (ptStart.X < 0) ? 0 : ptStart.X;
        ptStart.Y = (ptStart.Y < 0) ? 0 : ptStart.Y;

        MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width,
            height, false);
    }

}