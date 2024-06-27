using OpenTK.Graphics.OpenGL;

namespace Carpet
{
    public class Texture3D : Texture
    {
        public Texture3D(int width, int height, int depth,
            TextureMinFilter minFilter, TextureMagFilter magFilter,
            TextureWrapMode wrapS, TextureWrapMode wrapT, TextureWrapMode wrapR) : base(TextureTarget.Texture3D)
        {
            Width = width;
            Height = height;
            Depth = depth;

            GL.TexImage3D(Target, 0, InternalFormat, width, height, depth, 0, Format, Type, 0);

            GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)magFilter);

            GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)wrapS);
            GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)wrapT);
            GL.TexParameter(Target, TextureParameterName.TextureWrapR, (int)wrapR);
        }

        public Texture3D(int width, int height, int depth) 
            : this(width, height, depth, DefaultMinFilter, DefaultMagFilter,
                  DefaultWrapMode, DefaultWrapMode, DefaultWrapMode)
        {
            
        }

        public int Width { get; private init; }
        public int Height { get; private init; }
        public int Depth { get; private init; }

        public void SetData(byte[] data)
        {
            GL.TexSubImage3D(Target, 0, 0, 0, 0, Width, Height, Depth, Format, Type, data);
        }
    }
}
