namespace TheGuild.Infrastructure.MongoDb.Collections;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MongoCollectionAttribute : Attribute
{
    public string Name { get; }

    public MongoCollectionAttribute(string name) => Name = name;
}