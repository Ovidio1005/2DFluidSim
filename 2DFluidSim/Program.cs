using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Reflection;
using System.Text;
using _2DFluidSim.Fields;
using _2DFluidSim.Fluid;
using _2DFluidSim.Render;
using static _2DFluidSim.Render.FluidRenderer;

namespace _2DFluidSim;

internal class Program {
    static float T = 0;

    static void Main(string[] args) {
        FluidBox box = new(5, 10, 30);

        FluidRenderer renderer = new(@"C:\Users\Ovidiu\Desktop\tmp", "fluidtest.mp4", 5, 30, 1, null);
        box.AddFluid(new(box.WallRadius, box.WallRadius), new(box.Width - box.WallRadius, 8));
        BitmapPointRenderer imageRenderer = new(new(-0.5f, -0.5f), new(5.5f, 10.5f), 1440);
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

    class MyAction : IFluidAction {
        const float SPAWN_INTERVAL = 4;
        float LastInvokeTime = float.MinValue;

        public void Invoke(FluidBox box) {
            if(box.SimulationTime > LastInvokeTime + SPAWN_INTERVAL) {
                LastInvokeTime = box.SimulationTime;
                box.AddFluid(new(0.5f, 2), new(3, 4.5f));
            }

            box.RemoveFluid(new(9, 0), new(10, 1));
        }
    }
}
