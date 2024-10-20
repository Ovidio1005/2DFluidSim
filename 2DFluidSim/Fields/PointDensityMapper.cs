using System.Numerics;

namespace _2DFluidSim.Fields;
internal class PointDensityMapper {
    public int ResolutionX;
    public int ResolutionY;
    public float Step;

    public Vector2 Center = Vector2.Zero;
    public Vector2 Start {
        get => Center - new Vector2(Step * ResolutionX / 2, Step * ResolutionY / 2);
    }
    public Vector2 End {
        get => Center + new Vector2(Step * ResolutionX / 2, Step * ResolutionY / 2);
    }

    /// <summary>
    /// In points per (square) pixel
    /// </summary>
    public float ExpectedDensity = 1;

    public PointDensityMapper(int resolutionX, int resolutionY, float step) {
        ResolutionX = resolutionX;
        ResolutionY = resolutionY;
        Step = step;
    }
    /// <summary>
    /// <paramref name="expectedDensity"/> is in points per (square) pixel
    /// </summary>
    /// <param name="expectedDensity">In points per (square) pixel</param>
    public PointDensityMapper(int resolutionX, int resolutionY, float step, Vector2 center, float expectedDensity) : this(resolutionX, resolutionY, step) {
        Center = center;
        ExpectedDensity = expectedDensity;
    }

    public (int X, int Y) Pixel(Vector2 point) {
        Vector2 translated = point - Start;
        return ((int) (translated.X / Step), (int) (translated.Y / Step));
    }

    public float[,] Map(Vector2[] points) {
        int[,] counts = new int[ResolutionX, ResolutionY];

        void addCount(int x, int y, int amount) {
            if(x >= 0 && x < ResolutionX && y >= 0 && y < ResolutionY) counts[x, y] += amount;
        }

        foreach(Vector2 point in points) {
            (int x, int y) = Pixel(point);

            addCount(x, y, 4);
            addCount(x + 1, y, 2);
            addCount(x - 1, y, 2);
            addCount(x, y + 1, 2);
            addCount(x, y - 1, 2);
            addCount(x + 1, y + 1, 1);
            addCount(x + 1, y - 1, 1);
            addCount(x - 1, y + 1, 1);
            addCount(x - 1, y - 1, 1);
        }

        float[,] map = new float[ResolutionX, ResolutionY];
        for(int x = 0; x < ResolutionX; x++) {
            for(int y = 0; y < ResolutionY; y++) {
                map[x, y] = counts[x, y] / (ExpectedDensity * 16);
            }
        }

        return map;
    }
}
