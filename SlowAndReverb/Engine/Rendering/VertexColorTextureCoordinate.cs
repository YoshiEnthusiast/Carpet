using OpenTK.Mathematics;

namespace Carpet
{
    public readonly record struct VertexColorTextureCoordinate(Vector2 Position, Vector2 TextureCoordinate, Vector4 TextureBounds, Color Color);
}
