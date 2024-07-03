
namespace Carpet.RayMarching
{
    public sealed class Game : Engine
    {
        private const int LoadingState = 0;
        private const int MainState = 1;
        
        private const int Width = 320;
        private const int Height = 180;

        private readonly StateMachine<int> _stateMachine = new();

        private Thread _contentLoadingThread;
        private RayMarcher _rayMarcher;
        private Font _font;

        private MouseEmitter _mouseEmitter;
        private Emitter[] _emitters;

        private int _modeIndex;

        public Game(double updateFrequency, double drawFrequency, string name) : base(updateFrequency, drawFrequency, name)
        {
            _stateMachine.SetState(LoadingState, UpdateContentLoading, null);
            _stateMachine.SetState(MainState, UpdateGame, DrawGame, StartGame, null);
        }

        public static Scene CurrentScene { get; private set; }

        public static Layer MainLayer { get; private set; }
        public static Layer UILayer { get; private set; }
        public static Pipeline Pipeline { get; private set; }

        public static TexturePass OcclusionPass { get; private set; }
        public static TexturePass IntensityPass { get; private set; }
        public static Pass DistanceFieldPass { get; private set; }
        public static TexturePass RayMarchingPass { get; private set; }
        public static LayerPass MainLayerPass { get; private set; }
        public static LayerPass UILayerPass { get; private set; }

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

            Texture2D lightMapTexture = Texture2D.CreateEmpty(Width, Height);

            var material = new SceneMaterial();
            material.Textures[0] = lightMapTexture;

            MainLayer = new Layer(Width, Height, 0)
            {
                Material = material
            };
            UILayer = new Layer(Width, Height, 1);

            Pipeline = new Pipeline();

            RenderTarget occlusionMap = RenderTarget.CreateTexture(Width, Height);
            RenderTarget intensityMap = RenderTarget.CreateTexture(Width, Height);
            RenderTarget lightMap = RenderTarget.FromTexture(lightMapTexture);
            
            OcclusionPass = new TexturePass().SetRenderTarget(occlusionMap);
            IntensityPass = new TexturePass().SetRenderTarget(intensityMap);
            DistanceFieldPass = new Pass();
            RayMarchingPass = new TexturePass().SetRenderTarget(lightMap);
            MainLayerPass = new LayerPass().SetLayer(MainLayer);
            UILayerPass = new LayerPass().SetLayer(UILayer);

            MainLayerPass.Render += DrawMainLayer;
            UILayerPass.Render += DrawUILayer;

            Pipeline.AddPass(OcclusionPass);
            Pipeline.AddPass(DistanceFieldPass);
            Pipeline.AddPass(IntensityPass);
            Pipeline.AddPass(RayMarchingPass);
            Pipeline.AddPass(MainLayerPass);
            Pipeline.AddPass(UILayerPass);

            _font = new Font("testFont")
            {
                OutlineWidth = 1,
                OutlineColor = Color.Black,
            };
        }

        protected override void LoadContent()
        {
            _contentLoadingThread = new Thread(() =>
            {
                base.LoadContent();
            });

            _stateMachine.ForceState(LoadingState);

            _contentLoadingThread.Start();
        }

        private void UpdateGame(float deltaTime)
        {
            CurrentScene.Update(deltaTime);

            if (Input.IsMousePressed(MouseButton.Left))
            {
                _modeIndex++;

                if (_modeIndex >= 5)
                    _modeIndex = 0;

                UpdateMode();
            }
            
            _rayMarcher.Time = Engine.TimeElapsedFloat;
        }

        private void UpdateMode()
        {
            switch (_modeIndex)
            {
                case 0:
                    _rayMarcher.RaysPerPixel = 20;
                    _rayMarcher.Displacement = Content.GetTexture("prism");

                    foreach (Emitter emitter in _emitters)
                        emitter.Get<RayEmitter>().Visible = true;

                    break;
                case 1:
                    _rayMarcher.RaysPerPixel = 200;
                    _rayMarcher.Displacement = null;

                    foreach (Emitter emitter in _emitters)
                        emitter.Get<RayEmitter>().Visible = false;

                    break;
                case 2:
                    _rayMarcher.RaysPerPixel = 20;
                    _rayMarcher.Displacement = Content.GetTexture("tiles");

                    foreach (Emitter emitter in _emitters)
                        emitter.Get<RayEmitter>().Visible = true;

                    break;
                case 3:
                    _rayMarcher.RaysPerPixel = 25;
                    _rayMarcher.Displacement = null;
                    break;
                case 4:
                    _rayMarcher.RaysPerPixel = 20;
                    _rayMarcher.Displacement = Content.GetTexture("cubes");

                    foreach (Emitter emitter in _emitters)
                        emitter.Get<RayEmitter>().Visible = false;

                    break;
            }
        }

        private void DrawGame()
        {
            Pipeline.Process();
        }

        private void StartGame()
        {
            CurrentScene = new Scene(100f);

            _mouseEmitter = new MouseEmitter(0f, 0f);
            CurrentScene.Add(_mouseEmitter);

            _emitters = new Emitter[3];
            _emitters[0] = new Emitter(200f, 40f)
            {
                Color = Color.LightYellow,
                Radius = 15,
                Intensity = 0.3f
            };
            _emitters[1] = new Emitter(40f, 50f)
            {
                Color = Color.Red,
                Radius = 10,
                Intensity = 0.4f
            };
            _emitters[2] = new Emitter(290f, 150f)
            {
                Color = Color.LawnGreen,
                Radius = 15,
                Intensity = 0.3f
            };

            foreach (Emitter emitter in _emitters)
                CurrentScene.Add(emitter);

            CurrentScene.Add(new Occluder(50f, 140f)
            {
                SpeedMultiplier = 0.01f
            });
            CurrentScene.Add(new Occluder(200f, 120f)
            {
                Scale = 0.7f,
                SpeedMultiplier = 0.02f
            });
            CurrentScene.Add(new Occluder(100f, 30f)
            {
                Scale = 0.5f,
                SpeedMultiplier = 0.005f
            });

            _rayMarcher = new RayMarcher(CurrentScene, OcclusionPass, 
                    IntensityPass, DistanceFieldPass, RayMarchingPass);

            CurrentScene.AddSystem(_rayMarcher);

            CurrentScene.Initialize();
            UpdateMode();
        }

        private void UpdateContentLoading(float deltaTime)
        {
            if (_contentLoadingThread.IsAlive)
                return;

            _stateMachine.ForceState(MainState);
        }
        
        private void DrawMainLayer()
        {
            CurrentScene.Draw();
        }

        private void DrawUILayer()
        {
            _font.Draw("Mouse 1 to change modes!", new Vector2(10f), Color.White, 0f);
        }
    }
}
