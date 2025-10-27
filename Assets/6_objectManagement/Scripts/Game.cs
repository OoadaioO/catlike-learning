using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace obj.mamagement {
    public class Game : PersistableObject {

        const int saveVersion = 7;

        public static Game Instance { get; private set; }

        public float CreationSpeed { get; set; }
        public float DestructionSpeed { get; set; }


        [SerializeField]
        PersistentStorage storage;

        [SerializeField] ShapeFactory[] shapeFactories;


        [SerializeField]
        KeyCode createKey = KeyCode.C;

        [SerializeField]
        KeyCode newGameKey = KeyCode.N;

        [SerializeField]
        KeyCode saveKey = KeyCode.S;

        [SerializeField]
        KeyCode loadKey = KeyCode.L;

        [SerializeField]
        KeyCode destroyKey = KeyCode.X;

        [SerializeField]
        int levelCount;

        [SerializeField]
        bool reseedOnLoad;

        [SerializeField] Slider creationSpeedSlider;
        [SerializeField] Slider destructionSpeedSlider;





        List<Shape> shapes;

        float creationProgress, destructProgress;

        int loadedLevelBuildIndex;

        Random.State mainRandomState;


        private void Start() {

            mainRandomState = Random.state;
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
            BeginNewGame();
            StartCoroutine(LoadLevel(1));
        }

        private void OnEnable() {
            Instance = this;
            if (shapeFactories[0].FactoryId != 0) {
                for (int i = 0; i < shapeFactories.Length; i++) {
                    shapeFactories[i].FactoryId = i;
                }
            }
        }


        private void Update() {
            if (Input.GetKeyDown(createKey)) {
                GameLevel.Current.SpawnShapes();
            } else if (Input.GetKeyDown(newGameKey)) {
                BeginNewGame();
                StartCoroutine(LoadLevel(loadedLevelBuildIndex));
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


        }

        private void FixedUpdate() {
            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].GameUpdate();
            }


            creationProgress += Time.deltaTime * CreationSpeed;

            while (creationProgress >= 1f) {
                creationProgress -= 1f;
                GameLevel.Current.SpawnShapes();
            }

            destructProgress += Time.deltaTime * DestructionSpeed;

            while (destructProgress >= 1f) {
                destructProgress -= 1f;
                DestroyShape();
            }

            int limit = GameLevel.Current.PopulationLimit;
            if (limit > 0) {
                while (shapes.Count > limit) {
                    DestroyShape();
                }
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


        public void AddShape(Shape shape) {
            shape.SaveIndex = shapes.Count;
            shapes.Add(shape);
        }
        public Shape GetShape(int index) {
            return shapes[index];
        }


        void BeginNewGame() {
            Random.state = mainRandomState;
            int seed = Random.Range(0, int.MaxValue);
            mainRandomState = Random.state;
            Random.InitState(seed);

            creationSpeedSlider.value = CreationSpeed = 0f;
            destructionSpeedSlider.value = DestructionSpeed = 0f;


            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].Recycle();
            }
            shapes.Clear();
        }

        void DestroyShape() {
            if (shapes.Count > 0) {
                int index = Random.Range(0, shapes.Count);
                shapes[index].Recycle();
                int lastIndex = shapes.Count - 1;
                shapes[lastIndex].SaveIndex = index;
                shapes[index] = shapes[lastIndex];
                shapes.RemoveAt(lastIndex);
            }
        }



        public override void Save(GameDataWriter writer) {
            writer.Write(shapes.Count);
            writer.Write(Random.state);
            writer.Write(CreationSpeed);
            writer.Write(creationProgress);
            writer.Write(DestructionSpeed);
            writer.Write(destructProgress);
            writer.Write(loadedLevelBuildIndex);
            GameLevel.Current.Save(writer);
            for (int i = 0; i < shapes.Count; i++) {
                writer.Write(shapes[i].OriginFactory.FactoryId);
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

            StartCoroutine(LoadGame(reader));
        }

        IEnumerator LoadGame(GameDataReader reader) {

            int version = reader.Version;
            int count = version <= 0 ? -version : reader.ReadInt();

            if (version >= 4) {
                Random.State state = reader.ReadRandomState();
                if (!reseedOnLoad) {
                    Random.state = state;
                }

                creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
                creationProgress = reader.ReadFloat();
                destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
                destructProgress = reader.ReadFloat();
            }
            yield return LoadLevel(version < 3 ? 1 : reader.ReadInt());

            if (version >= 4) {
                GameLevel.Current.Load(reader);
            }

            for (int i = 0; i < count; i++) {
                int factoryId = version >= 6 ? reader.ReadInt() : 0;
                int shapeId = version > 0 ? reader.ReadInt() : 0;
                int materialId = version > 0 ? reader.ReadInt() : 0;
                Shape shape = shapeFactories[factoryId].Get(shapeId, materialId);
                shape.Load(reader);
            }

            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].ResolveShapeInstances();
            }
        }

    }
}
