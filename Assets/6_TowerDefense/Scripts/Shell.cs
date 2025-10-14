using UnityEngine;

namespace tower.defense {
    public class Shell : WarEntity {

        [SerializeField] LayerMask enemyLayerMask = ~0;

        Vector3 launchPoint, targetPoint, launchVelocity;
        float age, blastRadius, damage;

        const float gravity = 9.81f;

        public void Initialize(
            Vector3 launchPoint, Vector3 targetPoint, Vector3 launchVelocity,
            float blastRadius, float damage
        ) {
            this.launchPoint = launchPoint;
            this.targetPoint = targetPoint;
            this.launchVelocity = launchVelocity;
            this.blastRadius = blastRadius;
            this.damage = damage;
        }

        public override bool GameUpdate() {
            age += Time.deltaTime;
            Vector3 p = launchPoint + launchVelocity * age;
            p.y -= 0.5f * gravity * age * age;
            if (p.y <= 0f) {

                Game.SpawnExplosion().Initialize(targetPoint, blastRadius, damage);

                OriginFactory.Reclaim(this);
                return false;
            }

            transform.localPosition = p;

            Vector3 d = launchVelocity;
            d.y -= gravity * age;
            transform.localRotation = Quaternion.LookRotation(d);

            Game.SpawnExplosion().Initialize(p, 0.1f);

            return true;
        }
    }
}