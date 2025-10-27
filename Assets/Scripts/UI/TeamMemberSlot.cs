using UnityEngine;

/// <summary>
/// Legacy placeholder kept only to avoid missing-script warnings.
/// Remove this component and rely on EventPanelManager-driven team slots instead.
/// </summary>
[DisallowMultipleComponent]
public sealed class TeamMemberSlot : MonoBehaviour
{
    private void Awake()
    {
        LogController.LogWarning("TeamMemberSlot is obsolete. Please delete this component and use the new slot prefab managed by EventPanelManager.", this);
        enabled = false;
    }
}
