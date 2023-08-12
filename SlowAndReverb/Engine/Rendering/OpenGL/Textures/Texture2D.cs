﻿using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using StbImageWriteSharp;
using System;
using System.IO;
using ImageReadColorComponents = StbImageSharp.ColorComponents;
using ImageWriteColorComponents = StbImageWriteSharp.ColorComponents;

namespace SlowAndReverb
{
    public class Texture2D : Texture
    {
        private readonly int _colorComponentsCount = 4;

        private Texture2D(int width, int height, byte[] buffer) : base(TextureTarget.Texture2D)
        {
            Width = width;
            Height = height;

            GL.TexImage2D(Target, 0, InternalFormat, width, height, 0, Format, Type, buffer);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        private Texture2D(int width, int height, IntPtr buffer) : base(TextureTarget.Texture2D)
        {
            Width = width;
            Height = height;

            GL.TexImage2D(Target, 0, InternalFormat, width, height, 0, Format, Type, buffer);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public int Width { get; private init; }
        public int Height { get; private init; }

        public static Texture2D CreateEmpty(int width, int height)
        {
            return FromBytes(width, height, null);
        }

        public static Texture2D FromBytes(int width, int height, byte[] buffer)
        {
            CheckSize(width, height);

            return new Texture2D(width, height, buffer);
        }

        public static Texture2D FromBytes(int width, int height, IntPtr buffer)
        {
            CheckSize(width, height);

            return new Texture2D(width, height, buffer);
        }

        public static Texture2D FromStbImage(ImageResult image)
        {
            int width = image.Width;
            int height = image.Height;

            return FromBytes(width, height, image.Data);
        }

        public static Texture2D FromStream(Stream stream)
        {
            ImageResult image = ImageResult.FromStream(stream, ImageReadColorComponents.RedGreenBlueAlpha);

            return FromStbImage(image);
        }

        public static Texture2D FromFile(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return FromStream(stream);
        }

        public void SetData(byte[] buffer)
        {
            GL.TexSubImage2D(Target, 0, 0, 0, Width, Height, Format, Type, buffer);
        }

        public void SaveAsPng(Stream stream)
        {
            if (Deleted)
                return;

            int bufferSize = Width * Height * _colorComponentsCount;
            var buffer = new byte[bufferSize];

            GL.GetTextureImage(Handle, 0, Format, Type, bufferSize, buffer);

            var writer = new ImageWriter();
            writer.WritePng(buffer, Width, Height, ImageWriteColorComponents.RedGreenBlueAlpha, stream);
        }

        public void SaveAsPng(string fileName)
        {
            using (FileStream stream = File.OpenWrite(fileName))
                SaveAsPng(stream);
        }

        private static void CheckSize(int width, int height)
        {
            int maxSize = OpenGL.MaxTextureSize;

            if (width > maxSize || height > maxSize)
                throw new InvalidOperationException($"Max texture size ({maxSize}) exceeded. Given size: {width}, {height}");
        }
    }
}