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


        static int enemyLayerMask = 1 << 8;

        static Collider[] buffer = new Collider[100];

        public static int BufferCount { get; private set; }

        public static void SetEnemyLayerMask(int layerMask) => enemyLayerMask = layerMask;


        public static int FillBuffer(Vector3 position, float range) {

            Vector3 top = position;
            top.y += 3f;

            BufferCount = Physics.OverlapCapsuleNonAlloc(
                position, top, range, buffer, enemyLayerMask
            );

            return BufferCount;
        }

        public static TargetPoint GetBufferd(int index) {
            TargetPoint target = buffer[index].GetComponent<TargetPoint>();
            Debug.Assert(target != null, "Target non-enemy!:"+buffer[index], buffer[index]);
            return target;
        }

        public static TargetPoint RandomBuffered => GetBufferd(Random.Range(0, BufferCount));


    }
}