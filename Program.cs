﻿using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageEditor;

internal class Program
{
    static void Main(string[] args)
    {
        //Loads from bin folder
        var imageBitmap = TimeFunction(() => LoadFromPath("load.jpg"), nameof(LoadFromPath));

        //TimeFunction(() => GrayScale(imageBitmap), nameof(GrayScale)).Save("GrayScale.png", ImageFormat.Png);
        //TimeFunction(() => Invert(imageBitmap), nameof(Invert)).Save("Invert.png", ImageFormat.Png);

        TimeFunction(() => Base2(imageBitmap, new() { GreyScale }), nameof(Base2)).Save("GreyScale.png", ImageFormat.Png);
        TimeFunction(() => Base2(imageBitmap, new() { Invert }), nameof(Base2)).Save("Invert.png", ImageFormat.Png);
        TimeFunction(() => Base2(imageBitmap, new() { GreyScale, Invert }), nameof(Base2)).Save("GreyScaleInvert.png", ImageFormat.Png);
    }

    static T TimeFunction<T>(Func<T> function, string name)
    {
        Console.WriteLine($"Starting function: {name}");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = function();
        stopwatch.Stop();
        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    static private Bitmap Base2(Bitmap original, List<Action<int, int, byte[], int, int>> actions)
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

        string saveName = string.Empty;
        foreach (var action in actions)
        {
            saveName += action.Method.Name;
            action(bitmapX, bitmapY, pixels, bitmapData.Stride, bytesPerPixel);
        }

        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
        bitmapCopy.UnlockBits(bitmapData);

        return bitmapCopy;
    }

    private static void Invert(int bitmapX, int bitmapY, byte[] pixels, int stride, int bytesPerPixel)
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

    static private Bitmap Invert(Bitmap original)
    {
        var bitmapX = original.Size.Width;
        var bitmapY = original.Size.Height;
        var bitmapCopy = new Bitmap(bitmapX, bitmapY, PixelFormat.Format24bppRgb);
        
        //copy the original bitmap to the new bitmap
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

        Parallel.For(0, bitmapX, x =>
        {
            for (int y = 0; y < bitmapY; y++)
            {
                var index = (y * bitmapData.Stride) + (x * bytesPerPixel);
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

        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
        bitmapCopy.UnlockBits(bitmapData);
        return bitmapCopy;
    }

    static private Bitmap GrayScale(Bitmap original)
    {
        var bitmapX = original.Size.Width;
        var bitmapY = original.Size.Height;
        var bitmapCopy = new Bitmap(bitmapX, bitmapY, PixelFormat.Format24bppRgb);
        
        //copy the original bitmap to the new bitmap
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

        Parallel.For(0, bitmapX, x =>
        {
            for (int y = 0; y < bitmapY; y++)
            {
                var index = (y * bitmapData.Stride) + (x * bytesPerPixel);
                int r = pixels[index + 2];
                int g = pixels[index + 1];
                int b = pixels[index];

                int gray = (r + g + b) / 3;

                pixels[index] = (byte)gray;
                pixels[index + 1] = (byte)gray;
                pixels[index + 2] = (byte)gray;
            }
        });

        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
        bitmapCopy.UnlockBits(bitmapData);
        return bitmapCopy;
    }

    static private Bitmap LoadFromPath(string path)
    {
        if (File.Exists(path))
        {
            return new Bitmap(path);
        }
        return new Bitmap(0,0);
    }

}
