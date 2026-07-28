namespace Wolfy.PropTools.Customer.Authoring
{
public enum VersionedDataMigrationResult
{
    UpToDate,
    Migrated,
    NewerThanSupported,
    InvalidVersion
}
}
