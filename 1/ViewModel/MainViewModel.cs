using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System.IO;
using System.Reactive.Linq;
using System.Windows;

namespace BMWPaint;

public class MainViewModel : ReactiveObject
{
    public CanvasViewModel CanvasVM { get; init; }
    public MouseViewModel MouseVM { get; init; } = new();
    public MatrixViewModel MatrixVM { get; init; } = new();
    [Reactive] public ToolEnum? SelectTool { get; set; } = ToolEnum.Line;
    [Reactive] public bool CanPaste { get; set; } = false;

    private BMWLine? pasteData = null;

    private ToolEnum? _oldTool = ToolEnum.Line;
    private readonly ObservableList<IBMWObject> _objects = [];
    private readonly Dictionary<ToolEnum, ToolBase> _tools = [];
    public MainViewModel()
    {
        CanvasVM = new(_objects, MatrixVM);

        _tools[ToolEnum.Select] = new SelectTool(_objects, MatrixVM);
        _tools[ToolEnum.Copy] = new CopyTool(_objects, MatrixVM);
        _tools[ToolEnum.Line] = new LineTool(_objects, MatrixVM);

        MouseVM.MoveEvent += MouseMove;
        MouseVM.LeftClickEvent += LeftClick;
        MouseVM.RightClickEvent += RightClick;
        MouseVM.LeftDownEvent+= LeftDown;
        MouseVM.LeftDragEvent += LeftDrag;

        MouseVM.WheelEvent += MatrixVM.Zoom;
        MouseVM.MiddleDownEvent += PanStart;
        MouseVM.MiddleDragEvent += Pan;
        this.WhenAnyValue(x => x.SelectTool).Subscribe(ToolChanged);
    }
    private bool LeftDown(Point pt)
    {
        if (SelectTool == null)
            return false;

        return _tools[SelectTool.Value].LeftDown(pt);
    }
    private bool MouseMove(Point pt)
    {
        if (SelectTool == null)
            return false;

        return _tools[SelectTool.Value].MouseMove(pt);
    }
    private bool LeftDrag(Point pt)
    {
        if (SelectTool == null)
            return false;

        return _tools[SelectTool.Value].LeftDrag(pt);
    }
    private bool LeftClick(Point pt)
    {
        if (SelectTool == null)
            return false;

        return _tools[SelectTool.Value].LeftClick(pt);
    }
    private bool RightClick(Point pt)
    {
        if (SelectTool == null)
            return false;

        return _tools[SelectTool.Value].RightClick(pt);
    }

    private bool PanStart(Point pt)
    {
        MatrixVM.PanStart = pt;
        CanvasVM.Refresh();
        return true;
    }

    private bool Pan(Point pt)
    {
        MatrixVM.Pan(pt);
        CanvasVM.Refresh();
        return true;
    }
    private void ToolChanged(ToolEnum? select)
    {
        if (_oldTool != null)
            _tools[_oldTool.Value].Quit();

        if (select == null)
        {
            _oldTool = select;
            return;
        }
        if (select == ToolEnum.Paste)
        {
            _objects.Add(pasteData!);
            SelectTool = _oldTool;
            return;
        }
        else if (_tools[select.Value].Init() == false)
            SelectTool = _oldTool;
        else
            _oldTool = select;
    }

    internal void ClipboardChanged()
    {
        var data = Clipboard.GetDataObject();
        if (data == null)
        {
            CanPaste = false;
            return;
        }
        if (data.GetDataPresent(typeof(BMWLine)) == false)
        {
            CanPaste = false;
            return;
        }

        var line = data.GetData(typeof(BMWLine)) as BMWLine;
        if (line == null)
        {
            CanPaste = false;
            return;
        }

        pasteData = new BMWLine();
        foreach (var pt in line.Points)
            pasteData.Points.Add(new(pt.X + 10, pt.Y + 10));

        CanPaste = true;
    }
}