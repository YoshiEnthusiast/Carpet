using OpenTK.Mathematics;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public sealed class SpriteBatchItem
    {
        public required Texture Texture { get; init; }
        public required IEnumerable<VertexColorTextureCoordinate> Vertices { get; init; }
        public required IEnumerable<uint> Indices { get; init; }
        public required Material Material { get; init; }
        public Vector2 FrameResolution { get; init; }
        public float Depth { get; init; }
    }
}
