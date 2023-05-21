using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using System;
using System.Runtime.InteropServices;

namespace SlowAndReverb
{
    public static class OpenGL
    {
        private static IGLFWGraphicsContext s_context;

        public static int MaxTextureUnits { get; private set; }
        public static int MaxTextureSize { get; private set; }

        internal static void Initialize(IGLFWGraphicsContext context)
        {
            s_context = context;
            s_context.MakeCurrent();

            GL.Enable(EnableCap.ScissorTest);
            GL.Enable(EnableCap.Blend);

            GL.Enable(EnableCap.DebugOutput);

            GL.Khr.DebugMessageCallback(OnDebugMessage, IntPtr.Zero);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.GetInteger(GetPName.MaxTextureImageUnits, out int maxTextureUnits);
            GL.GetInteger(GetPName.MaxTextureSize, out int maxTextureSize);

            MaxTextureUnits = maxTextureUnits;
            MaxTextureSize = maxTextureSize;

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
    }
}
