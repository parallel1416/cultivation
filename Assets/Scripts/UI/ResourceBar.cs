using UnityEngine;

public class ResourceBar : MonoBehaviour
{
    private void Start()
    {
        if (ResourceBarUIManager.Instance != null)
        {
            ResourceBarUIManager.Instance.BindResourceBar(this.gameObject);
        }
        else
        {
            Debug.LogError("ResourceBarUIManager instance not found!");
        }
    }

    private void OnDestroy()
    {
        if (ResourceBarUIManager.Instance != null && ResourceBarUIManager.Instance.IsResourceBarActive())
        {
            ResourceBarUIManager.Instance.HideResourceBar();
        }
    }
}