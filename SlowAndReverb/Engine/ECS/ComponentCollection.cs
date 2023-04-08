using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowAndReverb
{
    public class ComponentCollection : DynamicCollection<Component>
    {
        private readonly Entity _entity;

        public ComponentCollection(Entity entity)
        {
            _entity = entity;
        }

        private World World => _entity.World;

        protected override void OnItemAdded(Component component)
        {
            World.OnComponentAdded(component);
            component.Added(_entity);
        }

        protected override void OnItemRemoved(Component component)
        {
            World.OnComponentRemoved(component);
            component.Removed();
        }
    }
}
