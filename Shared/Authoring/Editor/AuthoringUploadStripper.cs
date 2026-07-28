namespace Wolfy.PropTools.Customer.Authoring.Editor
{
using Wolfy.PropTools.Customer.Authoring;

#if UNITY_EDITOR && VRC_SDK_VRCSDK3

using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

public class AuthoringUploadStripper : IVRCSDKPreprocessAvatarCallback
{
    public int callbackOrder => -10000;

    public bool OnPreprocessAvatar(GameObject avatarGameObject)
    {
        if (avatarGameObject == null)
            return true;

        AuthoringBuildCleaner.StripAuthoringComponentsFrom(avatarGameObject);
        EditorUtility.SetDirty(avatarGameObject);

        return true;
    }
}

#endif
}
