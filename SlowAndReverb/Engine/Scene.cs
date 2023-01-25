using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SlowAndReverb
{
    public class Scene 
    {
        private readonly HashSet<Component> _components = new HashSet<Component>();
        private readonly HashSet<System> _systems = new HashSet<System>();

        public Scene()
        {
            Entities = new EntityHashMap(this, 100f);

            _systems.Add(new LightRenderer(this)); 
        }

        public static Scene Current { get; set; }

        public EntityHashMap Entities { get; private init; }
        public float Brightness { get; set; } = 1f;

        public IEnumerable<Component> Components => _components; 

        public virtual void Update(float deltaTime)
        {
            Entities.Update();

            foreach (Entity entity in Entities)
                entity.DoUpdate(deltaTime);

            foreach (System system in _systems)
                system.Update(deltaTime);



            if (Input.IsMouseDown(MouseButton.Left))
            {
                Vector2 mousePosition = Layers.Foreground.MousePosition;
                Vector2 blockPosition = new Vector2(mousePosition.RoundedX / 8, mousePosition.RoundedY / 8) * 8f;

                Block block = Scene.CheckPoint<Block>(blockPosition);

                if (block is not null)
                    return;

                foreach (Block b in CheckCircleAll<Block>(blockPosition, 16))
                    b.NeedsRefresh = true;

                var n = new Block("tileset", blockPosition.X, blockPosition.Y)
                {
                    NeedsRefresh = true
                };

                Scene.Add(n);
            }
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

            if (Engine.DebugCollition)
            {
                Entities.DrawBuckets(0f);

                foreach (Entity entity in Entities)
                    entity.DrawCollision(5f);
            }

            Graphics.EndCurrentLayer();
        }

        public virtual void Initilize()
        {
            foreach (Entity entity in Entities)
                entity.OnInitialize();
        }

        public virtual void OnEntityAdded(Entity entity)
        {
            foreach (Component component in entity.Components)
                _components.Add(component);
        }

        public virtual void OnEntityRemoved(Entity entity)
        {
            foreach (Component component in entity.Components)
                _components.Remove(component);
        }

        public virtual void OnComponentAdded(Entity entity, Component component)
        {
            _components.Add(component); 
        }

        public virtual void OnComponentRemoved(Entity entity, Component component)
        {
            _components.Remove(component);
        }

        public static void Add(Entity entity)
        {
            Current.AddEntity(entity);
        }

        public static void Remove(Entity entity)
        {
            Current.RemoveEntity(entity);
        }

        public static IEnumerable<T> CheckRectangleAll<T>(Rectangle rectangle) where T : Entity
        {
            return Current.CheckRectangleCollisionAll<T>(rectangle);
        }

        public static T CheckRectangle<T>(Rectangle rectangle) where T : Entity
        {
            return Current.CheckRectangleCollision<T>(rectangle);
        }

        public static IEnumerable<T> CheckPointAll<T>(Vector2 point) where T : Entity
        {
            return Current.CheckPointCollisionAll<T>(point);
        }

        public static T CheckPoint<T>(Vector2 point) where T : Entity
        {
            return Current.CheckPointCollision<T>(point);
        }

        public static IEnumerable<T> CheckCircleAll<T>(Vector2 position, float radius) where T : Entity
        {
            return Current.CheckCircleCollisionAll<T>(position, radius);
        }

        public static T CheckCircle<T>(Vector2 position, float radius) where T : Entity
        {
            return Current.CheckCircleCollision<T>(position, radius);   
        }

        public IEnumerable<T> CheckRectangleCollisionAll<T>(Rectangle rectangle) where T : Entity
        {
            foreach (Entity entity in Entities.GetNearby(rectangle))
            {
                if (entity is not T result)
                    continue;

                if (rectangle.Intersects(entity.Rectangle))
                    yield return result;
            }
        }

        // Maybe I should just copy the entity method instead of doing FirstOrDefault because it can be relatively very slow
        public T CheckRectangleCollision<T>(Rectangle rectangle) where T : Entity
        {
            return CheckRectangleCollisionAll<T>(rectangle).FirstOrDefault();
        }

        public IEnumerable<T> CheckPointCollisionAll<T>(Vector2 point) where T : Entity
        {
            var rectangle = new Rectangle(point, point + Vector2.One);

            foreach (T entity in CheckRectangleAll<T>(rectangle))
                if (entity.Position == point)
                    yield return entity;
        }

        public T CheckPointCollision<T>(Vector2 point) where T : Entity
        {
            return CheckPointCollisionAll<T>(point).FirstOrDefault();
        }

        public IEnumerable<T> CheckCircleCollisionAll<T>(Vector2 position, float radius) where T : Entity
        {
            var radiusVector = new Vector2(radius);
            var rectangle = new Rectangle(position - radiusVector, position + radiusVector);

            foreach (Entity entity in Entities.GetNearby(rectangle))
            {
                if (entity is not T result)
                    continue;

                float entityRight = entity.Right;
                float entityBottom = entity.Bottom;

                float closestX = position.X > entityRight ? entityRight : entity.Left;
                float closestY = position.Y > entityBottom ? entityBottom : entity.Top;

                float distance = Vector2.Distance(position, new Vector2(closestX, closestY));

                if (distance <= radius)
                    yield return result;
            }
        }

        public T CheckCircleCollision<T>(Vector2 position, float radius) where T : Entity
        {
            return CheckCircleCollisionAll<T>(position, radius).FirstOrDefault();  
        }

        public void AddEntity(Entity entity)
        {
            if (entity.Scene is not null)
                return;

            Entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            Entities.Remove(entity);
        }
    }
}
