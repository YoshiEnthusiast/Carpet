using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace SlowAndReverb
{
    public class Renderer
    {
        private readonly List<DrawableItem> _items = new List<DrawableItem>();

        private readonly VertexArray<VertexPositionTextureCoordinate> _vertexArray;

        private readonly RenderBuffer _renderBuffer;
        private readonly FrameBuffer _frameBuffer;

        private readonly ShaderProgram _defaultShader = Content.GetShaderProgram("default");

        private readonly Vector2 _topLeftVertexPosition = Vector2.Zero;
        private readonly Vector2 _topRightVertexPosition = new Vector2(1f, 0f);
        private readonly Vector2 _bottomLeftVertexPosition = new Vector2(0f, 1f);
        private readonly Vector2 _bottomRightVertexPosition = new Vector2(1f);

        private Matrix4 _projection;
        private Matrix4 _view;

        public Renderer(bool blending)
        {
            // amogus

            if (blending)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            var indices = new uint[]
            {
                0u, 1u, 2u,
                2u, 3u, 0u
            };

            var attributes = new VertexAttribute[]
            {
                VertexAttribute.Vec2,
                VertexAttribute.Vec2
            };

            var vertexBuffer = new VertexBuffer<VertexPositionTextureCoordinate>();
            var elementBuffer = new ElementBuffer();
            _vertexArray = new VertexArray<VertexPositionTextureCoordinate>(vertexBuffer, elementBuffer, attributes);

            elementBuffer.SetData(indices);

            _renderBuffer = new RenderBuffer();
            _frameBuffer = new FrameBuffer();

            _frameBuffer.Bind();
            _frameBuffer.SetRenderBuffer(_renderBuffer);
        }

        public void Begin(Texture target, Color clearColor, int width, int height, Matrix4 viewMatrix)
        {
            if (target is not null)
            {
                _frameBuffer.Bind();
                _frameBuffer.SetTexture(target);
            }
            else
            {
                _frameBuffer.UnBind();
            }

            GL.Viewport(0, 0, width, height);
            _projection = Matrix4.CreateOrthographicOffCenter(0f, width, height, 0f, -1f, 1f);

            _renderBuffer.Bind();
            _renderBuffer.SetResolution(width, height);

            GL.ClearColor(clearColor.ToColor4());

            _view = viewMatrix;
        }

        public void Submit(Texture texture, Material material, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, float angle, float alpha, bool flipHorizontal, bool flipVertical, float depth)
        {
            int textureWidth = texture.Width;
            int textureHeight = texture.Height;

            float boundsWidth = bounds.Width;
            float boundsHeight = bounds.Height;

            float left = bounds.X / textureWidth;
            float top = bounds.Y / textureHeight;
            float right = left + boundsWidth / textureWidth;
            float bottom = top + boundsHeight / textureHeight;

            if (flipHorizontal)
                RearrangeTextureCoordinates(ref left, ref right);

            if (flipVertical)
                RearrangeTextureCoordinates(ref top, ref bottom);

            float scaleX = scale.X;
            float scaleY = scale.Y;

            float originX = Maths.Ceiling(origin.X * scaleX);
            float originY = Maths.Ceiling(origin.Y * scaleY);

            Matrix4 model = Matrix4.CreateScale(textureWidth * (boundsWidth / textureWidth) * scaleX, textureHeight * (boundsHeight / textureHeight) * scaleY, 0f);

            if (angle != 0f)
                model *= Matrix4.CreateTranslation(-originX, -originY, 0f) * Matrix4.CreateRotationZ(angle) * Matrix4.CreateTranslation(originX, originY, 0f);

            model *= Matrix4.CreateTranslation(position.X - originX, position.Y - originY, 0f);

            Matrix4 transform = model * _view * _projection;

            _items.Add(new DrawableItem(texture, material)
            {
                Alpha = alpha,
                Transform = transform,  
                Depth = depth,
                FrameResolution = new Vector2(boundsWidth, boundsHeight),
                TopLeftVertext = new VertexPositionTextureCoordinate(_topLeftVertexPosition, new Vector2(left, top)),
                TopRightVertext = new VertexPositionTextureCoordinate(_topRightVertexPosition, new Vector2(right, top)),
                BottomLeftVertext = new VertexPositionTextureCoordinate(_bottomLeftVertexPosition, new Vector2(left, bottom)),
                BottomRightVertext = new VertexPositionTextureCoordinate(_bottomRightVertexPosition, new Vector2(right, bottom)),
            });
        }

        public void Submit(Texture texture, Rectangle bounds, Vector2 position, Vector2 scale, Vector2 origin, float angle, float alpha, bool flipHorizontal, bool flipVertical, float depth)
        {
            Submit(texture, null, bounds, position, scale, origin, angle, alpha, flipHorizontal, flipVertical, depth);
        }

        public void FlushDrawCalls()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            _vertexArray.Bind();

            foreach (DrawableItem item in _items.OrderBy(item => item.Depth))
            {
                var vertices = new VertexPositionTextureCoordinate[]
                {
                    item.TopLeftVertext,
                    item.TopRightVertext,
                    item.BottomRightVertext,
                    item.BottomLeftVertext
                };

                ShaderProgram shaderProgram = _defaultShader;
                Material material = item.Material;

                if (material is not null)
                {
                    shaderProgram = material.ShaderProgram;

                    material.Apply();
                }

                Render(item.Texture, item.FrameResolution, shaderProgram, vertices, item.Transform, item.Alpha);

                material?.Unapply();
            }

            _items.Clear();
        }

        private void Render(Texture texture, Vector2 frameResolution, ShaderProgram shaderProgram, VertexPositionTextureCoordinate[] vertices, Matrix4 transform, float alpha)
        {
            _vertexArray.VertexBuffer.SetData(vertices);

            texture.Bind();
            shaderProgram.Bind();

            shaderProgram.SetUniform("u_Transform", transform);
            shaderProgram.SetUniform("u_Alpha", alpha);
            shaderProgram.SetUniform("u_TexRes", new Vector2(texture.Width, texture.Height));
            shaderProgram.SetUniform("u_FrameRes", frameResolution);

            _vertexArray.Draw(PrimitiveType.Triangles);

            shaderProgram.UnBind();
            texture.UnBind();
        }

        private void RearrangeTextureCoordinates(ref float start, ref float end)
        {
            float temp = start;

            start = end;
            end = temp;
        }
    }
}
