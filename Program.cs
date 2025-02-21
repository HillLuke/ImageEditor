using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageEditor;

internal class Program
{
    static void Main(string[] args)
    {
        //var imageBitmap = StaticHelpers.TimeFunction(() => StaticHelpers.LoadFromPath("load.jpg"), nameof(StaticHelpers.LoadFromPath));

        //TimeFunction(() =>  new ImageEditorOne().Execute(imageBitmap), nameof(ImageEditorOne));

        var imageBitmap = StaticHelpers.TimeFunction(() => StaticHelpers.LoadFromPath("load.jpg"), nameof(StaticHelpers.LoadFromPath));
        
        StaticHelpers.TimeFunction(() =>  new ImageEditorTwo().Execute(imageBitmap), nameof(ImageEditorTwo));

    }
}

public record Result
{
    public Bitmap Bitmap { get; init; }
    public string[] ProcessesUsed { get; init; }
    public string SaveName {get; init;}
}