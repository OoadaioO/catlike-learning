using UnityEngine;

namespace obj.mamagement {
    public class GameLevel : MonoBehaviour {

        [SerializeField]
        SpawnZone spawnZone;

        private void Awake() {
            Game.Instance.SpawnZoneOfLevel = spawnZone;
        }

    }
}