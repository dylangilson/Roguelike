using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// to rename any of T objects, change filename and not the name within unity inspector
public class ScriptableObjectDataBase<T> : MonoBehaviour where T : ScriptableObject {
    static Dictionary<string, T> objects;

    public static void Init() {
        objects = new Dictionary<string, T>();

        var objectArray = Resources.LoadAll<T>("");

        foreach (var obj in objectArray) {
            if (objects.ContainsKey(obj.name)) {
                Debug.LogError($"There are two files with the name {obj.name}");

                continue;
            }

            objects[obj.name] = obj;
        }
    }

    public static T GetObjectByName(string name) {
        if (!objects.ContainsKey(name)) {
            Debug.LogError($"Object with the name {name} not found in DataBase");

            return null;
        }

        return objects[name];
    }
}
