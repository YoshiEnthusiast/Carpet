using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace SlowAndReverb
{
    public sealed class SpriteBatchItem
    {
        public required Texture2D Texture { get; init; }

        public required IEnumerable<VertexColorTextureCoordinate> Vertices { get; init; }
        public required IEnumerable<uint> Indices { get; init; }

        public required Material Material { get; init; }
        public required Rectangle Scissor { get; init; }

        public float Depth { get; init; }
    }
}
