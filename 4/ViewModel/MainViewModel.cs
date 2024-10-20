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
    public IReactiveCommand ShowCommand { get; set; }

    private ToolEnum? _oldTool = ToolEnum.Line;
    private readonly ObservableList<IBMWObject> _objects = [];
    private readonly Dictionary<ToolEnum, ToolBase> _tools = [];
    public MainViewModel()
    {
        CanvasVM = new(_objects, MatrixVM);
        ShowCommand = ReactiveCommand.Create(Show);
        _tools[ToolEnum.Line] = new LineTool(_objects, MatrixVM);

        MouseVM.MoveEvent += MouseMove;
        MouseVM.LeftClickEvent += LeftClick;
        MouseVM.RightClickEvent += RightClick;

        MouseVM.WheelEvent += MatrixVM.Zoom;
        MouseVM.MiddleDownEvent += PanStart;
        MouseVM.MiddleDragEvent += Pan;
        this.WhenAnyValue(x => x.SelectTool).Subscribe(ToolChanged);
    }

    private void Show()
    {
        Input temp = new Input();
        temp.ShowDialog();
    }

    private bool MouseMove(Point pt)
    {
        if (SelectTool == null)
            return false;

        return _tools[SelectTool.Value].MouseMove(pt);
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

        if (_tools[select.Value].Init() == false)
            SelectTool = _oldTool;
        else
            _oldTool = select;
    }
}