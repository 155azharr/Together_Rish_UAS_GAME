using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.2f;

    private Vector3 cameraStartPosition;
    private Vector3 backgroundStartPosition;

    public void Configure(Camera cameraToFollow, float factor)
    {
        targetCamera = cameraToFollow;
        parallaxFactor = Mathf.Clamp01(factor);
        CacheStartPositions();
    }

    private void Start()
    {
        CacheStartPositions();
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        if (targetCamera == null)
            return;

        Vector3 cameraDelta = targetCamera.transform.position - cameraStartPosition;
        transform.position = backgroundStartPosition + new Vector3(
            cameraDelta.x * parallaxFactor,
            cameraDelta.y * parallaxFactor,
            0f);
    }

    private void CacheStartPositions()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        if (targetCamera == null)
            return;

        cameraStartPosition = targetCamera.transform.position;
        backgroundStartPosition = transform.position;
    }
}
