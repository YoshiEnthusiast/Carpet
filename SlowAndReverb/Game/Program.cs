global using Vector2GL = OpenTK.Mathematics.Vector2;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Linq;
using System.Threading;

namespace SlowAndReverb
{
    internal sealed class Program
    {
        private static UntitledGame s_game;

        public static UntitledGame Game => s_game;

        private static void Main(string[] args)
        {
            // temporary
            Resolution initialREsolution = Resolution.SupportedResolutions.First();

            s_game = new UntitledGame(60d, 60d, "Untitled game");

            s_game.Run(initialREsolution, TextureLoadMode.LoadAtlas);

            return;

            Resolution resolution = Resolution.SupportedResolutions.First();

            var settings = new GameWindowSettings()
            {
                UpdateFrequency = 60d,
                RenderFrequency = 60d
            };

            var nativeSettings = new NativeWindowSettings()
            {
                Title = "Slow and reverb",
                WindowBorder = WindowBorder.Fixed,
                Size = new Vector2i(resolution.Width, resolution.Height)
            };

            var window = new OldDebugWindow(settings, nativeSettings);

            Resolution.Initialize(window);
            Resolution.SetCurrent(resolution);

            window.Run();
        }
    }
}
