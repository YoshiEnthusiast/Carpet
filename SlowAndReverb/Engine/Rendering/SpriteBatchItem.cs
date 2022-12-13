using OpenTK.Mathematics;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public sealed class SpriteBatchItem
    {
        public Texture Texture { get; init; }
        public IEnumerable<VertexColorTextureCoordinate> Vertices { get; init; }
        public IEnumerable<uint> Indices { get; init; }
        public Material Material { get; init; }
        public Vector2 FrameResolution { get; init; }
        public float Depth { get; init; }
    }
}
