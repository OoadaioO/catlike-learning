using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour {

    [SerializeField]
    bool floatToSleep = false;

    Rigidbody body;
    MeshRenderer render;
    float floatDelay;


    private void Awake() {
        body = GetComponent<Rigidbody>();
        render = GetComponent<MeshRenderer>();
        body.useGravity = false;
    }

    private void FixedUpdate() {
        if (floatToSleep) {
            if (body.IsSleeping()) {
                floatDelay = 0f;
                render.material.SetColor("_BaseColor", Color.grey);
                return;
            }
            if (body.velocity.sqrMagnitude < 0.0001f) {
                floatDelay += Time.deltaTime;
                if (floatDelay > 1f) {
                    return;
                }
                render.material.SetColor("_BaseColor", Color.yellow);
            } else {
                floatDelay = 0f;
            }
        }
        body.AddForce(CustomGravity.GetGravity(transform.position), ForceMode.Acceleration);
    }
}
