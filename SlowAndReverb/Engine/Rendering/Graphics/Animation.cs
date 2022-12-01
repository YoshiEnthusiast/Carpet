using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public struct Animation
    {
        private readonly IEnumerable<AnimationSegment> _segments;

        public Animation(bool loop, IEnumerable<AnimationSegment> segments)
        {
            _segments = segments;
            Looped = loop;
        }

        public Animation(int delay, bool loop, IEnumerable<int> frameIndices)
        {
            IEnumerable<AnimationSegment> segemts = frameIndices.Select(index => new AnimationSegment(index, delay));

            this = new Animation(loop, segemts);
        }

        public Animation()
        {
            this = new Animation(false, Enumerable.Empty<AnimationSegment>());
        }

        public IEnumerable<AnimationSegment> Segments => _segments;
        public bool Looped { get; init; }

        public override bool Equals(object obj)
        {
            return obj is Animation animation && Equals(animation);
        }

        public override int GetHashCode()
        {
            return Segments.GetHashCode() + GetHashCode() + Looped.GetHashCode();
        }

        public bool Equals(Animation animation)
        {
            return Looped == animation.Looped && Enumerable.SequenceEqual(Segments, animation.Segments);
        }

        public static bool operator ==(Animation animation, Animation other)
        {
            return animation.Equals(other);
        }

        public static bool operator !=(Animation animation, Animation other)
        {
            return !animation.Equals(other);
        }
    }
}
