using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tower.defense {
    public class FloatRangeSliderAttribute : PropertyAttribute {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public FloatRangeSliderAttribute(float min,float max){
            this.Min = min;
            Max = max < min ? min : max;
        }

    }
}