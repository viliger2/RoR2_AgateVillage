using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ApplyRandomRotationAndScale : Editor
{
    [MenuItem("GameObject/Apply Random Scale and Rotation", false, 10000)]
    public static void ApplyScaleAndRotation(MenuCommand menuCommand) {
        GameObject obj = (GameObject)menuCommand.context;
        var scale = UnityEngine.Random.Range(0.85f, 1.15f);
        obj.transform.localScale = new Vector3(scale, scale, scale);
        obj.transform.Rotate(0f, UnityEngine.Random.Range(0, 360), 0f);
    }
}
