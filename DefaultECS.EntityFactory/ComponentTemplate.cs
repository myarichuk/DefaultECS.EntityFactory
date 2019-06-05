using System;
using System.Collections.Generic;

namespace DefaultECS.EntityFactory
{
    public sealed class ComponentTemplate
    {
        private string _name;

        public string Name
        {
            get => _name ?? Type?.Name;
            set => _name = value;
        }

        public Type Type { get; set; }
        public Dictionary<string, object> Defaults { get; set; }

        public bool IsShared { get; set; } //is it the same instance for ALL cases?
    }
}