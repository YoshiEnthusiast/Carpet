using OpenTK.Mathematics;

namespace SlowAndReverb
{
    public readonly record struct VertexColorTextureIndex(Vector2 Position, Vector2 TextureCoordinate, Vector2 TextureResolution, Vector4 Color, float TextureIndex);
}
