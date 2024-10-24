using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ApplyRandomRotationAndScaleFull : Editor
{
    [MenuItem("GameObject/Apply Random Scale and Rotation (Full)", false, 10001)]
    public static void ApplyScaleAndRotation(MenuCommand menuCommand) {
        GameObject obj = (GameObject)menuCommand.context;
        var scale = UnityEngine.Random.Range(0.7f, 1.5f);
        obj.transform.localScale = new Vector3(scale, scale, scale);
        obj.transform.Rotate(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
    }
}
