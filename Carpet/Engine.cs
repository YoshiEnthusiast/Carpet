global using Vector2GL = OpenTK.Mathematics.Vector2;
global using SystemRandom = System.Random;
global using ComputeItem = Carpet.Std430LayoutItem;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using StbImageSharp;
using StbImageWriteSharp;
using System;
using System.Threading;
using System.Diagnostics;

// GLOBAL TASK LIST

// TODO: Scene methods that return multiple entities should accept lists
// TODO: Make maaaany things internal
// TODO: Gamepad support fix
// TODO: Redo content loading
// TODO: make u_Resolution the default uniform??

namespace Carpet
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

        public static float UpdateTime { get; private set; }
        public static float RenderTime { get; private set; }

        public static float TimeMultiplier { get; set; } = 1f;
        public static bool DebugCollision { get; set; }
        public static bool DebugLighting { get; set; }

        public static float TimeElapsedFloat => (float)TimeElapsed;

        public virtual void Run(Resolution initialResolution, TextureLoadMode textureMode)
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

            _window = new GameWindow(settings, nativeSettings)
            {
                VSync = VSyncMode.Off
            };

            StbImage.stbi_set_flip_vertically_on_load(1);
            StbImageWrite.stbi_flip_vertically_on_write(1);

            Resolution.Initialize(_window);
            Resolution.SetCurrent(initialResolution);

            OpenGL.Initialize(_window.Context);
            SFX.Initialize(null);

            ShaderProgramWrapper.InitializeUniforms();
            Input.Initialize(_window);

            OnInitialize();

            _window.VSync = VSyncMode.Off;
            _window.CursorState = CursorState.Hidden;
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
            // TODO: delta time is currently constant
            double time = 1f / _updateFrequency;

            UpdatesPerSecond = 1f / (float)time;
            DeltaTime = (float)time * TimeMultiplier * _deltaTimeMultiplier;
            TimeElapsed += time;

            SFX.Update();

            Update(DeltaTime);
        }

        private void OnDraw(FrameEventArgs args)
        {
            Draw();

            Graphics.DrawLayers();
            _window.SwapBuffers(); 
        }
    }
}
