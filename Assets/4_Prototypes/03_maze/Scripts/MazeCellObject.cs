using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCellObject : MonoBehaviour {

#if UNITY_EDITOR
    [NonSerialized]
    static List<Stack<MazeCellObject>> pools;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ClearPools() {
        if (pools == null) {
            pools = new List<Stack<MazeCellObject>>();
        } else {
            for (int i = 0; i < pools.Count; i++) {
                pools[i].Clear();
            }
        }
    }
#endif


    [System.NonSerialized]
    private Stack<MazeCellObject> pool;

    public MazeCellObject GetInstance() {

        if (pool == null) {
            pool = new Stack<MazeCellObject>();
        }
#if UNITY_EDITOR
        pools.Add(pool);
#endif
        if (pool.TryPop(out MazeCellObject instance)) {
            instance.gameObject.SetActive(true);
        } else {
            instance = Instantiate(this);
            instance.pool = pool;
        }
        return instance;
    }

    public void Recycle() {
        pool.Push(this);
        gameObject.SetActive(false);
    }
}
