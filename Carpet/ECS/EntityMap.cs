using System.Collections.Generic;

namespace Carpet
{
    public sealed class EntityMap : DynamicCollection<Entity>
    {
        private readonly Dictionary<Vector2, HashSet<Entity>> _buckets = [];
        private readonly Dictionary<Entity, List<HashSet<Entity>>> _previousBuckets = [];

        private readonly List<Vector2> _hashBuffer = [];

        private readonly Scene _scene;
        private readonly float _bucketSize;

        public EntityMap(Scene scene, float bucketSize)
        {
            _scene = scene;
            _bucketSize = bucketSize;
        }

        public override void Clear()
        {
            base.Clear();

            _buckets.Clear();
        }

        public HashSet<Entity> GetNearby(Entity entity, HashSet<Entity> buffer)
        {
            return GetNearby<Entity>(entity.Rectangle, buffer);
        }

        public HashSet<T> GetNearby<T>(Rectangle rectangle, HashSet<T> buffer) where T : Entity
        {
            if (buffer is null)
                buffer = new HashSet<T>();

            buffer.Clear();

            _hashBuffer.Clear();
            GetAffectedBucketsHash(_hashBuffer, rectangle);

            foreach (Vector2 hash in _hashBuffer)
            {
                if (!_buckets.TryGetValue(hash, out HashSet<Entity> entities))
                    continue;

                foreach (Entity entity in entities)
                {
                    if (entity is not T entityOfType)
                        continue;

                    buffer.Add(entityOfType);
                }
            }

            return buffer;
        }

        public HashSet<Entity> GetNearby(Rectangle rectangle, HashSet<Entity> buffer)
        {
            return GetNearby<Entity>(rectangle, buffer);
        }

        public void DrawBuckets(float depth)
        {
            foreach (Vector2 hash in _buckets.Keys)
            {
                Vector2 position = hash * _bucketSize;
                HashSet<Entity> entities = _buckets[hash];

                Color color = Color.Gray;

                if (entities.Count > 0)
                    color = Color.Yellow;

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

            _scene.OnEntityAdded(entity);
        }

        protected override void OnItemRemoved(Entity entity)
        {
            RemoveFromBuckets(entity);

            _scene.OnEntityRemoved(entity);
        }

        private void AddToBuckets(Entity entity)
        {
            _hashBuffer.Clear();
            GetAffectedBucketsHash(_hashBuffer, entity);

            List<HashSet<Entity>> buckets;

            if (!_previousBuckets.TryGetValue(entity, out buckets))
            {
                buckets = new List<HashSet<Entity>>();

                _previousBuckets[entity] = buckets;
            }

            foreach (Vector2 hash in _hashBuffer)
            {
                if (_buckets.TryGetValue(hash, out HashSet<Entity> bucket))
                {
                    bucket.Add(entity);
                }
                else
                {
                    bucket = new HashSet<Entity>()
                    {
                        entity
                    };

                    _buckets[hash] = bucket;
                }

                buckets.Add(bucket);
            }
        }

        private void RemoveFromBuckets(Entity entity)
        {
            foreach (HashSet<Entity> bucket in _previousBuckets[entity])
                bucket.Remove(entity);
        }

        private void GetAffectedBucketsHash(List<Vector2> buffer, Entity entity)
        {
            GetAffectedBucketsHash(buffer, entity.Rectangle);
        }

        private void GetAffectedBucketsHash(List<Vector2> buffer, Rectangle rectangle)
        {
            Vector2 topLeft = GetHash(rectangle.TopLeft);
            Vector2 bottomRight = GetHash(rectangle.BottomRight);

            for (int x = (int)topLeft.X; x < (int)bottomRight.X + 1; x++)
            {
                for (int y = (int)topLeft.Y; y < (int)bottomRight.Y + 1; y++)
                {
                    var hash = new Vector2(x, y);

                    buffer.Add(hash);
                }
            }
        }

        private Vector2 GetHash(Vector2 position)
        {
            return (position / _bucketSize).Floor();
        }
    }
}
