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

        private Scene Scene => _entity.Scene;

        protected override void OnItemAdded(Component component)
        {
            Scene.OnComponentAdded(component);
            component.Added(_entity);
        }

        protected override void OnItemRemoved(Component component)
        {
            Scene.OnComponentRemoved(component);
            component.Removed();
        }
    }
}
