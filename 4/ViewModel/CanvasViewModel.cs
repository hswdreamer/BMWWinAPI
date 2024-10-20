using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
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

    private SKPaint _strokePaint;
    private SKPaint _fillPaint;

    public CanvasViewModel(ObservableCollection<IBMWObject> objects, MatrixViewModel matrixVM)
    {
        _objects = objects;
        _matrixVM = matrixVM;
        _objects.CollectionChanged += (_, _) => Refresh();
        this.WhenAnyValue(x => x._matrixVM.Matrix).Subscribe((_) => Refresh());
        _strokePaint = new()
        {
            IsAntialias = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke
        };

        _fillPaint = new()
        {
            IsAntialias = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Fill,
            Color = SKColors.Blue,
            TextSize = 20,
            Typeface = SKFontManager.Default.MatchCharacter('가')
        };

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
                default:
                    break;
            }
        }
    }
    private void Draw(SKCanvas canvas, BMWLine line)
    {
        SKPath path = new();
        path.MoveTo(line.Points[0]);
        for (var i = 1; i < line.Points.Count; i++)
            path.LineTo(line.Points[i]);
        canvas.DrawPath(path, _strokePaint);
    }

}