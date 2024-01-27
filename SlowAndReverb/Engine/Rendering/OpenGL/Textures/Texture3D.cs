using OpenTK.Graphics.OpenGL;

namespace Carpet
{
    public class Texture3D : Texture
    {
        public Texture3D(int width, int height, int depth) : base(TextureTarget.Texture3D)
        {
            Width = width;
            Height = height;
            Depth = depth;

            GL.TexImage3D(Target, 0, InternalFormat, width, height, depth, 0, Format, Type, 0);
        }

        public int Width { get; private init; }
        public int Height { get; private init; }
        public int Depth { get; private init; }

        public void SetData(byte[] data)
        {
            GL.TexSubImage3D(Target, 0, 0, 0, 0, Width, Height, Depth, Format, Type, data);
        }

        protected override void SetParameters()
        {
            base.SetParameters();

            SetParameter(TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToBorder);
        }
    }
}
