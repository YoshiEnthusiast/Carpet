global using Vector2GL = OpenTK.Mathematics.Vector2;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using StbImageSharp;
using StbImageWriteSharp;

namespace SlowAndReverb
{
    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            Resolution.Initialize();
            Resolution resolution = Resolution.Current;

            var settings = new GameWindowSettings()
            {
                UpdateFrequency = 60d,
                RenderFrequency = 60d
            };

            var nativeSettings = new NativeWindowSettings()
            {
                Title = "Slow and reverb",
                Size = new Vector2i(resolution.Width, resolution.Height),
                WindowBorder = WindowBorder.Fixed
            };

            new Window(settings, nativeSettings).Run();
        }
    }
}
