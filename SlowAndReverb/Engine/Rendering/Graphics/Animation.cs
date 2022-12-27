using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public struct Animation
    {
        private readonly AnimationSegment[] _segments;

        public Animation(bool loop, IEnumerable<AnimationSegment> segments)
        {
            _segments = segments.ToArray();

            Looped = loop;
        }

        public Animation(float delay, bool loop, IEnumerable<int> frameIndices)
        {
            IEnumerable<AnimationSegment> segemts = frameIndices.Select(index => new AnimationSegment(index, delay));

            this = new Animation(loop, segemts);
        }

        public Animation()
        {
            this = new Animation(false, Enumerable.Empty<AnimationSegment>());
        }

        public int SegmentsCount => _segments.Length;
        public bool Looped { get; init; }

        public AnimationSegment SegmentAt(int index)
        {
            if (index >= _segments.Length)
                return default(AnimationSegment);   

            return _segments[index];
        }
    }
}
