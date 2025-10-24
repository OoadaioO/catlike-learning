using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace obj.mamagement {
    [CreateAssetMenu(menuName = "Object-Manager/Shape Factory", fileName = "Shape Factory")]
    public class ShapeFactory : ScriptableObject {

#if UNITY_EDITOR
        static bool reload;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnReload() {
            reload = true;
        }
#endif

        [SerializeField]
        Shape[] prefabs;

        [SerializeField]
        Material[] materials;

        [SerializeField]
        bool recycle;

        List<Shape>[] pools;

        Scene poolScene;



        public Shape Get(int shapeId = 0, int materialId = 0) {
            Shape instance;

            if (recycle) {
#if UNITY_EDITOR
                if (reload || pools == null) {
                    reload = false;
                    CreatePools();
                }
#endif
                if (pools == null) {
                    CreatePools();
                }

                List<Shape> pool = pools[shapeId];
                int lastIndex = pool.Count - 1;
                if (lastIndex >= 0) {
                    instance = pool[lastIndex];
                    instance.gameObject.SetActive(true);
                    pool.RemoveAt(lastIndex);
                } else {
                    instance = Instantiate(prefabs[shapeId]);
                    instance.ShapeId = shapeId;
                    instance.OriginalFactory = this;
                    SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
                }

            } else {
                instance = Instantiate(prefabs[shapeId]);
                instance.ShapeId = shapeId;
                instance.OriginalFactory = this;
                SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
            }

            instance.SetMaterial(materials[materialId], materialId);

            return instance;
        }

        public Shape GetRandom() {
            return Get(
                Random.Range(0, prefabs.Length),
                Random.Range(0, materials.Length)
            );
        }

        public void Reclaim(Shape shapeToRecycle) {
            if(shapeToRecycle.OriginalFactory != this){
                Debug.LogError("Tried to reclaim shape with wrong factory");
                return;
            }
            if (recycle) {
#if UNITY_EDITOR
                if (reload || pools == null) {
                    reload = false;
                    CreatePools();
                }
#endif
                if (pools == null) {
                    CreatePools();
                }
                pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
                shapeToRecycle.gameObject.SetActive(false);

            } else {
                Destroy(shapeToRecycle.gameObject);
            }
        }


        void CreatePools() {

            pools = new List<Shape>[prefabs.Length];
            for (int i = 0; i < pools.Length; i++) {
                pools[i] = new List<Shape>();
            }

            if (Application.isEditor) {
                poolScene = SceneManager.GetSceneByName(name);
                if (poolScene.isLoaded) {
                    GameObject[] rootObjects = poolScene.GetRootGameObjects();
                    for (int i = 0; i < rootObjects.Length; i++) {
                        Shape pooledShape = rootObjects[i].GetComponent<Shape>();
                        if (!pooledShape.gameObject.activeSelf) {
                            pools[pooledShape.ShapeId].Add(pooledShape);
                        }
                    }
                    return;
                }
            }

            poolScene = SceneManager.CreateScene(name);
        }

    }
}