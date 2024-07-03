using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carpet
{
    // TexturePass min and max depth to vertex shader

    public class SpriteBatch
    {
        private const int MaxSubmittedItems = 20000;
        private const int MaxSubmittedVertices = 20000 * 4;
        private const int MaxSubmittedElements = MaxSubmittedVertices * 6;

        private const int MaxItems = 1000;
        private const int MaxVertices = MaxItems * 4;
        private const int MaxElements = MaxVertices * 6;

        private const string TexturesUniformName = "u_Textures";

        private readonly List<SpriteBatchItem> _opaqueItems = [];
        private readonly List<SpriteBatchItem> _transparentItems = [];

        private readonly BasicMaterial _basicMaterial = new();

        private readonly VertexColorTextureCoordinate[] _submittedVertices = new VertexColorTextureCoordinate[MaxSubmittedVertices];
        private readonly uint[] _submittedElements = new uint[MaxSubmittedElements];

        private readonly VertexColorTextureIndex[] _vertices = new VertexColorTextureIndex[MaxVertices];
        private readonly uint[] _elements = new uint[MaxElements];
        private readonly int[] _textureUnits;

        private readonly VertexArray<VertexColorTextureIndex> _vertexArray;
        private readonly RenderBuffer _renderBuffer;
        private readonly FrameBuffer _frameBuffer;
        private readonly UniformBuffer _uniformBuffer;

        private readonly uint[] _quadElements = 
        {
            0u,
            1u,
            2u,
            2u,
            3u,
            1u
        };

        private RenderTarget _renderTarget;
        private Rectangle _fullScissor;

        private Matrix4 _transform;

        private int _submittedVerticesCount;
        private int _submittedElementsCount;

        private int _verticesCount;
        private int _elementsCount;
        private int _texturesCount;

        private int _extraTextures;

        private uint _currentElement;
        private int _itemsCount;

        private bool _began;

        public SpriteBatch()
        {
            var attributes = new VertexAttribute[]
            {
                VertexAttribute.Vec3,
                VertexAttribute.Vec2,
                VertexAttribute.Vec2,
                VertexAttribute.Vec4,
                VertexAttribute.Vec4,
                VertexAttribute.Float
            };

            var vertexBuffer = new VertexBuffer<VertexColorTextureIndex>();
            var elementBuffer = new ElementBuffer();

            vertexBuffer.Initialize(MaxVertices);
            elementBuffer.Initialize(MaxElements);

            _vertexArray = new VertexArray<VertexColorTextureIndex>(vertexBuffer, elementBuffer, attributes);

            _frameBuffer = new FrameBuffer();
            _renderBuffer = new RenderBuffer();

            _renderBuffer.Bind();
            _renderBuffer.SetResolution(0, 0);

            _frameBuffer.Bind();
            _frameBuffer.SetRenderBuffer(_renderBuffer);

            _textureUnits = Enumerable.Range(0, OpenGL.MaxTextureSize).ToArray();

            var uniformBlockItems = new Std140LayoutItem[]
            {
                Std140LayoutItem.Matrix(4)
            };

            _uniformBuffer = new UniformBuffer();
            _uniformBuffer.Initialize(uniformBlockItems);
        }

        public void Begin(RenderTarget target, BlendMode blendMode, Color clearColor, Matrix4? view)
        {
            if (_began)
                throw new InvalidOperationException($"{nameof(Begin)} can only be called again after End is called.");

            GL.BlendFunc(blendMode.SourceFactor, blendMode.DestinationFactor);

            _renderTarget = target;
            Texture2D targetTexture = _renderTarget.Texture;

            if (targetTexture is not null)
            {
                _frameBuffer.Bind();
                _frameBuffer.SetTexture(targetTexture);
            }
            else
            {
                _frameBuffer.Unbind();
            }

            int targetWidth = target.Width;
            int targetHeight = target.Height;

            _fullScissor = new Rectangle(0f, 0f, targetWidth, targetHeight);
            SetScissor(_fullScissor);

            GL.Viewport(0, 0, targetWidth, targetHeight);

            _renderBuffer.Bind();
            _renderBuffer.SetResolution(targetWidth, targetHeight);

            GL.ClearColor(clearColor.ToColor4());

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0f, targetWidth, targetHeight, 0f, -1f, 1f);
            _transform = view.GetValueOrDefault(Matrix4.Identity) * projection;

            _transform.Transpose();

            _uniformBuffer.Bind();
            _uniformBuffer.SetValue(_transform, 0);

            _began = true;
        }

        public void Submit(Texture2D texture, Material material, Rectangle? scissor, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 localOrigin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            int textureWidth = texture.Width;
            int textureHeight = texture.Height;

            float boundsWidth = bounds.Width;
            float boundsHeight = bounds.Height;

            float textureLeft = bounds.Left / textureWidth;
            float textureTop = 1f - bounds.Top / textureHeight;

            float normalizedWidth = boundsWidth / textureWidth;
            float normalizedHeight = boundsHeight / textureHeight;
            float textureRight = textureLeft + normalizedWidth;
            float textureBottom = textureTop - normalizedHeight;

            if (horizontalEffect != SpriteEffect.None)
            {
                float value = textureRight;

                textureRight = textureLeft;
                textureLeft = value;
            }

            if (verticalEffect != SpriteEffect.None)
            {
                float value = textureBottom;

                textureBottom = textureTop;
                textureTop = value;
            }

            // HACK: scale is no longer rounded
            // This should be done in Graphics.cs where nedded
            float spriteScaleX = scale.X;
            float spriteScaleY = scale.Y;

            float x = position.X;
            float y = position.Y;
            float width = boundsWidth * spriteScaleX;
            float height = boundsHeight * spriteScaleY;

            float originX = Maths.Floor(localOrigin.X * spriteScaleX);
            float originY = Maths.Floor(localOrigin.Y * spriteScaleY);

            if (horizontalEffect == SpriteEffect.Reflect)
                originX = width - originX;

            if (verticalEffect == SpriteEffect.Reflect)
                originY = height - originY;

            localOrigin = new Vector2(originX, originY);
            Vector2 origin = position + localOrigin;

            float right = x + width;
            float bottom = y + height;

            Vector2 topLeft = ApplyOrigin(new Vector2(x, y), localOrigin, origin, angle);
            Vector2 topRight = ApplyOrigin(new Vector2(right, y), localOrigin, origin, angle);
            Vector2 bottomLeft = ApplyOrigin(new Vector2(x, bottom), localOrigin, origin, angle);
            Vector2 bottomRight = ApplyOrigin(new Vector2(right, bottom), localOrigin, origin, angle);

            Vector4 textureBounds = new Vector4(textureLeft, textureTop, normalizedWidth, normalizedHeight);

            ReadOnlySpan<VertexColorTextureCoordinate> vertices = stackalloc VertexColorTextureCoordinate[]
            {
                new VertexColorTextureCoordinate(topLeft, new Vector2(textureLeft, textureTop), textureBounds, color),
                new VertexColorTextureCoordinate(topRight, new Vector2(textureRight, textureTop), textureBounds, color),
                new VertexColorTextureCoordinate(bottomLeft, new Vector2(textureLeft, textureBottom), textureBounds, color),
                new VertexColorTextureCoordinate(bottomRight, new Vector2(textureRight, textureBottom), textureBounds, color)
            };

            Submit(texture, material, scissor, vertices, _quadElements, depth);
        }

        public void Submit(Texture2D texture, Material material, Rectangle? scissor, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            Submit(texture, material, scissor, bounds, position, scale, origin, color, angle, SpriteEffect.None, SpriteEffect.None, depth);
        }

        public void Submit(Texture2D texture, Material material, Rectangle? scissor, Vector2 position, Color color, float angle, float depth)
        {
            Submit(texture, material, scissor, new Rectangle(0f, 0f, texture.Width, texture.Height), position, Vector2.One, Vector2.Zero, color, angle, depth);
        }

        public void Submit(Texture2D texture, Rectangle? scissor, Vector2 position, Color color, float depth)
        {
            Submit(texture, null, scissor, position, color, 0f, depth);
        }

        public void Submit(Texture2D texture, Rectangle? scissor, Vector2 position, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            Submit(texture, null, scissor, new Rectangle(0f, 0f, texture.Width, texture.Height), position, Vector2.One, Vector2.Zero, Color.White, 0f, horizontalEffect, verticalEffect, depth);
        }

        public void Submit(Texture2D texture, Rectangle? scissor, Vector2 position, float depth)
        {
            Submit(texture, scissor, position, SpriteEffect.None, SpriteEffect.None, depth);
        }

        public void Submit(Texture2D texture, Material material, Rectangle? scissor, VertexColorTextureCoordinate vertex1, VertexColorTextureCoordinate vertex2, VertexColorTextureCoordinate vertex3, VertexColorTextureCoordinate vertex4, float depth)
        {
            ReadOnlySpan<VertexColorTextureCoordinate> vertices = stackalloc VertexColorTextureCoordinate[]
            {
                vertex1,
                vertex2,
                vertex3,
                vertex4
            };

            Submit(texture, material, scissor, vertices, _quadElements, depth);
        }

        public void Submit(Texture2D texture, Material material, Rectangle? scissor,
            ReadOnlySpan<VertexColorTextureCoordinate> vertices, int verticesLength,
            ReadOnlySpan<uint> elements, int elementsLength, float depth)
        {
            CheckBegin(nameof(Submit));

            if (verticesLength < 1 || elementsLength < 1)
                return;

            int targetWidth = _renderTarget.Width;
            int targetHeight = _renderTarget.Height;

            Rectangle scissorRectangle = scissor.GetValueOrDefault(new Rectangle(0f, 0f, targetWidth, targetHeight));

            int scissorWidth = (int)scissorRectangle.Width;
            int scissorHeight = (int)scissorRectangle.Height;

            int scissorY = targetHeight - (int)scissorRectangle.Top - scissorHeight;

            // HACK:
            bool transparent = true;

            for (int i = 0; i < verticesLength; i++)
            {
                VertexColorTextureCoordinate vertex = vertices[i];

                if (vertex.Color.A < Color.MaxValue)
                    transparent = true;

                _submittedVertices[_submittedVerticesCount + i] = vertex;
            }

            for (int i = 0; i < elementsLength; i++)
            {
                uint element = elements[i];

                _submittedElements[_submittedElementsCount + i] = element;
            }

            var item = new SpriteBatchItem()
            {
                Texture = texture,
                VerticesPointer = new Pointer(_submittedVerticesCount, verticesLength),
                ElementsPointer = new Pointer(_submittedElementsCount, elementsLength),
                Material = material is null ? _basicMaterial : material,
                Scissor = new Rectangle((int)scissorRectangle.Left, scissorY, scissorWidth, scissorHeight),
                Depth = depth
            };

            _submittedVerticesCount += verticesLength;
            _submittedElementsCount += elementsLength;

            if (transparent)
                _transparentItems.Add(item);
            else
                _opaqueItems.Add(item);
        }

        public void Submit(Texture2D texture, Material material, Rectangle? scissor,
            ReadOnlySpan<VertexColorTextureCoordinate> vertices, ReadOnlySpan<uint> elements, float depth)
        {
            int verticesLength = vertices.Length;
            int elementsLength = elements.Length;

            Submit(texture, material, scissor, vertices, verticesLength, elements, elementsLength, depth);
        }

        public void End()
        {
            CheckBegin(nameof(End));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _transparentItems.Sort(CompareItems);

            _vertexArray.Bind();

            DrawItems(_opaqueItems);
            DrawItems(_transparentItems);

            _opaqueItems.Clear();
            _transparentItems.Clear();

            _submittedVerticesCount = 0;
            _submittedElementsCount = 0;

            _extraTextures = 0;
            _began = false;
        }

        private void DrawItems(IEnumerable<SpriteBatchItem> items)
        {
            Texture2D lastTexture = null;
            Material lastMaterial = null;

            Rectangle lastScissor = _fullScissor;

            foreach (SpriteBatchItem item in items)
            {
                Texture2D currentTexture = item.Texture;
                Material currentMaterial = item.Material;

                PipelineShaderProgram currentProgram = currentMaterial.ShaderProgram;
                PipelineShaderProgram lastProgram = lastMaterial?.ShaderProgram;

                Rectangle currentScissor = item.Scissor;
                bool scissorChanged = currentScissor != lastScissor;

                Pointer verticesPointer = item.VerticesPointer;
                Pointer elementsPointer = item.ElementsPointer;

                int verticesStartIndex = verticesPointer.StartIndex;
                int elementsStartIndex = elementsPointer.StartIndex;

                int verticesCount = verticesPointer.Length;
                int elementsCount = elementsPointer.Length;

                int maxTextureUnits = OpenGL.MaxTextureUnits;
                _extraTextures = currentMaterial.ExtraTexturesCount;
                int maxTextures = maxTextureUnits - _extraTextures;

                if (lastMaterial is not null && (scissorChanged || currentMaterial != lastMaterial || currentTexture != lastTexture && _texturesCount >= maxTextures ||
                    _verticesCount + verticesCount > MaxVertices || _elementsCount + elementsCount > MaxElements || _itemsCount >= MaxItems))
                {
                    Flush(lastMaterial);

                    lastTexture = null;
                }

                if (currentMaterial != lastMaterial)
                {
                    if (currentProgram != lastProgram)
                        currentProgram.Bind();

                    if (lastMaterial is not null)
                        lastMaterial.Unapply();

                    currentMaterial.Apply();

                    lastMaterial = currentMaterial;
                }

                if (currentTexture != lastTexture)
                {
                    int index = _texturesCount + _extraTextures;

                    TextureUnit unit = GetTextureUnit(index);
                    currentTexture.Bind(unit);

                    lastTexture = currentTexture;
                    _texturesCount++;
                }

                if (scissorChanged)
                {
                    SetScissor(currentScissor);

                    lastScissor = currentScissor;
                }

                for (int i = verticesStartIndex; i < verticesStartIndex + verticesCount; i++)
                {
                    VertexColorTextureCoordinate oldVertex = _submittedVertices[i];
                    Vector2 oldPosition = oldVertex.Position;

                    Vector3 position = new Vector3(oldPosition.X, oldPosition.Y, item.Depth);
                    Vector2 textureResolution = new Vector2(currentTexture.Width, currentTexture.Height);
                    Vector4 color = oldVertex.Color.ToVector4();
                    int textureIndex = _texturesCount - 1 + _extraTextures;

                    var newVertex = new VertexColorTextureIndex(position, oldVertex.TextureCoordinate, textureResolution, 
                        oldVertex.TextureBounds, color, textureIndex);

                    _vertices[_verticesCount] = newVertex;
                    _verticesCount++;
                }

                uint maxElement = uint.MinValue;

                for (int i = elementsStartIndex; i < elementsStartIndex + elementsCount; i++)
                {
                    uint localElement = _submittedElements[i];

                    if (localElement > maxElement)
                        maxElement = localElement;

                    uint element = localElement + _currentElement;

                    _elements[_elementsCount] = element;
                    _elementsCount++;
                }

                _currentElement += maxElement + 1;

                _itemsCount++;
            }

            if (_verticesCount > 0)
            {
                Flush(lastMaterial);

                lastMaterial.Unapply();
            }
        }

        private int CompareItems(SpriteBatchItem a, SpriteBatchItem b)
        {
            float depthA = a.Depth;
            float depthB = b.Depth;

            return depthA.CompareTo(depthB);
        }

        private void Flush(Material material)
        {
            PipelineShaderProgram program = material.ShaderProgram;

            program.SetUniform(TexturesUniformName, _textureUnits.Length, _textureUnits);

            _vertexArray.VertexBuffer.SetData(_verticesCount, _vertices);
            _vertexArray.ElementBuffer.SetData(_elementsCount, _elements);

            _vertexArray.Draw(PrimitiveType.Triangles);

            int maxTextureUnits = OpenGL.MaxTextureUnits;

            for (int i = _extraTextures; i < maxTextureUnits; i++)
            {
                TextureUnit unit = GetTextureUnit(i);
                Texture.UnbindUnit(TextureTarget.Texture2D, unit);
            }

            _verticesCount = 0;
            _elementsCount = 0;
            _texturesCount = 0;

            _currentElement = 0;

            _itemsCount = 0;
        }

        private Vector2 ApplyOrigin(Vector2 position, Vector2 localOrigin, Vector2 origin, float angle)
        {
            return position.Rotate(origin, angle) - localOrigin;
        }

        private TextureUnit GetTextureUnit(int index)
        {
            return TextureUnit.Texture0 + index;
        }

        private void SetScissor(Rectangle rectangle)
        {
            GL.Scissor((int)rectangle.Left, (int)rectangle.Top, (int)rectangle.Width, (int)rectangle.Height);
        }

        private void CheckBegin(string methodName)
        {
            if (!_began)
                throw new InvalidOperationException($"{methodName} can only be called after Begin is called");
        }
    }
}
