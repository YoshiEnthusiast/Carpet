using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using StbImageSharp;
using StbImageWriteSharp;
using System;

namespace SlowAndReverb
{
    public abstract class Engine
    {
        private readonly string _name;

        private readonly double _updateFrequency;
        private readonly double _drawFrequency;

        private readonly float _deltaTimeMultiplier = 50f;

        private GameWindow _window;

        public Engine(double updateFrequency, double drawFrequency, string name)
        {
            _name = name;

            _updateFrequency = updateFrequency;
            _drawFrequency = drawFrequency;

            Instance = this;
        }

        public static Engine Instance { get; private set; }

        public static float DeltaTime { get; private set; }
        public static float UpdatesPerSecond { get; private set; }
        public static double TimeElapsed { get; private set; }

        public static float TimeMultiplier { get; set; } = 1f;
        public static bool DebugCollition { get; set; }
        public static bool DebugLighting { get; set; } 

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

            StbImage.stbi_set_flip_vertically_on_load(1);
            StbImageWrite.stbi_flip_vertically_on_write(1);

            Resolution.Initialize(_window);
            Resolution.SetCurrent(initialResolution);

            OpenGL.Initialize(_window.Context);
            SFX.Initialize(null);
            Input.Initialize(_window);

            Material.InitializeUniforms();
            Content.Initialize("Content");
            Content.LoadGraphics(TextureLoadMode.SaveAtlas);
            Graphics.Initialize();

            OnInitialize();

            _window.VSync = VSyncMode.On;
            _window.Load += LoadContent;
            _window.UpdateFrame += OnUpdate;
            _window.RenderFrame += OnDraw;

            _window.Run();
        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual void LoadContent()
        {
            Content.Load();
        }

        protected abstract void Update(float deltaTime);

        protected abstract void Draw();

        private void OnUpdate(FrameEventArgs args)
        {
            double time = args.Time;

            UpdatesPerSecond = 1f / (float)args.Time;
            DeltaTime = (float)time * TimeMultiplier * _deltaTimeMultiplier;
            TimeElapsed += time;

            Update(DeltaTime);

            Input.Update();
        }

        private void OnDraw(FrameEventArgs args)
        {
            Draw();

            Graphics.DrawLayers();
            OpenGL.SwapBuffers();
        }
    }
}
