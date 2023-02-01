using OpenTK.Graphics.ES20;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public sealed class EntityMap : IEnumerable<Entity>
    {
        private readonly Dictionary<Vector2, HashSet<Entity>> _buckets = new Dictionary<Vector2, HashSet<Entity>>();
        private readonly Dictionary<Type, HashSet<Entity>> _entitiesByType = new Dictionary<Type, HashSet<Entity>>();
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();

        private readonly HashSet<Entity> _entitiesToAdd = new HashSet<Entity>();    
        private readonly HashSet<Entity> _entitiesToRemove = new HashSet<Entity>();

        private readonly Scene _scene;
        private readonly float _bucketSize;

        public EntityMap(Scene scene, float bucketSize)
        {
            _scene = scene;
            _bucketSize = bucketSize;
        }

        public void Update()
        {
            foreach (Entity entityToRemove in _entitiesToRemove)
            {
                if (!_entities.Remove(entityToRemove))
                    continue;

                RemoveFromBuckets(entityToRemove);

                HashSet<Entity> entities = _entitiesByType[entityToRemove.GetType()];
                entities.Remove(entityToRemove);

                entityToRemove.OnRemoved();
                _scene.OnEntityRemoved(entityToRemove);
            }

            foreach (Entity entity in _entities)
            {
                if (entity.NeverMoves)
                    continue; 

                RemoveFromBuckets(entity);

                AddToBuckets(entity);
            }

            foreach (Entity entityToAdd in _entitiesToAdd)
            {
                if (!_entities.Add(entityToAdd))
                    continue;

                Type type = entityToAdd.GetType();
                AddToBuckets(entityToAdd);

                if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
                {
                    entities.Add(entityToAdd);
                }
                else
                {
                    _entitiesByType[type] = new HashSet<Entity>()
                    {
                        entityToAdd
                    };
                }

                entityToAdd.OnAdded(_scene);
                _scene.OnEntityAdded(entityToAdd);  
            }

            _entitiesToAdd.Clear();
            _entitiesToRemove.Clear();
        }

        public void Add(Entity entity)
        {
            _entitiesToAdd.Add(entity);
        }

        public void Remove(Entity entity)
        {
            _entitiesToRemove.Add(entity);
        } 

        public void Clear()
        {
            _entities.Clear();
            _buckets.Clear();
            _entitiesToAdd.Clear();
            _entitiesToRemove.Clear();
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

        public IEnumerable<T> GetEntitiesOfType<T>() where T : Entity
        {
            foreach (Entity entity in GetEntitiesOfType(typeof(T)))
                yield return (T)entity;
        }

        public IEnumerable<Entity> GetEntitiesOfType(Type type)
        {
            if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
                return entities;

            return Enumerable.Empty<Entity>();  
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

        public IEnumerator<Entity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
