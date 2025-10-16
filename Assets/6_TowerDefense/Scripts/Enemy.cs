using tower.defense;
using Unity.VisualScripting;
using UnityEngine;
namespace tower.defense {
    public class Enemy : GameBehavior {

        [SerializeField] Transform model;
        [SerializeField] EnemyAnimationConfig animationConfig;

        EnemyAnimator animator;

        EnemyFactory originFactory;

        GameTile tileFrom, tileTo;
        Vector3 positionFrom, positionTo;
        float progress, progressFactor;

        Direction direction;
        DirectionChange directionChange;
        float directionAngleFrom, directionAngleTo;

        float pathOffset;
        float speed;

        Collider targetPointCollider;
        public Collider TargetPointCollider {
            get => targetPointCollider;
            set {
                Debug.Assert(targetPointCollider == null, "Redefined collider!");
                targetPointCollider = value;
            }
        }

        public float Scale { get; private set; }
        public float Health { get; private set; }

        public bool IsValidTarget => animator.CurrentClip == EnemyAnimator.Clip.Move;


        private void Awake() {
            animator.Configure(
                model.GetChild(0).gameObject.AddComponent<Animator>(),
                animationConfig
            );
        }

        private void OnDestroy() {
            animator.Destory();
        }

        public void Initialize(float scale, float speed, float pathOffset, float health) {
            Scale = scale;
            model.localScale = new Vector3(scale, scale, scale);
            this.speed = speed;
            this.pathOffset = pathOffset;
            Health = health;
            animator.PlayIntro();
            targetPointCollider.enabled = false;
        }

        public EnemyFactory OriginFactory {
            get => originFactory;
            set {
                Debug.Assert(originFactory == null, "Redefined Origin Factory");
                originFactory = value;
            }
        }

        public void SpawnOn(GameTile tile) {
            Debug.Assert(tile.NextTileOnPath != null, "no Where to go", this);
            tileFrom = tile;
            tileTo = tile.NextTileOnPath;
            progress = 0;
            PrepareIntro();
        }



        public override bool GameUpdate() {
            animator.GameUpdate();

            if (animator.CurrentClip == EnemyAnimator.Clip.Intro) {
                if (!animator.IsDone) {
                    return true;
                }
                animator.PlayMove(animationConfig.MovingAnimationSpeed * speed / Scale);
                targetPointCollider.enabled = true;
            } else if (animator.CurrentClip >= EnemyAnimator.Clip.Outro) {
                if (animator.IsDone) {
                    Recycle();
                    return false;
                }
                return true;
            }

            if (Health <= 0f) {
                animator.PlayDying();
                targetPointCollider.enabled = false;
                return true;
            }

            progress += Time.deltaTime * progressFactor;
            while (progress >= 1f) {
                if (tileTo == null) {
                    Game.EnemyReachedDestination();
                    animator.PlayOutro();
                    targetPointCollider.enabled = false;
                    return true;
                }
                // 时间标准化成插值百分比
                progress = (progress - 1f) / progressFactor;
                PrepareNextState();
                // 插值百分比转换成时间
                progress *= progressFactor;
            }

            if (directionChange == DirectionChange.None) {
                transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
            } else {
                float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
                transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            }
            return true;
        }

        void PrepareNextState() {
            tileFrom = tileTo;
            tileTo = tileFrom.NextTileOnPath;
            positionFrom = positionTo;
            if (tileTo == null) {
                PrepareOutro();
                return;
            }
            positionTo = tileFrom.ExitPoint;
            directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
            direction = tileFrom.PathDirection;
            directionAngleFrom = directionAngleTo;
            switch (directionChange) {
                case DirectionChange.None: PrepareForward(); break;
                case DirectionChange.TurnRight: PreparenTurnRight(); break;
                case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
                default: PreparenTurnAround(); break;
            }
        }

        void PrepareForward() {
            transform.localRotation = direction.GetRotation();
            directionAngleTo = direction.GetAngle();
            model.localPosition = new Vector3(pathOffset, 0f);
            progressFactor = speed;
        }

        void PreparenTurnRight() {
            directionAngleTo = directionAngleFrom + 90f;
            model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
            transform.localPosition = positionFrom + direction.GetHalfVector();

            progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
        }

        void PrepareTurnLeft() {
            directionAngleTo = directionAngleFrom - 90f;
            model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
            transform.localPosition = positionFrom + direction.GetHalfVector();
            progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
        }

        void PreparenTurnAround() {
            directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
            model.localPosition = new Vector3(pathOffset, 0f);
            transform.localPosition = positionFrom;
            progressFactor =
                speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
        }

        void PrepareIntro() {
            positionFrom = tileFrom.transform.localPosition;
            transform.position = positionFrom;
            positionTo = tileFrom.ExitPoint;

            direction = tileFrom.PathDirection;
            directionChange = DirectionChange.None;
            directionAngleFrom = directionAngleTo = direction.GetAngle();

            model.localPosition = new Vector3(pathOffset, 0f);
            transform.localRotation = direction.GetRotation();

            progressFactor = 2f * speed;
        }


        void PrepareOutro() {
            positionTo = tileFrom.transform.localPosition;
            directionChange = DirectionChange.None;
            directionAngleTo = direction.GetAngle();

            model.localPosition = new Vector3(pathOffset, 0f);
            transform.localRotation = direction.GetRotation();

            progressFactor = 2f * speed;
        }


        public void ApplyDamage(float damage) {
            Debug.Assert(damage >= 0, "Negtive Damage applied!", this);
            Health -= damage;

        }


        public override void Recycle() {
            animator.Stop();
            OriginFactory.Reclaim(this);
        }




    }
}