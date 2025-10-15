using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace tower.defense {
    [CreateAssetMenu(menuName = "tower/Enemy Wave", fileName = "EnemyWave")]
    public class EnemyWave : ScriptableObject {

        [SerializeField]
        EnemySpawnSequence[] spawnSequences = {
            new EnemySpawnSequence()
        };

        public WaveState Begin() => new WaveState(this);


        [System.Serializable]
        public struct WaveState {
            EnemyWave wave;
            int index;
            EnemySpawnSequence.State sequence;

            public WaveState(EnemyWave wave) {
                this.wave = wave;
                this.index = 0;
                Debug.Assert(wave.spawnSequences.Length > 0, "Empty Wave!");
                sequence = wave.spawnSequences[0].Begin();
            }


            public float Progress(float deltaTime) {
                deltaTime = sequence.Progress(deltaTime);
                while (deltaTime >= 0f) {
                    if (++index >= wave.spawnSequences.Length) {
                        return deltaTime;
                    }
                    sequence = wave.spawnSequences[index].Begin();
                    deltaTime = sequence.Progress(deltaTime);
                }
                return -1f;
            }
        }
    }
}