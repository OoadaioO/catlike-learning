using UnityEngine;

namespace obj.mamagement {
    public class GameLevel : MonoBehaviour {

        public static GameLevel Current { get; private set; }

        [SerializeField]
        SpawnZone spawnZone;

        private void Awake() {
            Game.Instance.SpawnZoneOfLevel = spawnZone;
        }

        private void OnEnable() {
            Current = this;
        }
    }
}