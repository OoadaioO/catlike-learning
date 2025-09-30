using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PrefabInstancePool<T> where T : MonoBehaviour {

    [NonSerialized]
    private Stack<T> pool;

    public T GetInstance(T prefab) {
        if (pool == null) {
            pool = new Stack<T>();
        }
#if UNITY_EDITOR
        else if (pool.TryPeek(out T i) && !i) {
            pool.Clear();
        }
#endif


        if (pool.TryPop(out T instance)) {
            instance.gameObject.SetActive(true);
        } else {
            instance = GameObject.Instantiate(prefab);
        }
        return instance;
    }

    public void Recycle(T instance) {
#if UNITY_EDITOR
        if (pool == null) {
            GameObject.Destroy(instance.gameObject);
            return;
        }
#endif
        pool.Push(instance);
        instance.gameObject.SetActive(false);
    }
}
