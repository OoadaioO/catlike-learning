using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ClockMaker : MonoBehaviour {
    [SerializeField] private Transform[] indicatorTransformArray;

    private void Awake() {
        SetupIndicators();
    }

    
    [Button("setup")]
    public void SetupIndicators() {
        if (indicatorTransformArray == null) return;

        for (int i = 0; i < indicatorTransformArray.Length; i++) {
            indicatorTransformArray[i].rotation = Quaternion.Euler(0, 30 * i, 0);
        }
    }

}
