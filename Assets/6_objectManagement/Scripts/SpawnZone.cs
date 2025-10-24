using UnityEngine;

namespace obj.mamagement {
    public abstract class SpawnZone : PersistableObject {


        [System.Serializable]
        public struct SpawnConfiguration {
            public enum SpawnMovementDirection {
                Forward,
                Upward,
                Outward,
                Random,
            }

            public ShapeFactory[] factories;

            public SpawnMovementDirection spawnMovementDirection;

            public FloatRange spawnSpeed;

            public FloatRange angularSpeed;

            public FloatRange scale;

            public ColorRangeHSV color;

            public bool uniformColor;
        }

        [SerializeField]
        SpawnConfiguration spawnConfig;


        public abstract Vector3 SpawnPoint { get; }

        public virtual Shape SpawnShape() {
            int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
            Transform t = shape.transform;
            t.localPosition = SpawnPoint;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
            if (spawnConfig.uniformColor) {
                shape.SetColor(spawnConfig.color.RandomInRange);
            } else {
                for (int i = 0; i < shape.ColorCount; i++) {
                    shape.SetColor(spawnConfig.color.RandomInRange, i);
                }
            }
            shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;

            Vector3 direction;
            if (spawnConfig.spawnMovementDirection == SpawnConfiguration.SpawnMovementDirection.Upward) {
                direction = transform.up;
            } else if (spawnConfig.spawnMovementDirection == SpawnConfiguration.SpawnMovementDirection.Outward) {
                direction = (t.localPosition - transform.position).normalized;
            } else if (spawnConfig.spawnMovementDirection == SpawnConfiguration.SpawnMovementDirection.Random) {
                direction = Random.onUnitSphere;
            } else {
                direction = transform.forward;
            }
            shape.Velocity = direction * spawnConfig.spawnSpeed.RandomValueInRange;
            return shape;
        }


    }
}