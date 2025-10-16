using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tower.defense {
    [CreateAssetMenu(menuName = "tower/Enemy Animation Config", fileName = "EnemyAnimationConfig")]
    public class EnemyAnimationConfig : ScriptableObject {

        [SerializeField]
        float movingAnimationSpeed = 1f;

        [SerializeField]
        AnimationClip move = default,intro = default,outro = default,dying = default;

        [SerializeField]
        AnimationClip appear, disappear;
        public AnimationClip Move => move;
        public AnimationClip Intro => intro;
        public AnimationClip Outro => outro;
        public AnimationClip Dying => dying;
        public AnimationClip Appear => appear;
        public AnimationClip Disappear => disappear;

        public float MovingAnimationSpeed => movingAnimationSpeed;



    }
}