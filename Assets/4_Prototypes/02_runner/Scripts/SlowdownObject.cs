using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowdownObject : SkylineObject {
    [SerializeField] private Transform item;
    [SerializeField] private ParticleSystem explosionSystem;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float speedFactor = 0.75f;
    [SerializeField] private float spawnProbability = 0.5f;

    public override void Check(Runner runner) {
        if (
            item.gameObject.activeSelf &&
            ((Vector2)item.position - runner.Position).sqrMagnitude < radius * radius
        ) {
            item.gameObject.SetActive(false);
            explosionSystem.Emit(explosionSystem.main.maxParticles);
            runner.SpeedX *= speedFactor;
        }
    }

    private void OnEnable() {
        item.gameObject.SetActive(Random.value < spawnProbability);
        explosionSystem.Clear();
    }

}
