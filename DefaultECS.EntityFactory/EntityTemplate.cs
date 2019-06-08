using System.Collections.Generic;

namespace DefaultECS.EntityFactory
{
    public sealed class EntityTemplate
    {
        public string Name { get; set; }

        //entity hierarchy support
        public string Parent { get; set; }
        public List<string> Children { get; set; } 

        public List<string> InheritsFrom { get; set; }

        public List<ComponentTemplate> Components { get; set; }
    }
}
