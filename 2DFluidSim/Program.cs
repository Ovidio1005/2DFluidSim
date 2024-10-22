using _2DFluidSim.Fluid;
using _2DFluidSim.Render;
using System.Numerics;
using System.Text;

namespace _2DFluidSim;

internal class Program {
    static float T = 0;

    static void Main(string[] args) {
        FluidBox box = new(2, 4, 0.04f);

        FluidRenderer renderer = new(@"C:\Users\Ovidiu\Desktop\tmp", "fluidtest.mp4", 1.5f, 30, 4, null);
        box.AddFluid(Vector2.Zero, new(box.Width, 3));
        BitmapPointRenderer imageRenderer = new(new(-0.2f, -0.2f), new(2.2f, 4.2f), 1080);
        imageRenderer.ParticleDensity = 1 / box.ParticleRadius;
        imageRenderer.BoxBLCorner = new(0, 0);
        imageRenderer.BoxTRCorner = new(2, 4);
        renderer.Render(box, imageRenderer, true, (progress) => {
            if((progress * 100) - ((int) (progress * 100)) != 0) return;

            StringBuilder sb = new();
            sb.Append('[');
            int amount = (int) (progress * 20);
            for(int i = 0; i < amount; i++) sb.Append("-");
            for(int i = 0; i < 20 - amount; i++) sb.Append(".");

            sb.Append($"] {(int) (progress * 100)}%");

            Console.Write("\r                              \r");
            Console.Write(sb.ToString());
        });
    }
}
