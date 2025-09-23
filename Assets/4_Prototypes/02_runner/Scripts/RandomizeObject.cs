using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeObject : SkylineObject {
    [SerializeField] private GameObject[] items;
    [SerializeField] private float spawnProbability;

    private void OnEnable() {
        for (int i = 0; i < items.Length; i++) {
            items[i].SetActive(Random.value < spawnProbability);
        }
    }

}
