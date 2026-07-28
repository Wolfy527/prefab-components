namespace Wolfy.PropTools.Customer.Authoring
{
using UnityEngine;

#if VRC_SDK_VRCSDK3
using VRC.SDKBase;
#endif

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, null, "Assembly-CSharp", "AuthoringOnlyComponent")]
public abstract class AuthoringOnlyComponent : MonoBehaviour
#if VRC_SDK_VRCSDK3
    , IEditorOnly, IPreprocessCallbackBehaviour
#endif
{
    private bool playModeRemovalQueued;

    public virtual bool RemoveGameObjectWithComponent => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StripLoadedAuthoringComponents()
    {
        AuthoringOnlyComponent[] components =
            Object.FindObjectsOfType<AuthoringOnlyComponent>(true);

        foreach (AuthoringOnlyComponent component in components)
        {
            if (component == null || !component.gameObject.scene.IsValid())
                continue;

            component.QueuePlayModeRemoval();
        }
    }

    protected virtual void Awake()
    {
        QueuePlayModeRemoval();
    }

    protected virtual void OnEnable()
    {
        QueuePlayModeRemoval();
    }

    private void QueuePlayModeRemoval()
    {
        if (!Application.isPlaying || playModeRemovalQueued)
            return;

        playModeRemovalQueued = true;

        if (RemoveGameObjectWithComponent)
            Destroy(gameObject);
        else
            Destroy(this);
    }

#if VRC_SDK_VRCSDK3
    public virtual int PreprocessOrder => -10000;

    public virtual bool OnPreprocess()
    {
#if UNITY_EDITOR
        AuthoringBuildCleaner.StripAuthoringComponentsFrom(gameObject);
#endif
        return true;
    }
#endif
}
}
