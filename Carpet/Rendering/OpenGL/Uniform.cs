using OpenTK.Graphics.OpenGL;

namespace Carpet
{
    public readonly record struct Uniform(string Name, ActiveUniformType Type, int Location);
}
