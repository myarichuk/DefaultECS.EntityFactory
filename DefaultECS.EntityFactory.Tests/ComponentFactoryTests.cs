using System;
using System.Collections.Generic;
using Xunit;

namespace DefaultECS.EntityFactory.Tests
{
    public class ComponentFactoryTests
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

        public struct FoobarStructComponent
        {
            public double Value;
        }

        public enum Color
        {
            Red,
            Green,
            Blue,
            White,
            Black
        }

        public class ComponentWithEnum
        {
            public Color Color { get; set; }
        }

        public struct StructComponentWithEnum
        {
            public Color Color { get; set; }
        }

        public class BarfooComponent
        { }

        public class FoobarComponent
        {
            public double ValueField;
            public double ValueProperty { get; set; }
            public bool BoolProperty { get; set; }
            public readonly string CtorValueField;

            public FoobarComponent(string ctorValueField)
            {
                CtorValueField = ctorValueField;
            }
        }

        [Fact]
        public void Should_throw_argument_null_on_null_name()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>()));
            Assert.Throws<ArgumentNullException>(() => factory.TryCreateComponent((string)null, out _));
        }

        [Fact]
        public void Should_throw_argument_null_on_null_name_inside_the_template()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(FoobarComponent), new ComponentTemplate() }
            }));
            Assert.Throws<ArgumentNullException>(() => factory.TryCreateComponent(nameof(FoobarComponent), out _));
        }

        [Fact]
        public void Can_resolve_component()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(BarfooComponent), new ComponentTemplate{ Type = typeof(BarfooComponent) } }
            }));

            Assert.True(factory.TryCreateComponent(nameof(BarfooComponent), out var component));
            Assert.IsType<BarfooComponent>(component);

            Assert.True(factory.TryCreateComponent<BarfooComponent>(nameof(BarfooComponent), out _));
        }

        [Fact]
        public void Can_resolve_struct_component()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(FoobarStructComponent), new ComponentTemplate
                    {
                        Type = typeof(FoobarStructComponent),
                        Defaults = new Dictionary<string, object>
                        {
                            { "Value", 123.123 }
                        }
                    }
                }
            }));

            Assert.True(factory.TryCreateComponent(nameof(FoobarStructComponent), out var component));
            Assert.IsType<FoobarStructComponent>(component);
            Assert.Equal(123.123, ((FoobarStructComponent)component).Value);
        }

        [Fact]
        public void Can_resolve_component_with_enum()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(ComponentWithEnum), new ComponentTemplate
                    {
                        Type = typeof(ComponentWithEnum),
                        Defaults = new Dictionary<string, object>
                        {
                            { "Color", Color.Blue }
                        }
                    }
                }
            }));

            Assert.True(factory.TryCreateComponent(nameof(ComponentWithEnum), out var component));
            Assert.IsType<ComponentWithEnum>(component);
            Assert.Equal(Color.Blue, ((ComponentWithEnum)component).Color);
        }

        [Fact]
        public void Can_resolve_struct_component_with_enum()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(StructComponentWithEnum), new ComponentTemplate
                    {
                        Type = typeof(StructComponentWithEnum),
                        Defaults = new Dictionary<string, object>
                        {
                            { "Color", Color.Black }
                        }
                    }
                }
            }));

            Assert.True(factory.TryCreateComponent(nameof(StructComponentWithEnum), out var component));
            Assert.IsType<StructComponentWithEnum>(component);
            Assert.Equal(Color.Black, ((StructComponentWithEnum)component).Color);
        }

        [Fact] 
        public void Should_resolve_the_same_instance_for_readonly_compoennts()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(BarfooComponent), new ComponentTemplate
                    {
                        Type = typeof(BarfooComponent),
                        IsShared = true
                    }
                }
            }));

            Assert.True(factory.TryCreateComponent(nameof(BarfooComponent), out var component));
            Assert.IsType<BarfooComponent>(component);

            Assert.True(factory.TryCreateComponent(nameof(BarfooComponent), out var component2));
            Assert.Same(component, component2);
        }
        /*
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
         */


        [Fact]
        public void Can_resolve_and_initialize_fields_and_properties_and_without_ctor_params()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(FoobarComponent), new ComponentTemplate
                {
                    Type = typeof(FoobarComponent), 
                    Defaults = new Dictionary<string, object>
                    {
                        { "ValueProperty", 456.456 },
                    }
                } }
            }));

            Assert.True(factory.TryCreateComponent<FoobarComponent>(nameof(FoobarComponent), out var component));
            Assert.Equal(456.456,component.ValueProperty);
        }

        [Fact]
        public void Can_resolve_and_initialize_fields_and_properties_and_ctor_params()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(FoobarComponent), new ComponentTemplate
                {
                    Type = typeof(FoobarComponent), 
                    Defaults = new Dictionary<string, object>
                    {
                        { "ValueField", 123.123 },
                        { "ValueProperty", 456.456 },
                        { "CtorValueField","ABC" },
                        { "Not_existing_field", "AAA" } //can handle not existing params/fields
                    }
                } }
            }));

            Assert.True(factory.TryCreateComponent<FoobarComponent>(nameof(FoobarComponent), out var component));
            Assert.Equal(123.123,component.ValueField);
            Assert.Equal(456.456,component.ValueProperty);
            Assert.Equal("ABC",component.CtorValueField);
        }

        [Fact]
        public void Can_convert_mismatched_types_from_number_to_string()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(FoobarComponent), new ComponentTemplate
                {
                    Type = typeof(FoobarComponent), 
                    Defaults = new Dictionary<string, object>
                    {
                        { "ValueField", 123.123 },
                        { "ValueProperty", 456.456 },
                        { "CtorValueField", 123 }
                    }
                } }
            }));

            Assert.True(factory.TryCreateComponent<FoobarComponent>(nameof(FoobarComponent), out var component));
            Assert.Equal(123.123,component.ValueField);
            Assert.Equal(456.456,component.ValueProperty);
            Assert.Equal("123", component.CtorValueField);
        }

        [Fact]
        public void Can_convert_mismatched_types_from_boolean_to_string()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(FoobarComponent), new ComponentTemplate
                {
                    Type = typeof(FoobarComponent), 
                    Defaults = new Dictionary<string, object>
                    {
                        { "ValueField", 123.123 },
                        { "ValueProperty", 456.456 },
                        { "CtorValueField", true }
                    }
                } }
            }));

            Assert.True(factory.TryCreateComponent<FoobarComponent>(nameof(FoobarComponent), out var component));
            Assert.Equal(123.123,component.ValueField);
            Assert.Equal(456.456,component.ValueProperty);
            Assert.Equal("True", component.CtorValueField);
        }

        [Fact]
        public void Can_convert_mismatched_types_from_string_to_boolean()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(FoobarComponent), new ComponentTemplate
                {
                    Type = typeof(FoobarComponent), 
                    Defaults = new Dictionary<string, object>
                    {
                        { "ValueField", 123.123 },
                        { "ValueProperty", 456.456 },
                        { "CtorValueField", true },
                        { "BoolProperty", "True" }
                    }
                } }
            }));

            Assert.True(factory.TryCreateComponent<FoobarComponent>(nameof(FoobarComponent), out var component));
            Assert.Equal(123.123,component.ValueField);
            Assert.Equal(456.456,component.ValueProperty);
            Assert.True(component.BoolProperty);
        }

        [Fact]
        public void Should_insert_default_instead_of_types_that_cannot_be_converted()
        {
            var factory = new ComponentFactory(new DictionaryComponentTemplateResolver(new Dictionary<string, ComponentTemplate>
            {
                { nameof(FoobarComponent), new ComponentTemplate
                {
                    Type = typeof(FoobarComponent), 
                    Defaults = new Dictionary<string, object>
                    {
                        { "ValueField", 123.123 },
                        { "ValueProperty", false }, //ValueProperty property is double but we provide a boolean
                        { "CtorValueField", true }
                    }
                } }
            }));

            Assert.True(factory.TryCreateComponent<FoobarComponent>(nameof(FoobarComponent), out var component));
            Assert.Equal(123.123,component.ValueField);
            Assert.Equal(0,component.ValueProperty); //because of mismatched type, convert it to double's default
            Assert.Equal("True", component.CtorValueField);
        }
    }
}
