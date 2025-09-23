using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingCamera : MonoBehaviour {


    [SerializeField] private AnimationCurve yCurve;

    private Vector3 offset, position;
    private float viewFactorX, viewFactorY;
    private ParticleSystem stars;


    private void Awake() {
        offset = transform.localPosition;
        Camera c = GetComponent<Camera>();

        viewFactorY = Mathf.Tan(c.fieldOfView * 0.5f * Mathf.Deg2Rad);
        viewFactorX = viewFactorY * c.aspect;

        InitializeStars();
    }

    private void InitializeStars() {
        stars = GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = stars.shape;
        Vector3 position = shape.position;
        position.y = viewFactorY * position.z * 0.5f;
        shape.position = position;
        shape.scale = new Vector3(2f * viewFactorX, viewFactorY) * position.z;
    }


    private void OnValidate() {

        if (stars == null) return;

        InitializeStars();
    }

    public void StartNewGame() {
        Track(Vector3.zero);
        stars.Clear();
        stars.Emit(stars.main.maxParticles);
    }

    public void Track(Vector3 focusPoint) {
        position = focusPoint + offset;
        position.y = yCurve.Evaluate(position.y);
        transform.localPosition = position;
    }

    public FloatRange VisibleX(float z) =>
        FloatRange.PositionExtents(position.x, viewFactorX * (z - position.z));
}
