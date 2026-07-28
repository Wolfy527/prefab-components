namespace Wolfy.PropTools.Customer.Authoring
{
public interface IVersionedDataMigration<T>
{
    int FromVersion { get; }
    int ToVersion { get; }

    void Apply(T target);
}
}
