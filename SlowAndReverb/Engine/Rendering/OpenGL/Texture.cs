using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpet
{
    public abstract class Texture : OpenGLObject
    {
        protected const PixelInternalFormat InternalFormat = PixelInternalFormat.Rgba;
        protected const PixelFormat Format = PixelFormat.Rgba;
        protected const PixelType Type = PixelType.UnsignedByte;

        protected const TextureMinFilter DefaultMinFilter = TextureMinFilter.Nearest;
        protected const TextureMagFilter DefaultMagFilter = TextureMagFilter.Nearest;
        protected const TextureWrapMode DefaultWrapMode = TextureWrapMode.ClampToEdge;

        private TextureUnit _unit;

        protected Texture(TextureTarget target)
        {
            GL.CreateTextures(target, 1, out int handle);

            Handle = handle;
            Target = target;

            Bind();
        } 

        protected TextureTarget Target { get; private init; }

        public static void UnbindUnit(TextureTarget target, TextureUnit unit)
        {
            GL.ActiveTexture(unit);

            GL.BindTexture(target, 0);
        }

        public override void Bind()
        {
            Bind(TextureUnit.Texture0);
        }

        public void Bind(TextureUnit unit)
        {
            _unit = unit;

            base.Bind();
        }

        protected override void Bind(int handle)
        {
            GL.ActiveTexture(_unit);

            GL.BindTexture(Target, handle);
        }

        protected override void Delete(int handle)
        {
            GL.DeleteTexture(handle);
        }
    }
}
