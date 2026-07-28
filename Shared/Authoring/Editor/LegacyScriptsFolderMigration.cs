namespace Wolfy.PropTools.Customer.Authoring.Editor
{
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class LegacyScriptsFolderMigration
{
    private const string LegacyScriptsPath =
        "Assets/Wolfy_527/~ Supporting Files/Scripts";
    private const string LegacyScriptsGuid =
        "1a754f8d169daa9408e3740cfeeab3aa";
    private const string LegacyGhostMaterialPath =
        "Assets/Wolfy_527/~ Supporting Files/Ghost Material.mat";
    private const string LegacyGhostMaterialGuid =
        "4342400023fc9204e9fab7239dec44ef";
    private const string PlaceholderMarkerName =
        "Prefab Components Migration Placeholder.txt";
    private const string PlaceholderMarkerContents =
        "Wolfy_527 Prefab Components migration placeholder";
    private const string InstallerRoot =
        "Assets/Wolfy_527/~ Supporting Files/Prefab Components Installer";
    private const string InstallerRootGuid =
        "d9ad23fa951c4b14bfc93923b7f36b0e";
    private const string BuilderPackagePath =
        "Packages/com.wolfy527.prefab-builder";
    private const double CleanupQuietSeconds = 5.0;

    private static bool cleanupScheduled;
    private static double cleanupQuietSince;

    static LegacyScriptsFolderMigration()
    {
        EditorApplication.delayCall += TryMigrate;
    }

    private static void TryMigrate()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrWhiteSpace(projectRoot))
        {
            Debug.LogError(
                "[Wolfy_527 - Prefab Components] Could not locate the Unity project " +
                "root, so legacy assets were left unchanged.");
            return;
        }

        string legacyScriptsPath = Path.GetFullPath(
            Path.Combine(projectRoot, LegacyScriptsPath));
        string placeholderMarkerPath = Path.Combine(
            legacyScriptsPath,
            PlaceholderMarkerName);
        bool hasPlaceholders = IsOwnedPlaceholder(placeholderMarkerPath);
        bool hasLegacyScripts =
            Directory.Exists(legacyScriptsPath) && !hasPlaceholders;
        string legacyGhostPath = Path.GetFullPath(
            Path.Combine(projectRoot, LegacyGhostMaterialPath));
        bool hasLegacyGhost = File.Exists(legacyGhostPath);

        if (!hasLegacyScripts && !hasLegacyGhost)
        {
            ScheduleTemporaryCleanup();
            return;
        }

        string assetsRoot = Path.GetFullPath(Application.dataPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!legacyScriptsPath.StartsWith(
                assetsRoot,
                StringComparison.OrdinalIgnoreCase) ||
            !legacyGhostPath.StartsWith(
                assetsRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError(
                "[Wolfy_527 - Prefab Components] Refused to migrate a legacy path " +
                "outside this project's Assets folder.");
            return;
        }

        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        string backupParent = Path.Combine(
            projectRoot,
            "Legacy Package Backups",
            "Wolfy_527 Prefab Components",
            timestamp + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        string backupScriptsPath = Path.Combine(backupParent, "Scripts");
        string legacyScriptsMetaPath = legacyScriptsPath + ".meta";
        string backupScriptsMetaPath = backupScriptsPath + ".meta";
        string backupGhostPath = Path.Combine(
            backupParent,
            "Ghost Material.mat");
        string legacyGhostMetaPath = legacyGhostPath + ".meta";
        string backupGhostMetaPath = backupGhostPath + ".meta";
        bool scriptsMoved = false;
        bool ghostMoved = false;

        try
        {
            if (hasLegacyScripts &&
                IsRecognizedLegacyScripts(legacyScriptsPath))
            {
                Directory.CreateDirectory(backupParent);
                Directory.Move(legacyScriptsPath, backupScriptsPath);
                scriptsMoved = true;

                if (File.Exists(legacyScriptsMetaPath))
                {
                    File.Move(
                        legacyScriptsMetaPath,
                        backupScriptsMetaPath);
                }

                CreateCompilePlaceholders(
                    backupScriptsPath,
                    legacyScriptsPath);
            }
            else if (hasLegacyScripts)
            {
                Debug.LogWarning(
                    "[Wolfy_527 - Prefab Components] A Scripts folder exists at " +
                    "the old location, but it does not match the known legacy " +
                    "package. It was left unchanged for safety.");
            }

            if (hasLegacyGhost &&
                string.Equals(
                    AssetDatabase.AssetPathToGUID(LegacyGhostMaterialPath),
                    LegacyGhostMaterialGuid,
                    StringComparison.OrdinalIgnoreCase))
            {
                Directory.CreateDirectory(backupParent);
                File.Move(legacyGhostPath, backupGhostPath);
                ghostMoved = true;

                if (File.Exists(legacyGhostMetaPath))
                    File.Move(legacyGhostMetaPath, backupGhostMetaPath);
            }
            else if (hasLegacyGhost)
            {
                Debug.LogWarning(
                    "[Wolfy_527 - Prefab Components] A Ghost Material exists at " +
                    "the old location, but it does not match the known legacy " +
                    "asset. It was left unchanged for safety.");
            }

            if (!scriptsMoved && !ghostMoved)
                return;

            if (scriptsMoved)
            {
                Debug.Log(
                    "[Wolfy_527 - Prefab Components] Moved obsolete legacy " +
                    "scripts to:\n" + backupScriptsPath);
            }

            if (ghostMoved)
            {
                Debug.Log(
                    "[Wolfy_527 - Prefab Components] Moved the obsolete legacy " +
                    "Ghost Material to:\n" + backupGhostPath);
            }

            // All filesystem work is complete before requesting a refresh. This
            // callback may reload the scripting domain, so nothing transactional
            // is allowed to follow it.
            ScheduleTemporaryCleanup();
            EditorApplication.delayCall += () =>
                AssetDatabase.Refresh(ImportAssetOptions.Default);
        }
        catch (Exception exception)
        {
            try
            {
                RestoreLegacyAssets(
                    scriptsMoved,
                    legacyScriptsPath,
                    legacyScriptsMetaPath,
                    backupScriptsPath,
                    backupScriptsMetaPath,
                    ghostMoved,
                    legacyGhostPath,
                    legacyGhostMetaPath,
                    backupGhostPath,
                    backupGhostMetaPath);
            }
            catch (Exception rollbackException)
            {
                exception = new AggregateException(
                    "Legacy migration failed and could not be fully rolled back.",
                    exception,
                    rollbackException);
            }

            Debug.LogError(
                "[Wolfy_527 - Prefab Components] Could not migrate legacy assets. " +
                "Close tools using those files and restart Unity to retry.\n" +
                exception);
        }
    }

    private static bool IsRecognizedLegacyScripts(string legacyScriptsPath)
    {
        string actualGuid = AssetDatabase.AssetPathToGUID(LegacyScriptsPath);
        return string.Equals(
                   actualGuid,
                   LegacyScriptsGuid,
                   StringComparison.OrdinalIgnoreCase) ||
               Directory.Exists(Path.Combine(legacyScriptsPath, "Customer"));
    }

    private static void CreateCompilePlaceholders(
        string backupScriptsPath,
        string legacyScriptsPath)
    {
        string backupRoot = Path.GetFullPath(backupScriptsPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(legacyScriptsPath);

        foreach (string sourcePath in Directory.GetFiles(
                     backupScriptsPath,
                     "*.cs",
                     SearchOption.AllDirectories))
        {
            string normalizedSourcePath = Path.GetFullPath(sourcePath);
            if (!normalizedSourcePath.StartsWith(
                    backupRoot,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "A legacy script resolved outside the migration backup.");
            }

            string relativePath =
                normalizedSourcePath.Substring(backupRoot.Length);
            string placeholderPath = Path.Combine(
                legacyScriptsPath,
                relativePath);
            string placeholderParent = Path.GetDirectoryName(placeholderPath);
            if (!string.IsNullOrWhiteSpace(placeholderParent))
                Directory.CreateDirectory(placeholderParent);

            File.WriteAllText(
                placeholderPath,
                "// Temporary compile placeholder. Removed when Unity exits." +
                Environment.NewLine);
        }

        File.WriteAllText(
            Path.Combine(legacyScriptsPath, PlaceholderMarkerName),
            PlaceholderMarkerContents + Environment.NewLine);
    }

    private static void RestoreLegacyAssets(
        bool scriptsMoved,
        string legacyScriptsPath,
        string legacyScriptsMetaPath,
        string backupScriptsPath,
        string backupScriptsMetaPath,
        bool ghostMoved,
        string legacyGhostPath,
        string legacyGhostMetaPath,
        string backupGhostPath,
        string backupGhostMetaPath)
    {
        if (scriptsMoved)
        {
            if (Directory.Exists(legacyScriptsPath))
                Directory.Delete(legacyScriptsPath, true);
            if (File.Exists(legacyScriptsMetaPath))
                File.Delete(legacyScriptsMetaPath);

            if (Directory.Exists(backupScriptsPath))
                Directory.Move(backupScriptsPath, legacyScriptsPath);
            if (File.Exists(backupScriptsMetaPath))
            {
                File.Move(
                    backupScriptsMetaPath,
                    legacyScriptsMetaPath);
            }
        }

        if (ghostMoved)
        {
            if (File.Exists(legacyGhostPath))
                File.Delete(legacyGhostPath);
            if (File.Exists(legacyGhostMetaPath))
                File.Delete(legacyGhostMetaPath);

            if (File.Exists(backupGhostPath))
                File.Move(backupGhostPath, legacyGhostPath);
            if (File.Exists(backupGhostMetaPath))
                File.Move(backupGhostMetaPath, legacyGhostMetaPath);
        }
    }

    private static void ScheduleTemporaryCleanup()
    {
        if (cleanupScheduled)
            return;

        cleanupScheduled = true;
        cleanupQuietSince = EditorApplication.timeSinceStartup;
        EditorApplication.update += TryCleanupTemporaryAssets;
        EditorApplication.projectChanged += ResetCleanupQuietPeriod;
        EditorApplication.quitting += CleanupPlaceholdersOnExit;
    }

    private static void TryCleanupTemporaryAssets()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            cleanupQuietSince = EditorApplication.timeSinceStartup;
            return;
        }

        if (EditorApplication.timeSinceStartup - cleanupQuietSince <
            CleanupQuietSeconds)
            return;

        EditorApplication.update -= TryCleanupTemporaryAssets;
        EditorApplication.projectChanged -= ResetCleanupQuietPeriod;
        EditorApplication.quitting -= CleanupPlaceholdersOnExit;
        cleanupScheduled = false;

        bool cleanupFailed = false;
        string projectRoot =
            Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrWhiteSpace(projectRoot))
            return;

        string legacyScriptsPath = Path.GetFullPath(
            Path.Combine(projectRoot, LegacyScriptsPath));
        string markerPath = Path.Combine(
            legacyScriptsPath,
            PlaceholderMarkerName);
        bool hasPlaceholders = IsOwnedPlaceholder(markerPath);

        if (!IsBuilderInstalled(projectRoot) &&
            AssetDatabase.IsValidFolder(InstallerRoot) &&
            string.Equals(
                AssetDatabase.AssetPathToGUID(InstallerRoot),
                InstallerRootGuid,
                StringComparison.OrdinalIgnoreCase) &&
            !AssetDatabase.DeleteAsset(InstallerRoot))
        {
            cleanupFailed = true;
        }

        if (cleanupFailed)
        {
            cleanupScheduled = true;
            cleanupQuietSince = EditorApplication.timeSinceStartup;
            EditorApplication.update += TryCleanupTemporaryAssets;
            EditorApplication.projectChanged += ResetCleanupQuietPeriod;
            EditorApplication.quitting += CleanupPlaceholdersOnExit;
        }
        else if (hasPlaceholders)
        {
            // Unity's incremental compiler can retain source paths from the
            // import that introduced the legacy scripts. Removing those paths
            // in the same session produces transient CS2001 errors even after
            // compilation appears idle. The placeholders are empty and inert,
            // so remove them safely while Unity is closing instead.
            EditorApplication.quitting += CleanupPlaceholdersOnExit;
        }
    }

    private static void ResetCleanupQuietPeriod()
    {
        cleanupQuietSince = EditorApplication.timeSinceStartup;
    }

    private static void CleanupPlaceholdersOnExit()
    {
        EditorApplication.update -= TryCleanupTemporaryAssets;
        EditorApplication.projectChanged -= ResetCleanupQuietPeriod;
        EditorApplication.quitting -= CleanupPlaceholdersOnExit;
        cleanupScheduled = false;

        try
        {
            string projectRoot =
                Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
                return;

            string legacyScriptsPath = Path.GetFullPath(
                Path.Combine(projectRoot, LegacyScriptsPath));
            string markerPath = Path.Combine(
                legacyScriptsPath,
                PlaceholderMarkerName);
            if (IsOwnedPlaceholder(markerPath))
            {
                Directory.Delete(legacyScriptsPath, true);
                string legacyMetaPath = legacyScriptsPath + ".meta";
                if (File.Exists(legacyMetaPath))
                    File.Delete(legacyMetaPath);
            }

            string installerPath = Path.GetFullPath(
                Path.Combine(projectRoot, InstallerRoot));
            if (!IsBuilderInstalled(projectRoot) &&
                IsOwnedInstallerDirectory(installerPath))
            {
                Directory.Delete(installerPath, true);
                string installerMetaPath = installerPath + ".meta";
                if (File.Exists(installerMetaPath))
                    File.Delete(installerMetaPath);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Wolfy_527 - Prefab Components] Temporary migration " +
                "placeholders could not be removed. They are safe to delete " +
                "manually after Unity closes.\n" + exception);
        }
    }

    private static bool IsOwnedPlaceholder(string markerPath)
    {
        return File.Exists(markerPath) &&
               string.Equals(
                   File.ReadAllText(markerPath).Trim(),
                   PlaceholderMarkerContents,
                   StringComparison.Ordinal);
    }

    private static bool IsOwnedInstallerDirectory(string installerPath)
    {
        if (!Directory.Exists(installerPath))
            return false;

        string metaPath = installerPath + ".meta";
        if (!File.Exists(metaPath))
            return false;

        foreach (string line in File.ReadAllLines(metaPath))
        {
            string trimmed = line.Trim();
            if (!trimmed.StartsWith(
                    "guid:",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return string.Equals(
                trimmed.Substring("guid:".Length).Trim(),
                InstallerRootGuid,
                StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsBuilderInstalled(string projectRoot)
    {
        return Directory.Exists(Path.GetFullPath(
            Path.Combine(projectRoot, BuilderPackagePath)));
    }
}
}
