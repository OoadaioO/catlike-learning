using UnityEngine;

public class Graph : MonoBehaviour {
    [SerializeField] private Transform pointPrefab;

    [Range(10, 100)]
    [SerializeField] private int resolution = 10;

    private Transform[] points;

    private void Awake() {

        points = new Transform[resolution];

        float step = 2f / resolution;
        Vector3 position = Vector3.zero;
        Vector3 scale = Vector3.one * step;
        for (int i = 0; i < resolution; i++) {
            Transform point = points[i] = Instantiate(pointPrefab);
            point.SetParent(transform, false);
            position.x = (i + 0.5f) * step - 1f;
            point.localPosition = position;
            point.localScale = scale;
        }
    }

    private void Update() {
        for (int i = 0; i < points.Length; i++) {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
            point.localPosition = position;
        }
    }
}
