using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace SlowAndReverb
{
    public class ComputeShaderProgram : ShaderProgram
    {
        public ComputeShaderProgram(string source)
        {
            int shader = CreateShader(source, ShaderType.ComputeShader);

            GL.LinkProgram(Handle);
            GL.DeleteShader(shader);

            ScanUniforms();
        }

        public void Compute(int width, int height, int depth)
        {
            GL.DispatchCompute(width, height, depth);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        }
    }
}
