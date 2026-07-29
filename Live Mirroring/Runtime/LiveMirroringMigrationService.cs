#if UNITY_EDITOR
namespace Wolfy.PropTools.Customer.LiveMirroring
{
using Wolfy.PropTools.Customer.Authoring;

public static class LiveMirroringMigrationService
{
    public const int CurrentDataVersion = 1;

    private static readonly VersionedDataMigrationPipeline<LiveMirroringSystem>
        Pipeline = new VersionedDataMigrationPipeline<LiveMirroringSystem>(
            CurrentDataVersion,
            new LiveMirroringMigrationV0ToV1()
        );

    public static void MigrateIfNeeded(LiveMirroringSystem system)
    {
        if (system == null)
            return;

        TryMigrate(system);
    }

    public static VersionedDataMigrationResult TryMigrate(
        LiveMirroringSystem system)
    {
        if (system == null)
            return VersionedDataMigrationResult.InvalidVersion;

        return Pipeline.Migrate(
            system,
            system.DataVersion,
            system.SetDataVersion
        );
    }
}
}
#endif
