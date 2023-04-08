using System;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public sealed class World
    {
        private readonly Dictionary<Type, HashSet<Entity>> _entitiesByType = new Dictionary<Type, HashSet<Entity>>();

        private readonly HashSet<Component> _components = new HashSet<Component>();
        private readonly Dictionary<Type, HashSet<Component>> _componentsByType = new Dictionary<Type, HashSet<Component>>();

        private readonly EntityMap _entityMap;

        public World(Scene scene, float bucketSize)
        {
            Scene = scene;

            _entityMap = new EntityMap(this, bucketSize);
        }

        public Scene Scene { get; private init; }

        public IEnumerable<Entity> Entities => _entityMap;
        public IEnumerable<Component> Components => _components;

        public void Update()
        {
            _entityMap.Update();
        }

        public void AddEntity(Entity entity)
        {
            _entityMap.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            _entityMap.Remove(entity);
        }

        public void OnEntityAdded(Entity entity)
        {
            entity.Added(Scene);

            Type type = entity.GetType();

            if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
            {
                entities.Add(entity);
            }
            else
            {
                _entitiesByType[type] = new HashSet<Entity>()
                {
                    entity
                };
            }

            foreach (Component component in entity.Components)
                _components.Add(component);
        }

        public void OnEntityRemoved(Entity entity)
        {
            entity.Removed();

            Type type = entity.GetType();

            if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
                entities.Remove(entity);

            foreach (Component component in entity.Components)
                _components.Remove(component);
        }

        public void OnComponentAdded(Component component)
        {
            _components.Add(component);

            Type type = component.GetType();

            if (_componentsByType.TryGetValue(type, out HashSet<Component> components))
            {
                components.Add(component);
            }
            else
            {
                _componentsByType[type] = new HashSet<Component>()
                {
                    component
                };
            }
        }

        public void OnComponentRemoved(Component component)
        {
            _components.Remove(component);

            Type type = component.GetType();

            if (_componentsByType.TryGetValue(type, out HashSet<Component> components))
                components.Remove(component);
        }

        public IEnumerable<Entity> GetEntitiesOfType(Type type)
        {
            if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
                return entities;

            return Enumerable.Empty<Entity>();  
        }

        public IEnumerable<T> GetEntitiesOfType<T>() where T : Entity
        {
            return GetEntitiesOfType(typeof(T)).OfType<T>();
        }

        public IEnumerable<Component> GetComponentsOfType(Type type)
        {
            if (_componentsByType.TryGetValue(type, out HashSet<Component> components))
                return components;

            return Enumerable.Empty<Component>();
        }

        public IEnumerable<T> GetComponentsOfType<T>() where T : Component
        {
            return GetComponentsOfType(typeof(T)).OfType<T>();
        }

        public T CheckRectangle<T>(Rectangle rectangle) where T : Entity
        {
            foreach (Entity entity in _entityMap.GetNearby(rectangle))
            {
                if (entity is not T result)
                    continue;

                if (rectangle.Intersects(entity.Rectangle))
                    return result;
            }

            return null;
        }

        public IEnumerable<T> CheckRectangleAll<T>(Rectangle rectangle) where T : Entity
        {
            foreach (Entity entity in _entityMap.GetNearby(rectangle))
            {
                if (entity is not T result)
                    continue;

                if (rectangle.Intersects(entity.Rectangle))
                    yield return result;
            }
        }

        public T CheckPoint<T>(Vector2 point) where T : Entity
        {
            var rectangle = new Rectangle(point, point + Vector2.One);

            foreach (T entity in CheckRectangleAll<T>(rectangle))
                if (entity.Position == point)
                    return entity;

            return null;
        }

        public IEnumerable<T> CheckPointAll<T>(Vector2 point) where T : Entity
        {
            var rectangle = new Rectangle(point, point + Vector2.One);

            foreach (T entity in CheckRectangleAll<T>(rectangle))
                if (entity.Position == point)
                    yield return entity;
        }

        public T CheckCircle<T>(Vector2 position, float radius) where T : Entity
        {
            Rectangle rectangle = Utilities.RectangleFromCircle(position, radius);

            foreach (Entity entity in _entityMap.GetNearby(rectangle))
            {
                if (entity is not T result)
                    continue;

                if (WithinCircle(entity, position, radius))
                    return result;
            }

            return null;
        }

        public IEnumerable<T> CheckCircleAll<T>(Vector2 position, float radius) where T : Entity
        {
            Rectangle rectangle = Utilities.RectangleFromCircle(position, radius);

            foreach (Entity entity in _entityMap.GetNearby(rectangle))
            {
                if (entity is not T result)
                    continue;

                if (WithinCircle(entity, position, radius))
                    yield return result;
            }
        }

        private bool WithinCircle(Entity entity, Vector2 position, float radius)
        {
            float entityRight = entity.Right;
            float entityBottom = entity.Bottom;

            float closestX = position.X > entityRight ? entityRight : entity.Left;
            float closestY = position.Y > entityBottom ? entityBottom : entity.Top;

            float distance = Vector2.Distance(position, new Vector2(closestX, closestY));

            if (distance <= radius)
                return true;

            return false;
        }
    }
}
