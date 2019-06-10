using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using Xunit;

namespace DefaultECS.EntityFactory.Tests
{
    public class EntityTests
    {
        public class DictionaryEntityTemplateResolver : ITemplateResolver
        {
            private readonly Dictionary<string, EntityTemplate> _templates;

            public DictionaryEntityTemplateResolver(Dictionary<string, EntityTemplate> templates)
            {
                _templates = templates;
            }

            public ComponentTemplate ResolveComponentTemplate(string name)
            {
                throw new NotImplementedException();
             
            }

            public EntityTemplate ResolveEntityTemplate(string name)
            {
                _templates.TryGetValue(name, out var template);
                return template;
            }
        }

        [Fact]
        public void Can_compose_simple_entity()
        {
            var factory = new EntityFactory(
                new DictionaryEntityTemplateResolver(new Dictionary<string, EntityTemplate>()),
                new World());

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

        [Fact]
        public void Can_compose_entity_hierarchy()
        {
            var factory = new EntityFactory(
                new DictionaryEntityTemplateResolver(new Dictionary<string, EntityTemplate>
                {
                    { "ChildFirstLevel",
                        new EntityTemplate
                        {
                            Name = "ChildFirstLevel",
                            Children = new List<string>{ "ChildSecondLevel" },
                            Components = new List<ComponentTemplate>
                            {
                                new ComponentTemplate
                                {
                                    Type = typeof(ComponentFactoryTests.StructComponentWithEnum),
                                    Defaults = new Dictionary<string, object>
                                    {
                                        { "Color", ComponentFactoryTests.Color.Blue }
                                    }
                                }
                            }
                        }
                    },
                    { "ChildSecondLevel",
                        new EntityTemplate
                        {
                            Name = "ChildSecondLevel",
                            Components = new List<ComponentTemplate>
                            {
                                new ComponentTemplate
                                {
                                    Type = typeof(ComponentFactoryTests.FoobarComponent),
                                    Defaults = new Dictionary<string, object>
                                    {
                                        { "ValueProperty", 123.456 }
                                    }
                                }
                            }
                        }
                    }
                }),
                new World());

            Assert.True(factory.TryCreate(new EntityTemplate
            {
                Name = "RootEntity",
                Children = new List<string>{ "ChildFirstLevel" },
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
            }, out var rootEntity));

            var firstLevelChildren = rootEntity.GetChildren().ToArray();

            ref var foobarStructComponent = ref rootEntity.Get<ComponentFactoryTests.FoobarStructComponent>();
            ref var structComponentWithEnum = ref firstLevelChildren[0].Get<ComponentFactoryTests.StructComponentWithEnum>();

            var secondLevelChildren = firstLevelChildren[0].GetChildren().ToArray();
            ref var foobarComponent = ref secondLevelChildren[0].Get<ComponentFactoryTests.FoobarComponent>();

            Assert.Equal(ComponentFactoryTests.Color.Blue, structComponentWithEnum.Color);
            Assert.Equal(123.123, foobarStructComponent.Value);
            Assert.Equal(123.456, foobarComponent.ValueProperty);
        }

            [Fact]
        public void Can_compose_entity_hierarchy_that_starts_from_non_root_entity()
        {
            var resolver = new DictionaryEntityTemplateResolver(new Dictionary<string, EntityTemplate>
            {
                { "ChildFirstLevel",
                    new EntityTemplate
                    {
                        Name = "ChildFirstLevel",
                        Parent = "RootEntity",
                        Children = new List<string>{ "ChildSecondLevel" },
                        Components = new List<ComponentTemplate>
                        {
                            new ComponentTemplate
                            {
                                Type = typeof(ComponentFactoryTests.StructComponentWithEnum),
                                Defaults = new Dictionary<string, object>
                                {
                                    { "Color", ComponentFactoryTests.Color.Blue }
                                }
                            }
                        }
                    }
                },
                { "ChildSecondLevel",
                    new EntityTemplate
                    {
                        Name = "ChildSecondLevel",
                        Parent = "ChildFirstLevel",
                        Components = new List<ComponentTemplate>
                        {
                            new ComponentTemplate
                            {
                                Type = typeof(ComponentFactoryTests.FoobarComponent),
                                Defaults = new Dictionary<string, object>
                                {
                                    { "ValueProperty", 123.456 }
                                }
                            }
                        }
                    }
                },
                {
                    "RootEntity",
                    new EntityTemplate
                    {
                        Name = "RootEntity",
                        Children = new List<string>{ "ChildFirstLevel" },
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
                    }
                }
            });

            var factory = new EntityFactory(resolver, new World());

            Assert.True(factory.TryCreate(resolver.ResolveEntityTemplate("ChildSecondLevel"), out var rootEntity));

            var firstLevelChildren = rootEntity.GetChildren().ToArray();
            
            ref var foobarStructComponent = ref rootEntity.Get<ComponentFactoryTests.FoobarStructComponent>();
            ref var structComponentWithEnum = ref firstLevelChildren[0].Get<ComponentFactoryTests.StructComponentWithEnum>();

            var secondLevelChildren = firstLevelChildren[0].GetChildren().ToArray();
            ref var foobarComponent = ref secondLevelChildren[0].Get<ComponentFactoryTests.FoobarComponent>();

            Assert.Equal(ComponentFactoryTests.Color.Blue, structComponentWithEnum.Color);
            Assert.Equal(123.123, foobarStructComponent.Value);
            Assert.Equal(123.456, foobarComponent.ValueProperty);
        }

        [Fact]
        public void Can_compose_entity_that_inherits()
        {
            var factory = new EntityFactory(
                new DictionaryEntityTemplateResolver(new Dictionary<string, EntityTemplate>
                {
                    { "ParentEntity", 
                        new EntityTemplate 
                        {
                            Name = "ParentEntity",
                            Components = new List<ComponentTemplate>
                            {
                                new ComponentTemplate
                                {
                                    Type = typeof(ComponentFactoryTests.ComponentWithEnum),
                                    Defaults = new Dictionary<string, object>
                                    {
                                        { "Color", ComponentFactoryTests.Color.Blue }
                                    }
                                }               
                            }
                        }
                    }
                }),
                new World());

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
                    }                   
                },
                InheritsFrom = new List<string> { "ParentEntity" }
            }, out var entity));

            ref var foobarStructComponent = ref entity.Get<ComponentFactoryTests.FoobarStructComponent>();
            ref var componentWithEnum = ref entity.Get<ComponentFactoryTests.ComponentWithEnum>();

            Assert.Equal(123.123, foobarStructComponent.Value);
            Assert.Equal(ComponentFactoryTests.Color.Blue, componentWithEnum.Color);
        }

        [Fact]
        public void Can_compose_entity_that_inherits_will_override_default_with_types_upwards_in_inheritance_hierarchy()
        {
            var factory = new EntityFactory(
                new DictionaryEntityTemplateResolver(new Dictionary<string, EntityTemplate>
                {
                    { "ParentEntity", 
                        new EntityTemplate 
                        {
                            Name = "ParentEntity",
                            Components = new List<ComponentTemplate>
                            {
                                new ComponentTemplate
                                {
                                    Type = typeof(ComponentFactoryTests.ComponentWithEnum),
                                    Defaults = new Dictionary<string, object>
                                    {
                                        { "Color", ComponentFactoryTests.Color.Blue }
                                    }
                                },
                                new ComponentTemplate
                                {
                                    Type = typeof(ComponentFactoryTests.FoobarStructComponent),
                                    Defaults = new Dictionary<string, object>
                                    {
                                        { "Value", 555.555 }
                                    }
                                }   
                            }
                        }
                    }
                }),
                new World());

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
                    }                   
                },
                InheritsFrom = new List<string> { "ParentEntity" }
            }, out var entity));

            ref var foobarStructComponent = ref entity.Get<ComponentFactoryTests.FoobarStructComponent>();
            ref var componentWithEnum = ref entity.Get<ComponentFactoryTests.ComponentWithEnum>();

            Assert.Equal(123.123, foobarStructComponent.Value);
            Assert.Equal(ComponentFactoryTests.Color.Blue, componentWithEnum.Color);
        }
    }
}
