using UnityEngine;

namespace obj.mamagement {
    public class RotatingObject : GameLevelObject {

        [SerializeField]
        Vector3 angularVelocity;

        public override void GameUpdate() {
            transform.Rotate(angularVelocity * Time.deltaTime);
        }
        
    }
}