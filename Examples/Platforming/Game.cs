using System.Xml;

namespace Carpet.Examples.Platforming
{
    public sealed class Game : Engine
    {
        private const int MaxLights = 100;
        private const float BlockSize = 8f;

        private readonly StateMachine<GlobalState> _stateMachine = new();

        private Thread _contentLoadingThread;

        private Font _font;
        private bool _regenerateGroups;

        public Game(double updateFrequency, double drawFrequency, string name) : base(updateFrequency, drawFrequency, name)
        {
            _stateMachine.SetState(GlobalState.Loading, UpdateContentLoading, null);
            _stateMachine.SetState(GlobalState.Game, UpdateGame, DrawGame, StartGame, null);
        }

        public static Layer ForegroundLayer { get; private set; }
        public static Layer BackgroundLayer { get; private set; }
        public static Layer UILayer { get; private set; }

        public static Pipeline Pipeline { get; private set; }

        public static LayerPass ForegroundPass { get; private set; }
        public static LayerPass UIPass { get; private set; }

        public static TexturePass OcclusionPass { get; private set; }
        public static TexturePass DistancePass { get; private set; }
        public static TexturePass LightPass { get; private set; }

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
            Content.Initialize("Content");
            Content.LoadGraphics(TextureLoadMode.SaveAtlas);
            Graphics.Initialize();
            ShaderProgramWrapper.InitializeUniforms(GetType().Assembly);

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

            ForegroundLayer.Camera.SetCenterOrigin();

            UILayer = new Layer(320, 180, 3f);

            Pipeline = new Pipeline();

            OcclusionPass = GPULightRenderer.CreateOcclusionPass(320, 180, MaxLights);
            DistancePass = GPULightRenderer.CreateDistancePass(256, 100);
            LightPass = GPULightRenderer.CreateLightPass(320, 180);

            Texture2D lightMap = LightPass.GetTexture();
            foregroundMaterial.Textures[0] = lightMap;

            Pipeline.AddPass(OcclusionPass);
            Pipeline.AddPass(DistancePass);
            Pipeline.AddPass(LightPass);

            ForegroundPass = Pipeline.AddPass(new LayerPass().SetLayer(ForegroundLayer));
            testLayer.Camera.SetCenterOrigin();

            UIPass = Pipeline.AddPass(new LayerPass().SetLayer(UILayer));
            UIPass.Render += RenderUI;

            DebugConsole.Initialize(UILayer, "testFont");

            Argument.AddType<EntityArgument>();
            DebugConsole.AddCommandContainer(typeof(Commands));

            _font = new Font("testFont")
            {
                OutlineWidth = 1,
                OutlineColor = Color.Black,
                BottomMargin = 2f
            };
        }

        protected override void LoadContent()
        {
            _contentLoadingThread = new Thread(() =>
            {
                base.LoadContent();
            });

            _stateMachine.ForceState(GlobalState.Loading);

            _contentLoadingThread.Start();
        }

        private void UpdateGame(float deltaTime)
        {
            Controls.Profile.Update(deltaTime);
            DebugConsole.Update(deltaTime);

            CurrentScene.GetEntityOfType<Player>().Awake = !DebugConsole.Open;

            if (Input.IsPressed(Key.Escape))
                Paused = !Paused;

            if (!Paused)
            {
                CurrentScene.Update(deltaTime);

                if (_regenerateGroups)
                {
                    CurrentScene.GetSystem<BlockGroupsSystem>().Initialize();
                    CurrentScene.Update(0f);

                    _regenerateGroups = false;
                }

                Vector2 position = (ForegroundLayer.MousePosition / BlockSize).Floor() * 
                    BlockSize + new Vector2(BlockSize / 2f);
                Block existingBlock = CurrentScene.CheckPosition<Block>(position);

                if (Input.IsMousePressed(MouseButton.Middle) || Input.IsPressed(Key.Space))
                {
                    if (existingBlock is not null)
                    {
                        CurrentScene.Remove(existingBlock);
                        
                        RemoveGroups();
                    }
                }
                else
                {
                    bool leftPressed = Input.IsMousePressed(MouseButton.Left);
                    bool rightPressed = Input.IsMousePressed(MouseButton.Right);

                    if (existingBlock is null && (leftPressed || rightPressed)) 
                    {
                        Block block;

                        if (leftPressed)
                            block = new DefaultBlock(position.X, position.Y);
                        else
                            block = new AnchorBlock(position.X, position.Y);

                        CurrentScene.Add(block);

                        RemoveGroups();
                    }
                }
            }
        }

        private void RemoveGroups()
        {
            foreach (BlockGroup group in CurrentScene.GetEntitiesOfType<BlockGroup>(null))
                CurrentScene.Remove(group);

            foreach (AutoTile tile in CurrentScene.GetEntitiesOfType<AutoTile>(null))
                tile.NeedsRefresh = true;

            _regenerateGroups = true;
        }

        private void DrawGame()
        {
            Pipeline.Process();
        }

        private void StartGame()
        {
            CurrentScene = new DemoScene(100f);

            CurrentScene.AddSystem(new CameraSystem(0.12f, CurrentScene))
                .AddSystem(new GPULightRenderer(CurrentScene, OcclusionPass,
                    DistancePass, LightPass, MaxLights, false, ForegroundPass))
                .AddSystem(new BlockGroupsSystem(CurrentScene))
                .AddSystem(new ParticleSystem(CurrentScene, 1000));

            CurrentScene.Add(new Player(300f, 30f));

            for (int i = 0; i < 80; i++)
                CurrentScene.Add(new DefaultBlock(68f + i * 8f, 100));

            CurrentScene.Add(new DefaultBlock(340f, 76f));
            CurrentScene.Add(new DefaultBlock(348f, 76f));
            CurrentScene.Add(new AnchorBlock(436f, -20f));
            CurrentScene.Add(new AnchorBlock(436f, -12f));
            CurrentScene.Add(new DefaultBlock(388f, -20f));
            CurrentScene.Add(new DefaultBlock(380f, -20f));
            CurrentScene.Add(new DefaultBlock(348f, -36f));
            CurrentScene.Add(new DefaultBlock(412f, -68f));
            CurrentScene.Add(new DefaultBlock(420f, -68f));
            CurrentScene.Add(new DefaultBlock(348f, -44f));
            CurrentScene.Add(new DefaultBlock(348f, -52f));
            CurrentScene.Add(new DefaultBlock(348f, -76f));
            CurrentScene.Add(new DefaultBlock(348f, -68f));
            CurrentScene.Add(new DefaultBlock(348f, -60f));
            CurrentScene.Add(new DefaultBlock(348f, -84f));
            CurrentScene.Add(new DefaultBlock(348f, -92f));
            CurrentScene.Add(new DefaultBlock(404f, -68f));
            CurrentScene.Add(new DefaultBlock(428f, -68f));
            CurrentScene.Add(new DefaultBlock(436f, -68f));
            CurrentScene.Add(new DefaultBlock(436f, -60f));
            CurrentScene.Add(new DefaultBlock(436f, -52f));
            CurrentScene.Add(new DefaultBlock(436f, -44f));
            CurrentScene.Add(new AnchorBlock(436f, -28f));
            CurrentScene.Add(new DefaultBlock(572f, -12f));
            CurrentScene.Add(new DefaultBlock(548f, -44f));
            CurrentScene.Add(new DefaultBlock(540f, -44f));
            CurrentScene.Add(new DefaultBlock(532f, -44f));
            CurrentScene.Add(new DefaultBlock(620f, -12f));
            CurrentScene.Add(new AnchorBlock(580f, -76f));
            CurrentScene.Add(new DefaultBlock(596f, -12f));
            CurrentScene.Add(new AnchorBlock(580f, -100f));
            CurrentScene.Add(new DefaultBlock(580f, -12f));
            CurrentScene.Add(new AnchorBlock(580f, -92f));
            CurrentScene.Add(new AnchorBlock(580f, -84f));
            CurrentScene.Add(new DefaultBlock(588f, -12f));
            CurrentScene.Add(new DefaultBlock(620f, -4f));
            CurrentScene.Add(new DefaultBlock(596f, -4f));
            CurrentScene.Add(new DefaultBlock(620f, 4f));
            CurrentScene.Add(new DefaultBlock(596f, 12f));
            CurrentScene.Add(new DefaultBlock(596f, 4f));
            CurrentScene.Add(new DefaultBlock(620f, 12f));
            CurrentScene.Add(new DefaultBlock(620f, -20f));
            CurrentScene.Add(new DefaultBlock(620f, -28f));
            CurrentScene.Add(new DefaultBlock(620f, -36f));
            CurrentScene.Add(new DefaultBlock(620f, -44f));
            CurrentScene.Add(new DefaultBlock(612f, -44f));
            CurrentScene.Add(new DefaultBlock(604f, -44f));
            CurrentScene.Add(new DefaultBlock(604f, 60f));
            CurrentScene.Add(new DefaultBlock(564f, -12f));
            CurrentScene.Add(new DefaultBlock(596f, -44f));
            CurrentScene.Add(new DefaultBlock(612f, 60f));
            CurrentScene.Add(new DefaultBlock(596f, 60f));
            CurrentScene.Add(new DefaultBlock(620f, 60f));

            CurrentScene.Add(new Coin(608f, 41f));

            CurrentScene.Initialize();

            ForegroundLayer.Camera.Position = new Vector2(400f, 50f);
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            XmlDocument document = Utilities.LoadXML(Content.Folder + "/defaultInputSettings.xml");
            var settings = new InputSettings(document.DocumentElement);
            var profile = new InputProfile(settings);

            Controls.Profile = profile;

            _stateMachine.ForceState(GlobalState.Game);
        }

        private void RenderUI()
        {
            DebugConsole.Draw();

            Vector2 mousePosition = ForegroundLayer.MousePosition;
            Vector2 position = ((mousePosition / BlockSize).Floor() * BlockSize
                - ForegroundLayer.Camera.Position + ForegroundLayer.Size / 2f).Ceiling();

            Graphics.DrawRectangle(new Rectangle(position, position + new Vector2(BlockSize)),
                    Color.Red, -1f);

            _font.Draw("WASD = move. C = jump. V = grapple\n" + 
                    "Mouse 1 = block. Mouse 2 = anchor block\n"+
                    "Mouse wheel = remove block",
                    new Vector2(10f),
                    Color.White, -1f);
        }
    }
}
