using System;
using System.Threading;
using System.Xml;

namespace Carpet
{
    public sealed class Demo : Engine
    {
        private const int MaxLights = 100;

        private readonly StateMachine<GlobalState> _stateMachine = new();

        private Thread _contentLoadingThread;

        public Demo(double updateFrequency, double drawFrequency, string name) : base(updateFrequency, drawFrequency, name)
        {
            _stateMachine.SetState(GlobalState.Loading, UpdateContentLoading, null);
            _stateMachine.SetState(GlobalState.Game, UpdateGame, DrawGame, StartGame, null);
        }

        public static Layer ForegroundLayer { get; private set; }
        public static Layer BackgroundLayer { get; private set; }
        public static Layer ConsoleLayer { get; private set; }

        public static Pipeline Pipeline { get; private set; }

        public static LayerPass ForegroundPass { get; private set; }
        public static LayerPass ConsolePass { get; private set; }
        public static LayerPass TestPass { get; private set; }

        public static Pass OcclusionPass { get; private set; }
        public static Pass DistancePass { get; private set; }
        public static Pass LightPass { get; private set; }

        public static Scene CurrentScene { get; set; }
        public static bool Paused { get; set; }

        protected override void Update(float deltaTime)
        {
            Input.Update();

            _stateMachine.DoUpdate(deltaTime);

            Input.Clear();
        }

        protected override void Draw()
        {
            _stateMachine.DoDraw();
        }

        protected override void OnInitialize()
        {
            BackgroundLayer = new Layer(1280, 720, 0f);

            var foregroundMaterial = new ForegroundMaterial();

            ForegroundLayer = new Layer(320, 180, 1f)
            {
                Material = foregroundMaterial,
                ClearColor = new Color(40, 40, 40)
            };

            var testLayer = new Layer(520, 380, 10f)
            {
                ClearColor = Color.Black
            };

            TestPass = new LayerPass()
                .SetLayer(testLayer);

            ForegroundLayer.Camera.SetCenterOrigin();

            ConsoleLayer = new Layer(320, 180, 3f);

            Pipeline = new Pipeline();

            OcclusionPass = RayLightRenderer.CreateOcclusionPass(320, 180, MaxLights);
            DistancePass = RayLightRenderer.CreateDistancePass(256, 100);
            LightPass = RayLightRenderer.CreateLightPass(320, 180);

            Texture2D lightMap = LightPass.GetTexture();
            foregroundMaterial.Textures.Add(lightMap);

            Pipeline.AddPass(OcclusionPass);
            Pipeline.AddPass(DistancePass);
            Pipeline.AddPass(LightPass);

            ForegroundPass = Pipeline.AddPass(new LayerPass().SetLayer(ForegroundLayer));
            testLayer.Camera.SetCenterOrigin();
            TestPass.Render += Test;
            // Pipeline.AddPass(TestPass);

            ConsolePass = Pipeline.AddPass(new LayerPass().SetLayer(ConsoleLayer));
            ConsolePass.Render += OnConsoleRender;

            Editor.Initialize();

            DebugConsole.Initialize(ConsoleLayer, "testFont");

            Argument.AddType<Game.Entity>();
            DebugConsole.AddCommandContainer(typeof(TestCommands));
        }

        private void Test()
        {
            foreach (LightOccluder c in CurrentScene.GetComponentsOfType<LightOccluder>())
                c.DrawOcclusion();
        }

        protected override void LoadContent()
        {
            _contentLoadingThread = new Thread(() =>
            {
                // Track progress

                base.LoadContent();
            });

            _stateMachine.ForceState(GlobalState.Loading);

            _contentLoadingThread.Start();
        }

        private void UpdateGame(float deltaTime)
        {
            Controls.Profile.Update(deltaTime);
            DebugConsole.Update(deltaTime);


            if (Input.IsPressed(Key.Escape))
            {
                Paused = !Paused;
            }

            if (!Paused)
                CurrentScene.Update(deltaTime);
        }

        private void DrawGame()
        {
                RayTracer.Test();
            //CurrentScene.Draw();
            Pipeline.Process();
        }

        private void StartGame()
        {
            CurrentScene = new Scene();

            CurrentScene.AddSystem(new CameraSystem(0.18f, CurrentScene))
                .AddSystem(new RayLightRenderer(CurrentScene, OcclusionPass, 
                    DistancePass, LightPass, MaxLights, false, ForegroundPass))
                //.AddSystem(new LightRenderer(this, Demo.OccludeBufferPass, Demo.ShadowBufferPass,
                //    Demo.LightmapPass))
                // .AddSystem(new RayLightRenderer(CurrentScene, 256, 100, 200, ForegroundPass))
                .AddSystem(new BlockGroupsSystem(CurrentScene))
                .AddSystem(new ParticleSystem(CurrentScene, 1000))
                .AddSystem(new DebugSystem(CurrentScene));

            CurrentScene.Add(new TestEntity(0f, 0f));
            CurrentScene.Add(new TestEntity2(0f, 0f));
            //Scene.Add(new TestEntity2(180f, 90f));
            //Scene.Add(new Platform(120f, 100f));
            
            // 25
            for (int i = 0; i < 80; i++)
                CurrentScene.Add(new DefaultBlock(68f + i * 8f, 100)); // 148

            //Scene.Add(new Block("tileset", 68f, 140f));
            //Scene.Add(new Block("tileset", 68f + 8f * 25f, 140f));

            CurrentScene.Add(new Player(300f, -20f));
            CurrentScene.Add(new TestAnchor(200f, 20f));

            CurrentScene.Add(new FlyingLantern(80f, 80f));

            CurrentScene.Add(new Coin(150f, 60f));
            //CurrentScene.Add(new Coin(230f, 80f));

            CurrentScene.Add(new TestPlatform(300f, 60f));
            CurrentScene.Add(new SpinningBoxes(600f, 30f));

            //CurrentScene.Add(new Boids(0f, 0f)
            //{
            //    Size = new Vector2(600f, 300f),
            //    Position = new Vector2(400f, 20f)
            //});

            
            CurrentScene.Initialize();
            ForegroundLayer.Camera.Position = new Vector2(400f, 50f);
            //CurrentScene.GetSystem<BackgroundSystem>().CurrentBackground = new TestBackground();
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            XmlDocument document = Utilities.LoadXML(Content.Folder + "\\defaultInputSettings.xml");
            var settings = new InputSettings(document.DocumentElement);
            var profile = new InputProfile(settings);

            Controls.Profile = profile;

            _stateMachine.ForceState(GlobalState.Game);
        }

        private void OnConsoleRender()
        {
            DebugConsole.Draw();
        }
    }
}
