using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Carpet
{
    public class GPULightRenderer : System
    {
        private const int MaxBloomPoints = 100;

        private readonly TexturePass _occlusionPass;
        private readonly TexturePass _distancePass;
        private readonly TexturePass _lightPass;

        private readonly Texture2D _occlusionMap;
        private readonly Texture2D _distanceMap;

        private readonly LayerPass _foregroundPass;

        private readonly Light[] _renderedLights;
        private readonly GPULightMaterial[] _lightMaterials;
        private readonly DistanceMapMaterial[] _distanceMapMaterials;
        private readonly BloomMaterial[] _bloomMaterials = new BloomMaterial[MaxBloomPoints];

        private readonly int _maxLights;
        private readonly float _maxRadius;

        private readonly List<Light> _lightsBuffer = [];
        private readonly List<LightOccluder> _occludersBuffer = [];
        private readonly List<BloomPoint> _bloomBuffer = [];

        private Vector2 _occlusionOrigin;

        private int _lightsRendered;
        private int _bloomMaterialsAllocated;

        private static readonly Color[] s_masks =
        {
            new Color(255, 0, 0, 0),
            new Color(0, 255, 0, 0),
            new Color(0, 0, 255, 0),
            new Color(0, 0, 0, 255)
        };

        public GPULightRenderer(Scene scene, TexturePass occlusionPass, TexturePass distancePass,
            TexturePass lightPass, int maxRadius, bool glare, LayerPass foregroundPass) : base(scene)
        {
            _foregroundPass = foregroundPass;
            _maxRadius = maxRadius;

            _occlusionPass = occlusionPass;
            _occlusionOrigin = _occlusionPass.Size / 2f;

            _distancePass = distancePass;
            _lightPass = lightPass;

            _occlusionMap = _occlusionPass.GetTexture();
            _distanceMap = _distancePass.GetTexture();

            _maxLights = _distanceMap.Height * s_masks.Length;

            _renderedLights = new Light[_maxLights];
            _distanceMapMaterials = new DistanceMapMaterial[_maxLights];
            _lightMaterials = new GPULightMaterial[_maxLights];

            for (int i = 0; i < _maxLights; i++)
            {
                var distanceMapMaterial = new DistanceMapMaterial();
                _distanceMapMaterials[i] = distanceMapMaterial;

                var lightMaterial = new GPULightMaterial()
                {
                    Resolution = _lightPass.Size,
                    Glare = glare
                };

                lightMaterial.Textures[0] = _occlusionMap;
                _lightMaterials[i] = lightMaterial;
            }
        }

        public Color InitialColor { get; private set; } = new Color(140, 140, 140);

        public static TexturePass CreateOcclusionPass(int foregroundWidth, int foregroundHeight, int maxRadius)
        {
            int additionalSize = maxRadius * 2;

            RenderTarget target = RenderTarget.CreateTexture(foregroundWidth + additionalSize,
                foregroundHeight + additionalSize);

            var pass = new TexturePass()
                .SetRenderTarget(target);

            return pass;
        }

        public static TexturePass CreateDistancePass(int anglesCount, int maxLights)
        {
            int height = Maths.Ceiling((float)maxLights / s_masks.Length);

            Texture2D texture = Texture2D.CreateEmpty(anglesCount, height,
                TextureMinFilter.Nearest, TextureMagFilter.Nearest,
                TextureWrapMode.Repeat, TextureWrapMode.ClampToEdge);

            RenderTarget target = RenderTarget.FromTexture(texture);

            var pass = new TexturePass(BlendMode.Additive)
                .SetRenderTarget(target);

            return pass;
        }

        public static TexturePass CreateLightPass(int foregroundWidth, int foregroundHeight)
        {
            RenderTarget target = RenderTarget.CreateTexture(foregroundWidth, foregroundHeight);

            var pass = new TexturePass(BlendMode.Additive)
                .SetRenderTarget(target);

            return pass;
        }

        public override void Initialize()
        {
            _occlusionPass.Render += RenderOcclusionMap;
            _distancePass.Render += RenderDistanceMap;
            _lightPass.Render += RenderLightMap;
        }

        public override void Terminate()
        {
            _occlusionPass.Render -= RenderOcclusionMap;
            _distancePass.Render -= RenderDistanceMap;
            _lightPass.Render -= RenderLightMap;
        }

        public override void Update(float deltaTime)
        {
            Matrix4 lightPassView = _foregroundPass.View.Value;

            Layer layer = _foregroundPass.GetLayer();
            Camera camera = layer.Camera;
            Vector2 scale = camera.Scale;

            Vector2 position = camera.Position.Floor();
            float x = position.X;
            float y = position.Y;

            float originX = _occlusionOrigin.X;
            float originY = _occlusionOrigin.Y;

            float angle = camera.Angle;

            Matrix4 occlusionView = Matrix4.CreateScale(scale.X, scale.Y, 0f);

            if (angle != 0f)
                occlusionView *= Matrix4.CreateTranslation(-originX, -originY, 0f)
                    * Matrix4.CreateRotationZ(angle)
                    * Matrix4.CreateTranslation(originX, originY, 0f);

            occlusionView *= Matrix4.CreateTranslation(-x + originX, -y + originY, 0f);

            _occlusionPass.SetView(occlusionView);
            _lightPass.SetView(lightPassView);

            var clearColor = new Color(InitialColor.R, InitialColor.G,
                InitialColor.B, (byte)0);
            _lightPass.ClearColor = clearColor;
        }

        private void RenderOcclusionMap()
        {
            foreach (LightOccluder occluder in Scene.GetComponentsOfType<LightOccluder>(_occludersBuffer))
                occluder.DrawOcclusion();
        }

        private void RenderDistanceMap()
        {
            _lightsRendered = 0;

            float scaleX = (float)_distanceMap.Width / _occlusionMap.Width;
            float scaleY = 1f / _occlusionMap.Height;

            Camera camera = _foregroundPass.GetLayer().Camera;
            Vector2 cameraTopLeft = camera.Position.Floor()
                - _occlusionOrigin;

            Scene.GetComponentsOfType<Light>(_lightsBuffer);
            int lightsCount = Math.Min(_lightsBuffer.Count, _maxLights);

            for (int i = 0; i < lightsCount; i++)
            {
                Light light = _lightsBuffer[i];

                int masksCount = s_masks.Length;
                int maskIndex = _lightsRendered % masksCount;
                Color mask = s_masks[maskIndex];
                float radius = Maths.Min(light.Radius, _maxRadius);

                Vector2 positionOnOcclusionMap = light.Position.Floor() 
                    - cameraTopLeft;

                float y = _lightsRendered / masksCount;

                DistanceMapMaterial material = _distanceMapMaterials[_lightsRendered];

                material.LightPosition = positionOnOcclusionMap;
                material.Radius = radius;

                Graphics.Draw(_occlusionMap, material, new Vector2(0f, y), new Vector2(scaleX, scaleY),
                    Vector2.Zero, mask, 0f, 0f);

                _renderedLights[_lightsRendered] = light;

                _lightsRendered++;
            }
        }

        private void RenderLightMap()
        {
            for (int i = 0; i < _lightsRendered; i++)
            {
                Light light = _renderedLights[i];
                GPULightMaterial material = _lightMaterials[i];

                material.Index = i;

                material.Rotation = light.Rotation;
                material.Angle = light.Angle;
                material.FalloffAngle = light.FalloffAngle;

                material.StartDistance = light.StartDistance;
                material.StartFade = light.StartFade;

                material.Volume = light.Volume;

                Graphics.Draw(_distanceMap, material, light.Bounds, light.Color, 0f);
            }

            Scene.GetComponentsOfType<BloomPoint>(_bloomBuffer);
            int bloomPointsCount = Math.Min(_bloomBuffer.Count, MaxBloomPoints);

            if (bloomPointsCount > _bloomMaterialsAllocated)
            {
                for (int i = _bloomMaterialsAllocated; i < bloomPointsCount; i++)
                    _bloomMaterials[i] = new BloomMaterial();

                _bloomMaterialsAllocated = bloomPointsCount;
            }

            for (int i = 0; i < bloomPointsCount; i++)
            {
                BloomPoint bloom = _bloomBuffer[i];
                BloomMaterial material = _bloomMaterials[i];

                material.Volume = bloom.Volume;

                Graphics.FillRectangle(bloom.Bounds, material, bloom.Color, 0f);
            }
        }
    }
}
