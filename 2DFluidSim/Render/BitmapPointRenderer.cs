using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using _2DFluidSim.Fields;

namespace _2DFluidSim.Render;
internal class BitmapPointRenderer
{
    public const float PRESSURE_RESOLUTION_DOWNSCALE = 4;
    public Vector2 BLCorner;
    public Vector2 TRCorner;

    public Vector2? BoxBLCorner;
    public Vector2? BoxTRCorner;

    public Color BGColor = Color.White;
    public Color LineColor = Color.Black;
    public float MinPressure = 0;
    public float MaxPressure = 5;
    public float ParticleDensity = 100;

    public int ResolutionX
    {
        get => (int)(ResolutionY * ((TRCorner.X - BLCorner.X) / (TRCorner.Y - BLCorner.Y)));
    }
    public int ResolutionY;

    private float Step
    {
        get => (TRCorner.Y - BLCorner.Y) / ResolutionY;
    }

    private PointDensityMapper Mapper;

    public BitmapPointRenderer(Vector2 bLCorner, Vector2 tRCorner, int imageHeight)
    {
        BLCorner = bLCorner;
        TRCorner = tRCorner;
        ResolutionY = imageHeight;

        Mapper = new(
            (int)(ResolutionX / PRESSURE_RESOLUTION_DOWNSCALE),
            (int)(ResolutionY / PRESSURE_RESOLUTION_DOWNSCALE),
            Step * PRESSURE_RESOLUTION_DOWNSCALE,
            (BLCorner + TRCorner) / 2,
            (float)Math.Pow(Step * PRESSURE_RESOLUTION_DOWNSCALE * ParticleDensity, 2)
        );
    }

    public virtual Bitmap Render(Vector2[] points)
    {
        Bitmap result = new(ResolutionX, ResolutionY);
        Mapper.ExpectedDensity = (float)Math.Pow(Step * PRESSURE_RESOLUTION_DOWNSCALE * ParticleDensity, 2); // In case it got changed

        float[,] pressureMap = Mapper.Map(points);

        for (int x = 0; x < ResolutionX; x++)
        {
            for (int y = 0; y < ResolutionY; y++)
            {
                result.SetPixel(x, y, BGColor);
            }
        }

        foreach (Vector2 point in points)
        {
            float pressure = pressureMap[Mapper.Pixel(point).X, Mapper.Pixel(point).Y];
            float huePercent = (pressure - MinPressure) / (MaxPressure - MinPressure);
            if (huePercent < 0) huePercent = 0;
            if (huePercent > 1) huePercent = 1;
            float hue = 120 * (1 - huePercent);
            (int r, int g, int b) = HsvToRgb(hue, 1, 1);
            Color pointColor = Color.FromArgb(r, g, b);

            (int x, int y) = Pixel(point);
            TryDrawPoint(pointColor, x, y, result);
        }

        if (BoxBLCorner is not null && BoxTRCorner is not null)
        {
            Vector2 boxBRCorner = new(BoxTRCorner.Value.X, BoxBLCorner.Value.Y);
            Vector2 boxTLCorner = new(BoxBLCorner.Value.X, BoxTRCorner.Value.Y);

            (int blX, int blY) = Pixel(BoxBLCorner.Value);
            (int brX, int brY) = Pixel(boxBRCorner);
            (int tlX, int tlY) = Pixel(boxTLCorner);
            (int trX, int trY) = Pixel(BoxTRCorner.Value);

            DrawVerticalLine(blX, blY, tlY, result);
            DrawVerticalLine(brX, brY, trY, result);
            DrawHorizontalLine(blY, blX, brX, result);
            DrawHorizontalLine(tlY, tlX, trX, result);
        }

        return result;
    }

    private (int X, int Y) Pixel(Vector2 point)
    {
        Vector2 transformed = point - BLCorner;
        int x = (int)(transformed.X / Step);
        int y = (int)(transformed.Y / Step);
        y = ResolutionY - y - 1; // Image coordinates start at the top left, not the bottom left
        return (x, y);
    }

    private void TryDrawPoint(Color color, int x, int y, Bitmap image)
    {
        Color fullC = color;
        Color halfC = MixColors(fullC, BGColor);
        Color quartC = MixColors(halfC, BGColor);

        if (InBounds(x, y, image)) image.SetPixel(x, y, fullC);

        if (InBounds(x - 1, y, image)) image.SetPixel(x - 1, y, halfC);
        if (InBounds(x + 1, y, image)) image.SetPixel(x + 1, y, halfC);
        if (InBounds(x, y - 1, image)) image.SetPixel(x, y - 1, halfC);
        if (InBounds(x, y + 1, image)) image.SetPixel(x, y + 1, halfC);

        if (InBounds(x - 1, y - 1, image)) image.SetPixel(x - 1, y - 1, quartC);
        if (InBounds(x - 1, y + 1, image)) image.SetPixel(x - 1, y + 1, quartC);
        if (InBounds(x + 1, y - 1, image)) image.SetPixel(x + 1, y - 1, quartC);
        if (InBounds(x + 1, y + 1, image)) image.SetPixel(x + 1, y + 1, quartC);
    }

    private Color MixColors(Color c1, Color c2)
    {
        return Color.FromArgb(
            (c1.R + c2.R) / 2,
            (c1.G + c2.G) / 2,
            (c1.B + c2.B) / 2
        );
    }

    private bool InBounds(int x, int y, Bitmap image) => x >= 0 && x < image.Width && y >= 0 && y < image.Height;

    private void DrawHorizontalLine(int y, int startX, int endX, Bitmap image)
    {
        if (y < 0 || y >= image.Height) return;

        for (int x = startX; x < endX; x++)
        {
            if (x >= 0 && x < image.Width) image.SetPixel(x, y, LineColor);
        }
    }

    private void DrawVerticalLine(int x, int startY, int endY, Bitmap image)
    {
        if (x < 0 || x >= image.Width) return;

        for (int y = startY; y < endY; y++)
        {
            if (y >= 0 && y < image.Height) image.SetPixel(x, y, LineColor);
        }
    }

    /// <summary>
    /// Taken straight from https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
    /// </summary>
    protected (int R, int G, int B) HsvToRgb(double h, double S, double V)
    {
        int r, g, b;

        double H = h;
        while (H < 0) { H += 360; };
        while (H >= 360) { H -= 360; };
        double R, G, B;
        if (V <= 0) { R = G = B = 0; }
        else if (S <= 0)
        {
            R = G = B = V;
        }
        else
        {
            double hf = H / 60.0;
            int i = (int)Math.Floor(hf);
            double f = hf - i;
            double pv = V * (1 - S);
            double qv = V * (1 - S * f);
            double tv = V * (1 - S * (1 - f));
            switch (i)
            {

                // Red is the dominant color

                case 0:
                    R = V;
                    G = tv;
                    B = pv;
                    break;

                // Green is the dominant color

                case 1:
                    R = qv;
                    G = V;
                    B = pv;
                    break;
                case 2:
                    R = pv;
                    G = V;
                    B = tv;
                    break;

                // Blue is the dominant color

                case 3:
                    R = pv;
                    G = qv;
                    B = V;
                    break;
                case 4:
                    R = tv;
                    G = pv;
                    B = V;
                    break;

                // Red is the dominant color

                case 5:
                    R = V;
                    G = pv;
                    B = qv;
                    break;

                // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                case 6:
                    R = V;
                    G = tv;
                    B = pv;
                    break;
                case -1:
                    R = V;
                    G = pv;
                    B = qv;
                    break;

                // The color is not defined, we should throw an error.

                default:
                    //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                    R = G = B = V; // Just pretend its black/white
                    break;
            }
        }
        r = Clamp((int)(R * 255.0));
        g = Clamp((int)(G * 255.0));
        b = Clamp((int)(B * 255.0));

        return (r, g, b);
    }

    /// <summary>
    /// Clamp a value to 0-255
    /// </summary>
    int Clamp(int i)
    {
        if (i < 0) return 0;
        if (i > 255) return 255;
        return i;
    }
}
