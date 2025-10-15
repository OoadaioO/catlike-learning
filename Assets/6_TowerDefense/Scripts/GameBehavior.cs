using UnityEngine;

namespace tower.defense {
    public  abstract class GameBehavior : MonoBehaviour {
        public virtual bool GameUpdate() => true;

        public abstract void Recycle();
    }
}