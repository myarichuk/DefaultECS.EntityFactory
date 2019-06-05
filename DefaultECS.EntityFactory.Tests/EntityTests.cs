using System;
using System.Collections.Generic;
using System.Text;
using DefaultEcs;
using Xunit;

namespace DefaultECS.EntityFactory.Tests
{
    public class EntityTests
    {
        public class DictionaryComponentTemplateResolver : ITemplateResolver
        {
            private readonly Dictionary<string, ComponentTemplate> _componentTemplates;

            public DictionaryComponentTemplateResolver(Dictionary<string, ComponentTemplate> componentTemplates)
            {
                _componentTemplates = componentTemplates;
            }

            public ComponentTemplate ResolveComponentTemplate(string name)
            {
                _componentTemplates.TryGetValue(name, out var template);
                return template;
            }

            public EntityTemplate ResolveEntityTemplate(string name)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Can_compose_entity()
        {
            var factory = new EntityFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>()), new World());

            Assert.True(factory.TryCreate(new EntityTemplate
            {
                Name = "TestEntity",
                Components = new List<ComponentTemplate>
                {
                    new ComponentTemplate
                    {
                        Type = typeof(ComponentFactoryTests.FoobarStructComponent),
                        Defaults = new Dictionary<string, object>
                        {
                            { "Value", 123.123 }
                        }
                    },
                    new ComponentTemplate
                    {
                        Type = typeof(ComponentFactoryTests.ComponentWithEnum),
                        Defaults = new Dictionary<string, object>
                        {
                            { "Color", ComponentFactoryTests.Color.Blue }
                        }
                    }
                }
            }, out var entity));

            ref var foobarStructComponent = ref entity.Get<ComponentFactoryTests.FoobarStructComponent>();
            ref var componentWithEnum = ref entity.Get<ComponentFactoryTests.ComponentWithEnum>();

            Assert.Equal(123.123, foobarStructComponent.Value);
            Assert.Equal(ComponentFactoryTests.Color.Blue, componentWithEnum.Color);
        }
    }
}
