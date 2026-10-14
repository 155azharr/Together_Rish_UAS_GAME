using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<NinjaGirlController>() != null && GameProgressManager.Instance != null)
            GameProgressManager.Instance.CompleteLevel();
    }
}
