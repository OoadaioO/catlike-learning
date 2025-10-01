
using TMPro;
using Unity.Collections;
using UnityEngine;

namespace bouncyball {

    public class Player : MonoBehaviour {

        [Min(0f)]
        [SerializeField]
        private float radius = 0.5f;

        [Min(0f)]
        [SerializeField]
        private float cursorFollowSpeed = 40f, cursorSnapDuration = 0.05f;

        [Min(0f)]
        [SerializeField]
        private float fireCooldown = 0.1f, fireSpreadAngle = 5f;

        [SerializeField] private BulletManager bulletManager;

        [SerializeField] private HealthBar healthBar;

        [Min(1)]
        [SerializeField] private int maxHealth = 10;


        [SerializeField] private TextMeshPro scoreDisplayText;

        [SerializeField]
        ParticleSystem explosionParticleSystem;

        [SerializeField, Min(1)]
        int hitParticleCount = 100, destructionParticleCount = 400;


        private Vector2 previousPosition, velocity, fireOffset;
        private float directionAngle, previousDirectionAngle, cooldown;

        private int lastCheckedHealth, lastCheckedScore;

        private NativeReference<int> health, score;

        public float Radius => radius;


        public Vector2 Position {
            get; private set;
        }

        public Vector2 TargetPosition {
            private get; set;
        }

        public bool FreeAim {
            get; set;
        }

        public void Initialize(ref HitJob hitJob) {

            health = new NativeReference<int>(Allocator.Persistent);

            score = new NativeReference<int>(Allocator.Persistent);
            scoreDisplayText.SetText("");

            gameObject.SetActive(false);

            hitJob.health = health;
            hitJob.score = score;
            hitJob.playerRadius = radius;
        }

        public void Dispose() {
            health.Dispose();
            score.Dispose();
        }


        public void StartNewGame(Vector2 position) {
            Position = TargetPosition = previousPosition = position;
            velocity = Vector2.zero;
            directionAngle = previousDirectionAngle = 0f;
            fireOffset = new Vector2(0f, radius);

            score.Value = lastCheckedScore = 0;
            scoreDisplayText.SetText("");

            health.Value = lastCheckedHealth = maxHealth;
            healthBar.Show(1f);
            gameObject.SetActive(true);
        }

        public void UpdateState(float dt) {
            previousPosition = Position;
            previousDirectionAngle = directionAngle;

            Vector2 targetVector = TargetPosition - Position;
            float squareTargetDistance = targetVector.sqrMagnitude;
            if (squareTargetDistance > 0.0001f) {
                Position = Vector2.SmoothDamp(
                    Position, TargetPosition, ref velocity, cursorSnapDuration, cursorFollowSpeed, dt
                );

                if (FreeAim) {
                    fireOffset = targetVector * (radius / Mathf.Sqrt(squareTargetDistance));
                    directionAngle = Mathf.Atan2(targetVector.x, targetVector.y) * -Mathf.Rad2Deg;
                }
            }

            cooldown -= dt;
            if (cooldown <= 0f) {
                cooldown += fireCooldown;
                bulletManager.Add(
                    Position - fireOffset,
                    directionAngle + 180f + Random.Range(-fireSpreadAngle, fireSpreadAngle)
                );

            }
        }

        public bool UpdateVisualization(float dtInterpolator) {
            transform.SetLocalPositionAndRotation(
                Vector2.LerpUnclamped(previousPosition, Position, dtInterpolator),
                Quaternion.Euler(0f, 0f, Mathf.LerpAngle(previousDirectionAngle, directionAngle, dtInterpolator))
            );

            if (lastCheckedScore != score.Value) {
                lastCheckedScore = score.Value;
                scoreDisplayText.SetText("{0}", lastCheckedScore);
            }

            if (lastCheckedHealth == health.Value) {
                return false;
            }
            lastCheckedHealth = health.Value;
            healthBar.Show((float)lastCheckedHealth / maxHealth);
            bool isDestroyed = lastCheckedHealth <= 0;

            explosionParticleSystem.Emit(
            new ParticleSystem.EmitParams {
                position = Position,
                applyShapeToPosition = true
            },
            isDestroyed ? destructionParticleCount : hitParticleCount
        );

            if (isDestroyed) {
                gameObject.SetActive(false);
            }
            return isDestroyed;

        }


    }

}