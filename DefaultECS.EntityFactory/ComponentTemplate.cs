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

        private sealed class TypeEqualityComparer : IEqualityComparer<ComponentTemplate>
        {
            public bool Equals(ComponentTemplate x, ComponentTemplate y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Type == y.Type;
            }

            public int GetHashCode(ComponentTemplate obj)
            {
                return (obj.Type != null ? obj.Type.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<ComponentTemplate> Comparer { get; } = new TypeEqualityComparer();
    }
}