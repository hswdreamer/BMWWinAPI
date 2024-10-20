using PInvoke;
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
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        WindowInteropHelper helper = new(Application.Current.MainWindow);
        HWnd = helper.Handle;
        HwndSource.FromHwnd(HWnd).AddHook(new HwndSourceHook(WndProc));
    }

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        try
        {
            switch ((User32.WindowMessage)msg)
            {
                case User32.WindowMessage.WM_SETCURSOR:
                    if (LOWORD(lParam) == -2)
                        handled = true;
                    break;
            }
        }
        catch
        {
        }

        return IntPtr.Zero;
    }
    public static short LOWORD(nint dword) => (short)(dword & 0xFFFF);

}