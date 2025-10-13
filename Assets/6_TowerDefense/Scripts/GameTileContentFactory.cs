using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "tower/Game Tile Content Factory", fileName = "GameFileContentFactory")]
public class GameTileContentFactory : ScriptableObject {

    [SerializeField] GameTileContent destinationPrefab;
    [SerializeField] GameTileContent emptyPrefab;
    [SerializeField] GameTileContent wallPrefab;

    Scene contentScene;

    public void Reclaim(GameTileContent content) {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }


    GameTileContent Get(GameTileContent prefab) {
        GameTileContent instance = Instantiate(prefab);
        instance.OriginFactory = this;
        MoveToFactoryToScene(instance.gameObject);
        return instance;
    }

    void MoveToFactoryToScene(GameObject o) {

        if (!contentScene.isLoaded) {
            if (Application.isEditor) {
                contentScene = SceneManager.GetSceneByName(name);
                if (!contentScene.isLoaded) {
                    contentScene = SceneManager.CreateScene(name);
                }
            } else {
                contentScene = SceneManager.CreateScene(name);
            }
        }
        SceneManager.MoveGameObjectToScene(o, contentScene);
    }


    public GameTileContent Get(GameTileContentType type) {
        switch (type) {
            case GameTileContentType.Destination: return Get(destinationPrefab);
            case GameTileContentType.Empty: return Get(emptyPrefab);
            case GameTileContentType.Wall: return Get(wallPrefab);
        }
        Debug.Assert(false, "Unsupport type:" + type);
        return null;
    }

}
