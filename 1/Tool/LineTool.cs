using SkiaSharp.Views.WPF;
using System.Windows;

namespace BMWPaint;

public class LineTool(ObservableList<IBMWObject> objects, MatrixViewModel matrixVM) : ToolBase(objects, matrixVM)
{
    private BMWLine? _obj = null;
    public override bool LeftClick(Point pt)
    {
        var skPt = MatrixVM.LogicalPoint(pt);

        if (_obj == null)
        {
            _obj = new();
            _obj.Points.Add(skPt.ToPoint());
            _obj.Points.Add(skPt.ToPoint());
            Objects.Add(_obj);
        }
        else
        {
            _obj.Points.Add(skPt.ToPoint());
            Objects.Tick();
        }

        return true;
    }
    public override bool MouseMove(Point pt)
    {
        if (_obj == null)
            return false;

        var skPt = MatrixVM.LogicalPoint(pt);

        _obj.Points[^1] = skPt.ToPoint();
        Objects.Tick();

        return true;
    }
    public override bool RightClick(Point pt)
    {
        if (_obj == null)
            return false;

        var skPt = MatrixVM.LogicalPoint(pt);

        _obj.Points[^1] = skPt.ToPoint();
        Objects.Tick();
        _obj = null;

        return true;
    }
}