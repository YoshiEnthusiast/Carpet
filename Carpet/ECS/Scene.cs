using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Carpet
{
    public class Scene
    {
        private readonly HashSet<Component> _components = [];
        private readonly DynamicCollection<System> _systems = [];

        private readonly Dictionary<Type, HashSet<Entity>> _entitiesByType = [];
        private readonly Dictionary<ulong, Entity> _entitiesByID = [];
        private readonly Dictionary<Type, HashSet<Component>> _componentsByType = [];

        private readonly CoroutineRunner _coroutineRunner = new();

        private readonly List<Entity> _entityBuffer = [];
        private readonly HashSet<Entity> _nearbyBuffer = [];

        private EntityMap _entityMap;

        public Scene(float bucketSize)
        {
            _entityMap = new EntityMap(this, bucketSize);
        }

        public Rectangle Rectangle { get; set; }

        public IEnumerable<Entity> Entities => _entityMap;
        public IEnumerable<Component> Components => _components;

        public Vector2 TopLeft => Rectangle.TopLeft;
        public Vector2 BottomRight => Rectangle.BottomRight;
        public float Left => Rectangle.Left;
        public float Right => Rectangle.Right;
        public float Top => Rectangle.Top;
        public float Bottom => Rectangle.Bottom;

        #region General Methods

        public virtual void Update(float deltaTime)
        {
            _entityMap.Update();
            _systems.Update();

            UpdateEntities(deltaTime);

            _coroutineRunner.DoUpdate(deltaTime);

            foreach (System system in _systems)
                system.Update(deltaTime);
        }

        public virtual void Draw()
        {
            foreach (Entity entity in Entities)
                entity.DoDraw();

            foreach (System system in _systems)
                system.Draw();

            foreach (System system in _systems)
                system.OnLateDraw();
        }

        public virtual void Initialize()
        {
            _systems.Update();
            _entityMap.Update();

            UpdateEntities(0f);

            foreach (System system in _systems)
                system.Initialize();

            _entityMap.Update();
        }

        public virtual void Terminate()
        {

        }

        #endregion

        #region Add / Remove

        public ulong Add(Entity entity)
        {
            _entityMap.Add(entity);

            return entity.ID;
        }

        public void Remove(Entity entity)
        {
            _entityMap.Remove(entity); 
        }

        public Scene AddSystem(System system)
        {
            _systems.Add(system);

            return this;
        }

        #endregion

        #region Entity / Component / System finders

        public T GetSystem<T>() where T : System
        {
            foreach (System system in _systems)
                if (system is T result)
                    return result;

            return default;
        }

        public void OnEntityAdded(Entity entity)
        {
            entity.Added(this);

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

            _entitiesByID[entity.ID] = entity;

            foreach (Component component in entity.Components)
                _components.Add(component);
        }

        public void OnEntityRemoved(Entity entity)
        {
            entity.Removed();

            Type type = entity.GetType();

            if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
                entities.Remove(entity);

            _entitiesByID.Remove(entity.ID);

            foreach (Component component in entity.Components)
            {
                _components.Remove(component);

                OnComponentRemoved(component);
            }
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

        public T GetEntityByID<T>(ulong id) where T : Entity
        {
            if (_entitiesByID.TryGetValue(id, out Entity entity))
                return (T)entity;

            return null;
        }

        public Entity GetEntityByID(ulong id)
        {
            return GetEntityByID<Entity>(id);
        }

        public List<Entity> GetEntitiesOfType(Type type, List<Entity> buffer)
        {
            buffer = PrepareBuffer(buffer);

            if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
                buffer.AddRange(entities);

            return buffer;
        }

        public List<T> GetEntitiesOfType<T>(List<T> buffer) where T : Entity
        {
            buffer = PrepareBuffer(buffer);

            foreach (Entity entity in _entityMap.Items)
                if (entity is T result)
                    buffer.Add(result);

            return buffer;
        }

        public Entity GetEntityOfType(Type type) 
        {
            if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
                return entities.FirstOrDefault();

            return default;
        }

        public T GetEntityOfType<T>() where T : Entity
        {
            return GetEntityOfType(typeof(T)) as T;
        }

        public List<Component> GetComponentsOfType(Type type, List<Component> buffer)
        {
            buffer = PrepareBuffer(buffer);

            if (_componentsByType.TryGetValue(type, out HashSet<Component> components))
                buffer.AddRange(components);

            return buffer;
        }

        public List<T> GetComponentsOfType<T>(List<T> buffer) where T : Component
        {
            buffer = PrepareBuffer(buffer);

            foreach (Component component in _components)
                if (component is T result)
                    buffer.Add(result);

            return buffer;
        }

        public T CheckRectangle<T>(Rectangle rectangle) where T : Entity
        {
            foreach (Entity entity in _entityMap.GetNearby(rectangle, _nearbyBuffer))
            {
                if (entity is not T result)
                    continue;

                if (rectangle.Intersects(entity.Rectangle))
                    return result;
            }

            return null;
        }

        public List<T> CheckRectangleAll<T>(Rectangle rectangle, List<T> buffer) where T : Entity
        {
            buffer = PrepareBuffer(buffer);

            foreach (Entity entity in _entityMap.GetNearby(rectangle, _nearbyBuffer))
            {
                if (entity is not T result)
                    continue;

                if (rectangle.Intersects(entity.Rectangle))
                    buffer.Add(result);
            }

            return buffer;
        }

        public T CheckPosition<T>(Vector2 position) where T : Entity
        {
            var rectangle = new Rectangle(position, position + Vector2.One);

            foreach (Entity entity in _entityMap.GetNearby(rectangle, _nearbyBuffer))
            {
                if (entity is not T result)
                    continue;

                if (entity.Position == position)
                    return result;
            }

            return null;
        }

        public List<T> CheckPositionAll<T>(Vector2 position, List<T> buffer) where T : Entity
        {
            buffer = PrepareBuffer(buffer);
            var rectangle = new Rectangle(position, position + Vector2.One);

            foreach (Entity entity in _entityMap.GetNearby(rectangle, _nearbyBuffer))
            {
                if (entity is not T result)
                    continue;

                if (entity.Position == position)
                    buffer.Add(result);
            }

            return buffer;
        }

        public T CheckPoint<T>(Vector2 point) where T : Entity
        {
            var rectangle = new Rectangle(point, point + Vector2.One);

            return CheckRectangle<T>(rectangle);
        }

        public List<T> CheckPointAll<T>(Vector2 point, List<T> buffer) where T : Entity
        {
            var rectangle = new Rectangle(point, point + Vector2.One);

            return CheckRectangleAll<T>(rectangle, buffer);
        }

        public T CheckCircle<T>(Vector2 position, float radius) where T : Entity
        {
            Rectangle rectangle = Rectangle.FromCircle(position, radius);

            foreach (Entity entity in _entityMap.GetNearby(rectangle, _nearbyBuffer))
            {
                if (entity is not T result)
                    continue;

                if (WithinCircle(entity, position, radius))
                    return result;
            }

            return null;
        }

        public List<T> CheckCircleAll<T>(Vector2 position, float radius, List<T> buffer) where T : Entity
        {
            buffer = PrepareBuffer(buffer);
            Rectangle rectangle = Rectangle.FromCircle(position, radius);

            foreach (Entity entity in _entityMap.GetNearby(rectangle, _nearbyBuffer))
            {
                if (entity is not T result)
                    continue;

                if (WithinCircle(entity, position, radius))
                    buffer.Add(result);
            }

            return buffer;
        }

        public T CheckLine<T>(Vector2 start, Vector2 end) where T : Entity
        {
            float startX = start.X;
            float startY = start.Y;
            float endX = end.X;
            float endY = end.Y;

            float left = Maths.Min(startX, endX);
            float top = Maths.Min(startY, endY);
            float right = Maths.Max(startX, endX);
            float bottom = Maths.Max(startY, endY);

            var rectangle = new Rectangle(new Vector2(left, top), new Vector2(right, bottom));

            foreach (Entity entity in _entityMap.GetNearby(rectangle, _nearbyBuffer))
            {
                if (entity is not T result)
                    continue;

                foreach (Line surface in entity.Rectangle.GetSurfaces())
                    if (Maths.TryGetIntersectionPoint(start, end, surface.Start, surface.End, out _))
                        return result;
            }

            return null;
        }

        public List<T> CheckLineAll<T>(Vector2 start, Vector2 end, List<T> buffer) where T : Entity
        {
            buffer = PrepareBuffer(buffer);

            float startX = start.X;
            float startY = start.Y;
            float endX = end.X;
            float endY = end.Y;

            float left = Maths.Min(startX, endX);
            float top = Maths.Min(startY, endY);
            float right = Maths.Max(startX, endX);
            float bottom = Maths.Max(startY, endY);

            var rectangle = new Rectangle(new Vector2(left, top), new Vector2(right, bottom));

            foreach (Entity entity in _entityMap.GetNearby(rectangle, _nearbyBuffer))
            {
                if (entity is not T result)
                    continue;

                foreach (Line surface in entity.Rectangle.GetSurfaces())
                {
                    if (Maths.TryGetIntersectionPoint(start, end, surface.Start, surface.End, out _))
                    {
                        buffer.Add(result);

                        break;
                    }
                }
            }

            return buffer;
        }

        public T CheckLine<T>(Line line) where T : Entity
        {
            return CheckLine<T>(line.Start, line.End);
        }

        public List<T> CheckLineAll<T>(Line line, List<T> buffer) where T : Entity
        {
            return CheckLineAll<T>(line.Start, line.End, buffer);
        }

        public T CheckRectangleComponent<T>(Rectangle rectangle) where T : Component
        {
            IEnumerable<Entity> entities = CheckRectangleAll<Entity>(rectangle, _entityBuffer);

            return GetComponent<T>(entities);   
        }

        public List<T> CheckRectangleAllComponent<T>(Rectangle rectangle, List<T> buffer) where T : Component
        {
            buffer = PrepareBuffer(buffer);

            IEnumerable<Entity> entities = CheckRectangleAll<Entity>(rectangle, _entityBuffer);

            return GetComponents<T>(entities, buffer);
        }

        public T CheckCircleComponent<T>(Vector2 center, float radius) where T : Component
        {
            IEnumerable<Entity> entities = CheckCircleAll<Entity>(center, radius, _entityBuffer);

            return GetComponent<T>(entities);
        }

        public List<T> CheckCircleAllComponent<T>(Vector2 center, float radius, List<T> buffer) where T : Component
        {
            buffer = PrepareBuffer(buffer);

            IEnumerable<Entity> entities = CheckCircleAll<Entity>(center, radius, _entityBuffer);

            return GetComponents<T>(entities, buffer);
        }

        public T CheckPointComponent<T>(Vector2 point) where T : Component
        {
            IEnumerable<Entity> entities = CheckPointAll<Entity>(point, _entityBuffer);

            return GetComponent<T>(entities);
        }

        public List<T> CheckPointAllComponent<T>(Vector2 point, List<T> buffer) where T : Component
        {
            buffer = PrepareBuffer(buffer);

            IEnumerable<Entity> entities = CheckPointAll<Entity>(point, _entityBuffer);

            return GetComponents<T>(entities, buffer);
        }

        public T CheckPositionComponent<T>(Vector2 point) where T : Component
        {
            IEnumerable<Entity> entities = CheckPositionAll<Entity>(point, _entityBuffer);

            return GetComponent<T>(entities);
        }

        public List<T> CheckPositionAllComponent<T>(Vector2 point, List<T> buffer) where T : Component
        {
            buffer = PrepareBuffer(buffer);

            IEnumerable<Entity> entities = CheckPositionAll<Entity>(point, _entityBuffer);

            return GetComponents<T>(entities, buffer);
        }

        public T CheckLineComponent<T>(Vector2 start, Vector2 end) where T : Component
        {
            IEnumerable<Entity> entities = CheckLineAll<Entity>(start, end, _entityBuffer);

            return GetComponent<T>(entities);
        }

        public List<T> CheckLineAllComponent<T>(Vector2 start, Vector2 end, List<T> buffer) where T : Component
        {
            buffer = PrepareBuffer(buffer);

            IEnumerable<Entity> entities = CheckLineAll<Entity>(start, end, _entityBuffer);

            return GetComponents<T>(entities, buffer);
        }

        public T CheckLineComponent<T>(Line line) where T : Component
        {
            IEnumerable<Entity> entities = CheckLineAll<Entity>(line, _entityBuffer);

            return GetComponent<T>(entities);
        }

        public IEnumerable<T> CheckLineAllComponent<T>(Line line, List<T> buffer) where T : Component
        {
            buffer = PrepareBuffer(buffer);

            IEnumerable<Entity> entities = CheckLineAll<Entity>(line, _entityBuffer);

            return GetComponents<T>(entities, buffer);
        }

        private T GetComponent<T>(IEnumerable<Entity> entities) where T : Component
        {
            foreach (Entity entity in entities)
            {
                T component = entity.Get<T>();

                if (component is not null)
                    return component;
            }

            return default;
        }

        #endregion

        #region Coroutines

        public int StartCoroutine(IEnumerator enumerator, float initialDelay)
        {
            return _coroutineRunner.StartCoroutine(enumerator, initialDelay);
        }

        public int StartCoroutine(IEnumerator enumerator)
        {
            return _coroutineRunner.StartCoroutine(enumerator);
        }

        public bool StopCoroutine(int id)
        {
            return _coroutineRunner.StopCoroutine(id);
        }

        public void StopAllCoroutines()
        {
            _coroutineRunner.StopAllCoroutines();
        }

        #endregion

        #region Helpers

        private List<T> GetComponents<T>(IEnumerable<Entity> entities, List<T> buffer) where T : Component
        {
            foreach (Entity entity in entities)
            {
                T component = entity.Get<T>();

                if (component is not null)
                    buffer.Add(component);
            }

            return buffer;
        }

        private void UpdateEntities(float deltaTime)
        {
            foreach (Entity entity in Entities)
                entity.DoUpdate(deltaTime);
        }

        private bool WithinCircle(Entity entity, Vector2 center, float radius)
        {
            return Maths.WithinCircle(entity.Rectangle, center, radius);
        }
        
        private List<T> PrepareBuffer<T>(List<T> buffer)
        {
            if (buffer is null)
                buffer = new List<T>();

            buffer.Clear();

            return buffer;
        }

        #endregion
    }
}
