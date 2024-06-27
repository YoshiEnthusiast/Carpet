using OpenTK.Mathematics;

namespace Carpet
{
    public readonly record struct VertexColorTextureIndex(Vector3 Position, Vector2 TextureCoordinate, Vector2 TextureResolution, Vector4 TextureBounds, Vector4 Color, float TextureIndex);
}
