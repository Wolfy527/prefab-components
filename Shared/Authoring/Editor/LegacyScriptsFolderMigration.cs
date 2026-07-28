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
    private const string SessionKey =
        "Wolfy.PropComponents.LegacyScriptsFolderMigration.1.0.3.1";

    static LegacyScriptsFolderMigration()
    {
        EditorApplication.delayCall += TryMigrate;
    }

    private static void TryMigrate()
    {
        if (SessionState.GetBool(SessionKey, false))
            return;

        if (!AssetDatabase.IsValidFolder(LegacyScriptsPath))
            return;

        SessionState.SetBool(SessionKey, true);

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrWhiteSpace(projectRoot))
        {
            Debug.LogError(
                "[Wolfy_527 - Prefab Components] Could not locate the Unity project " +
                "root, so the obsolete Scripts folder was left unchanged.");
            return;
        }

        string assetsRoot = Path.GetFullPath(Application.dataPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string legacyAbsolutePath = Path.GetFullPath(
            Path.Combine(projectRoot, LegacyScriptsPath));

        if (!legacyAbsolutePath.StartsWith(
                assetsRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError(
                "[Wolfy_527 - Prefab Components] Refused to migrate a legacy path " +
                "outside this project's Assets folder.");
            return;
        }

        string actualGuid = AssetDatabase.AssetPathToGUID(LegacyScriptsPath);
        bool hasExpectedGuid = string.Equals(
            actualGuid,
            LegacyScriptsGuid,
            StringComparison.OrdinalIgnoreCase);
        bool hasLegacyCustomerFolder = Directory.Exists(
            Path.Combine(legacyAbsolutePath, "Customer"));

        if (!hasExpectedGuid && !hasLegacyCustomerFolder)
        {
            Debug.LogWarning(
                "[Wolfy_527 - Prefab Components] A Scripts folder exists at the old " +
                "location, but it does not match the known legacy package. It was " +
                "left unchanged for safety.");
            return;
        }

        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        string backupParent = Path.Combine(
            projectRoot,
            "Legacy Package Backups",
            "Wolfy_527 Prefab Components",
            timestamp);
        string backupScriptsPath = Path.Combine(backupParent, "Scripts");
        string legacyMetaPath = legacyAbsolutePath + ".meta";
        string backupMetaPath = backupScriptsPath + ".meta";

        bool folderMoved = false;
        try
        {
            Directory.CreateDirectory(backupParent);
            Directory.Move(legacyAbsolutePath, backupScriptsPath);
            folderMoved = true;

            if (File.Exists(legacyMetaPath))
                File.Move(legacyMetaPath, backupMetaPath);

            Debug.Log(
                "[Wolfy_527 - Prefab Components] Moved obsolete legacy scripts to: " +
                backupScriptsPath);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[Wolfy_527 - Prefab Components] Could not move the obsolete Scripts " +
                "folder out of Assets. Close tools using files in that folder and " +
                "restart Unity to retry.\n" + exception);
        }
        finally
        {
            if (folderMoved)
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
    }

}
}
