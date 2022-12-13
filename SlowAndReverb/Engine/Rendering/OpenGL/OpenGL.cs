using OpenTK.Graphics.OpenGL;

namespace SlowAndReverb
{
    public static class OpenGL
    {
        private static int s_maxTextureUnits;
        private static int s_maxTextureSize;

        public static void Initialize()
        {
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.GetInteger(GetPName.MaxTextureImageUnits, out s_maxTextureUnits);
            GL.GetInteger(GetPName.MaxTextureSize, out s_maxTextureSize);
        }

        public static int MaxTextureUnits => s_maxTextureUnits;
        public static int MaxTextureSize => s_maxTextureSize;
    }
}
