using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private int checkpointNumber = 1;
    [SerializeField] private Vector3 respawnPosition;
    [SerializeField] private int objectiveId;
    private bool activated;

    public void Configure(int number, Vector3 spawnPosition, int itemId)
    {
        checkpointNumber = number;
        respawnPosition = spawnPosition;
        objectiveId = itemId;
        activated = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
            return;

        NinjaGirlController player = other.GetComponentInParent<NinjaGirlController>();
        if (player == null || GameProgressManager.Instance == null)
            return;

        activated = true;
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
            trigger.enabled = false;

        GameProgressManager.Instance.ActivateCheckpoint(checkpointNumber, respawnPosition, objectiveId);
        StartCoroutine(CollectAndVanish());
    }

    private IEnumerator CollectAndVanish()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            startColors[i] = renderers[i].color;

        Vector3 startScale = transform.localScale;
        PlayCollectBurst();

        const float duration = 0.55f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            transform.position += Vector3.up * (Time.deltaTime * 1.1f);
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.65f, eased);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                Color color = startColors[i];
                color.a = 1f - progress;
                renderers[i].color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void PlayCollectBurst()
    {
        GameObject burstObject = new GameObject("CheckpointCollectBurst");
        burstObject.transform.position = transform.position + Vector3.up * 0.7f;
        ParticleSystem particles = burstObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        main.duration = 0.35f;
        main.loop = false;
        main.startLifetime = 0.55f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.13f);
        main.startColor = objectiveId == 1
            ? new Color(1f, 0.78f, 0.28f, 1f)
            : new Color(0.35f, 0.88f, 1f, 1f);
        main.gravityModifier = -0.2f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.22f;

        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sortingOrder = 20;
        particles.Play();
        Destroy(burstObject, 1.2f);
    }
}
