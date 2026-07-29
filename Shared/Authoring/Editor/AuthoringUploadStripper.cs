namespace Wolfy.PropTools.Customer.Authoring.Editor
{
using Wolfy.PropTools.Customer.Authoring;

#if UNITY_EDITOR && VRC_SDK_VRCSDK3

using System.Diagnostics;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using Debug = UnityEngine.Debug;

public class AuthoringUploadStripper : IVRCSDKPreprocessAvatarCallback
{
    public int callbackOrder => -10000;

    public bool OnPreprocessAvatar(GameObject avatarGameObject)
    {
        if (avatarGameObject == null)
            return true;

        Stopwatch stopwatch = Stopwatch.StartNew();
        AuthoringBuildCleaner.CleanupReport report =
            AuthoringBuildCleaner.StripAuthoringComponentsFrom(
                avatarGameObject);
        stopwatch.Stop();

        if (report.HasChanges)
        {
            Debug.Log(
                $"[Prefab Components Timing] Upload cleanup removed " +
                $"{report.ComponentsRemoved} authoring component(s) and " +
                $"{report.GameObjectsRemoved} generated object(s) in " +
                $"{stopwatch.Elapsed.TotalMilliseconds:F2} ms.",
                avatarGameObject);
        }

        return true;
    }
}

#endif
}
