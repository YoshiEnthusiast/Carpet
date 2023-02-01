using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    // Fix multiple textures thing!!!!
    public class SpriteBatch
    {
        private const int MaxItems = 1000;
        private const int MaxVertices = MaxItems * 4;
        private const int MaxElements = MaxVertices * 6;

        private readonly List<SpriteBatchItem> _items = new List<SpriteBatchItem>();

        private readonly Material _basicMaterial = new BasicMaterial();

        private readonly VertexColorTextureIndex[] _vertices = new VertexColorTextureIndex[MaxVertices];
        private readonly uint[] _elements = new uint[MaxElements];
        private readonly int[] _textureUnits;

        private readonly VertexArray<VertexColorTextureIndex> _vertexArray;
        private readonly RenderBuffer _renderBuffer;
        private readonly FrameBuffer _frameBuffer;

        private readonly uint[] _quadElements = new uint[]
        {
            0u,       
            1u,          
            2u,
            2u, 
            3u, 
            1u
        };

        private RenderTarget _renderTarget;

        private Matrix4 _transform;

        private int _verticesCount;
        private int _elementsCount;
        private int _texturesCount;

        private uint _currentElement;
        private int _itemsCount;

        private bool _began;

        public SpriteBatch()
        {
            GL.Enable(EnableCap.ScissorTest);
            GL.Enable(EnableCap.Blend);

            var attributes = new VertexAttribute[]
            {
                VertexAttribute.Vec2,
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
        }

        public void Begin(RenderTarget target, BlendMode blendMode, Color clearColor, Rectangle? scissor, Matrix4? view)
        {
            if (_began)
                throw new InvalidOperationException($"{nameof(Begin)} can only be called again after End is called.");

            GL.BlendFunc(blendMode.SourceFactor, blendMode.DestinationFactor);

            Texture targetTexture = target.Texture;

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

            GL.Viewport(0, 0, targetWidth, targetHeight);

            _renderBuffer.Bind();
            _renderBuffer.SetResolution(targetWidth, targetHeight);

            GL.ClearColor(clearColor.ToColor4());

            Rectangle scissorRectangle = scissor.GetValueOrDefault(new Rectangle(0f, 0f, targetWidth, targetHeight)); 
            GL.Scissor((int)scissorRectangle.Left, (int)scissorRectangle.Top, (int)scissorRectangle.Width, (int)scissorRectangle.Height);

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0f, targetWidth, targetHeight, 0f, -1f, 1f);
            _transform = view.GetValueOrDefault(Matrix4.Identity) * projection;

            _began = true;
        }

        public void Submit(Texture texture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 localOrigin, Color color, float angle, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
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

            float spriteScaleX = scale.RoundedX;
            float spriteScaleY = scale.RoundedY;

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

            var vertices = new VertexColorTextureCoordinate[]
            {
                new VertexColorTextureCoordinate(topLeft, new Vector2(textureLeft, textureTop), textureBounds, color),
                new VertexColorTextureCoordinate(topRight, new Vector2(textureRight, textureTop), textureBounds, color),
                new VertexColorTextureCoordinate(bottomLeft, new Vector2(textureLeft, textureBottom), textureBounds, color),
                new VertexColorTextureCoordinate(bottomRight, new Vector2(textureRight, textureBottom), textureBounds, color)
            };

            Submit(texture, material, vertices, _quadElements, depth);
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

        public void Submit(Texture texture, Vector2 position, SpriteEffect horizontalEffect, SpriteEffect verticalEffect, float depth)
        {
            Submit(texture, null, new Rectangle(0f, 0f, texture.Width, texture.Height), position, Vector2.One, Vector2.Zero, Color.White, 0f, horizontalEffect, verticalEffect, depth);
        }

        public void Submit(Texture texture, Vector2 position, float depth)
        {
            Submit(texture, position, SpriteEffect.None, SpriteEffect.None, depth);
        }

        public void Submit(Texture texture, Material material, VertexColorTextureCoordinate vertex1, VertexColorTextureCoordinate vertex2, VertexColorTextureCoordinate vertex3, VertexColorTextureCoordinate vertex4, float depth)
        {
            var vertices = new VertexColorTextureCoordinate[]
            {
                vertex1,
                vertex2,
                vertex3,
                vertex4
            };

            Submit(texture, material, vertices, _quadElements, depth);
        }

        public void Submit(Texture texture, Material material, VertexColorTextureCoordinate[] vertices, uint[] elements, float depth)
        {
            CheckBegin(nameof(Submit));

            if (vertices.Length < 1 || elements.Length < 1)
                return;

            var item = new SpriteBatchItem()
            {
                Texture = texture,
                Vertices = vertices,
                Indices = elements,
                Material = material is null ? _basicMaterial : material,
                Depth = depth
            };

            _items.Add(item);
        }

        public void End()
        {
            CheckBegin(nameof(End));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            IEnumerable<SpriteBatchItem> orderedItems = _items.OrderBy(item => item.Depth)
                 .ThenBy(item => item.Texture.GetHashCode());

            _vertexArray.Bind();

            Texture lastTexture = null;
            Material lastMaterial = null;

            foreach (SpriteBatchItem item in orderedItems)
            {
                Texture currentTexture = item.Texture;
                Material currentMaterial = item.Material;

                ShaderProgram currentProgram = currentMaterial.ShaderProgram;
                ShaderProgram lastProgram = lastMaterial?.ShaderProgram;

                IEnumerable<VertexColorTextureCoordinate> vertices = item.Vertices;
                IEnumerable<uint> elements = item.Indices;

                int verticesCount = vertices.Count();
                int elementsCount = elements.Count();

                int maxTextureUnits = OpenGL.MaxTextureUnits;
                int extraTextures = currentMaterial.TextureUniformsCount;
                int maxTextures = maxTextureUnits - extraTextures;

                if (lastMaterial is not null && currentMaterial != lastMaterial || currentTexture != lastTexture && _texturesCount >= maxTextures || _verticesCount + verticesCount > MaxVertices || _elementsCount + elementsCount > MaxElements || _itemsCount >= MaxItems)
                    Flush(lastMaterial);

                if (currentMaterial != lastMaterial)
                {
                    if (currentProgram != lastProgram)
                    {
                        currentProgram.Bind();
                        currentProgram.SetUniform("u_Transform", _transform);

                        lastTexture = null;
                    }

                    currentMaterial.Apply();

                    lastMaterial = currentMaterial;
                }

                if (currentTexture != lastTexture)
                {
                    int unit = _texturesCount + extraTextures;

                    currentTexture.Bind(TextureUnit.Texture0 + unit);

                    lastTexture = currentTexture;
                    _texturesCount++;
                }

                foreach (VertexColorTextureCoordinate oldVertex in vertices)
                {
                    var newVertex = new VertexColorTextureIndex(oldVertex.Position, oldVertex.TextureCoordinate, new Vector2(currentTexture.Width, currentTexture.Height), oldVertex.TextureBounds, oldVertex.Color.ToVector4(), _texturesCount - 1 + extraTextures);

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
            ShaderProgram program = material.ShaderProgram;

            program.SetUniform("u_Textures", _textureUnits.Length, _textureUnits); 

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
