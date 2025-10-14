using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles click detection on the tower sprite in the Map view
/// Triggers transition to Tower scene when clicked
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TowerClickHandler : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.0f;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip clickSound;
    
    [Header("Visual Feedback (Optional)")]
    [SerializeField] private ParticleSystem clickParticles;
    [SerializeField] private float scaleAnimationDuration = 0.2f;
    [SerializeField] private Vector3 scaleMultiplier = new Vector3(1.1f, 1.1f, 1f);

    private Vector3 originalScale;
    private bool isAnimating = false;

    private void Awake()
    {
        originalScale = transform.localScale;
        
        // Ensure we have a collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("TowerClickHandler requires a Collider2D component!");
        }
    }

    private void OnMouseDown()
    {
        // Check if transition manager is busy
        if (SceneTransitionManager.Instance.IsTransitioning)
        {
            return;
        }

        // Check if we clicked on UI instead
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        OnTowerClicked();
    }

    private void OnTowerClicked()
    {
        Debug.Log("Tower clicked! Starting transition...");

        // Play click sound
        if (clickSound != null && AudioSource.FindObjectOfType<AudioSource>() != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, transform.position);
        }

        // Play particle effect
        if (clickParticles != null)
        {
            clickParticles.Play();
        }

        // Visual feedback - scale animation
        if (!isAnimating)
        {
            StartCoroutine(ScaleFeedback());
        }

        // Trigger transition
        Vector3 towerWorldPosition = transform.position;
        SceneTransitionManager.Instance.TransitionMapToTower(
            towerWorldPosition, 
            transitionDuration,
            OnTransitionComplete
        );
    }

    private System.Collections.IEnumerator ScaleFeedback()
    {
        isAnimating = true;
        
        // Scale up
        float elapsed = 0f;
        while (elapsed < scaleAnimationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleAnimationDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, Vector3.Scale(originalScale, scaleMultiplier), t);
            yield return null;
        }

        // Scale back
        elapsed = 0f;
        while (elapsed < scaleAnimationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleAnimationDuration / 2f);
            transform.localScale = Vector3.Lerp(Vector3.Scale(originalScale, scaleMultiplier), originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }

    private void OnTransitionComplete()
    {
        Debug.Log("Transition to Tower complete!");
        // Could trigger additional events here
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
