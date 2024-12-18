using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Content.Interaction;
using static SteerGrapHandPose;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SingleSelectObjectGrabHandPose : MonoBehaviour
{
    [Header("<i>1) Sử dụng cho việc cầm, nắm, kích hoạt những vật tĩnh với những điểm attach cho trước\n" +
               "2) Chỉ sử dụng đơn điểm chọn </i>")]
    [Space(5)]

    [Header("Hand Data")]
    public Transform GameojectUsed_RightHand;
    public Transform GameojectUsed_LefttHand;

    public HandData RightHand;
    public HandData LeftHand;

    public HandData Active_RightHand;
    public HandData Active_LeftHand;
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

            if (grap.selectMode == InteractableSelectMode.Multiple)
            {
                Debug.LogError("you are using this script " + this + "is single , hãy thay thế bằng script khác nhận đa điểm");
            } 
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
            foreach (var grab in grabHandDatas)
            {
                //Debug.Log(grab.GameobjectUsed.name);

                if (grab.hand.handType == HandData.HandType.right)
                {
                    grab.hand.transform.SetPositionAndRotation(
                        RightHand.root.position, RightHand.root.rotation);
                }
                else
                {
                    grab.hand.transform.SetPositionAndRotation(
                        LeftHand.root.position, LeftHand.root.rotation);
                }
            }
        }
    }
}
