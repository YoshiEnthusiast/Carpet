using OpenTK.Graphics.OpenGL;

namespace SlowAndReverb
{
    public class CubeMap : Texture
    {
        private const int Faces = 6;

        public CubeMap(int size) : base(TextureTarget.TextureCubeMap)
        {
            Size = size;

            for (int i = 0; i < Faces; i++)
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, InternalFormat, size, size, 0, Format, Type, 0);
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

        protected override void SetParameters()
        {
            base.SetParameters();

            SetParameter(TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToBorder);
        }

        private void SetFace(TextureTarget face, byte[] buffer)
        {
            GL.TexSubImage2D(face, 0, 0, 0, Size, Size, Format, Type, buffer);
        }
    }
}
