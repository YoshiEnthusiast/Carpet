using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public class LightRenderer : System
    {
        public const float MaxRadius = 320f;

        private readonly VertexColorTextureCoordinate[] _vertices = new VertexColorTextureCoordinate[4000];
        private readonly uint[] _elements = new uint[6000];
        private readonly float _shadowLengthMultiplier = Maths.Sqrt(2f) * 2f;

        private readonly List<Line> _debugSurfaces = new List<Line>();
        private readonly List<Line> _debugRays = new List<Line>();

        private readonly Color[] _masks = new Color[]
        {
            new Color(1f, 0f, 0f, 0f),
            new Color(0f, 1f, 0f, 0f),
            new Color(0f, 0f, 1f, 0f),
            new Color(0f, 0f, 0f, 1f)
        };

        private readonly int _maxLights;
        private readonly int _shadowCellWidth;
        private readonly int _iterations = 12;

        private int _verticesCount;
        private int _elementsCount;
        private uint _currentElement;

        public LightRenderer(Scene scene) : base(scene)
        {
            RenderTarget shadowBuffer = RenderTargets.ShadowBuffer;

            _shadowCellWidth = shadowBuffer.Width / (int)MaxRadius;
            _maxLights = _shadowCellWidth * shadowBuffer.Height / (int)MaxRadius * _masks.Length;
        }

        public InBoundsBehaviour BoundsBehaviour { get; set; } = InBoundsBehaviour.PutOut;

        public override void OnBeforeDraw()
        {
            _debugRays.Clear();
            _debugSurfaces.Clear();

            IEnumerable<Light> lights = Scene.GetComponentsOfType<Light>();
            int lightsCount = Maths.Min(lights.Count(), _maxLights);

            var data = new LightData[lightsCount];
            var lightToRender = 0;

            foreach (Light light in lights)
            {
                IEnumerable<Line> surfaces = GetCastingSurfaces(light, out bool inBounds);

                if (inBounds && BoundsBehaviour == InBoundsBehaviour.PutOut)
                    continue;

                Vector2 lightPosition = light.Position.Floor();
                Rectangle lightBounds = light.Bounds;
                var bounds = new Rectangle(lightBounds.TopLeft - Vector2.One, lightBounds.BottomRight + Vector2.One);

                IEnumerable<Line> boundsSurfaces = bounds.GetSurfaces();

                int masksCount = _masks.Length;
                int maskIndex = lightToRender % masksCount;
                Color mask = _masks[maskIndex];
                int cellIndex = lightToRender / masksCount;
                Vector2 cellPosition = new Vector2(cellIndex % _shadowCellWidth, cellIndex / _shadowCellWidth) * MaxRadius;
                Vector2 offset = cellPosition - lightBounds.TopLeft;

                data[lightToRender] = new LightData(light, cellPosition, maskIndex);

                foreach (Line surface in surfaces)
                {
                    Vector2 start = surface.Start;
                    Vector2 end = surface.End;

                    float startAngle = Maths.Atan2(lightPosition, start);
                    float endAngle = Maths.Atan2(lightPosition, end);

                    float length = light.Radius * _shadowLengthMultiplier;
                    Vector2 endProjection = ProjectPoint(end, endAngle, length, boundsSurfaces);

                    float destinationAngle = Maths.Atan2(start, endProjection);

                    float delta = Maths.DeltaAngle(startAngle, destinationAngle);

                    float increment = delta / _iterations;

                    uint startElement = AddVertex(start, offset, mask);

                    Vector2 firstProjection = ProjectPoint(start, startAngle, length, boundsSurfaces);
                    uint previousElement = AddVertex(firstProjection, offset, mask);

                    _debugRays.Add(new Line(start, firstProjection));

                    for (int j = 1; j < _iterations + 1; j++)
                    {
                        float angle = startAngle + increment * j;
                        Vector2 projection = ProjectPoint(start, angle, length, boundsSurfaces);

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

                    _debugRays.Add(new Line(end, _debugRays.Last().End));
                    _debugRays.Add(new Line(end, endProjection));

                    _debugSurfaces.Add(surface);
                }

                lightToRender++;
            }

            var vertices = new VertexColorTextureCoordinate[_verticesCount];
            var elements = new uint[_elementsCount];

            Array.Copy(_vertices, vertices, _verticesCount);
            Array.Copy(_elements, elements, _elementsCount);

            SpriteBatch batch = Graphics.SpriteBatch;
            RenderTarget shadowBuffer = RenderTargets.ShadowBuffer;

            batch.Begin(shadowBuffer, BlendMode.Additive, Color.Transparent, null);
            batch.Submit(Graphics.BlankTexture.ActualTexture, null, null, vertices, elements, 0f);
            batch.End();

            _verticesCount = 0;
            _elementsCount = 0;
            _currentElement = 0;

            Matrix4 view = Layers.Foreground.Camera.GetViewMatrix();
            batch.Begin(RenderTargets.LightMap, BlendMode.Additive, Scene.Color, view);

            for (int i = 0; i < lightToRender; i++)
            {
                LightData lightData = data[i];

                Light light = lightData.Light;

                float circumference = light.Radius * 2f;
                Vector2 cellPosition = lightData.CellPosition;

                var material = new LightMaterial()
                {
                    ShadowBounds = new Vector4(cellPosition.X, shadowBuffer.Height - cellPosition.Y, circumference, circumference),
                    Mask = lightData.MaskIndex
                };

                Graphics.FillRectangle(light.Bounds, material, light.Color, 0f);
            }

            batch.End();
        }

        public override void Draw()
        {
            if (!Engine.DebugLighting)
                return;

            Color red = Color.Red;
            float debugDepth = Depths.Debug;

            foreach (Light light in Scene.GetComponentsOfType<Light>())
            {
                Graphics.DrawRectangle(light.Bounds, red, debugDepth);

                Graphics.DrawCircle(light.Position, Color.CoolOrange, (int)light.Radius, debugDepth);
            }

            foreach (Line ray in _debugRays)
                Graphics.DrawLine(ray.Start, ray.End, Color.DarkGreen, debugDepth);

            foreach (Line surface in _debugSurfaces)
            {
                Vector2 start = surface.Start;
                Vector2 end = surface.End;

                Graphics.DrawCircle(start, red, 3, debugDepth);
                Graphics.DrawCircle(end, red, 3, debugDepth);

                Graphics.DrawLine(start, end, red, debugDepth);
            }
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

        private IEnumerable<Line> GetCastingSurfaces(Light light, out bool inBounds)
        {
            var result = new List<Line>();

            Vector2 position = light.Position;
            Rectangle bounds = light.Bounds;

            inBounds = false;

            foreach (LightOccluder lightOccluder in Scene.CheckRectangleAllComponent<LightOccluder>(bounds))
            {
                Rectangle occluder = lightOccluder.EntityRectangle;

                if (occluder.Contains(position))
                {
                    inBounds = true;

                    if (BoundsBehaviour == InBoundsBehaviour.Exclude)
                        continue;
                }

                if (occluder.TryGetIntersectionRectangle(bounds, out Rectangle rectangle))
                    occluder = rectangle;

                Line[] surfaces = occluder.GetSurfaces()
                    .OrderBy(surface => Vector2.Distance(position, surface.GetMidPoint()))
                    .ToArray();

                Line closest = surfaces[0];

                if (!PointIsTowards(position, closest))
                    result.Add(surfaces[1]);

                result.Add(closest);
            }

            return result;
        }

        private Vector2 ProjectPoint(Vector2 point, float angle, float length, IEnumerable<Line> bounds)
        {
            Vector2 projection = new Vector2(point.X + length, point.Y).Rotate(point, angle);
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

        private readonly record struct LightData(Light Light, Vector2 CellPosition, int MaskIndex);

        public enum InBoundsBehaviour
        {
            Ignore,
            Exclude,
            PutOut
        }
    }
}
