using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivelyCamera : MonoBehaviour {
    [SerializeField]
    private float jostleStrength = 40f,
        sprintStrength = 100f,
        dampingStrength = 10f,
        pushStrength = 1f,
        maxDeltaTime = 1f / 60f;
    private Vector3 anchorPosition, velocity;

    private void Awake() {
        anchorPosition = transform.position;
    }

    public float JostleY() => velocity.y += jostleStrength;

    public void PushXZ(Vector2 impulse) {
        velocity.x += pushStrength * impulse.x;
        velocity.y += pushStrength * impulse.y;
    }

    private void LateUpdate() {
        TimeStep(Time.deltaTime);
        float dt = Time.deltaTime;

        while (dt > maxDeltaTime) {
            TimeStep(maxDeltaTime);
            dt -= maxDeltaTime;
        }
        TimeStep(dt);
    }

    private void TimeStep(float dt) {
        Vector3 displacement = anchorPosition - transform.position;
        Vector3 acceleration = sprintStrength * displacement - velocity * dampingStrength;
        velocity += acceleration * dt;
        transform.localPosition += velocity * dt;
    }
}
