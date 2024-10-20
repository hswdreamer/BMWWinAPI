using PInvoke;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Interop;

namespace BMWPaint;

public partial class MainWindow : RibbonWindow
{
    private HwndSource hwndSource;
    private static nint NextClipboardChain;
    private static IntPtr HWnd = IntPtr.Zero;
    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += Window_Loaded;
        this.Closed += Window_Closed;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        WindowInteropHelper helper = new(Application.Current.MainWindow);
        HWnd = helper.Handle;
        HwndSource.FromHwnd(HWnd).AddHook(new HwndSourceHook(WndProc));
        NextClipboardChain = SetClipboardViewer(HWnd);
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        ChangeClipboardChain(HWnd, NextClipboardChain);
        HWnd = IntPtr.Zero;
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        try
        {
            switch ((User32.WindowMessage)msg)
            {
                case User32.WindowMessage.WM_CLIPBOARDUPDATE:
                    break;
                case User32.WindowMessage.WM_CHANGECBCHAIN:
                    if (wParam == NextClipboardChain)
                        NextClipboardChain = lParam;
                    else if (NextClipboardChain != 0)
                        User32.SendMessage(NextClipboardChain, (User32.WindowMessage)msg, wParam, lParam);
                    break;
                case User32.WindowMessage.WM_DRAWCLIPBOARD:
                    (Application.Current.MainWindow.DataContext as MainViewModel)?.ClipboardChanged();
                    User32.SendMessage(NextClipboardChain, (User32.WindowMessage)msg, wParam, lParam);
                    break;
            }
        }
        catch
        {
        }

        return IntPtr.Zero;
    }

    [DllImport("User32.dll")]
    public static extern IntPtr SetClipboardViewer(IntPtr hWnd);
    [DllImport("User32.dll")]
    public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

}