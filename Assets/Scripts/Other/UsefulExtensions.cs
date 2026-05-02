using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class GameObjectExtensions {
    /// <summary>
    /// Destroy the object. Uses destroy immediate if in editor.
    /// </summary>
    public static void SmartDestroy(this GameObject gameObject) {
        if (Application.isPlaying) {
            GameObject.Destroy(gameObject);
        }
        else {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}

public static class TransformExtensions {
    /// <summary>
    /// Correctly destroys all children of the transform.
    /// </summary>
    public static void DestroyAllChildren(this Transform transform) {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            transform.GetChild(i).gameObject.SmartDestroy();
        }
    }
}

public static class EnumerableExtensions {
    public static string ToDelimitedString<T>(this IEnumerable<T> enumerable, string delim = ", ") {
        StringBuilder sb = new StringBuilder();
        foreach (var a in enumerable) {
            sb.Append(a);
            sb.Append(delim);
        }

        return sb.ToString();
    }
}

public static class ListExtensions {
    public static T GetRandom<T>(this List<T> list) {
        if (list == null || list.Count == 0)
            return default;

        return list[Random.Range(0, list.Count)];
    }

    public static void Shuffle<T>(this IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}