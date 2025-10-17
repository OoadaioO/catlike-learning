


using UnityEngine;

namespace obj.mamagement {

    public class Shape : PersistableObject {

        static int colorPropertyId = Shader.PropertyToID("_BaseColor");
        static MaterialPropertyBlock sharedPropertyBlock;

        public int ShapeId {
            get => shapeId;
            set {
                if (shapeId == int.MinValue && value != int.MinValue) {
                    shapeId = value;
                } else {
                    Debug.LogError("Not allow to change shapeId");
                }

            }
        }
        int shapeId = int.MinValue;

        public int MaterialId { get; private set; }

        Color color;

        MeshRenderer meshRenderer;

        private void Awake() {
            meshRenderer = GetComponent<MeshRenderer>();
        }


        public void SetMaterial(Material material, int materialId) {
            meshRenderer.material = material;
            MaterialId = materialId;
        }

        public void SetColor(Color color) {
            this.color = color;
            if (sharedPropertyBlock == null) {
                sharedPropertyBlock = new MaterialPropertyBlock();
            }
            sharedPropertyBlock.SetColor(colorPropertyId, color);
            meshRenderer.SetPropertyBlock(sharedPropertyBlock);
        }


        public override void Save(GameDataWriter writer) {
            base.Save(writer);
            writer.Write(color);
        }

        public override void Load(GameDataReader reader) {
            base.Load(reader);
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }

    }
}