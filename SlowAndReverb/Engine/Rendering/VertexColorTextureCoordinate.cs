using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public readonly record struct VertexColorTextureCoordinate(Vector2 Position, Vector2 TextureCoordinate, Vector4 TextureBounds, Color Color);
}
