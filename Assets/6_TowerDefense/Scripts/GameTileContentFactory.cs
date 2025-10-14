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
    [SerializeField] Tower towerPrefab;


    public void Reclaim(GameTileContent content) {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }


    GameTileContent Get(GameTileContent prefab) {
        GameTileContent instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }



    public GameTileContent Get(GameTileContentType type) {
        switch (type) {
            case GameTileContentType.Destination: return Get(destinationPrefab);
            case GameTileContentType.Empty: return Get(emptyPrefab);
            case GameTileContentType.Wall: return Get(wallPrefab);
            case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
            case GameTileContentType.Tower: return Get(towerPrefab);
        }
        Debug.Assert(false, "Unsupport type:" + type);
        return null;
    }

}
