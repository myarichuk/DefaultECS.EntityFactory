namespace DefaultECS.EntityFactory
{
    public interface ITemplateResolver
    {
        ComponentTemplate ResolveComponentTemplate(string name);
        EntityTemplate ResolveEntityTemplate(string name);
    }
}