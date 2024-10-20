using SkiaSharp;
using SkiaSharp.Views.WPF;
using System.Windows;

namespace BMWPaint;

public class CopyTool(ObservableList<IBMWObject> objects, MatrixViewModel matrixVM) : ToolBase(objects, matrixVM)
{
    public override bool Init()
    {
        var entity = objects.OfType<BMWLine>().FirstOrDefault(x => x.IsRed);
        if (entity == null)
            return false;
        entity.IsRed = false;
        var dataObject = new DataObject();
        dataObject.SetData(typeof(BMWLine), entity);
        Clipboard.SetDataObject(dataObject);
        return false;
    }
}