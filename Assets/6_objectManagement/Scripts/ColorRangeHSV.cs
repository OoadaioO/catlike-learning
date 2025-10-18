using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace obj.mamagement {
    [System.Serializable]
    public struct ColorRangeHSV {
        
        [FloatRangeSlider(0,1)]
        public FloatRange hue, saturation, value;

        public Color RandomInRange {
            get {
                return Random.ColorHSV(
                    hue.min, hue.max,
                    saturation.min, saturation.max,
                    value.min, value.max
                );
            }
        }
    }
}
