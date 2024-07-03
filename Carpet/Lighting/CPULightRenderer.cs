using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Carpet
{
    // TODO: Rename all cringe
    public class CPULightRenderer : System
    {
        public const float MaxRadius = 320f;
        private const int SurfacesPerOccluder = 4;
        private const int MaxBloomPoints = 100;

        private readonly TexturePass _occludePass;
        private readonly TexturePass _shadowPass;
        private readonly TexturePass _lightmapPass;

        private readonly Camera _camera;

        private readonly VertexColorTextureCoordinate[] _vertices = new VertexColorTextureCoordinate[4000];
        private readonly uint[] _elements = new uint[6000];
        private readonly float _shadowLengthMultiplier = Maths.Sqrt(2f) * 2f;

        private readonly LightData[] _lightData;
        private readonly LightMaterial[] _lightMaterials;
        private readonly BloomMaterial[] _bloomMaterials = new BloomMaterial[MaxBloomPoints];
        private readonly Line[] _surfaceBuffer = new Line[SurfacesPerOccluder];

        private readonly List<Line> _surfaces = [];
        private readonly List<Rectangle> _rectangles = [];


        private readonly SolidColorMaterial _solidColorMaterial = new();
        private readonly List<Line> _debugSurfaces = [];
        private readonly List<Line> _debugRays = [];

        private readonly Color[] _masks =
        {
            new Color(1f, 0f, 0f, 0f),
            new Color(0f, 1f, 0f, 0f),
            new Color(0f, 0f, 1f, 0f),
            new Color(0f, 0f, 0f, 1f)
        };

        private readonly int _maxLights;
        private readonly int _shadowCellWidth;
        private readonly int _iterations = 12;

        private Vector2 _currentLightPosition;

        private int _lightsCount;
        private int _verticesCount;
        private int _elementsCount;
        private uint _currentElement;

        private int _lightMaterialsAllocated = 0;
        private int _bloomMaterialsAllocated = 0;

        public CPULightRenderer(Scene scene, TexturePass occludePass, TexturePass shadowPass, 
            TexturePass lightmapPass, Camera camera) : base(scene)
        {
            _occludePass = occludePass;
            _shadowPass = shadowPass;
            _lightmapPass = lightmapPass;

            _camera = camera;

            RenderTarget shadowBuffer = _shadowPass.GetRenderTarget();

            _shadowCellWidth = shadowBuffer.Width / (int)MaxRadius;
            _maxLights = _shadowCellWidth * shadowBuffer.Height / (int)MaxRadius * _masks.Length;

            _lightData = new LightData[_maxLights];
            _lightMaterials = new LightMaterial[_maxLights];
        }

        public Color InitialColor { get; set; } = new Color(140, 140, 140);
        public InBoundsBehavior BoundsBehavior { get; set; } = InBoundsBehavior.PutOut;
        public float DebugDepth { get; set; }

        public override void Initialize()
        {
            _occludePass.Render += OnOccludeBufferRender;
            _shadowPass.Render += OnShadowBufferRender;
            _lightmapPass.Render += OnLightmapRender;
        }

        public override void Update(float deltaTime)
        {
            // TODO: uhh
            Matrix4 view = _camera.GetViewMatrix();

            _lightmapPass.SetView(view);
            _lightmapPass.ClearColor = new Color(InitialColor.R, InitialColor.G, InitialColor.B, (byte)0);
        }

        public override void Draw()
        {
            if (!Engine.DebugLighting)
                return;

            Color red = Color.Red;

            foreach (Light light in Scene.GetComponentsOfType<Light>())
            {
                Graphics.DrawRectangle(light.Bounds, red, DebugDepth);

                Graphics.DrawCircle(light.Position, Color.Orange, (int)light.Radius, DebugDepth);
            }

            foreach (Line ray in _debugRays)
                Graphics.DrawLine(ray.Start, ray.End, Color.DarkGreen, DebugDepth);

            foreach (Line surface in _debugSurfaces)
            {
                Vector2 start = surface.Start;
                Vector2 end = surface.End;

                Graphics.DrawCircle(start, red, 3, DebugDepth);
                Graphics.DrawCircle(end, red, 3, DebugDepth);

                Graphics.DrawLine(start, end, red, DebugDepth);
            }
        }

        public override void Terminate()
        {
            _occludePass.Render -= OnOccludeBufferRender;
            _shadowPass.Render -= OnShadowBufferRender;
            _lightmapPass.Render -= OnLightmapRender;
        }

        private void OnOccludeBufferRender()
        {
            _debugRays.Clear();
            _debugSurfaces.Clear();

            IEnumerable<Light> lights = Scene.GetComponentsOfType<Light>();

            _lightsCount = 0;

            RenderTarget occludeBuffer = _occludePass.GetRenderTarget();

            foreach (Light light in lights)
            {
                _surfaces.Clear();
                _rectangles.Clear();

                FindCastingSurfaces(light, _surfaces, _rectangles, out bool inBounds);

                if (inBounds && BoundsBehavior == InBoundsBehavior.PutOut)
                    continue;

                Vector2 lightPosition = light.Position.Floor();
                Rectangle lightBounds = light.Bounds;
                var bounds = new Rectangle(lightBounds.TopLeft - Vector2.One, lightBounds.BottomRight + Vector2.One);

                FillSurfaceBuffer(bounds);

                int masksCount = _masks.Length;
                int maskIndex = _lightsCount % masksCount;
                Color mask = _masks[maskIndex];
                int cellIndex = _lightsCount / masksCount;
                Vector2 cellPosition = new Vector2(cellIndex % _shadowCellWidth, cellIndex / _shadowCellWidth) * MaxRadius;
                Vector2 offset = cellPosition - lightBounds.TopLeft;

                _lightData[_lightsCount] = new LightData(light, cellPosition, maskIndex);

                foreach (Line surface in _surfaces)
                {
                    Vector2 start = surface.Start;
                    Vector2 end = surface.End;

                    float startAngle = Maths.Atan2(lightPosition, start);
                    float endAngle = Maths.Atan2(lightPosition, end);

                    float length = light.Radius * _shadowLengthMultiplier;
                    Vector2 endProjection = ProjectPoint(end, endAngle, length, _surfaceBuffer);

                    float destinationAngle = Maths.Atan2(start, endProjection);

                    float delta = Maths.DeltaAngle(startAngle, destinationAngle);

                    float increment = delta / _iterations;

                    uint startElement = AddVertex(start, offset, mask);

                    Vector2 firstProjection = ProjectPoint(start, startAngle, length, _surfaceBuffer);
                    uint previousElement = AddVertex(firstProjection, offset, mask);

                    _debugRays.Add(new Line(start, firstProjection));

                    for (int j = 1; j < _iterations + 1; j++)
                    {
                        float angle = startAngle + increment * j;
                        Vector2 projection = ProjectPoint(start, angle, length, _surfaceBuffer);

                        uint currentElement = AddVertex(projection, offset, mask);

                        AddElement(startElement);
                        AddElement(previousElement);
                        AddElement(currentElement);

                        previousElement = currentElement;

                        _debugRays.Add(new Line(start, projection));
                    }

                    uint endElement = AddVertex(end, offset, mask);

                    AddElement(startElement);
                    AddElement(previousElement);
                    AddElement(endElement);

                    uint endProjectionElement = AddVertex(endProjection, offset, mask);

                    AddElement(endElement);
                    AddElement(previousElement);
                    AddElement(endProjectionElement);

                    int debugRaysCount = _debugRays.Count;
                    Line lastDebugRay = _debugRays[debugRaysCount - 1];
                    _debugRays.Add(new Line(end, lastDebugRay.End));
                    _debugRays.Add(new Line(end, endProjection));

                    _debugSurfaces.Add(surface);
                }

                // TODO: Make scissor the property of SpriteBatch and not Graphics
                Graphics.Scissor = new Rectangle(0f, 0f, 2240f, 2240f);

                foreach (Rectangle rectangle in _rectangles)
                {
                    Vector2 relativePosition = rectangle.TopLeft.Floor() - lightBounds.TopLeft;

                    Vector2 topLeft = relativePosition + cellPosition;
                    Vector2 bottomRight = topLeft + rectangle.Size.Floor();

                    Graphics.FillRectangle(topLeft, bottomRight, mask, 0f);
                }

                _lightsCount++;
            }
        }

        private void OnShadowBufferRender()
        {
            Graphics.SpriteBatch.Submit(Graphics.BlankTexture.ActualTexture, _solidColorMaterial, null,
                _vertices, _verticesCount, _elements, _elementsCount, 0f);
        }

        private void OnLightmapRender()
        {
            RenderTarget shadowBuffer = _shadowPass.GetRenderTarget();

            _verticesCount = 0;
            _elementsCount = 0;
            _currentElement = 0;

            if (_lightsCount > _lightMaterialsAllocated)
            {
                for (int i = _lightMaterialsAllocated; i < _lightsCount; i++)
                {
                    _lightMaterials[i] = new LightMaterial()
                    {
                        ShadowTextureResolution = shadowBuffer.Size
                    };
                }

                _lightMaterialsAllocated = _lightsCount;
            }

            for (int i = 0; i < _lightsCount; i++)
            {
                LightData lightData = _lightData[i];

                Light light = lightData.Light;

                float circumference = light.Radius * 2f;
                Vector2 cellPosition = lightData.CellPosition;

                LightMaterial material = _lightMaterials[i];

                material.ShadowBounds = new Vector4(cellPosition.X, shadowBuffer.Height - cellPosition.Y, circumference, circumference);
                material.Mask = lightData.MaskIndex;
                material.Rotation = light.Rotation;
                material.Angle = light.Angle;
                material.FalloffAngle = light.FalloffAngle;
                material.StartDistance = light.StartDistance;
                material.StartFade = light.StartFade;
                material.Volume = light.Volume;
                material.ShadowFalloff = light.ShadowFalloff;

                Graphics.FillRectangle(light.Bounds, material, light.Color, 0f);
            }

            //TODO: PassProcess bloom points here
        }

        private uint AddVertex(Vector2 position, Vector2 offset, Color mask)
        {
            var vertex = new VertexColorTextureCoordinate(position + offset, Graphics.BlankTextureCoordinate, Vector4.Zero, mask);

            _vertices[_verticesCount] = vertex;
            _verticesCount++;

            uint element = _currentElement;
            _currentElement++;

            return element;
        }

        private void AddElement(uint element)
        {
            _elements[_elementsCount] = element;
            _elementsCount++;
        }

        private void FindCastingSurfaces(Light light, List<Line> surfaces, List<Rectangle> rectangles, out bool inBounds)
        {
            _currentLightPosition = light.Position;
            Rectangle bounds = light.Bounds;

            inBounds = false;

            foreach (LightOccluder lightOccluder in Scene.CheckRectangleAllComponent<LightOccluder>(bounds))
            {
                Rectangle rectangle = lightOccluder.EntityRectangle;

                if (rectangle.Contains(_currentLightPosition))
                {
                    inBounds = true;

                    if (BoundsBehavior == InBoundsBehavior.Exclude)
                        continue;
                }

                if (rectangle.TryGetIntersectionRectangle(bounds, out Rectangle intersection))
                    rectangle = intersection;

                FillSurfaceBuffer(rectangle);

                Array.Sort(_surfaceBuffer, CompareSurfaces);

                Line closest = _surfaceBuffer[0];

                if (!PointIsTowards(_currentLightPosition, closest))
                    surfaces.Add(_surfaceBuffer[1]);

                surfaces.Add(closest);
                rectangles.Add(rectangle);
            }
        }

        private Vector2 ProjectPoint(Vector2 point, float angle, float length, IEnumerable<Line> bounds)
        {
            var projection = new Vector2(point.X + length, point.Y).Rotate(point, angle);
            var ray = new Line(point, projection);

            foreach (Line bound in bounds)
                if (Maths.TryGetIntersectionPoint(ray, bound, out Vector2 result))
                    return result;

            return ray.End;
        }

        private bool PointIsTowards(Vector2 point, Line surface)
        {
            float x = point.X;

            float y = point.Y;

            Vector2 start = surface.Start;
            Vector2 end = surface.End;

            float startX = start.X;
            float startY = start.Y;
            float endX = end.X;
            float endY = end.Y;

            if (x == startX || y == startY)
                return false;

            if (x >= Maths.Min(startX, endX) && x <= Maths.Max(startX, endX) || y >= Maths.Min(startY, endY) && y <= Maths.Max(startY, endY))
                return true;

            return false;
        }

        private int CompareSurfaces(Line a, Line b)
        {
            float distanceA = Vector2.Distance(_currentLightPosition, a.GetMidPoint());
            float distanceB = Vector2.Distance(_currentLightPosition, b.GetMidPoint());

            return distanceA.CompareTo(distanceB);  
        }

        private void FillSurfaceBuffer(Rectangle rectangle)
        {
            _surfaceBuffer[0] = rectangle.TopSurface;
            _surfaceBuffer[1] = rectangle.LeftSurface;
            _surfaceBuffer[2] = rectangle.RightSurface;
            _surfaceBuffer[3] = rectangle.BottomSurface;
        }

        private readonly record struct LightData(Light Light, Vector2 CellPosition, int MaskIndex);

        public enum InBoundsBehavior
        {
            Ignore,
            Exclude,
            PutOut
        }
    }
}
