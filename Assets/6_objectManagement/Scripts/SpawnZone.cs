using UnityEngine;

namespace obj.mamagement {
    public abstract class SpawnZone : PersistableObject {


        [System.Serializable]
        public struct SpawnConfiguration {
            public enum MovementDirection {
                Forward,
                Upward,
                Outward,
                Random,
            }

            public ShapeFactory[] factories;

            public MovementDirection movementDirection;

            public FloatRange spawnSpeed;

            public FloatRange angularSpeed;

            public FloatRange scale;

            public ColorRangeHSV color;

            public bool uniformColor;

            public MovementDirection oscillationDirection;
            public FloatRange oscillationAmplitude;
            public FloatRange oscillationFreqency;

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
            float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
            if (angularSpeed != 0f) {
                RotationShapeBehavior rotation = shape.AddBehavior<RotationShapeBehavior>();
                rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
            }

            float speed = spawnConfig.spawnSpeed.RandomValueInRange;
            if (speed != 0f) {
                MovementShapeBehaviour movement = shape.AddBehavior<MovementShapeBehaviour>();
                movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
            }
            SetupOscillation(shape);
            return shape;
        }

        Vector3 GetDirectionVector(
            SpawnConfiguration.MovementDirection direction, Transform t
        ) {

            switch (direction) {
                case SpawnConfiguration.MovementDirection.Upward:
                    return transform.up;
                case SpawnConfiguration.MovementDirection.Outward:
                    return (t.localPosition - transform.position).normalized;
                case SpawnConfiguration.MovementDirection.Random:
                    return Random.onUnitSphere;
                default:
                    return transform.forward;
            }

        }

        void SetupOscillation(Shape shape) {
            float amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
            float frequency = spawnConfig.oscillationFreqency.RandomValueInRange;
            if (amplitude == 0f || frequency == 0f) {
                return;
            }
            var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
            oscillation.Offset = GetDirectionVector(spawnConfig.oscillationDirection, shape.transform) * amplitude;
            oscillation.Frequency = frequency;
        }


    }
}