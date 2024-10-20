using _2DFluidSim.Fluid;
using _2DFluidSim.Render;
using System.Text;
using static _2DFluidSim.Render.FluidRenderer;

namespace _2DFluidSim;

internal class Program {
    static float T = 0;

    static void Main(string[] args) {
        FluidBox box = new(5, 10, 20);

        FluidRenderer renderer = new(@"C:\Users\Ovidiu\Desktop\tmp", "fluidtest.mp4", 15, 60, 2, null);
        box.AddFluid(new(box.WallRadius, box.WallRadius), new(box.Width - box.WallRadius, 8));
        BitmapPointRenderer imageRenderer = new(new(-0.5f, -0.5f), new(5.5f, 10.5f), 1080);
        imageRenderer.ParticleDensity = box.ParticleDensity;
        imageRenderer.BoxBLCorner = new(0, 0);
        imageRenderer.BoxTRCorner = new(5, 10);
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
