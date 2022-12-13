using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using StbImageWriteSharp;
using System;
using System.IO;
using ImageReadColorComponents = StbImageSharp.ColorComponents;
using ImageWriteColorComponents = StbImageWriteSharp.ColorComponents;

namespace SlowAndReverb
{
    public class Texture : OpenGLObject
    {
        private readonly int _width;
        private readonly int _height;

        private readonly int _colorComponentsCount = 4;

        private TextureUnit? _unit;

        private Texture(int handle, int width, int height)
        {
            Handle = handle;

            _width = width;
            _height = height;
        }

        public int Width => _width;
        public int Height => _height;

        public static Texture CreateEmpty(int width, int height)
        {
            return FromBytes(width, height, null, PixelFormat.Rgba);
        }

        public static Texture FromBytes(int width, int height, byte[] buffer, PixelFormat format)
        {
            int handle = Create();

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, format, PixelType.UnsignedByte, buffer);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle, width, height);
        }

        public static Texture FromBytes(int width, int height, IntPtr buffer, PixelFormat format)
        {
            int handle = Create();

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, format, PixelType.UnsignedByte, buffer);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle, width, height);
        }

        public static Texture FromStbImage(ImageResult image)
        {
            int width = image.Width;
            int height = image.Height;

            return FromBytes(width, height, image.Data, PixelFormat.Rgba);
        }

        public static Texture FromStream(Stream stream)
        {
            ImageResult image = ImageResult.FromStream(stream, ImageReadColorComponents.RedGreenBlueAlpha);

            return FromStbImage(image);
        }

        public static Texture FromFile(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return FromStream(stream);
        }

        public void SaveAsPng(Stream stream)
        {
            int bufferSize = _width * _height * _colorComponentsCount;
            var buffer = new byte[bufferSize];

            GL.GetTextureImage(Handle, 0, PixelFormat.Rgba, PixelType.UnsignedByte, bufferSize, buffer);

            var writer = new ImageWriter();
            writer.WritePng(buffer, _width, _height, ImageWriteColorComponents.RedGreenBlueAlpha, stream);
        }

        public void SaveAsPng(string fileName)
        {
            using (FileStream stream = File.OpenWrite(fileName))
                SaveAsPng(stream);
        }

        public void Bind(TextureUnit unit)
        {
            _unit = unit;
            Bind(Handle);
        }

        public override void Bind()
        {
            Bind(TextureUnit.Texture0);
        }

        public override void UnBind()
        {
            base.UnBind();

            _unit = null;
        }

        protected override void Bind(int handle)
        {
            GL.ActiveTexture(_unit.Value);

            GL.BindTexture(TextureTarget.Texture2D, handle);
        }

        private static int Create()
        {
            GL.CreateTextures(TextureTarget.Texture2D, 1, out int handle);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            SetParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            SetParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            SetParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            SetParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            return handle;
        }

        private static void SetParameter(TextureParameterName name, int value)
        {
            GL.TexParameter(TextureTarget.Texture2D, name, value);
        }
    }
}
