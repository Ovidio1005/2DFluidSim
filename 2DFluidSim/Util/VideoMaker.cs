using System.Diagnostics;

namespace _2DFluidSim.Util;
internal class VideoMaker {
    public static string FfmpegPath = @"C:\Users\Ovidiu\Documents\_PROJECTS_\VisualStudioProjects\2DFluidSim\FFMPEG";

    private static string GetExePath() => FfmpegPath.EndsWith(".exe")
        ? FfmpegPath
        : FfmpegPath.EndsWith(@"\")
        ? FfmpegPath + "ffmpeg.exe"
        : FfmpegPath + @"\ffmpeg.exe";

    public static void MakeVideo(string imagesPath, string outputPath, int framerate, Action<string?>? stdoutHandler = null, Action<string?>? sterrHandler = null) {
        // Construct the FFmpeg command
        string arguments = $"-framerate {framerate} -i \"{imagesPath}\" -vf \"crop=trunc(iw/2)*2:trunc(ih/2)*2\" -c:v libx264 -pix_fmt yuv420p \"{outputPath}\"";

        // Start the FFmpeg process
        ProcessStartInfo processStartInfo = new ProcessStartInfo {
            FileName = GetExePath(),
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using(Process process = new Process { StartInfo = processStartInfo }) {
            process.OutputDataReceived += (sender, e) => stdoutHandler?.Invoke(e.Data);
            process.ErrorDataReceived += (sender, e) => sterrHandler?.Invoke(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}
