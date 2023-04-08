using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

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

        public override void OnBeforeDraw()
        {
            _debugRays.Clear();
            _debugSurfaces.Clear();

            Light[] lights = Scene.Components.OfType<Light>().ToArray();
            int lightsCount = Math.Min(lights.Length, _maxLights);

            var data = new LightData[lightsCount];

            for (int i = 0; i < lightsCount; i++)
            {
                Light light = lights[i];

                Vector2 lightPosition = light.Position.Round();
                Rectangle lightBounds = light.Bounds;
                var bounds = new Rectangle(lightBounds.TopLeft - Vector2.One, lightBounds.BottomRight + Vector2.One);

                IEnumerable<Line> boundsSurfaces = GetRectangleSurfaces(bounds);

                int masksCount = _masks.Length;
                int maskIndex = i % masksCount;
                Color mask = _masks[maskIndex];
                int cellIndex = i / masksCount;
                Vector2 cellPosition = new Vector2(cellIndex % _shadowCellWidth, cellIndex / _shadowCellWidth) * MaxRadius;
                Vector2 offset = cellPosition - lightBounds.TopLeft;

                data[i] = new LightData(cellPosition, maskIndex);

                foreach (Line surface in GetCastingSurfaces(light))
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
            }

            var vertices = new VertexColorTextureCoordinate[_verticesCount];
            var elements = new uint[_elementsCount];

            Array.Copy(_vertices, vertices, _verticesCount);
            Array.Copy(_elements, elements, _elementsCount);

            SpriteBatch batch = Graphics.SpriteBatch;
            RenderTarget shadowBuffer = RenderTargets.ShadowBuffer;

            batch.Begin(shadowBuffer, BlendMode.Additive, Color.Transparent, null, null);
            batch.Submit(Graphics.BlankTexture.ActualTexture, null, vertices, elements, 0f);
            batch.End();

            _verticesCount = 0;
            _elementsCount = 0;
            _currentElement = 0;

            batch.Begin(RenderTargets.LightMap, BlendMode.Additive, Scene.Color, null, null);

            for (int i = 0; i < lightsCount; i++)
            {
                Light light = lights[i];
                LightData lightData = data[i];
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

            foreach (Light light in Scene.Components.OfType<Light>())
            {
                Graphics.DrawRectangle(light.Bounds, Color.Red, 11f);

                Graphics.DrawCircle(light.Position, new Color(233, 129, 46), (int)light.Radius, 10f);
            }

            foreach (Line ray in _debugRays)
                Graphics.DrawLine(ray.Start, ray.End, Color.DarkGreen, 10f);

            foreach (Line surface in _debugSurfaces)
            {
                Graphics.DrawCircle(surface.Start, Color.Red, 3, 10f);
                Graphics.DrawCircle(surface.End, Color.Red, 3, 10f);

                Graphics.DrawLine(surface.Start, surface.End, Color.Red, 10f);
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

        private IEnumerable<Line> GetCastingSurfaces(Light light)
        {
            Vector2 position = light.Position;
            Rectangle bounds = light.Bounds;

            foreach (Entity entity in Scene.CheckRectangleAll<Entity>(bounds))
            {
                // temporary
                if (entity.Get<LightOccluder>() is null)
                    continue;

                Rectangle occluder = entity.Rectangle;

                if (occluder.Contains(position))
                    continue;

                if (occluder.TryGetIntersectionRectangle(bounds, out Rectangle result))
                    occluder = result;

                Line[] surfaces = GetRectangleSurfaces(occluder).
                    OrderBy(surface => Vector2.Distance(position, surface.GetMidPoint()))
                    .ToArray();

                Line closest = surfaces[0];

                if (!PointIsTowards(position, closest))
                    yield return surfaces[1];

                yield return closest;
            }
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

        private IEnumerable<Line> GetRectangleSurfaces(Rectangle occluder)
        {
            Vector2 topLeft = occluder.TopLeft;
            Vector2 topRight = occluder.TopRight;
            Vector2 bottomLeft = occluder.BottomLeft;
            Vector2 bottomRight = occluder.BottomRight;

            yield return new Line(topLeft, topRight);
            yield return new Line(topRight, bottomRight);
            yield return new Line(bottomRight, bottomLeft);
            yield return new Line(bottomLeft, topLeft);
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

            if (x >= Math.Min(startX, endX) && x <= Math.Max(startX, endX) || y >= Math.Min(startY, endY) && y <= Math.Max(startY, endY))
                return true;

            return false;
        }

        private readonly record struct LightData(Vector2 CellPosition, int MaskIndex);
    }
}
