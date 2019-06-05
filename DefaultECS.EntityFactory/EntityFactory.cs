using System.Collections.Generic;
using System.Linq;
using DefaultEcs;

namespace DefaultECS.EntityFactory
{
    public class EntityFactory
    {
        private readonly World _world;
        private readonly ComponentFactory _componentFactory;

        public EntityFactory(ITemplateResolver templateResolver)
        {
            _componentFactory = new ComponentFactory(templateResolver);
        }
    }
}
