using UnityEngine;

namespace tower.defense {
    public class GameBehavior : MonoBehaviour {
        public virtual bool GameUpdate() => true;
    }
}