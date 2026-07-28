namespace Wolfy.PropTools.Customer.Authoring
{
using System;
using System.Collections.Generic;

public sealed class VersionedDataMigrationPipeline<T>
{
    private readonly Dictionary<int, IVersionedDataMigration<T>> migrationsByVersion =
        new Dictionary<int, IVersionedDataMigration<T>>();

    public int CurrentVersion { get; }

    public VersionedDataMigrationPipeline(
        int currentVersion,
        params IVersionedDataMigration<T>[] migrations)
    {
        if (currentVersion < 0)
            throw new ArgumentOutOfRangeException(nameof(currentVersion));

        CurrentVersion = currentVersion;

        if (migrations != null)
        {
            foreach (IVersionedDataMigration<T> migration in migrations)
                Register(migration);
        }

        ValidateCompleteChain();
    }

    public VersionedDataMigrationResult Migrate(
        T target,
        int dataVersion,
        Action<int> setDataVersion)
    {
        if (setDataVersion == null)
            throw new ArgumentNullException(nameof(setDataVersion));

        if (dataVersion < 0)
            return VersionedDataMigrationResult.InvalidVersion;

        if (dataVersion > CurrentVersion)
            return VersionedDataMigrationResult.NewerThanSupported;

        if (dataVersion == CurrentVersion)
            return VersionedDataMigrationResult.UpToDate;

        int version = dataVersion;

        while (version < CurrentVersion)
        {
            IVersionedDataMigration<T> migration = migrationsByVersion[version];
            migration.Apply(target);
            version = migration.ToVersion;

            // Persist every completed step. If a later migration fails, Unity can
            // safely resume at the first unfinished step instead of rerunning work.
            setDataVersion(version);
        }

        return VersionedDataMigrationResult.Migrated;
    }

    private void Register(IVersionedDataMigration<T> migration)
    {
        if (migration == null)
            throw new ArgumentException("Migration entries cannot be null.");

        if (migration.FromVersion < 0)
        {
            throw new ArgumentException(
                "Migration source versions cannot be negative.");
        }

        if (migration.ToVersion != migration.FromVersion + 1)
        {
            throw new ArgumentException(
                $"Migration {migration.FromVersion} -> {migration.ToVersion} must advance exactly one version.");
        }

        if (migration.ToVersion > CurrentVersion)
        {
            throw new ArgumentException(
                $"Migration {migration.FromVersion} -> {migration.ToVersion} exceeds current version {CurrentVersion}.");
        }

        if (migrationsByVersion.ContainsKey(migration.FromVersion))
        {
            throw new ArgumentException(
                $"More than one migration starts at version {migration.FromVersion}.");
        }

        migrationsByVersion.Add(migration.FromVersion, migration);
    }

    private void ValidateCompleteChain()
    {
        for (int version = 0; version < CurrentVersion; version++)
        {
            if (!migrationsByVersion.ContainsKey(version))
            {
                throw new ArgumentException(
                    $"Missing required migration {version} -> {version + 1}.");
            }
        }
    }
}
}
