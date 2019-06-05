using System.Collections.Generic;

namespace DefaultECS.EntityFactory
{
    public sealed class EntityTemplate
    {
        public string Name { get; set; }

        public string Parent { get; set; } //template name of parent
        public List<string> Children { get; set; } //template names of children

        public List<string> InheritsFrom { get; set; }

        public List<ComponentTemplate> Components { get; set; }
    }
}
