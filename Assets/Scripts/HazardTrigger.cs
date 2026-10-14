using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HazardTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<NinjaGirlController>() != null && GameProgressManager.Instance != null)
            GameProgressManager.Instance.HitHazard();
    }
}
