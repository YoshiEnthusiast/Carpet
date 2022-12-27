using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace SlowAndReverb
{
    public abstract class Engine
    {
        private static Engine s_instance;

        private static float s_deltaTime;
        private static float s_updatesPerSecond;

        private readonly string _name;

        private readonly double _updateFrequency;
        private readonly double _drawFrequency;

        private GameWindow _window;

        public Engine(double updateFrequency, double drawFrequency, string name)
        {
            _name = name;

            _updateFrequency = updateFrequency;
            _drawFrequency = drawFrequency;

            s_instance = this;
        }

        public static Engine Instance => s_instance;

        public static float DeltaTime => s_deltaTime;
        public static float UpdatesPerSecond => s_updatesPerSecond;

        public static float TimeMultiplier { get; set; } = 1f;

        internal virtual void Run(Resolution initialResolution, TextureLoadMode textureMode)
        {
            var settings = new GameWindowSettings()
            {
                UpdateFrequency = _updateFrequency,
                RenderFrequency = _drawFrequency
            };

            var nativeSettings = new NativeWindowSettings()
            {
                Title = _name,
                WindowBorder = WindowBorder.Fixed, 
                Size = new Vector2i(initialResolution.Width, initialResolution.Height)
            };

            _window = new GameWindow(settings, nativeSettings);

            Resolution.Initialize(_window);
            Resolution.SetCurrent(initialResolution);

            OpenGL.Initialize(_window.Context);
            SFX.Initialize(null);
            Input.Initialize(_window);

            Content.LoadGraphics(textureMode);
            Graphics.Initialize();

            _window.Load += LoadContent;
            _window.UpdateFrame += OnUpdate;
            _window.RenderFrame += OnDraw;

            _window.Run();
        }

        protected virtual void LoadContent()
        {
            Content.Load();
        }

        protected abstract void Update(float deltaTime);

        protected abstract void Draw();

        private void OnUpdate(FrameEventArgs args)
        {
            s_updatesPerSecond = 1f / (float)args.Time;
            s_deltaTime = s_updatesPerSecond / (float)_updateFrequency * TimeMultiplier;

            Update(s_deltaTime);
        }

        private void OnDraw(FrameEventArgs args)
        {
            Draw();

            Graphics.DrawLayers();
            OpenGL.SwapBuffers();
        }
    }
}
