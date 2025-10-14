using UnityEngine;


namespace tower.defense {
    public class Explosion : WarEntity {

        static int colorPropertyID = Shader.PropertyToID("_BaseColor");
        static MaterialPropertyBlock propertyBlock;


        [SerializeField, Range(0f, 1f)]
        float duration = .5f;

        [SerializeField]
        AnimationCurve opacityCurve;

        [SerializeField]
        AnimationCurve scaleCurve;

        float age;
        float scale;
        MeshRenderer meshRenderer;

        Color color;

        private void Awake() {
            meshRenderer = GetComponent<MeshRenderer>();
            color = meshRenderer.material.GetColor(colorPropertyID);
            Debug.Assert(meshRenderer != null, "Explosion without renderer");
        }

        public void Initialize(Vector3 position, float blastRadius, float damage = 0f) {

            if (damage > 0) {
                TargetPoint.FillBuffer(position, blastRadius);
                for (int i = 0; i < TargetPoint.BufferCount; i++) {
                    TargetPoint.GetBufferd(i).Enemy.ApplyDamage(damage);
                }
            }
            transform.localPosition = position;

            scale = 2f * blastRadius;
        }


        public override bool GameUpdate() {

            age += Time.deltaTime;
            if (age >= duration) {
                OriginFactory.Reclaim(this);
                return false;
            }

            if (propertyBlock == null) {
                propertyBlock = new MaterialPropertyBlock();
            }
            float t = age / duration;
            Color c = color;
            c.a = opacityCurve.Evaluate(t);
            propertyBlock.SetColor(colorPropertyID, c);
            meshRenderer.SetPropertyBlock(propertyBlock);
            transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));

            return true;
        }

    }
}