using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using System;
using System.Runtime.InteropServices;

namespace SlowAndReverb
{
    public static class OpenGL
    {
        private static IGLFWGraphicsContext s_context;

        private static int s_maxTextureUnits;
        private static int s_maxTextureSize;

        internal static void Initialize(IGLFWGraphicsContext context)
        {
            s_context = context;
            s_context.MakeCurrent();

            GL.Enable(EnableCap.DebugOutput);

            GL.Khr.DebugMessageCallback(OnDebugMessage, IntPtr.Zero);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.GetInteger(GetPName.MaxTextureImageUnits, out s_maxTextureUnits);
            GL.GetInteger(GetPName.MaxTextureSize, out s_maxTextureSize);

            Console.WriteLine("OpenGL initialized");
        }

        public static void SwapBuffers()
        {
            s_context.SwapBuffers();
        }

        private static void OnDebugMessage(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr messagepoPointer, IntPtr parameterPointer)
        {
            string message = Marshal.PtrToStringAnsi(messagepoPointer);

            Console.WriteLine($"[OpenGL] {message}");
        }

        public static int MaxTextureUnits => s_maxTextureUnits;
        public static int MaxTextureSize => s_maxTextureSize;
    }
}
