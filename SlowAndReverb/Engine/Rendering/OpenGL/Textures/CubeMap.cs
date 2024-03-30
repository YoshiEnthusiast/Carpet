using OpenTK.Graphics.OpenGL;

namespace Carpet
{
    public class CubeMap : Texture
    {
        private const int Faces = 6;

        public CubeMap(int size, TextureMinFilter minFilter, TextureMagFilter magFilter,
            TextureWrapMode wrapS, TextureWrapMode wrapT, TextureWrapMode wrapR) : base(TextureTarget.TextureCubeMap)
        {
            Size = size;

            for (int i = 0; i < Faces; i++)
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, InternalFormat, size, size, 0, Format, Type, 0);

            GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)magFilter);

            GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)wrapS);
            GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)wrapT);
            GL.TexParameter(Target, TextureParameterName.TextureWrapR, (int)wrapR);
        }

        public CubeMap(int size)
            : this(size, DefaultMinFilter, DefaultMagFilter,
                  DefaultWrapMode, DefaultWrapMode, DefaultWrapMode)
        {
            
        }

        public int Size { get; private init; }

        public void SetPositiveX(byte[] buffer)
        {
            SetFace(TextureTarget.TextureCubeMapPositiveX, buffer);
        }

        public void SetPositiveY(byte[] buffer)
        {
            SetFace(TextureTarget.TextureCubeMapPositiveY, buffer);
        }

        public void SetPositiveZ(byte[] buffer)
        {
            SetFace(TextureTarget.TextureCubeMapPositiveZ, buffer);
        }

        public void SetNegativeX(byte[] buffer)
        {
            SetFace(TextureTarget.TextureCubeMapNegativeX, buffer);
        }

        public void SetNegativeY(byte[] buffer)
        {
            SetFace(TextureTarget.TextureCubeMapNegativeY, buffer);
        }

        public void SetNegativeZ(byte[] buffer)
        {
            SetFace(TextureTarget.TextureCubeMapNegativeZ, buffer);
        }

        private void SetFace(TextureTarget face, byte[] buffer)
        {
            GL.TexSubImage2D(face, 0, 0, 0, Size, Size, Format, Type, buffer);
        }
    }
}
