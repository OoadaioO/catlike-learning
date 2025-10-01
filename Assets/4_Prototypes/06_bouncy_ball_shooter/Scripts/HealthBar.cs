using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bouncyball {
    public class HealthBar : MonoBehaviour {
        private Vector3 scale;
        float maxSize;

        private void Awake() {
            scale = transform.localScale;
            maxSize = scale.x;
            transform.localScale = Vector3.zero;
        }

        public void Show(float healthPercentage){
            scale.x = Mathf.Max(healthPercentage * maxSize, 0f);
            transform.localScale = scale;

        }
    }
}