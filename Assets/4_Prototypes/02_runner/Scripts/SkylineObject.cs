using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkylineObject : MonoBehaviour {
    [SerializeField] private float extents;
    [SerializeField] private FloatRange gapY;

    public float MaxX => transform.localPosition.x + extents;

    [System.NonSerialized]
    private Stack<SkylineObject> pool;

    public SkylineObject Next {
        get; set;
    }

    public FloatRange GapY => gapY.Shift(transform.localPosition.y);

#if UNITY_EDITOR
    static List<Stack<SkylineObject>> pools;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ClearPools() {
        if (pools == null) {
            pools = new List<Stack<SkylineObject>>();
        } else {
            for (int i = 0; i < pools.Count; i++) {
                pools[i].Clear();
            }
        }
    }

#endif


    public SkylineObject GetInstance() {
        if (pool == null) {
            pool = new Stack<SkylineObject>();
#if UNITY_EDITOR
            pools.Add(pool);
#endif
        }
        if (pool.TryPop(out SkylineObject instance)) {
            instance.gameObject.SetActive(true);
        } else {
            instance = Instantiate(this);
            instance.pool = pool;
        }
        return instance;
    }

    public Vector3 PlaceAfter(Vector3 position) {
        position.x += extents;
        transform.localPosition = position;
        position.x += extents;
        return position;
    }

    public SkylineObject Recycle() {
        pool.Push(this);
        gameObject.SetActive(false);
        SkylineObject n = Next;
        Next = null;
        return n;
    }

    public void FillGap(Vector3 position, float gap) {
        extents = gap * 0.5f;
        position.x += extents;
        transform.localPosition = position;
    }

    public virtual void Check(Runner runner) {

    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {

        Vector3 scale = transform.localScale;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(0f, -scale.y * 0.5f, 0f), new Vector3(extents * 2, scale.y, 0.1f));
    }
#endif

}
