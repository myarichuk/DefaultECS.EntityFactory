using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using DefaultEcs;
using Fasterflect;

namespace DefaultECS.EntityFactory
{
    public class EntityFactory
    {
        private readonly ITemplateResolver _templateResolver;
        private readonly World _world;
        private readonly ComponentFactory _componentFactory;
        private readonly Dictionary<Type, MethodInfo> _componentSetMethodsByTypes = new Dictionary<Type, MethodInfo>();
        private static readonly Type EntityType = typeof(Entity);

        private static readonly MethodInfo TypelessSetMethod =
            typeof(Entity).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);

        public EntityFactory(ITemplateResolver templateResolver, World world)
        {
            _templateResolver = templateResolver;
            _world = world;
            _componentFactory = new ComponentFactory(templateResolver);
        }

        public bool TryCreate(string name, out Entity entity)
        {
            entity = default;
            var template = _templateResolver.ResolveEntityTemplate(name);
            return template != null && TryCreate(template, out entity);
        }

        public bool TryCreate(EntityTemplate template, out Entity entity)
        {
            entity = _world.CreateEntity();
            foreach (var componentTemplate in template.Components)
            {
                if (_componentFactory.TryCreateComponent(componentTemplate, out var componentInstance))
                {
                    if (!_componentSetMethodsByTypes.TryGetValue(componentTemplate.Type, out var setMethod))
                    {
                        //TODO: make this faster with DynamicDelegate (emit IL dynamic methods for each component type)
                        setMethod = TypelessSetMethod.MakeGenericMethod(componentTemplate.Type);
                        _componentSetMethodsByTypes.Add(componentTemplate.Type, setMethod);
                    }
                    
                    setMethod.Invoke(entity, new []{ componentInstance });
                }
            }

            return true;
        }
    }
}
