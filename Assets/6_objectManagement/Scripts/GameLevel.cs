using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace obj.mamagement {
    public partial class GameLevel : PersistableObject {

        public static GameLevel Current { get; private set; }

        public int PopulationLimit {
            get {
                return populationLimit;
            }
        }

        [SerializeField]
        SpawnZone spawnZone;

        [FormerlySerializedAs("persistableObjects")]
        [SerializeField]
        GameLevelObject[] levelObjects;

        [SerializeField]
        int populationLimit;

      
        private void OnEnable() {
            Current = this;
            if (levelObjects == null) {
                levelObjects = new GameLevelObject[0];
            }
        }

        public void SpawnShapes() {
            spawnZone.SpawnShapes();
        }

        public void GameUpdate() {
            for (int i = 0; i < levelObjects.Length; i++) {
                levelObjects[i].GameUpdate();
            }
        }



        public override void Save(GameDataWriter writer) {
            writer.Write(levelObjects.Length);
            for (int i = 0; i < levelObjects.Length; i++) {
                levelObjects[i].Save(writer);
            }
        }
        public override void Load(GameDataReader reader) {
            int saveCount = reader.ReadInt();
            for (int i = 0; i < saveCount; i++) {
                levelObjects[i].Load(reader);
            }
        }
    }
}