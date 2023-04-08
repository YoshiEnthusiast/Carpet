using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public sealed class EntityMap : DynamicCollection<Entity>
    {
        private readonly Dictionary<Vector2, HashSet<Entity>> _buckets = new Dictionary<Vector2, HashSet<Entity>>();

        private readonly World _world;
        private readonly float _bucketSize;

        public EntityMap(World world, float bucketSize)
        {
            _world = world;
            _bucketSize = bucketSize;
        }

        public override void Clear()
        {
            base.Clear();

            _buckets.Clear();
        }

        public IEnumerable<Entity> GetNearby(Entity entity)
        {
            return GetNearby(entity.Rectangle);
        }

        public IEnumerable<Entity> GetNearby(Rectangle rectangle)
        {
            var result = new HashSet<Entity>();

            foreach (Vector2 hash in GetAffectedBucketsHash(rectangle))
                if (_buckets.TryGetValue(hash, out HashSet<Entity> entities))
                    foreach (Entity entity in entities)
                        result.Add(entity);

            return result;
        }

        public void DrawBuckets(float depth)
        {
            foreach (Vector2 hash in _buckets.Keys)
            {
                Vector2 position = hash * _bucketSize;
                HashSet<Entity> entities = _buckets[hash];

                Color color = entities.Any() ? Color.Yellow : Color.Grey;

                Graphics.DrawRectangle(new Rectangle(position, position + new Vector2(_bucketSize)), color, depth);
                Graphics.DrawString($"{hash.X} {hash.Y}", position + new Vector2(2f), depth);
            }
        }

        protected override void OnUpdate()
        {
            foreach (Entity entity in this)
            {
                if (entity.NeverMoves)
                    continue;

                RemoveFromBuckets(entity);

                AddToBuckets(entity);
            }
        }

        protected override void OnItemAdded(Entity entity)
        {
            AddToBuckets(entity);

            _world.OnEntityAdded(entity);
        }

        protected override void OnItemRemoved(Entity entity)
        {
            RemoveFromBuckets(entity);

            _world.OnEntityRemoved(entity);
        }

        private void AddToBuckets(Entity entity)
        {
            foreach (Vector2 hash in GetAffectedBucketsHash(entity))
            {
                if (_buckets.TryGetValue(hash, out HashSet<Entity> entities))
                {
                    entities.Add(entity);

                    continue;
                }

                _buckets.Add(hash, new HashSet<Entity>()
                {
                    entity
                });
            }
        }

        private void RemoveFromBuckets(Entity entity)
        {
            // Slow.    Dictionary<Entity, IEnumerable<Vector2>> ???????
            foreach (HashSet<Entity> entities in _buckets.Values)
                entities.Remove(entity);

            //foreach (Vector2 hash in GetAffectedBucketsHash(entity))
            //    if (_buckets.TryGetValue(hash, out HashSet<Entity> entities))
            //        entities.Remove(entity);
        }

        private IEnumerable<Vector2> GetAffectedBucketsHash(Entity entity)
        {
            return GetAffectedBucketsHash(entity.Rectangle);
        }

        private IEnumerable<Vector2> GetAffectedBucketsHash(Rectangle rectangle)
        {
            Vector2 topLeft = GetHash(rectangle.TopLeft);
            Vector2 bottomRight = GetHash(rectangle.BottomRight);

            for (int x = (int)topLeft.X; x < (int)bottomRight.X + 1; x++)
                for (int y = (int)topLeft.Y; y < (int)bottomRight.Y + 1; y++)
                    yield return new Vector2(x, y);
        }

        private Vector2 GetHash(Vector2 position)
        {
            return (position / _bucketSize).Round();
        }
    }
}
