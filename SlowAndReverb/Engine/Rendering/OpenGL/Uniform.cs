using OpenTK.Graphics.OpenGL;

namespace SlowAndReverb
{
    public readonly record struct Uniform(string Name, ActiveUniformType Type, int Location);
}
