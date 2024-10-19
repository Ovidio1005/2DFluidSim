using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _2DFluidSim.Fields;
internal class PointDensityMapper
{
    public int ResolutionX;
    public int ResolutionY;
    public float Step;

    public Vector2 Center = Vector2.Zero;
    public Vector2 Start
    {
        get => Center - new Vector2(Step * ResolutionX / 2, Step * ResolutionY / 2);
    }
    public Vector2 End
    {
        get => Center + new Vector2(Step * ResolutionX / 2, Step * ResolutionY / 2);
    }

    /// <summary>
    /// In points per (square) pixel
    /// </summary>
    public float ExpectedDensity = 1;

    public PointDensityMapper(int resolutionX, int resolutionY, float step)
    {
        ResolutionX = resolutionX;
        ResolutionY = resolutionY;
        Step = step;
    }
    /// <summary>
    /// <paramref name="expectedDensity"/> is in points per (square) pixel
    /// </summary>
    /// <param name="expectedDensity">In points per (square) pixel</param>
    public PointDensityMapper(int resolutionX, int resolutionY, float step, Vector2 center, float expectedDensity) : this(resolutionX, resolutionY, step)
    {
        Center = center;
        ExpectedDensity = expectedDensity;
    }

    public (int X, int Y) Pixel(Vector2 point)
    {
        Vector2 translated = point - Start;
        return ((int)(translated.X / Step), (int)(translated.Y / Step));
    }

    public float[,] Map(Vector2[] points)
    {
        //Console.WriteLine($"DEBUG: Map(Vector2[] points) called, points.Length = {points.Length}");
        int[,] counts = new int[ResolutionX, ResolutionY];
        foreach (Vector2 point in points)
        {
            (int x, int y) = Pixel(point);
            //Console.WriteLine($"DEBUG: point at ({point.X:F2}, {point.Y:F2}), pixel is ({x}, {y})");

            if (x >= 0 && x < ResolutionX && y >= 0 && y < ResolutionY) counts[x, y]++;
        }

        float[,] map = new float[ResolutionX, ResolutionY];
        for (int x = 0; x < ResolutionX; x++)
        {
            for (int y = 0; y < ResolutionY; y++)
            {
                //Console.WriteLine($"DEBUG: pixel at ({x}, {y}) has count {counts[x, y]}, density is {counts[x, y] / ExpectedDensity}");
                map[x, y] = counts[x, y] / ExpectedDensity;
            }
        }

        return map;
    }
}
