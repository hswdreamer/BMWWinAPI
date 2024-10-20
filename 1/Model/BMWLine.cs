using SkiaSharp;
using System.Windows;

namespace BMWPaint;

[Serializable]
public class BMWLine : IBMWObject
{
    public List<Point> Points = [];
    public bool IsRed { get; set; } = false;
}