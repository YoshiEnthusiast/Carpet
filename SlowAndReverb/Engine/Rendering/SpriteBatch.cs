using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public class SpriteBatch
    {
        private const int MaxItems = 1000;
        private const int MaxVertices = MaxItems * 4;
        private const int MaxElements = MaxVertices * 6;

        private readonly List<SpriteBatchItem> _items = new List<SpriteBatchItem>();

        private readonly Material _basicMaterial = new BasicMaterial();

        private readonly VertexColorTextureIndex[] _vertices = new VertexColorTextureIndex[MaxVertices];
        private readonly uint[] _elements = new uint[MaxElements];
        private readonly int[] _textureUnits = new int[OpenGL.MaxTextureUnits];

        private readonly VertexArray<VertexColorTextureIndex> _vertexArray;
        private readonly RenderBuffer _renderBuffer;
        private readonly FrameBuffer _frameBuffer;

        private RenderTarget _renderTarget;

        private Matrix4 _transform;

        private int _verticesCount;
        private int _elementsCount;
        private int _texturesCount;

        private uint _currentElement;
        private int _itemsCount;

        private bool _began;

        public SpriteBatch(bool blending)
        {
            GL.Enable(EnableCap.ScissorTest);

            if (blending)
            {
                GL.Enable(EnableCap.Blend);

                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            var attributes = new VertexAttribute[]
            {
                VertexAttribute.Vec2,
                VertexAttribute.Vec2,
                VertexAttribute.Vec2,
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

            _frameBuffer.Bind();
            _frameBuffer.SetRenderBuffer(_renderBuffer);
        }

        public RenderTarget CurrentTarget => _renderTarget;

        public void Begin(RenderTarget target, Color clearColor, Rectangle scissor, Matrix4 view)
        {
            if (_began)
                throw new InvalidOperationException("Begin can only be called again after End is called.");

            Texture targetTexture = target.Texture;

            if (targetTexture is not null)
            {
                _frameBuffer.Bind();
                _frameBuffer.SetTexture(targetTexture);
            }
            else
            {
                _frameBuffer.UnBind();
            }

            int targetWidth = target.Width;
            int targetHeight = target.Height;

            GL.Viewport(0, 0, targetWidth, targetHeight);

            _renderBuffer.Bind();
            _renderBuffer.SetResolution(targetWidth, targetHeight);

            GL.ClearColor(clearColor.ToColor4());

            GL.Scissor((int)scissor.X, (int)scissor.Y, (int)scissor.Width, (int)scissor.Height);

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0f, targetWidth, targetHeight, 0f, -1f, 1f);
            _transform = view * projection;

            _began = true;
        }

        public void Submit(Texture texture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 localOrigin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            int textureWidth = texture.Width;
            int textureHeight = texture.Height;

            float boundsWidth = bounds.Width;
            float boundsHeight = bounds.Height; 

            float textureLeft = bounds.X / textureWidth;
            float textureTop = bounds.Y / textureHeight;
            float textureRight = textureLeft + boundsWidth / textureWidth;
            float textureBottom = textureTop + boundsHeight / textureHeight;

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

            var vertices = new VertexColorTextureCoordinate[]
            {
                new VertexColorTextureCoordinate(topLeft, new Vector2(textureLeft, textureTop), color),
                new VertexColorTextureCoordinate(topRight, new Vector2(textureRight, textureTop), color),
                new VertexColorTextureCoordinate(bottomLeft, new Vector2(textureLeft, textureBottom), color),
                new VertexColorTextureCoordinate(bottomRight, new Vector2(textureRight, textureBottom), color)
            };

            var indices = new uint[]
            {
                0u, 1u, 2u,
                2u, 3u, 1u
            };

            Submit(texture, material, vertices, indices, depth);
        }

        public void Submit(Texture texture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, Color color, float angle, float depth)
        {
            Submit(texture, material, bounds, position, scale, origin, color, angle, SpriteEffect.None, SpriteEffect.None, depth);
        }

        public void Submit(Texture texture, Material material, Vector2 position, Color color, float angle, float depth)
        {
            Submit(texture, material, new Rectangle(0f, 0f, texture.Width, texture.Height), position, Vector2.One, Vector2.Zero, color, angle, depth);
        }

        public void Submit(Texture texture, Vector2 position, Color color, float depth)
        {
            Submit(texture, null, position, color, 0f, depth);
        }

        public void Submit(Texture texture, Vector2 position, float depth)
        {
            Submit(texture, position, Color.White, depth);
        }

        public void Submit(Texture texture, Material material, VertexColorTextureCoordinate[] vertices, uint[] indices, float depth)
        {
            CheckBegin("Submit");

            var item = new SpriteBatchItem()
            {
                Texture = texture,
                Vertices = vertices,
                Indices = indices,
                Material = material is null ? _basicMaterial : material,
                Depth = depth
            };

            _items.Add(item);
        }

        public void End()
        {
            CheckBegin("End");

            GL.Clear(ClearBufferMask.ColorBufferBit);

            IEnumerable<SpriteBatchItem> orderedItems = _items.OrderBy(item => item.Depth)
                 .ThenBy(item => item.Texture.GetHashCode());

            _vertexArray.Bind();

            Texture lastTexture = null;
            Material lastMaterial = null;

            foreach (SpriteBatchItem item in orderedItems)
            {
                Texture currentTexture = item.Texture;
                Material currentMaterial = item.Material;

                IEnumerable<VertexColorTextureCoordinate> vertices = item.Vertices;
                IEnumerable<uint> elements = item.Indices;

                int verticesCount = vertices.Count();
                int elementsCount = elements.Count();

                int maxTextureUnits = OpenGL.MaxTextureUnits;
                int extraTextures = currentMaterial.TextureUniformsCount;
                int maxTextures = maxTextureUnits - extraTextures;

                ShaderProgram program = currentMaterial.ShaderProgram;

                if (lastMaterial is not null && currentMaterial != lastMaterial || currentTexture != lastTexture && _texturesCount >= maxTextures || _verticesCount + verticesCount > MaxVertices || _elementsCount + elementsCount > MaxElements || _itemsCount >= MaxItems)
                    Flush(lastMaterial);

                lastMaterial = currentMaterial;

                if (currentTexture != lastTexture)
                {
                    int unit = _texturesCount + extraTextures;

                    currentTexture.Bind(TextureUnit.Texture0 + unit);

                    _textureUnits[_texturesCount] = unit;

                    lastTexture = currentTexture;
                    _texturesCount++;
                }

                foreach (VertexColorTextureCoordinate oldVertex in vertices)
                {
                    var newVertex = new VertexColorTextureIndex(oldVertex.Position, oldVertex.TextureCoordinate, new Vector2(currentTexture.Width, currentTexture.Height), oldVertex.Color.ToVector4(), _texturesCount - 1);

                    _vertices[_verticesCount] = newVertex;
                    _verticesCount++;
                }

                foreach (uint localElement in elements)
                {
                    uint element = localElement + _currentElement;

                    _elements[_elementsCount] = element;
                    _elementsCount++;
                }

                _currentElement += elements.Max() + 1;

                _itemsCount++;
            }

            if (_verticesCount > 0)
                Flush(lastMaterial);

            _items.Clear();

            _began = false;
        }
        
        private void Flush(Material material)
        {
            material.Apply();

            ShaderProgram program = material.ShaderProgram;

            program.SetUniform("u_Transform", _transform);
            program.SetUniform("u_Textures", _texturesCount, _textureUnits);

            _vertexArray.VertexBuffer.SetData(_verticesCount, _vertices);
            _vertexArray.ElementBuffer.SetData(_elementsCount, _elements);

            _vertexArray.Draw(PrimitiveType.Triangles);

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

        private void CheckBegin(string methodName)
        {
            if (!_began)
                throw new InvalidOperationException($"{methodName} can only be called after Begin is called");
        }
    }
}
