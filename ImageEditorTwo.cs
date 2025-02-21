using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImageEditor;

public class ImageEditorTwo {

    public Bitmap Execute(Bitmap imageBitmap)
    {
        var _ = StaticHelpers.TimeFunction(() => Base(imageBitmap, new List<IImageProcessingStrategy>
        {
            new GreyScaleStrategy(),
            new InvertStrategy(),
            new AdjustBrightnessStrategy(120)
        }), nameof(Base));
        StaticHelpers.SaveImage(_.Bitmap, $"ImageEditorTwo-{_.SaveName}");  

        _ = StaticHelpers.TimeFunction(() => Base(imageBitmap, new List<IImageProcessingStrategy>
        {
            new EdgeDetectionStrategy(100,255)
        }), nameof(Base));
        StaticHelpers.SaveImage(_.Bitmap, $"ImageEditorTwo-{_.SaveName}");  

        _ = StaticHelpers.TimeFunction(() => Base(imageBitmap, new List<IImageProcessingStrategy>
        {
            new PencilSketchStrategy(255,255)
        }), nameof(Base));

        return imageBitmap;
    }

    private Result Base(Bitmap original, List<IImageProcessingStrategy> strategies)
    {
        var bitmapX = original.Size.Width;
        var bitmapY = original.Size.Height;
        var bitmapCopy = new Bitmap(bitmapX, bitmapY, PixelFormat.Format24bppRgb);

        // Copy the original bitmap to the new bitmap
        using (var g = Graphics.FromImage(bitmapCopy))
        {
            g.DrawImage(original, 0, 0, bitmapX, bitmapY);
        }

        var rect = new Rectangle(0, 0, bitmapCopy.Width, bitmapCopy.Height);
        var bitmapData = bitmapCopy.LockBits(rect, ImageLockMode.ReadWrite, bitmapCopy.PixelFormat);

        int bytesPerPixel = Image.GetPixelFormatSize(bitmapCopy.PixelFormat) / 8;
        int byteCount = bitmapData.Stride * bitmapCopy.Height;
        byte[] pixels = new byte[byteCount];
        IntPtr ptrFirstPixel = bitmapData.Scan0;
        Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

        var imageData = new ImageData
        {
            Width = bitmapX,
            Height = bitmapY,
            Pixels = pixels,
            Stride = bitmapData.Stride,
            BytesPerPixel = bytesPerPixel
        };

        foreach (var strategy in strategies)
        {
            strategy.Process(imageData);
        }

        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
        bitmapCopy.UnlockBits(bitmapData);

        return new Result
        {
            Bitmap = bitmapCopy,
            ProcessesUsed = strategies.Select(x => x.GetType().Name).ToArray(),
            SaveName = string.Join("-", strategies.Select(x => x.GetName()))
        };
    }
}

public class ImageData
{
    public int Width { get; init; }
    public int Height { get; init; }
    public byte[] Pixels { get; init; }
    public int Stride { get; init; }
    public int BytesPerPixel { get; init; }
}

public interface IImageProcessingStrategy
{
    void Process(ImageData imageData);

    abstract string GetName();
}

public class GreyScaleStrategy : IImageProcessingStrategy
{
    public string GetName()
    {
        return nameof(GreyScaleStrategy);
    }

    public void Process(ImageData imageData)
    {
        Parallel.For(0, imageData.Width, x =>
        {
            for (int y = 0; y < imageData.Height; y++)
            {
                var index = (y * imageData.Stride) + (x * imageData.BytesPerPixel);
                int b = imageData.Pixels[index];
                int g = imageData.Pixels[index + 1];
                int r = imageData.Pixels[index + 2];

                int gray = (r + g + b) / 3;

                imageData.Pixels[index] = (byte)gray;
                imageData.Pixels[index + 1] = (byte)gray;
                imageData.Pixels[index + 2] = (byte)gray;
            }
        });
    }
}

public class InvertStrategy : IImageProcessingStrategy
{
    public string GetName()
    {
        return nameof(InvertStrategy);
    }

    public void Process(ImageData imageData)
    {
        Parallel.For(0, imageData.Width, x =>
        {
            for (int y = 0; y < imageData.Height; y++)
            {
                var index = (y * imageData.Stride) + (x * imageData.BytesPerPixel);
                int b = imageData.Pixels[index];
                int g = imageData.Pixels[index + 1];
                int r = imageData.Pixels[index + 2];

                int invertedB = 255 - b;
                int invertedG = 255 - g;
                int invertedR = 255 - r;

                imageData.Pixels[index] = (byte)invertedB;
                imageData.Pixels[index + 1] = (byte)invertedG;
                imageData.Pixels[index + 2] = (byte)invertedR;
            }
        });
    }
}

public class AdjustBrightnessStrategy : IImageProcessingStrategy
{
    private readonly int _adjustPercent;

    public AdjustBrightnessStrategy(int adjustPercent)
    {
        _adjustPercent = adjustPercent;
    }

    public string GetName()
    {
        return $"{nameof(AdjustBrightnessStrategy)}-{_adjustPercent}";
    }

    public void Process(ImageData imageData)
    {
        Parallel.For(0, imageData.Width, x =>
        {
            for (int y = 0; y < imageData.Height; y++)
            {
                var index = (y * imageData.Stride) + (x * imageData.BytesPerPixel);
                int b = imageData.Pixels[index];
                int g = imageData.Pixels[index + 1];
                int r = imageData.Pixels[index + 2];

                int newB = Clamp(b * _adjustPercent / 100);
                int newG = Clamp(g * _adjustPercent / 100);
                int newR = Clamp(r * _adjustPercent / 100);

                imageData.Pixels[index] = (byte)newB;
                imageData.Pixels[index + 1] = (byte)newG;
                imageData.Pixels[index + 2] = (byte)newR;
            }
        });
    }

    private int Clamp(int value)
    {
        return Math.Min(255, Math.Max(0, value));
    }
}

public class EdgeDetectionStrategy : IImageProcessingStrategy
{
    private readonly int _threshold;
    private readonly byte _edgeColor;

    public EdgeDetectionStrategy(int threshold = 128, byte edgeColor = 255)
    {
        _threshold = threshold;
        _edgeColor = edgeColor;
    }

    public string GetName()
    {
        return $"{nameof(EdgeDetectionStrategy)}-Threshold{_threshold}-EdgeColor{_edgeColor}";
    }

    public void Process(ImageData imageData)
    {
        Parallel.For(0, imageData.Width, x =>
        {
            for (int y = 0; y < imageData.Height; y++)
            {
                var index = (y * imageData.Stride) + (x * imageData.BytesPerPixel);
                int b = imageData.Pixels[index];
                int g = imageData.Pixels[index + 1];
                int r = imageData.Pixels[index + 2];

                int gray = (r + g + b) / 3;
                int edge = gray > _threshold ? _edgeColor : 0;

                imageData.Pixels[index] = (byte)edge;
                imageData.Pixels[index + 1] = (byte)edge;
                imageData.Pixels[index + 2] = (byte)edge;
            }
        });
    }
}

public class PencilSketchStrategy : IImageProcessingStrategy
{
    private readonly int _threshold;
    private readonly byte _edgeColor;

    public PencilSketchStrategy(int threshold = 128, byte edgeColor = 255)
    {
        _threshold = threshold;
        _edgeColor = edgeColor;
    }

    public string GetName()
    {
        return $"{nameof(PencilSketchStrategy)}-Threshold{_threshold}-EdgeColor{_edgeColor}";
    }

    public void Process(ImageData imageData)
    {
        Parallel.For(0, imageData.Width, x =>
        {
            for (int y = 0; y < imageData.Height; y++)
            {
                var index = (y * imageData.Stride) + (x * imageData.BytesPerPixel);
                int b = imageData.Pixels[index];
                int g = imageData.Pixels[index + 1];
                int r = imageData.Pixels[index + 2];

                int gray = (r + g + b) / 3;
                int edge = gray > _threshold ? _edgeColor : 0;
                int pencilGray = (int)(edge * 0.5 + gray * 0.5);

                imageData.Pixels[index] = (byte)pencilGray;
                imageData.Pixels[index + 1] = (byte)pencilGray;
                imageData.Pixels[index + 2] = (byte)pencilGray;
            }
        });
    }
}