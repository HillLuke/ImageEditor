using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImageEditor;

public class ImageEditorOne{
    public Bitmap Execute(Bitmap imageBitmap){
        var _ = StaticHelpers.TimeFunction(() => Base(imageBitmap, new() { GreyScale }), nameof(Base));
        StaticHelpers.SaveImage(_.Bitmap, $"ImageEditorOne-{string.Join("-", _.ProcessesUsed)}");     

        _ = StaticHelpers.TimeFunction(() => Base(imageBitmap, new() { Invert }), nameof(Base));
        StaticHelpers.SaveImage(_.Bitmap, $"ImageEditorOne-{string.Join("-", _.ProcessesUsed)}");     
        
        _ = StaticHelpers.TimeFunction(() => Base(imageBitmap, new() { GreyScale, Invert }), nameof(Base));
        StaticHelpers.SaveImage(_.Bitmap, $"ImageEditorOne-{string.Join("-", _.ProcessesUsed)}");     

        return imageBitmap;
    }
    private Result Base(Bitmap original, List<Action<int, int, byte[], int, int>> actions)
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

        foreach (var action in actions)
        {
            action(bitmapX, bitmapY, pixels, bitmapData.Stride, bytesPerPixel);
        }

        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
        bitmapCopy.UnlockBits(bitmapData);

        return new Result 
        {
            Bitmap = bitmapCopy,
            ProcessesUsed = actions.Select(x => x.Method.Name).ToArray()
        };
    }

    private void Invert(int bitmapX, int bitmapY, byte[] pixels, int stride, int bytesPerPixel)
    {
        Parallel.For(0, bitmapX, x =>
        {
            for (int y = 0; y < bitmapY; y++)
            {
                var index = (y * stride) + (x * bytesPerPixel);
                int r = pixels[index + 2];
                int g = pixels[index + 1];
                int b = pixels[index];

                int invertedR = 255 - r;
                int invertedG = 255 - g;
                int invertedB = 255 - b;

                pixels[index] = (byte)invertedR;
                pixels[index + 1] = (byte)invertedG;
                pixels[index + 2] = (byte)invertedB;
            }
        });
    }

    private static void AdjustBrightness(int bitmapX, int bitmapY, byte[] pixels, int stride, int bytesPerPixel, int adjustpercent)
    {
        Parallel.For(0, bitmapX, x =>
        {
            for (int y = 0; y < bitmapY; y++)
            {
                var index = (y * stride) + (x * bytesPerPixel);
                int r = pixels[index + 2];
                int g = pixels[index + 1];
                int b = pixels[index];

                int newR = r * adjustpercent / 100;
                int NewG = g * adjustpercent / 100;
                int NewB = b * adjustpercent / 100;

                pixels[index] = (byte)newR;
                pixels[index + 1] = (byte)NewG;
                pixels[index + 2] = (byte)NewB;
            }
        });
    }

    private static void GreyScale(int bitmapX, int bitmapY, byte[] pixels, int stride, int bytesPerPixel)
    {
        Parallel.For(0, bitmapX, x =>
        {
            for (int y = 0; y < bitmapY; y++)
            {
                var index = (y * stride) + (x * bytesPerPixel);
                int r = pixels[index + 2];
                int g = pixels[index + 1];
                int b = pixels[index];

                int gray = (r + g + b) / 3;

                pixels[index] = (byte)gray;
                pixels[index + 1] = (byte)gray;
                pixels[index + 2] = (byte)gray;
            }
        });
    }
}