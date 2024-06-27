using OpenTK.Graphics.OpenGL;

namespace Carpet
{
    public sealed class RenderBuffer : OpenGLObject
    {
        public RenderBuffer()
        {
            GL.GenRenderbuffers(1, out int handle);
            Handle = handle;
        }

        public void SetResolution(int width, int height)
        {
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        }

        protected override void Bind(int handle)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handle);
        }
    }
}
