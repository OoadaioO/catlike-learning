using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace obj.mamagement {
    public enum ShapeBehaviorType {
        Movement,
        Rotation,
        Oscillation
    }

    public static class ShapeBehaviorTypeMethods {
        public static ShapeBehaviour GetInstance(this ShapeBehaviorType type) {
            switch (type) {
                case ShapeBehaviorType.Movement:
                    return ShapeBehaviorPool<MovementShapeBehaviour>.Get();
                case ShapeBehaviorType.Rotation:
                    return ShapeBehaviorPool<RotationShapeBehavior>.Get();
                case ShapeBehaviorType.Oscillation:
                    return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
            }
            Debug.Log("Forget to support type:" + type);
            return null;
        }
    }
}