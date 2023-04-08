using System.Collections.Generic;

namespace SlowAndReverb
{
    public class Scene
    {
        private readonly DynamicCollection<System> _systems = new DynamicCollection<System>();

        public Scene()
        {
            World = new World(this, 100f);

            AddSystem(new LightRenderer(this));
            AddSystem(new BlockGroupsSystem(this));
            AddSystem(new ParticleSystem(this, 1000));
            AddSystem(new DebugSystem(this));
        }

        public World World { get; private init; }
        public Color Color { get; set; } = Color.White;

        public IEnumerable<Entity> Entities => World.Entities;
        public IEnumerable<Component> Components => World.Components;

        public virtual void Update(float deltaTime)
        {
            _systems.Update();
            World.Update();

            UpdateEntities(deltaTime);

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
        }

        public virtual void Initialize()
        {
            _systems.Update();
            World.Update();

            UpdateEntities(0f);

            foreach (System system in _systems)
                system.Initialize();

            World.Update();
        }

        public void Add(Entity entity)
        {
            World.AddEntity(entity);
        }

        public void Remove(Entity entity)
        {
            World.RemoveEntity(entity); 
        }

        public void AddSystem(System system)
        {
            _systems.Add(system);
        }

        public void RemoveSystem(System system)
        {
            _systems.Remove(system);
        }

        public T GetSystem<T>() where T : System
        {
            foreach (System system in _systems)
                if (system is T result)
                    return result;

            return default;
        }

        public T CheckRectangle<T>(Rectangle rectangle) where T : Entity
        {
            return World.CheckRectangle<T>(rectangle);
        }

        public IEnumerable<T> CheckRectangleAll<T>(Rectangle rectangle) where T : Entity
        {
            return World.CheckRectangleAll<T>(rectangle);
        }

        public T CheckPoint<T>(Vector2 point) where T : Entity
        {
            return World.CheckPoint<T>(point);
        }

        public IEnumerable<T> CheckPointAll<T>(Vector2 point) where T : Entity
        {
            return World.CheckPointAll<T>(point);
        }

        public T CheckCircle<T>(Vector2 position, float radius) where T : Entity
        {
            return World.CheckCircle<T>(position, radius);
        }

        public IEnumerable<T> CheckCircleAll<T>(Vector2 position, float radius) where T : Entity
        {
            return World.CheckCircleAll<T>(position, radius);
        }

        private void UpdateEntities(float deltaTime)
        {
            foreach (Entity entity in Entities)
                entity.DoUpdate(deltaTime);
        }
    }
}
