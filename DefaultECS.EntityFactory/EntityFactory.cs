using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DefaultEcs;

namespace DefaultECS.EntityFactory
{
    //note: this is NOT thread-safe by design
    public class EntityFactory
    {
        private readonly ITemplateResolver _templateResolver;
        private readonly World _world;
        private readonly ComponentFactory _componentFactory;
        private readonly Dictionary<Type, MethodInfo> _componentSetMethodsByTypes = new Dictionary<Type, MethodInfo>();

        private readonly Dictionary<string, EntityTemplate> _entityTemplateCache = new Dictionary<string, EntityTemplate>();
        private readonly Queue<(Entity parent, EntityTemplate template)> _traversalQueue = new Queue<(Entity parent, EntityTemplate template)>();

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
            if (template.Parent == null && (template.Children == null || template.Children.Count == 0))
            {
                entity = CreateSingleEntityInternal(template);
                return true;
            }

            return TryCreateEntityGraphInternal(template, out entity);
        }

        private bool TryCreateEntityGraphInternal(EntityTemplate template, out Entity rootEntity)
        {
            rootEntity = default;

            //precaution
            if (template.Parent == null && (template.Children == null || template.Children.Count == 0))
                return false;

            var parent = template.Parent;

            EntityTemplate rootTemplate = null;

            //first find the root
            while (parent != null)
            {
               rootTemplate = _templateResolver.ResolveEntityTemplate(parent);
                if (rootTemplate == null)
                    return false; //TODO: consider throwing exception here...

                _entityTemplateCache.TryAdd(parent, rootTemplate);
                parent = rootTemplate.Parent;
            }

            rootTemplate = rootTemplate ?? template;
            var currentEntity = CreateSingleEntityInternal(rootTemplate);
            rootEntity = currentEntity; //we want to return the root of the hierarchy
            _traversalQueue.Clear();
            _traversalQueue.EnqueueAll(ResolveChildren(rootTemplate).Select(t => (currentEntity, t)));

            while (_traversalQueue.Count > 0)
            {
                var childInfo = _traversalQueue.Dequeue();
                var childEntity = CreateSingleEntityInternal(childInfo.template);
                childEntity.SetAsChildOf(in childInfo.parent);

                _traversalQueue.EnqueueAll(ResolveChildren(childInfo.template).Select(t => (childEntity, t)));
            }

            IEnumerable<EntityTemplate> ResolveChildren(EntityTemplate parentTemplate)
            {
                foreach (var childName in parentTemplate?.Children ?? Enumerable.Empty<string>())
                {
                    if (_entityTemplateCache.TryGetValue(childName, out var resolvedTemplate) &&
                        resolvedTemplate != null /*precaution, should never happen*/)
                    {
                        yield return resolvedTemplate;
                        continue;
                    }

                    resolvedTemplate = _templateResolver.ResolveEntityTemplate(childName);
                    if (resolvedTemplate != null)
                    {
                        _entityTemplateCache.TryAdd(childName, resolvedTemplate);
                        yield return resolvedTemplate;
                    }
                }
            }

            return true;
        }

        private Entity CreateSingleEntityInternal(EntityTemplate template)
        {
            var entity = _world.CreateEntity();

            //component types that are higher in inheritance hierarchy will override the ones from lower hierarchy positions
            //TODO: add logging/explanation information on which components were overridden - will help with debugging content later
            foreach (var componentTemplate in CollectInheritedComponents(template)
                .Distinct(ComponentTemplate.Comparer))
            {
                if (_componentFactory.TryCreateComponent(componentTemplate, out var componentInstance))
                {
                    if (!_componentSetMethodsByTypes.TryGetValue(componentTemplate.Type, out var setMethod))
                    {
                        //TODO: make this faster with DynamicDelegate (emit IL dynamically for Set() - per component type)
                        setMethod = TypelessSetMethod.MakeGenericMethod(componentTemplate.Type);
                        _componentSetMethodsByTypes.Add(componentTemplate.Type, setMethod);
                    }

                    setMethod.Invoke(entity, new[] {componentInstance});
                }
            }

            return entity;
        }

        private IEnumerable<ComponentTemplate> CollectInheritedComponents(EntityTemplate template)
        {
            if(template == null) //precaution
                yield break;

            
            foreach (var componentTemplate in template.Components)
                yield return componentTemplate;         

            foreach (var templateName in template.InheritsFrom ?? Enumerable.Empty<string>())
            {
                var inheritedTemplate = _templateResolver.ResolveEntityTemplate(templateName);
                if (inheritedTemplate == null) 
                    continue;

                foreach (var componentTemplate in CollectInheritedComponents(inheritedTemplate))
                    yield return componentTemplate;
            }

        }   
    }
}
