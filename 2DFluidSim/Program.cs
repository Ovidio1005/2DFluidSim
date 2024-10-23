using _2DFluidSim.Fluid;
using _2DFluidSim.Render;
using _2DFluidSim.Util;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace _2DFluidSim;

internal class Program {
    static int P = -1;
    static float LastProgress = 0;
    static long LastMillis = 0;
    static float AvgSpeed = 0;

    static void Main(string[] args) {
        FluidBox box = new(2, 4, 0.04f);

        //FluidRenderer renderer = new(@"C:\Users\Ovidiu\Desktop\tmp", "fluidtest.mp4", 1.5f, 30, 4, null);
        VideoMaker.FfmpegPath = @"C:\Users\Chilianu\Documents\_PROJECTS_\VisualStudioProjects\2DFluidSim\FFMPEG";
        FluidRenderer renderer = new(@"C:\Users\Chilianu\Desktop\tmp", "fluidtest.mp4", 5f, 30, 4, null);
        box.AddFluid(Vector2.Zero, new(box.Width, 3));
        BitmapPointRenderer imageRenderer = new(new(-0.2f, -0.2f), new(2.2f, 4.2f), 1080);
        imageRenderer.ParticleDensity = 1 / box.ParticleRadius;
        imageRenderer.BoxBLCorner = new(0, 0);
        imageRenderer.BoxTRCorner = new(2, 4);

        Stopwatch sw = Stopwatch.StartNew();
        renderer.Render(box, imageRenderer, true, (progress) => {
            if(box.SimulationTime >= 0.5f) box.Gravity = Vector2.Zero;

            if((int) (progress * 1000) <= P) return;
            P = (int) (progress * 100);

            long millis = sw.ElapsedMilliseconds;
            long deltaMillis = millis - LastMillis;
            LastMillis = millis;
            float deltaProgress = LastProgress - progress;
            LastProgress = progress;
            float currentSpeed = deltaProgress / deltaMillis;
            AvgSpeed = (3 * AvgSpeed + currentSpeed) / 4;
            float millisLeft = (1 - progress) / AvgSpeed;
            TimeSpan timeLeft = TimeSpan.FromMilliseconds(millisLeft);

            StringBuilder sb = new();
            sb.Append('[');
            int amount = (int) (progress * 20);
            for(int i = 0; i < amount; i++) sb.Append("-");
            for(int i = 0; i < 20 - amount; i++) sb.Append(".");

            sb.Append($"] {(int) (progress * 100)}% - remaining: {(timeLeft.Minutes < 0 ? -timeLeft.Minutes : timeLeft.Minutes)}:{(timeLeft.Seconds < 0 ? -timeLeft.Seconds : timeLeft.Seconds):D2}");

            Console.Write("\r                                                            \r");
            Console.Write(sb.ToString());
        });
    }
}
