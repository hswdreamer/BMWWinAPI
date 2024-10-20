using SkiaSharp;
using SkiaSharp.Views.WPF;
using System.Windows;

namespace BMWPaint;

public class SelectTool(ObservableList<IBMWObject> objects, MatrixViewModel matrixVM) : ToolBase(objects, matrixVM)
{
    private BMWRect? _obj = null;
    public override bool LeftClick(Point pt)
    {
        var skPt = MatrixVM.LogicalPoint(pt);
        if (_obj == null)
        {
            _obj = new();
            _obj.Rect = new(skPt.X, skPt.Y, skPt.X, skPt.Y);
            _obj.IsDash = true;
            _obj.Color = SKColors.Red;
            Objects.Add(_obj);
        }
        else
        {
            Objects.Remove(_obj);
            Select();
            Objects.Tick();
            _obj = null;
        }

        return true;
    }
    public override bool MouseMove(Point pt)
    {
        if (_obj == null)
            return false;

        var skPt = MatrixVM.LogicalPoint(pt);

        _obj!.Rect = new SKRect(_obj.Rect.Left, _obj.Rect.Top, skPt.X, skPt.Y);
        Objects.Tick();

        return true;
    }
    private void Select()
    {
        foreach (var line in Objects.OfType<BMWLine>())
        {
            foreach (var pt in line.Points)
            {
                if (_obj.Rect.Contains(pt.ToSKPoint()))
                {
                    line.IsRed = true;
                    return;
                }
            }
        }
    }
}