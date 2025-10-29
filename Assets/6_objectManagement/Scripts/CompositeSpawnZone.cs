using UnityEngine;

namespace obj.mamagement {
    public class CompositeSpawnZone : SpawnZone {

        [SerializeField]
        SpawnZone[] spawnZones;

        [SerializeField]
        bool sequential;

        [SerializeField]
        bool overrideConfig;

        int nextSeqential;

        public override Vector3 SpawnPoint {
            get {
                int index;
                if (sequential) {
                    index = nextSeqential++;
                    if (nextSeqential >= spawnZones.Length) {
                        nextSeqential = 0;
                    }
                } else {
                    index = Random.Range(0, spawnZones.Length);
                }
                return spawnZones[index].SpawnPoint;
            }
        }

        public override void SpawnShapes() {
            if(overrideConfig){
                 base.SpawnShapes();
            }else {
                int index;
                if (sequential) {
                    index = nextSeqential++;
                    if (nextSeqential >= spawnZones.Length) {
                        nextSeqential = 0;
                    }
                } else {
                    index = Random.Range(0, spawnZones.Length);
                }
                spawnZones[index].SpawnShapes();
            }
        }

        public override void Save(GameDataWriter writer) {
            base.Save(writer);
            writer.Write(nextSeqential);
        }

        public override void Load(GameDataReader reader) {
            if (reader.Version >= 8) {
                base.Load(reader);
            }
            nextSeqential = reader.ReadInt();
        }
    }
}