using _2DFluidSim.Fluid;
using _2DFluidSim.Util;
using System.Drawing;

namespace _2DFluidSim.Render;
internal class FluidRenderer {
    string OutputFolder;
    string OutputFilename;
    public float SimulationDuration;
    public int FPS;
    public int StepsPerFrame;
    IFluidAction? StepCallback;

    public FluidRenderer(string outputFolder, string outputFilename, float simulationDuration, int fps = 30, int stepsPerFrame = 4, IFluidAction? stepCallback = null) {
        OutputFolder = outputFolder;
        OutputFilename = outputFilename;
        SimulationDuration = simulationDuration;
        FPS = fps;
        StepsPerFrame = stepsPerFrame;
        StepCallback = stepCallback;
    }

    public void Render(FluidBox box, BitmapPointRenderer imageRenderer, bool video = true, Action<float>? progressHandler = null) {
        box.TimeStep = 1f / (FPS * StepsPerFrame);
        int frames = (int) (SimulationDuration * FPS);

        string outFolder = OutputFolder.EndsWith('\\') ? OutputFolder : OutputFolder + "\\";
        string outName = OutputFilename.Split('.')[0];
        string outExtension = OutputFilename.Split(".").Length > 1 ? OutputFilename.Split(".")[1] : (video ? "mp4" : "png");

        string imagesFolder = video ? $"{outFolder}tmp{{{Guid.NewGuid()}}}\\" : outFolder;
        string imagesExtension = video ? "png" : outExtension;

        if(video) Directory.CreateDirectory(imagesFolder);

        for(int i = 0; i < frames; i++) {
            for(int j = 0; j < StepsPerFrame; j++) {
                box.Step();
                StepCallback?.Invoke(box);
                progressHandler?.Invoke(((i * StepsPerFrame) + j + 1) / (float) (frames * StepsPerFrame));
            }

            Bitmap image = imageRenderer.Render(box.GetParticles().Select(p => p.Position).ToArray());
            image.Save($"{imagesFolder}{outName}{ZeroPad(i, 5)}.{imagesExtension}");
        }

        if(video) {
            VideoMaker.MakeVideo($"{imagesFolder}{outName}%05d.{imagesExtension}", $"{outFolder}{outName}.{outExtension}", FPS);
            Directory.Delete(imagesFolder, true);
        }
    }

    private static string ZeroPad(int number, int length) {
        string str = number.ToString();
        while(str.Length < length) str = "0" + str;
        return str;
    }

    internal interface IFluidAction {
        public abstract void Invoke(FluidBox box);
    }
}
