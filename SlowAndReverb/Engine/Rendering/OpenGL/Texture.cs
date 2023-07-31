using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public abstract class Texture : OpenGLObject
    {
        protected const PixelInternalFormat InternalFormat = PixelInternalFormat.Rgba;
        protected const PixelFormat Format = PixelFormat.Rgba;
        protected const PixelType Type = PixelType.UnsignedByte;

        private TextureUnit _unit;

        protected Texture(TextureTarget target)
        {
            GL.CreateTextures(target, 1, out int handle);

            Handle = handle;
            Target = target;

            Bind();
            SetParameters();
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

        protected virtual void SetParameters()
        {
            SetParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            SetParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            SetParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            SetParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        }

        protected void SetParameter(TextureParameterName parameter, int value)
        {
            GL.TexParameter(Target, parameter, value);
        }
    }
}
