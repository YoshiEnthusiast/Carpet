namespace Carpet
{
    public struct SpriteBatchItem
    {
        public required Texture2D Texture { get; init; }

        public required Pointer VerticesPointer { get; init; }
        public required Pointer ElementsPointer { get; init; }

        public required Material Material { get; init; }

        public float Depth { get; init; }
    }
}
