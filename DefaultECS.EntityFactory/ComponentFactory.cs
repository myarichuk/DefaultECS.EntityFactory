﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Fasterflect;
using FastMember;

namespace DefaultECS.EntityFactory
{
    public class ComponentFactory
    {
        private readonly ITemplateResolver _templateResolver;
        private static readonly Dictionary<string, object> EmptyParams = new Dictionary<string, object>();
        private readonly Dictionary<string, object> _sharedComponents = new Dictionary<string, object>();
        private readonly Dictionary<string, TypeAccessor> _typeAccessors = new Dictionary<string, TypeAccessor>();

        public ComponentFactory(ITemplateResolver templateResolver)
        {
            _templateResolver = templateResolver;
        }

        public bool TryCreateComponent<T>(string name, out T component)
        {
            var success = TryCreateComponent(name, out var componentAsObject);
            component = default;
            if (!(componentAsObject is T componentAsT))
                return false;

            component = componentAsT;
            return success;
        }

        public bool TryCreateComponent(string name, out object component)
        {
            if (string.IsNullOrWhiteSpace(name)) 
                Throw.Exception<ArgumentNullException>(nameof(name));

            component = null;
            var template = _templateResolver.ResolveComponentTemplate(name);
            if (template == null)
                return false;

            if(string.IsNullOrWhiteSpace(template.Name))
                Throw.Exception<ArgumentNullException>(nameof(name), "Template's name should not be null. It is used to resolve the type of component object at runtime.");

            return TryCreateComponent(template, out component);
        }

        public bool TryCreateComponent(ComponentTemplate template, out object component)
        {
            var name = template.Name;
            if (template.IsShared)
            {
                Debug.Assert(name != null, nameof(name) + " != null"); //at this point should never be null...

                if (_sharedComponents.TryGetValue(name, out component))
                    return component != null; //should be always true, this is a precaution

                component = InstantiateAndInitialize(template);
                _sharedComponents.Add(name, component);
                return true;
            }

            component = InstantiateAndInitialize(template);
            return component != null;

            object InstantiateAndInitialize(ComponentTemplate componentTemplate)
            {
                if (!componentTemplate.Type.IsClass) 
                    return CreateInstanceWithDefaultCtor(componentTemplate);
                try
                {
                    return componentTemplate.Type.TryCreateInstance(componentTemplate.Defaults ?? EmptyParams);
                }
                catch (MissingMethodException) //didn't find ctor for params provided...
                {
                    //try the parameterless ctor and manually set the params
                    //especially if we are a struct - assume we have only parameterless ctor

                    return CreateInstanceWithDefaultCtor(componentTemplate);
                }
            }

            object CreateInstanceWithDefaultCtor(ComponentTemplate componentTemplate)
            {
                object instance;
                try
                {
                    instance = componentTemplate.Type.CreateInstance();
                }
                catch (MissingMemberException) //no default ctor
                {
                    instance = FormatterServices.GetUninitializedObject(componentTemplate.Type);
                }

                if (componentTemplate.Defaults?.Count > 0)
                {
                    Debug.Assert(name != null, nameof(name) + " != null"); //at this point should never be null...

                    if (!_typeAccessors.TryGetValue(name, out var typeAccessor))
                    {
                        typeAccessor = TypeAccessor.Create(componentTemplate.Type);
                        _typeAccessors.Add(name, typeAccessor);
                    }

                    foreach (var param in componentTemplate.Defaults)
                        typeAccessor[instance, param.Key] = param.Value;
                }

                return instance;
            }
        }
    }
}
