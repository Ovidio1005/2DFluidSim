using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using _2DFluidSim.Fields;
using _2DFluidSim.Fluid;

namespace _2DFluidSim.Render;
internal class DensityMapRenderer : BitmapPointRenderer
{
    private float Width = 0, Height = 0;
    private int PixelWidth = 0, PixelHeight = 0;
    private float BoxParticleDensity = 0;
    public DensityMapRenderer(Vector2 bLCorner, Vector2 tRCorner, int imageHeight) : base(bLCorner, tRCorner, imageHeight) { }

    public void SetParams(FluidBox box)
    {
        Width = box.Width;
        Height = box.Height;
        BoxParticleDensity = box.ParticleDensity;

        float pMapStep = FluidBox.PARTICLES_PER_PIXEL_LENGTH / ParticleDensity;
        PixelWidth = (int)(Width / pMapStep);
        PixelHeight = (int)(Height / pMapStep);
    }

    public override Bitmap Render(Vector2[] points)
    {
        Bitmap result = new(PixelWidth, PixelHeight);
        PointDensityMapper mapper = new(PixelWidth, PixelHeight, FluidBox.PARTICLES_PER_PIXEL_LENGTH / BoxParticleDensity, new(Width / 2, Height / 2), FluidBox.PARTICLES_PER_PIXEL_LENGTH * FluidBox.PARTICLES_PER_PIXEL_LENGTH);
        float[,] pressureMap = mapper.Map(points);

        for (int x = 0; x < PixelWidth; x++)
        {
            for (int y = 0; y < PixelHeight; y++)
            {
                float pressureNorm = (pressureMap[x, y] - MinPressure) / (MaxPressure - MinPressure);
                if (pressureNorm < 0) pressureNorm = 0;
                if (pressureNorm > 1) pressureNorm = 1;

                float hue = 240 * (1 - pressureNorm);
                (int r, int g, int b) = HsvToRgb(hue, 1, 1);
                result.SetPixel(x, PixelHeight - y - 1, Color.FromArgb(r, g, b));
            }
        }

        return result;
    }
}
