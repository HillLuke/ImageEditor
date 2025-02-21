using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

public static class StaticHelpers
{
    static public T TimeFunction<T>(Func<T> function, string name)
    {
        Console.WriteLine($"Starting function: {name}");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = function();
        stopwatch.Stop();
        Console.WriteLine($"Finished function: {name} Time: {stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    static public T TimeFunction<T>(Func<T> function, string name, int bestOf)
    {
        Console.WriteLine($"Starting function: {name}");
        long bestTime = long.MaxValue;
        T bestResult = default;

        for (int i = 0; i < bestOf; i++)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = function();
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds < bestTime)
            {
                bestTime = stopwatch.ElapsedMilliseconds;
                bestResult = result;
            }
        }

        Console.WriteLine($"{name} Best Time: {bestTime}ms");
        return bestResult;
    }

    static public Bitmap LoadFromPath(string path)
    {
        if (File.Exists(path))
        {
            return new Bitmap(path);
        }
        return new Bitmap(0,0);
    }

    static public void SaveImage(Bitmap imageBitmap, string name)
    {
        imageBitmap.Save($"{name}.jpg", ImageFormat.Jpeg);
    }
}