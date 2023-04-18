using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;

namespace SlowAndReverb
{
    public sealed class OldDebugWindow : GameWindow
    {
        private readonly Texture _spikeGrenade;
        private readonly Sprite _yosh;

        private readonly Sprite _needler;
        private readonly Sprite _test;

        float _time = 0f;

        private TestMaterial _testMaterial;
        private RGBTextMaterial _textMaterial;

        private DrunkMaterial _drunkMaterial;

        public unsafe OldDebugWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings) : base(settings, nativeSettings)
        {
            OpenGL.Initialize(Context);
            SFX.Initialize(null);
            Content.LoadGraphics(TextureLoadMode.LoadAtlas);
            Content.Load();
            Graphics.Initialize();

            Input.Initialize(this);

            UpdateFrame += OnUpdate;
            RenderFrame += OnRender;

            Console.WriteLine(OpenGL.MaxTextureSize);
            Console.WriteLine(OpenGL.MaxTextureUnits);

            _yosh = new Sprite("Yosh")
            {
                Depth = 5f
            };

            //_testMaterial = new TestMaterial();

            _needler = new Sprite("needler", 19, 16)
            {
                Depth = 10f,
                //HorizontalEffect = SpriteEffect.Reflect
            };

            _needler.AddAnimation("amogus", new Animation(10, true, new int[]
            {
                4,
                3,
                2,
                1,
                0
            }));

            _needler.SetCenterOrigin();

            _needler.SetAnimation("amogus");

            _test = new Sprite("burn")
            {

            };

            _test.SetCenterOrigin();

            _layer = new Layer(320, 180, 1f)
            {
                ClearColor = new Color(80, 80, 80, 255)
            };

            _layer2 = new Layer(320, 180, 2f);

            _textMaterial = new RGBTextMaterial();

            _font = new Font("testFont")
            {
                AdditionalAdvance = 2,
                OutlineWidth = 1,
                Material = _textMaterial,
                MaxWidth = 200f,
                Multiline = true,
                BottomMargin = 2f
            };

            //_drunkMaterial = new DrunkMaterial()
            //{
            //    MaxTilt = 5
            //};

            _layer.Camera.SetCenterOrigin();
            _layer.Camera.Position = new Vector2(160, 90);

            //s = new SoundEffect("sample")
            //{
            //    Looping = false
            //};
            //s.Play();
        }

        private Font _font;

        private string a = "";

        SoundEffect s;
        private void OnUpdate(FrameEventArgs args)
        {
            float updatesPerSecond = 1f / (float)args.Time;
            float deltaTime = updatesPerSecond / (float)UpdateFrequency;

            SFX.Update();

            //if (Input.IsPressed(Key.I))
            //    s.Pause();
            //else if (Input.IsPressed(Key.O))
            //    s.Play();

            //_needler.Texture.SaveAsPng("n.png");

            //_drunkMaterial.Time += 0.1f;

            float amogus = 0.2f;
            float impostor = 0.05f;
            Camera c = _layer.Camera;

            if (Input.IsDown(Key.D))
                c.X += amogus;
            else if (Input.IsDown(Key.A))
                c.X -= amogus;

            if (Input.IsDown(Key.S))
                c.Y += amogus;
            else if (Input.IsDown(Key.W))
                c.Y -= amogus;

            if (Input.IsDown(Key.Right))
                c.Zoom += impostor;
            else if (Input.IsDown(Key.Left))
                c.Zoom -= impostor;

            a = Input.UpdateTextInputString(a);
            //Console.WriteLine(a);

            //Console.WriteLine(string.Join(',', Input.PressedKeys.Select(k => Input.KeyToChar(k)).ToArray()));

            _time += 0.01f;
            //_test.Angle = _time;

            //_testMaterial.Time = _time;

            ////_needler.Alpha = (float)Math.Abs(Maths.Sin(_angle));

            _textMaterial.Time = _time;

            //if (Input.IsPressed(Key.O))
            //{
            //    var atlas = new Atlas(OpenGL.MaxTextureSize);

            //    foreach (string path in Directory.GetFiles("test"))
            //    {
            //        Texture texture = Texture.FromFile(path);

            //        atlas.Add(texture, Path.GetFileName(path));
            //    }

            //    atlas.Build(5);

            //    atlas.Texture.SaveAsPng("atlas2.png");
            //    atlas.Data.Save("amogus.xml");

            //    Console.WriteLine("saved");
            //}

            Input.ResetState();
        }

        private Layer _layer;
        private Layer _layer2;

        private void OnRender(FrameEventArgs args)
        {
            Graphics.BeginLayer(_layer);
            //_test.Draw(_layer.MousePosition);
            //_font.Draw(a, 90f, 50f, 1f);
            //Graphics.DrawLine(new Vector2(10f), _layer.MousePosition, new Color(255, 0, 0), 1f);
            ////Graphics.DrawCircle(_layer.MousePosition, new Color(255, 0, 127), 50, 1f);
            //Graphics.DrawRectangle(new Rectangle(new Vector2(5f), _layer.MousePosition), new Color(255, 0, 127), 1f);
            ////Graphics.DrawCircleWithLines(_layer.MousePosition, new Color(0, 255, 0), 50, 1f);
            Graphics.EndCurrentLayer();

            Graphics.BeginLayer(_layer2);
            _needler.Draw(64f, 36f);
            Graphics.EndCurrentLayer();

            Graphics.DrawLayers();

            Context.SwapBuffers();
        }
    }
}
