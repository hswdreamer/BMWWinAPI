using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System.Collections.ObjectModel;
using System.Windows;

namespace BMWPaint;

public class CanvasViewModel : ReactiveObject
{
    [Reactive] public int Update { get; set; } = 0;
    public Size ActualSize { get; set; } = new();
    public void Refresh() => ++Update;

    private ObservableCollection<IBMWObject> _objects;
    private MatrixViewModel _matrixVM;


    public CanvasViewModel(ObservableCollection<IBMWObject> objects, MatrixViewModel matrixVM)
    {
        _objects = objects;
        _matrixVM = matrixVM;
        _objects.CollectionChanged += (_, _) => Refresh();
        this.WhenAnyValue(x => x._matrixVM.Matrix).Subscribe((_) => Refresh());
    }

    public void PaintSurface(SKCanvas canvas)
    {
        canvas.Clear(SKColors.White);
        canvas.SetMatrix(_matrixVM.Matrix);
        foreach (var obj in _objects)
        {
            switch (obj)
            {
                case BMWLine line: Draw(canvas, line); break;
                case BMWRect rect: Draw(canvas, rect); break;
                default:
                    break;
            }
        }
    }
    private void Draw(SKCanvas canvas, BMWLine line)
    {
        SKPath path = new();
        SKPaint paint = new()
        {
            IsAntialias = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
            Color = line.IsRed ? SKColors.Red : SKColors.Black,
        };
            
        path.MoveTo(line.Points[0].ToSKPoint());
        for (var i = 1; i < line.Points.Count; i++)
            path.LineTo(line.Points[i].ToSKPoint());
        canvas.DrawPath(path, paint);
    }
    private void Draw(SKCanvas canvas, BMWRect rect)
    {
        SKPaint paint = new()
        {
            IsAntialias = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
            Color = rect.Color,
        };
        if (rect.IsDash)
            paint.PathEffect = SKPathEffect.CreateDash([5, 2], 0);

        canvas.DrawRect(rect.Rect, paint);
    }

}