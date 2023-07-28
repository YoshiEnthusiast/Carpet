using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public sealed class FrameBuffer : OpenGLObject
    {
        public FrameBuffer()
        {
            GL.GenFramebuffers(1, out int handle);
            Handle = handle;
        }

        public void SetTexture(Texture2D texture)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture.Handle, 0);
        }

        public void SetRenderBuffer(RenderBuffer buffer)
        {
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, buffer.Handle);    
        }

        protected override void Bind(int handle)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);  
        }
    }
}
