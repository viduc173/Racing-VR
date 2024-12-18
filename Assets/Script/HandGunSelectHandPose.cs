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

public class HandGunSelectHandPose : MonoBehaviour
{
    #region parameters
    [Header("<i>1) Sử dụng cho việc cầm, nắm, kích hoạt những vật tĩnh với những điểm attach cho trước\n" +
               "2) Chỉ sử dụng đơn điểm chọn </i>")]
    [Space(5)]

    [Header("Hand Data")]
    public HandData RightHand;
    public HandData LeftHand;

    public HandData Active_RightHand;
    public HandData Active_LeftHand;

    public HandData Fix_RightHand;
    public HandData Fix_LeftHand;
    public struct GrabHandData
    {
        public bool isFixPose;
        public bool isUsing;

        public HandData hand;
        public HandData finalHand;

        public int ChosenPoint;

        public Vector3 _finalHandPosition;
        public Quaternion _finalHandRotation;

        public Quaternion[] _startingFingerRotation;
        public Quaternion[] _finalFingerRotation;
    }
    private GrabHandData[] grabHandDatas = new GrabHandData[10];
    private int grabHandDatas_Count = 0;
    private bool _isTwoHand = false;
    private XRBaseInteractable _grap;
    private GunControlSystem _gun;
    private bool _currentAttackState;

    #endregion

    #region Setters & Getters
    /// <summary>
    /// Số lượng tay đang cầm vật phẩm
    /// </summary>
    public int GrabHandCount { get { return grabHandDatas_Count; } }
    #endregion

    public void OnEnable()
    {
        if (TryGetComponent(out _gun))
        {
            _currentAttackState = false;
        }
        else
        {
            Debug.LogError("Please add GunControlSystem!");
        }

        if (TryGetComponent(out _grap))
        {
            _isTwoHand = _grap as TwoHandGunInteractable;
        }
        else
        {
            Debug.LogError("Can't Find XRBaseInteractable in " + this);
        }
    }
    public void StartGrap(SelectEnterEventArgs arg0)
    {
        if (arg0.interactorObject is XRDirectInteractor)
        {
            GrabHandData _grab = new();

            _grab.hand = arg0.interactorObject.transform.GetComponentInChildren<HandData>();
            _grab.hand.animator.enabled = false;
            _grab.isFixPose = false;
            _grab.isUsing = true;

            if (_grab.hand.handType == HandData.HandType.right)
            {
                _grab.finalHand = RightHand;
                SetDataValues(ref _grab, RightHand);
            }
            else
            if (_grab.hand.handType == HandData.HandType.left)
            {
                _grab.finalHand = LeftHand;
                SetDataValues(ref _grab, LeftHand);
            }
            else
            {
                Debug.LogError("Error : Can't recognize Hand Type (Left or Right) Please setup hand.handType");
                return;
            }

            SetHandData(_grab, _grab._finalFingerRotation);
            grabHandDatas[grabHandDatas_Count++] = _grab;

            //Thêm các quy định cho gun khi một tay nào đó cầm vào
            _gun.SetHandInput(_grab.hand.transform.GetComponent<HandInputValue>());
        }
    }
    public void EndGrap(SelectExitEventArgs arg0)
    {
        if (arg0.interactorObject is XRDirectInteractor && grabHandDatas_Count > 0)
        {
            int id = 0;
            while (id < grabHandDatas_Count
                && grabHandDatas[id].hand != arg0.interactorObject.transform.GetComponentInChildren<HandData>())
            {
                if (id == grabHandDatas_Count) return;
                id++;
            }

            //Xóa các quy định cho gun khi một tay nào đó cầm vào
            _gun.UnSetHandInput();

            grabHandDatas[id].hand.animator.enabled = true;
            SetHandData(grabHandDatas[id], grabHandDatas[id]._startingFingerRotation);
            grabHandDatas[id].hand.root.localPosition = Vector3.zero;
            grabHandDatas[id].hand.root.localRotation =
                Quaternion.Euler(0, 0, grabHandDatas[id].hand.handType == HandData.HandType.left ? 90 : -90);

            //Mark for deletion
            grabHandDatas[id].isUsing = false;

            if (grabHandDatas_Count > 1)
            {
                if (_isTwoHand)
                    ChangeHandHold();
                else
                    RemoveAllHand();
            }
        }
    }
    public void StartFixGrap(SelectEnterEventArgs arg0)
    {
        if (arg0.interactorObject is XRDirectInteractor)
        {
            GrabHandData _grab = new();

            _grab.hand = arg0.interactorObject.transform.GetComponentInChildren<HandData>();
            _grab.hand.animator.enabled = false;
            _grab.isFixPose = true;
            _grab.isUsing = true;

            if (_grab.hand.handType == HandData.HandType.right)
            {
                _grab.finalHand = Fix_RightHand;
                SetDataValues(ref _grab, Fix_RightHand);
            }
            else
            if (_grab.hand.handType == HandData.HandType.left)
            {
                _grab.finalHand = Fix_LeftHand;
                SetDataValues(ref _grab, Fix_LeftHand);
            }
            else
            {
                Debug.LogError("Error : Can't recognize Hand Type (Left or Right) Please setup hand.handType");
                return;
            }

            SetHandData(_grab, _grab._finalFingerRotation);

            grabHandDatas[grabHandDatas_Count++] = _grab;
        }
    }
    public void EndFixGrap(SelectExitEventArgs arg0)
    {
        if (arg0.interactorObject is XRDirectInteractor && grabHandDatas_Count > 0)
        {
            int id = 0;
            while (id < grabHandDatas_Count
                && grabHandDatas[id].hand != arg0.interactorObject.transform.GetComponentInChildren<HandData>())
            {
                if (id == grabHandDatas_Count) return;
                id++;
            }

            grabHandDatas[id].hand.animator.enabled = true;
            SetHandData(grabHandDatas[id], grabHandDatas[id]._startingFingerRotation);
            grabHandDatas[id].hand.root.localPosition = Vector3.zero;
            grabHandDatas[id].hand.root.localRotation =
                Quaternion.Euler(0, 0, grabHandDatas[id].hand.handType == HandData.HandType.left ? 90 : -90);

            //Mark for deletion
            grabHandDatas[id].isUsing = false;
        }
    }
    private void SetDataValues(ref GrabHandData h1, HandData h2)
    {
        h1._startingFingerRotation = new Quaternion[h1.hand.fingerBones.Length];
        h1._finalFingerRotation = new Quaternion[h2.fingerBones.Length];
        for (int i = 0; i < h1.hand.fingerBones.Length; i++)
        {
            h1._startingFingerRotation[i] = h1.hand.fingerBones[i].localRotation;
            h1._finalFingerRotation[i] = h2.fingerBones[i].localRotation;
        }
    }
    private void SetHandData(GrabHandData h, Quaternion[] newBonesRotation)
    {
        for (int i = 0; i < newBonesRotation.Length; i++)
        {
            h.hand.fingerBones[i].localRotation = newBonesRotation[i];
        }
    }
    private void UpdateHandPose()
    {
        if (grabHandDatas_Count > 0)
        {
            Debug.Log(grabHandDatas_Count);

            int _currentCount = 0;
            for (int id = 0; id < grabHandDatas_Count; id++)
            {
                var grab = grabHandDatas[id];
                //Check if it's fix hand and fix hand is still hold
                if (grab.isFixPose && !_isTwoHand && 
                    grab.hand.transform.GetComponent<HandInputValue>().selectValue == 0)
                {
                    var s = new SelectExitEventArgs
                    {
                        interactorObject = grab.hand.transform.parent.GetComponent<IXRSelectInteractor>()
                    };
                    EndFixGrap(s);
                }

                if (grab.isUsing)
                {
                    grab.hand.transform.SetPositionAndRotation
                                        (grab.finalHand.root.position, grab.finalHand.root.rotation);
                    if (id != _currentCount) grabHandDatas[_currentCount] = grabHandDatas[id];
                    _currentCount++;
                }
            }
            grabHandDatas_Count = _currentCount;
        }
    }

    /// <summary>
    /// Thay đổi tay cầm chính
    /// </summary>
    public void ChangeHandHold()
    {
        for (int i = 0; i < grabHandDatas_Count; i++)
        {
            if (grabHandDatas[i].isUsing)
            {
                var s1 = new SelectExitEventArgs();
                s1.interactorObject = grabHandDatas[i].hand.transform.parent.GetComponent<IXRSelectInteractor>();
                EndFixGrap(s1);

                var s2 = new SelectEnterEventArgs();
                s2.interactorObject = s1.interactorObject;
                StartGrap(s2);

                break;
            }
        }
    }
    /// <summary>
    /// Bỏ tay ra khỏi vũ khí hoàn toàn
    /// </summary>
    public void RemoveAllHand()
    {
        for (int i = 0; i < grabHandDatas_Count; i++)
        {
            var s = new SelectExitEventArgs();
            s.interactorObject = grabHandDatas[i].hand.transform.parent.GetComponent<IXRSelectInteractor>();
            EndFixGrap(s);
        }
    }
    /// <summary>
    /// Có phải tay đang giữ cố định súng hay không
    /// </summary>
    /// <returns></returns>
    public bool IsBeingFixHand(IXRSelectInteractor interactor)
    {
        for (int i = 0; i < grabHandDatas_Count; i++)
        {
            if (grabHandDatas[i].isUsing && grabHandDatas[i].hand.transform.Equals(interactor.transform)) return true;
        }
        return false;
    }
    /// <summary>
    /// Không có tay nào đang cầm vật phẩm?
    /// </summary>
    public void LateUpdate()
    {
        UpdateHandPose();
    }
}
