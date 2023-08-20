﻿using OpenTK.Graphics.ES11;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SlowAndReverb
{
    public class Scene
    {
        private readonly EntityMap _entityMap;
        private readonly HashSet<Component> _components = new HashSet<Component>();
        private readonly DynamicCollection<System> _systems = new DynamicCollection<System>();

        private readonly Dictionary<Type, HashSet<Entity>> _entitiesByType = new Dictionary<Type, HashSet<Entity>>();
        private readonly Dictionary<Type, HashSet<Component>> _componentsByType = new Dictionary<Type, HashSet<Component>>();

        private readonly CoroutineRunner _coroutineRunner = new CoroutineRunner();

        public Scene()
        {
            _entityMap = new EntityMap(this, 100f);

            AddSystem(new LightRenderer(this))
                .AddSystem(new BlockGroupsSystem(this))
                .AddSystem(new ParticleSystem(this, 1000))
                .AddSystem(new CameraSystem(0.18f, this))
                .AddSystem(new DebugSystem(this));
        }

        public Color Color { get; set; } = Color.White;

        public Rectangle Rectangle { get; private set; }

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
            foreach (System system in _systems)
                system.OnBeforeDraw();

            Graphics.BeginLayer(Layers.Foreground);

            foreach (Entity entity in Entities)
                entity.DoDraw();

            foreach (System system in _systems)
                system.Draw();

            foreach (System system in _systems)
                system.OnLateDraw();

            Graphics.EndCurrentLayer();

            Graphics.BeginLayer(Layers.Background);

            Graphics.EndCurrentLayer();
        }

        public virtual void Initialize()
        {
            _systems.Update();
            _entityMap.Update();

            UpdateEntities(0f);

            foreach (System system in _systems)
                system.Initialize();

            _entityMap.Update();

            float left = float.PositiveInfinity;
            float top = float.PositiveInfinity;

            float right = float.NegativeInfinity;
            float bottom = float.NegativeInfinity;

            foreach (BlockGroup group in GetEntitiesOfType<BlockGroup>())
            {
                left = Maths.Min(left, group.Left);
                top = Maths.Min(top, group.Top);

                right = Maths.Max(right, group.Right);
                bottom = Maths.Max(bottom, group.Bottom);
            }

            Layer foreground = Layers.Foreground;

            float foregroundWidth = foreground.Width;
            float foregroundHeight = foreground.Height;

            float xDifference = right - left;
            float yDifference = bottom - top;   

            if (xDifference < foregroundWidth)
            {
                float value = (foregroundWidth - xDifference) / 2f;

                left -= value;
                right += value;
            }

            if (yDifference < foregroundHeight)
            {
                float value = (foregroundHeight - yDifference) / 2f;

                top -= value;
                bottom += value;
            }

            Rectangle = new Rectangle(new Vector2(left, top), new Vector2(right, bottom));
        }

        public virtual void Terminate()
        {

        }

        public void SetPalette(Palette palette)
        {
            ForegroundMaterial material = Layers.Foreground.Material as ForegroundMaterial;

            material.SetPalette(palette);
        }

        #endregion

        #region Add / Remove

        public void Add(Entity entity)
        {
            _entityMap.Add(entity);
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

        public void RemoveSystem(System system)
        {
            _systems.Remove(system);
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

        public IEnumerable<Entity> GetEntitiesOfType(Type type)
        {
            if (_entitiesByType.TryGetValue(type, out HashSet<Entity> entities))
                return entities;

            return Enumerable.Empty<Entity>();
        }

        public IEnumerable<T> GetEntitiesOfType<T>() where T : Entity
        {
            return _entityMap.OfType<T>();
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

        public T CheckPosition<T>(Vector2 position) where T : Entity
        {
            foreach (T entity in CheckPointAll<T>(position))
                if (entity.Position == position)
                    return entity;

            return null;
        }

        public IEnumerable<T> CheckPositionAll<T>(Vector2 position) where T : Entity
        {
            foreach (T entity in CheckPointAll<T>(position))
                if (entity.Position == position)
                    yield return entity;
        }

        public T CheckPoint<T>(Vector2 point) where T : Entity
        {
            var rectangle = new Rectangle(point, point + Vector2.One);

            return CheckRectangle<T>(rectangle);
        }

        public IEnumerable<T> CheckPointAll<T>(Vector2 point) where T : Entity
        {
            var rectangle = new Rectangle(point, point + Vector2.One);

            return CheckRectangleAll<T>(rectangle);
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

            foreach (Entity entity in _entityMap.GetNearby(rectangle))
            {
                if (entity is not T result)
                    continue;

                foreach (Line surface in entity.Rectangle.GetSurfaces())
                    if (Maths.TryGetIntersectionPoint(start, end, surface.Start, surface.End, out _))
                        return result;
            }

            return null;
        }

        public IEnumerable<T> CheckLineAll<T>(Vector2 start, Vector2 end) where T : Entity
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

            foreach (Entity entity in _entityMap.GetNearby(rectangle))
            {
                if (entity is not T result)
                    continue;

                foreach (Line surface in entity.Rectangle.GetSurfaces())
                {
                    if (Maths.TryGetIntersectionPoint(start, end, surface.Start, surface.End, out _))
                    {
                        yield return result;

                        break;
                    }
                }
            }
        }

        public T CheckLine<T>(Line line) where T : Entity
        {
            return CheckLine<T>(line.Start, line.End);
        }

        public IEnumerable<T> CheckLineAll<T>(Line line) where T : Entity
        {
            return CheckLineAll<T>(line.Start, line.End);
        }

        public T CheckRectangleComponent<T>(Rectangle rectangle) where T : Component
        {
            IEnumerable<Entity> entities = CheckRectangleAll<Entity>(rectangle);

            return GetComponent<T>(entities);   
        }

        public IEnumerable<T> CheckRectangleAllComponent<T>(Rectangle rectangle) where T : Component
        {
            IEnumerable<Entity> entities = CheckRectangleAll<Entity>(rectangle);

            return GetComponents<T>(entities);
        }

        public T CheckCircleComponent<T>(Vector2 center, float radius) where T : Component
        {
            IEnumerable<Entity> entities = CheckCircleAll<Entity>(center, radius);

            return GetComponent<T>(entities);
        }

        public IEnumerable<T> CheckCircleAllComponent<T>(Vector2 center, float radius) where T : Component
        {
            IEnumerable<Entity> entities = CheckCircleAll<Entity>(center, radius);

            return GetComponents<T>(entities);
        }

        public T CheckPointComponent<T>(Vector2 point) where T : Component
        {
            IEnumerable<Entity> entities = CheckPointAll<Entity>(point);

            return GetComponent<T>(entities);
        }

        public IEnumerable<T> CheckPointAllComponent<T>(Vector2 point) where T : Component
        {
            IEnumerable<Entity> entities = CheckPointAll<Entity>(point);

            return GetComponents<T>(entities);
        }

        public T CheckPositionComponent<T>(Vector2 point) where T : Component
        {
            IEnumerable<Entity> entities = CheckPositionAll<Entity>(point);

            return GetComponent<T>(entities);
        }

        public IEnumerable<T> CheckPositionAllComponent<T>(Vector2 point) where T : Component
        {
            IEnumerable<Entity> entities = CheckPositionAll<Entity>(point);

            return GetComponents<T>(entities);
        }

        public T CheckLineComponent<T>(Vector2 start, Vector2 end) where T : Component
        {
            IEnumerable<Entity> entities = CheckLineAll<Entity>(start, end);

            return GetComponent<T>(entities);
        }

        public IEnumerable<T> CheckLineAllComponent<T>(Vector2 start, Vector2 end) where T : Component
        {
            IEnumerable<Entity> entities = CheckLineAll<Entity>(start, end);

            return GetComponents<T>(entities);
        }

        public T CheckLineComponent<T>(Line line) where T : Component
        {
            IEnumerable<Entity> entities = CheckLineAll<Entity>(line);

            return GetComponent<T>(entities);
        }

        public IEnumerable<T> CheckLineAllComponent<T>(Line line) where T : Component
        {
            IEnumerable<Entity> entities = CheckLineAll<Entity>(line);

            return GetComponents<T>(entities);
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

        private IEnumerable<T> GetComponents<T>(IEnumerable<Entity> entities) where T : Component
        {
            foreach (Entity entity in entities)
            {
                T component = entity.Get<T>();

                if (component is not null)
                    yield return component;
            }
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

        #endregion
    }
}
