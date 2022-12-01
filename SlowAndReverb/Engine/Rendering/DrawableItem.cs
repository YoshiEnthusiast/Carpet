using OpenTK;
using OpenTK.Mathematics;

namespace SlowAndReverb
{
    public sealed class DrawableItem
    {
        private readonly Texture _texture;
        private readonly Material _material;

        public DrawableItem(Texture texture, Material material)
        {
            _texture = texture;
            _material = material;
        }

        public Texture Texture => _texture;
        public Material Material => _material;

        public float Depth { get; set; }
        public float Alpha { get; set; }
        public Vector2 FrameResolution { get; set; }
        public Matrix4 Transform { get; set; }

        public VertexPositionTextureCoordinate TopLeftVertext { get; set; }
        public VertexPositionTextureCoordinate TopRightVertext { get; set; }
        public VertexPositionTextureCoordinate BottomLeftVertext { get; set; }
        public VertexPositionTextureCoordinate BottomRightVertext { get; set; }
    }
}
