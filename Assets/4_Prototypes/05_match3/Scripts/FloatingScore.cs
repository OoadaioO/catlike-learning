using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingScore : MonoBehaviour {

    [SerializeField] private TextMeshPro displayText;

    [Range(0.1f, 1f)]
    [SerializeField] private float displayDuration = 0.5f;

    [Range(0f, 4f)]
    [SerializeField] private float raiseSpeed = 2f;

    private float age;

    private PrefabInstancePool<FloatingScore> pool;

    public void Show(Vector3 position, int value) {
        FloatingScore instance = pool.GetInstance(this);
        instance.pool = pool;
        instance.displayText.SetText("{0}", value);
        instance.transform.localPosition = position;
        instance.age = 0f;
    }

    private void Update() {
        age += Time.deltaTime;
        if (age >= displayDuration) {
            pool.Recycle(this);
        } else {
            Vector3 p = transform.localPosition;
            p.y += raiseSpeed * Time.deltaTime;
            transform.localPosition = p;
        }
    }
}
