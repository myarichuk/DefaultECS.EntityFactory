using CommonServiceLocator;

namespace DefaultECS.EntityFactory
{
    public class DefaultTemplateResolver : ITemplateResolver
    {
        private readonly IServiceLocator _serviceLocator;

        public DefaultTemplateResolver(IServiceLocator serviceLocator) => 
            _serviceLocator = serviceLocator;

        public ComponentTemplate ResolveComponentTemplate(string name) =>
             _serviceLocator.GetInstance<ComponentTemplate>(name);

        public EntityTemplate ResolveEntityTemplate(string name) =>
            _serviceLocator.GetInstance<EntityTemplate>(name);
    }
}
