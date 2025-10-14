using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tower.defense {
    public class TargetPoint : MonoBehaviour {

        public Enemy Enemy { get; private set; }

        public Vector3 Position => transform.position;

        private void Awake() {
            Enemy = transform.root.GetComponent<Enemy>();
            Debug.Assert(Enemy != null, "Target Point Without Enemy root!", this);
            Debug.Assert(GetComponent<SphereCollider>() != null, "Target Point Without sphere collider", this);
        }
    }
}