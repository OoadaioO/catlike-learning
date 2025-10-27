using UnityEngine;

namespace obj.mamagement {
    public sealed class RotationShapeBehavior : ShapeBehaviour {
        public Vector3 AngularVelocity { get; set; }

        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Rotation;

        public override void GameUpdate(Shape shape) {
            shape.transform.Rotate(AngularVelocity * Time.deltaTime);
        }

        public override void Load(GameDataReader reader) {
            AngularVelocity = reader.ReadVector3();
        }

        public override void Save(GameDataWriter writer) {
            writer.Write(AngularVelocity);
        }

        public override void Recycle() {
            ShapeBehaviorPool<RotationShapeBehavior>.Reclaim(this);
        }
    }
}
