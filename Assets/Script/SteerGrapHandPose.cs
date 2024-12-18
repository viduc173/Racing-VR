using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Content.Interaction;
using System.ComponentModel;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// Sử dụng cho việc cầm nắm xoay vô lăng hoặc các thứ tròn tương tự
/// </summary>
public class SteerGrapHandPose : MonoBehaviour
{
    [Header("<i>1) Sử dụng cho việc cầm, nắm, xoay vô lăng hoặc thứ tròn tương tự \n" +
               "2) Có thể sử dụng đa điểm chọn hoặc đơn điểm chọn, nhưng đề xuất dùng đa điểm chọn</i>", order = 0)]
    [Space(5)]

    [Space(10)]
    [Header("Hand Data")]
    public Transform GameojectUsed_RightHand;
    public Transform GameojectUsed_LefttHand;

    public HandData RightHand;
    public HandData LeftHand;
    public HandData Active_RightHand;
    public HandData Active_LeftHand;

    [Header("Hold Point")]
    [Tooltip("Điểm gốc")]
    public Transform rootHoldPoint;
    [Range(5, 90, order = 10)]
    public int AngeDiv = 30;
    [Tooltip("Khoảng cách giữa điểm gốc và điểm nắm")]
    public float distance;
    public struct GrabHandData
    {
        public HandData hand;
        public int ChosenPoint;
        public Transform GameobjectUsed;

        public Vector3 _finalHandPosition;
        public Quaternion _finalHandRotation;

        public Quaternion[] _startingFingerRotation;
        public Quaternion[] _finalFingerRotation;
    }
    private List<GrabHandData> grabHandDatas = new();

    public void OnEnable()
    {
        if (TryGetComponent<XRBaseInteractable>(out var grap))
        {
            grap.selectEntered.AddListener(StartGrap);
            grap.selectExited.AddListener(EndGrap);

            if (grap.selectMode == InteractableSelectMode.Single)
                Debug.LogWarning("You should chose [Select Mode : Multiple] in " + grap + "  to get the best experience");
        }
        else
        {
            Debug.LogError("Can't Find XRBaseInteractable in " + this);
        }
    }

    void StartGrap(SelectEnterEventArgs arg0)
    {
        if (arg0.interactorObject is XRDirectInteractor)
        {
            GrabHandData _grap = new();

            _grap.hand = arg0.interactorObject.transform.GetComponentInChildren<HandData>();
            _grap.hand.animator.enabled = false;

            if (_grap.hand.handType == HandData.HandType.right)
            {
                _grap.GameobjectUsed = GameojectUsed_RightHand.transform;
                SetDataValues(ref _grap, RightHand);
            }
            else
            if (_grap.hand.handType == HandData.HandType.left)
            {
                _grap.GameobjectUsed = GameojectUsed_LefttHand.transform;
                SetDataValues(ref _grap, LeftHand);
            }
            else
            {
                Debug.LogError("Lỗi không phân loại tay tương tác");
                return;
            }


            SetHandData(_grap, _grap._finalFingerRotation);
            FindHoldPoint(ref _grap);

            grabHandDatas.Add(_grap);

            Debug.Log(grabHandDatas.Count);
        }
    }

    void EndGrap(SelectExitEventArgs arg0)
    {
        if (arg0.interactorObject is XRDirectInteractor && grabHandDatas.Count > 0)
        {
            var id = grabHandDatas.FindIndex(
                x => x.hand == arg0.interactorObject.transform.GetComponentInChildren<HandData>());

            grabHandDatas[id].hand.animator.enabled = true;
            SetHandData(grabHandDatas[id], grabHandDatas[id]._startingFingerRotation);
            grabHandDatas[id].hand.root.localPosition = Vector3.zero;
            grabHandDatas[id].hand.root.localRotation =
                Quaternion.Euler(0, 0, grabHandDatas[id].hand.handType == HandData.HandType.left ? 90 : -90);

            grabHandDatas.RemoveAt(id);
        }
    }

    public void FindHoldPoint(ref GrabHandData grapHandData)
    {
        var minDis = Mathf.Infinity;
        for (int i = 0; i <= 360; i += AngeDiv)
        {
            var v3 = rootHoldPoint.position +
                Quaternion.AngleAxis(i, rootHoldPoint.up) * rootHoldPoint.forward * distance;
            if ((v3 - grapHandData.hand.transform.position).magnitude < minDis)
            {
                minDis = (v3 - grapHandData.hand.transform.position).magnitude;
                grapHandData.ChosenPoint = i;
            }
        }

        grapHandData.GameobjectUsed.localRotation = rootHoldPoint.localRotation;
        grapHandData.GameobjectUsed.eulerAngles -= new Vector3(0, grapHandData.ChosenPoint, 0);
    }

    void SetHoldPoint(GrabHandData grap, Transform Root)
    {
        grap.GameobjectUsed.localEulerAngles = Root.localEulerAngles + new Vector3(0, grap.ChosenPoint, 0);
    }

    public void SetDataValues(ref GrabHandData h1, HandData h2)
    {
        h1._startingFingerRotation = new Quaternion[h1.hand.fingerBones.Length];
        h1._finalFingerRotation = new Quaternion[h2.fingerBones.Length];
        for (int i = 0; i < h1.hand.fingerBones.Length; i++)
        {
            h1._startingFingerRotation[i] = h1.hand.fingerBones[i].localRotation;
            h1._finalFingerRotation[i] = h2.fingerBones[i].localRotation;
        }
    }

    public void SetHandData(GrabHandData h, Quaternion[] newBonesRotation)
    {
        for (int i = 0; i < newBonesRotation.Length; i++)
        {
            h.hand.fingerBones[i].localRotation = newBonesRotation[i];
        }
    }

    public void LateUpdate()
    {
        if (grabHandDatas.Count > 0)
        {
            foreach (var grap in grabHandDatas)
            {
                SetHoldPoint(grap, rootHoldPoint);

                Debug.Log(grap.GameobjectUsed.name);

                if (grap.hand.handType == HandData.HandType.right)
                {
                    grap.hand.transform.SetPositionAndRotation(
                        RightHand.root.position, RightHand.root.rotation);
                }
                else
                {
                    grap.hand.transform.SetPositionAndRotation(
                        LeftHand.root.position, LeftHand.root.rotation);
                }
            }
        }
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Mirror Selected Right Grap Pose")]
    public static void MirrorRightPose()
    {
        var handPose = Selection.activeGameObject.GetComponent<SteerGrapHandPose>();
        handPose.MirrorPose(handPose.LeftHand, handPose.RightHand);
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
