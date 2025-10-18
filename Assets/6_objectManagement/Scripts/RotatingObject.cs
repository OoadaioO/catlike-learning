using UnityEngine;

namespace obj.mamagement {
    public class RotatingObject : PersistableObject {

        [SerializeField]
        Vector3 angularVelocity;

        private void FixedUpdate() {
            transform.Rotate(angularVelocity * Time.deltaTime);
        }
    }
}