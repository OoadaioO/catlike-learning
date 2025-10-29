using System.Collections;
using System.Collections.Generic;
using obj.mamagement;
using UnityEditor;
using UnityEngine;

static class RegisterLevelObjectMenuItem {

    const string menuItem = "GameObject/Register Level Object";

    [MenuItem(menuItem, true)]
    static bool ValidateRegisterLevelObject() {
        if (Selection.objects.Length == 0) {
            return false;
        }
        foreach (Object o in Selection.objects) {
            if (o is not GameObject) {
                return false;
            }
        }
        return true;
    }


    [MenuItem(menuItem)]
    static void RegisterLevelObject() {
        foreach (Object o in Selection.objects) {
            Register(o as GameObject);
        }
    }


    static void Register(GameObject o) {

        // 仅当选择的是Prefab资源（而非场景中的实例）时给出警告
        if (PrefabUtility.IsPartOfPrefabAsset(o)) {
            Debug.LogWarning(o.name + " is a prefab asset.", o);
            return;
        }

        if (!o.TryGetComponent<GameLevelObject>(out var levelObject)) {
            Debug.LogWarning(o.name + " isn't a game level object.", o);
            return;
        }

        foreach (GameObject rootObject in o.scene.GetRootGameObjects()) {
            if (rootObject.TryGetComponent<GameLevel>(out var gameLevel)) {
                if (gameLevel.HasLevelObject(levelObject)) {
                    Debug.LogWarning(o.name + " is already registered.", o);
                    return;
                }
                Undo.RecordObject(gameLevel, "Register Level Object.");
                gameLevel.RegisterLevelObject(levelObject);
                Debug.Log(
                    o.name + " registered to game level " +
                    gameLevel.name + " in scene " + o.scene.name + ".", o
                );
                return;
            }
        }
        Debug.LogWarning(o.name + " isn't part of a game level.", o);
    }

}
