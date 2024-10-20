using SkiaSharp;

namespace BMWPaint;
public class BMWRect : IBMWObject
{
    public SKRect Rect { get; set; }
    public SKColor Color { get; set; } = SKColors.Black;
    public bool IsDash { get; set; } = false;
}