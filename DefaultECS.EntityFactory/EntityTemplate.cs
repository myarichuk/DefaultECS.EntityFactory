using System.Collections.Generic;

namespace DefaultECS.EntityFactory
{
    public sealed class EntityTemplate
    {
        public string Name { get; set; }

        public EntityTemplate Parent { get; set; }
        public List<EntityTemplate> Children { get; set; }

        public List<EntityTemplate> InheritsFrom { get; set; }

        public List<ComponentTemplate> Components { get; set; }
    }
}
