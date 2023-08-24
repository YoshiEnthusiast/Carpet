using OpenTK.Graphics.OpenGL;

namespace SlowAndReverb
{
    public sealed class PipelineShaderProgram : ShaderProgram
    {
        public PipelineShaderProgram(string vertexSource, string fragmentSource, string geometrySource)
        {
            bool geometrySourceExists = geometrySource is not null;

            int vertexShader = CreateShader(vertexSource, ShaderType.VertexShader);
            int fragmentShader = CreateShader(fragmentSource, ShaderType.FragmentShader);
            int geometryShader = 0;

            if (geometrySourceExists)
                geometryShader = CreateShader(geometrySource, ShaderType.GeometryShader);

            GL.LinkProgram(Handle);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            if (geometrySourceExists)
                GL.DeleteShader(geometryShader);

            ScanUniforms();
        }
    }
}