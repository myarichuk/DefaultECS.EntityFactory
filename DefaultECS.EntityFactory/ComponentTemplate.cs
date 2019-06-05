using System;
using System.Collections.Generic;

namespace DefaultECS.EntityFactory
{
    public sealed class ComponentTemplate
    {
        public string Name => Type?.Name;
        public Type Type { get; set; }
        public Dictionary<string, object> Defaults { get; set; }

        public bool IsShared { get; set; } //is it the same instance for ALL cases?
    }
}