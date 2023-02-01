using OpenTK.Graphics.OpenGL;

namespace SlowAndReverb
{
    public sealed class BlendMode
    {
        public static readonly BlendMode AlphaBlend = new BlendMode()
        {
            SourceFactor = BlendingFactor.SrcAlpha,

            DestinationFactor = BlendingFactor.OneMinusSrcAlpha
        };

        public static readonly BlendMode Additive = new BlendMode()
        {
            SourceFactor = BlendingFactor.One,

            DestinationFactor = BlendingFactor.One
        };

        public BlendMode(BlendingFactor sourceFactor, BlendingFactor destinationFactor)
        {
            SourceFactor = 
        }

        public BlendingFactor SourceFactor { get; init; }
        public BlendingFactor DestinationFactor { get; init; }
    }
}
