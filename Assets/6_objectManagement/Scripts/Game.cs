using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace obj.mamagement {
    public class Game : PersistableObject {

        const int saveVersion = 3;


        public float CreationSpeed { get; set; }
        public float DestructionSpeed { get; set; }


        public PersistentStorage storage;
        public ShapeFactory shapeFactory;


        public KeyCode createKey = KeyCode.C;
        public KeyCode newGameKey = KeyCode.N;
        public KeyCode saveKey = KeyCode.S;
        public KeyCode loadKey = KeyCode.L;
        public KeyCode destroyKey = KeyCode.X;

        public int levelCount;


        List<Shape> shapes;

        float creationProgress, destructProgress;

        int loadedLevelBuildIndex;


        private void Start() {
            shapes = new List<Shape>();

            if (Application.isEditor) {

                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    Scene loadedLevel = SceneManager.GetSceneAt(i);
                    if (loadedLevel.name.Contains("object-level")) {
                        SceneManager.SetActiveScene(loadedLevel);
                        loadedLevelBuildIndex = loadedLevel.buildIndex;
                        return;
                    }
                }

            }

            StartCoroutine(LoadLevel(1));
        }

        private void Update() {
            if (Input.GetKeyDown(createKey)) {
                CreateShape();
            } else if (Input.GetKeyDown(newGameKey)) {
                BeginNewGame();
            } else if (Input.GetKeyDown(saveKey)) {
                storage.Save(this, saveVersion);
            } else if (Input.GetKeyDown(loadKey)) {
                BeginNewGame();
                storage.Load(this);
            } else if (Input.GetKeyDown(destroyKey)) {
                DestroyShape();
            } else {
                for (int i = 1; i <= levelCount; i++) {
                    if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
                        BeginNewGame();
                        StartCoroutine(LoadLevel(i));
                    }
                }
            }

            creationProgress += Time.deltaTime * CreationSpeed;

            while (creationProgress >= 1f) {
                creationProgress -= 1f;
                CreateShape();
            }

            destructProgress += Time.deltaTime * DestructionSpeed;

            while (destructProgress >= 1f) {
                destructProgress -= 1f;
                DestroyShape();
            }
        }

        IEnumerator LoadLevel(int levelBuildIndex) {
            enabled = false;

            if (loadedLevelBuildIndex > 0) {
                yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
            }

            yield return SceneManager.LoadSceneAsync(
                levelBuildIndex, LoadSceneMode.Additive
            );
            SceneManager.SetActiveScene(
                SceneManager.GetSceneByBuildIndex(levelBuildIndex)
            );
            loadedLevelBuildIndex = levelBuildIndex;
            enabled = true;
        }

        void CreateShape() {
            Shape instance = shapeFactory.GetRandom();
            Transform t = instance.transform;
            t.position = Random.insideUnitSphere * 5f;
            t.rotation = Random.rotation;
            t.localScale = Vector3.one * Random.Range(0.1f, 1f);
            instance.SetColor(Random.ColorHSV(
                    hueMin: 0f,
                    hueMax: 1f,
                    saturationMin: 0.5f, saturationMax: 1f,
                    valueMin: 0.25f, valueMax: 1f,
                    alphaMin: 1f, alphaMax: 1f)
            );

            shapes.Add(instance);
        }

        void BeginNewGame() {
            for (int i = 0; i < shapes.Count; i++) {
                shapeFactory.Reclaim(shapes[i]);
            }
            shapes.Clear();
        }

        void DestroyShape() {
            if (shapes.Count > 0) {
                int index = Random.Range(0, shapes.Count);
                shapeFactory.Reclaim(shapes[index]);
                int lastIndex = shapes.Count - 1;
                shapes[index] = shapes[lastIndex];
                shapes.RemoveAt(lastIndex);
            }
        }



        public override void Save(GameDataWriter writer) {
            writer.Write(shapes.Count);
            writer.Write(loadedLevelBuildIndex);
            for (int i = 0; i < shapes.Count; i++) {
                writer.Write(shapes[i].ShapeId);
                writer.Write(shapes[i].MaterialId);
                shapes[i].Save(writer);
            }
        }

        public override void Load(GameDataReader reader) {
            int version = reader.Version;
            if (version > saveVersion) {
                Debug.LogError("Unsupported fureture save version " + version);
                return;
            }
            int count = version <= 0 ? -version : reader.ReadInt();
            StartCoroutine(LoadLevel(version < 3 ? 1 : reader.ReadInt()));
            for (int i = 0; i < count; i++) {
                int shapeId = version > 0 ? reader.ReadInt() : 0;
                int materialId = version > 0 ? reader.ReadInt() : 0;
                Shape shape = shapeFactory.Get(shapeId, materialId);
                shape.Load(reader);
                shapes.Add(shape);
            }
        }

    }
}
