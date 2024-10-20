using PInvoke;
using SkiaSharp.Views.WPF;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace BMWPaint;
/// <summary>
/// Input.xaml에 대한 상호 작용 논리
/// </summary>
public partial class Input : Window
{
    private static User32.SafeHookHandle MouseHookHandle = User32.SafeHookHandle.Null;
    private static User32.SafeHookHandle MessageHookHandle = User32.SafeHookHandle.Null;
    private static SKElement? Canvas = null;
    public Input()
    {
        InitializeComponent();

        Canvas = FindChild<SKElement>(Application.Current.MainWindow);
        if (Canvas == null)
            return;
        Loaded += Window_Loaded;
        Closed += Window_Closed;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        MouseHookHandle = User32.SetWindowsHookEx(User32.WindowsHookType.WH_MOUSE, MouseHookProc, IntPtr.Zero, GetCurrentThreadId());
        MessageHookHandle = User32.SetWindowsHookEx(User32.WindowsHookType.WH_GETMESSAGE, MessageHookProc, IntPtr.Zero, GetCurrentThreadId());
    }
    private void Window_Closed(object? sender, EventArgs e)
    {
        Canvas = null;

        MouseHookHandle?.Dispose();
        MouseHookHandle = User32.SafeHookHandle.Null;
        MessageHookHandle?.Dispose();
        MessageHookHandle = User32.SafeHookHandle.Null;
    }

    private static int MessageHookProc(int nCode, nint wParam, nint lParam)
    {
        if (MessageHookHandle.IsInvalid)
            return 0;
        if (nCode < 0)
            return User32.CallNextHookEx(MessageHookHandle.DangerousGetHandle(), nCode, wParam, lParam);

        var data = MessageHookStruct.Create(lParam);
        if ((User32.WindowMessage)data.message == User32.WindowMessage.WM_MOUSEWHEEL)
        {
            var message = new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, data.HIWPARAM)
            {
                RoutedEvent = Mouse.MouseWheelEvent,
                Source = Canvas
            };
           Canvas?.RaiseEvent(message);
        }

        return User32.CallNextHookEx(MessageHookHandle.DangerousGetHandle(), nCode, wParam, lParam);
    }
    private static int MouseHookProc(int nCode, nint wParam, nint lParam)
    {
        if (MouseHookHandle.IsInvalid)
            return 0;
        if (nCode < 0)
            return User32.CallNextHookEx(MouseHookHandle.DangerousGetHandle(), nCode, wParam, lParam);

        switch ((User32.WindowMessage)wParam)
        {
            case User32.WindowMessage.WM_NCMBUTTONDOWN:
                {
                    var message = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle)
                    {
                        RoutedEvent = Mouse.MouseDownEvent,
                        Source =Canvas
                    };
                   Canvas?.RaiseEvent(message);
                }
                break;
            case User32.WindowMessage.WM_NCMBUTTONUP:
                {
                    var message = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle)
                    {
                        RoutedEvent = Mouse.MouseUpEvent,
                        Source =Canvas
                    };
                   Canvas?.RaiseEvent(message);
                }
                break;
            case User32.WindowMessage.WM_NCMOUSEMOVE:
                {
                    if ((User32.GetKeyState((int)User32.VirtualKey.VK_MBUTTON) & 0x80) == 0x80)
                    {
                        var message = new MouseEventArgs(Mouse.PrimaryDevice, 0)
                        {
                            RoutedEvent = Mouse.MouseMoveEvent,
                            Source =Canvas
                        };
                       Canvas?.RaiseEvent(message);
                    }
                }
                break;
        }

        return User32.CallNextHookEx(MouseHookHandle.DangerousGetHandle(), nCode, wParam, lParam);
    }
    private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent is T t)
            return t;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t1)
                return t1;
            var ret = FindChild<T>(child);
            if (ret != null)
                return ret;
        }
        return null;
    }
    [DllImport("kernel32.dll")]
    public static extern int GetCurrentThreadId();

    [StructLayout(LayoutKind.Sequential)]
    public struct MessageHookStruct
    {
        public nint hwnd;
        public int message;
        public nint wParam;
        public nint lParam;
        public int time;
        public int x;
        public int y;
        public static MessageHookStruct Create(nint ptr)
        {
            var ret = Marshal.PtrToStructure<MessageHookStruct>(ptr);
            return ret;
        }
        public Point Point => new(x, y);
        public readonly short LOWPARAM => LOWORD(wParam);
        public readonly short HIWPARAM => HIWORD(wParam);
        public readonly short LOLPARAM => LOWORD(lParam);
        public readonly short HILPARAM => HIWORD(lParam);
    }
    public static short LOWORD(nint dword) => (short)(dword & 0xFFFF);
    public static short HIWORD(nint dword) => (short)((dword >> 16) & 0xFFFF);
}