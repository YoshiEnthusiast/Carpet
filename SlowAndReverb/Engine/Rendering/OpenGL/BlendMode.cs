﻿using OpenTK.Graphics.OpenGL;

namespace SlowAndReverb
{
    public sealed class BlendMode
    {
        public static readonly BlendMode AlphaBlend = new BlendMode()
        {
            SourceFactor = BlendingFactor.One,

            DestinationFactor = BlendingFactor.OneMinusSrcAlpha
        };

        public static readonly BlendMode Additive = new BlendMode()
        {
            SourceFactor = BlendingFactor.One,

            DestinationFactor = BlendingFactor.One
        };

        public static readonly BlendMode NotPremultiplied = new BlendMode()
        {
            SourceFactor = BlendingFactor.SrcAlpha,

            DestinationFactor = BlendingFactor.OneMinusSrcAlpha
        };

        public static readonly BlendMode Opaque = new BlendMode()
        {
            SourceFactor = BlendingFactor.One,

            DestinationFactor = BlendingFactor.Zero
        };

        public BlendMode(BlendingFactor sourceFactor, BlendingFactor destinationFactor)
        {
            SourceFactor = sourceFactor;
            DestinationFactor = destinationFactor;
        }

        public BlendMode()
        {

        }

        public BlendingFactor SourceFactor { get; init; }
        public BlendingFactor DestinationFactor { get; init; }
    }
}
