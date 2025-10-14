using System.Collections;
using System.Collections.Generic;
using tower.defense;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "tower/Game Tile Content Factory", fileName = "GameFileContentFactory")]
public class GameTileContentFactory : GameObjectFactory {

    [SerializeField] GameTileContent destinationPrefab;
    [SerializeField] GameTileContent emptyPrefab;
    [SerializeField] GameTileContent wallPrefab;
    [SerializeField] GameTileContent spawnPointPrefab;

    [SerializeField]
    Tower[] towerPrefabs = default;

    public void Reclaim(GameTileContent content) {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }


    T Get<T>(T prefab) where T : GameTileContent {
        T instance = CreateGameObjectInstance<T>(prefab);
        instance.OriginFactory = this;
        return instance;
    }


    public GameTileContent Get(GameTileContentType type) {
        switch (type) {
            case GameTileContentType.Destination: return Get(destinationPrefab);
            case GameTileContentType.Empty: return Get(emptyPrefab);
            case GameTileContentType.Wall: return Get(wallPrefab);
            case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
        }
        Debug.Assert(false, "Unsupport non-tower type:" + type);
        return null;
    }

    public Tower Get(TowerType type) {
        Debug.Assert((int)type < towerPrefabs.Length, "Unsupported tower type!");
        Tower prefab = towerPrefabs[(int)type];
        Debug.Assert(prefab.TowerType == type, "Tower prefab at wrong index!");
        return Get(prefab);
    }



}
