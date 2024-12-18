using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MirrorGrapPose : MonoBehaviour
{
    public HandData HandUsedToMirror;
    public HandData HandToMirror;

#if UNITY_EDITOR
    [MenuItem("Tools/Mirror Selected Grap Pose")]
    public static void MakeMirrorPose()
    {
        var handPose = Selection.activeGameObject.GetComponent<MirrorGrapPose>();
        handPose.MirrorPose(handPose.HandToMirror, handPose.HandUsedToMirror);
    }   

#endif

    public void MirrorPose(HandData poseToMirror, HandData poseUsedToMirror)
    {
        Vector3 mirroredPosition = poseUsedToMirror.root.localPosition;
        mirroredPosition.x *= -1;

        Quaternion mirroredQuatarnion = poseUsedToMirror.root.localRotation;
        mirroredQuatarnion.y *= -1;
        mirroredQuatarnion.z *= -1;

        poseToMirror.root.localPosition = mirroredPosition;
        poseToMirror.root.localRotation = mirroredQuatarnion;

        for (int i = 0; i < poseUsedToMirror.fingerBones.Length; i++)
        {
            poseToMirror.fingerBones[i].localRotation = poseUsedToMirror.fingerBones[i].localRotation;
        }
    }
}
