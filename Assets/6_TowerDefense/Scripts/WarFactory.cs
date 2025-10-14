using UnityEngine;

namespace tower.defense {
    [CreateAssetMenu(menuName = "tower/War Factory", fileName = "WarFactory")]
    public class WarFactory : GameObjectFactory {


        [SerializeField]
        Explosion explosionPrefab;


        [SerializeField]
        Shell shellPrefab;

        public Explosion Explosion => Get(explosionPrefab);
        
        public Shell Shell => Get(shellPrefab);

        T Get<T>(T prefab) where T : WarEntity {
            T instance = CreateGameObjectInstance(prefab);
            instance.OriginFactory = this;
            return instance;
        }

        public void Reclaim(WarEntity entity) {
            Debug.Assert(entity.OriginFactory == this, "Wrong factory reclaimed!    ");
            Destroy(entity.gameObject);
        }
    }
}