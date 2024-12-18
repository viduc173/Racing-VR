using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunControlSystem : MonoBehaviour
{
    public enum GunType
    {
        Auto,
        NonAuto
    }

    [Header("Gun Type")]
    public GunType gunType;

    [Header("Gun Control")]
    public Transform Model;
    public Transform Magizne_Model;

    [Header("Bullet Setting")]
    public Transform bulletSpawnPoint;
    public GameObject bullet;

    [Header("Gun Setting")]
    public float damage;
    public float damage_buf = 0;

    [Tooltip("Bao nhiêu giây thì có thể bắn thêm 1 viên đạn ví dụ speed: 0.1 => 0.1s/viên ")]
    public float speed;
    public float speed_buf = 0;

    [Tooltip("Băng đạn")]
    public float magizne;
    public float magizne_remain;
    public float magizne_buf = 0;

    [Tooltip("Đột giật của súng theo position")]
    public Vector3 recoilPos;
    public Vector3 recoilPos_buf = Vector3.zero;

    [Tooltip("Độ giật của súng theo rotation")]
    public Vector3 recoilRot;
    public Vector3 recoilRot_buf = Vector3.zero;

    public float returnSpeed;
    public float snappiness;

    private Vector3 _recoilPosTarget = Vector3.zero;
    private Vector3 _recoilRotTarget = Vector3.zero;

    private Vector3 _currentRecoilPosTarget = Vector3.zero;
    private Vector3 _currentRecoilRotTarget = Vector3.zero;
    private float attackTimeoutDelta = 0;
    
    private GameObject temp_b = null;
    private GameObject temp_b1 = null;
    
    private HandGunSelectHandPose handPose;
    private HandInputValue handInput;

    private bool _currentAttackState = false;
    private bool attack = false;

    private void Awake()
    {
        handPose = GetComponent<HandGunSelectHandPose>();
        magizne_remain = magizne;
    }
    private void OnDestroy()
    {
        Destroy(temp_b, Random.Range(0.5f, 1));
        Destroy(temp_b1, Random.Range(0.7f, 1));
    }
    private void OnDisable()
    {
        Destroy(temp_b, Random.Range(0.5f, 1));
        Destroy(temp_b1, Random.Range(0.7f, 1));
    }
    public void SetHandInput(HandInputValue handInput)
    {
        this.handInput = handInput;

        //Ghi nhận bấm nút để tháo băng đạn
        handInput.primaryPressEvent.AddListener(DropMagize);
    }
    public void UnSetHandInput()
    {
        //Đặt lại để không tấn công khi súng rời tay
        attack = false;
        _currentAttackState = false;

        //Xóa ghi nhận bấm nút thể tháo băng đạn
        handInput.primaryPressEvent.RemoveListener(DropMagize);

        //Xóa handInput
        handInput = null;
    }    
    public void CheckAttack()
    {
        var _isAttack = (handInput.triggerValue > 0.7);
        if (_currentAttackState != _isAttack)
        {
            _currentAttackState = _isAttack;
            attack = _currentAttackState;
        }
    }
    public void Fire()
    {
        if (temp_b == null)
        {
            temp_b = Instantiate(bullet);
            temp_b1 = Instantiate(bullet);
            temp_b.SetActive(false);
            temp_b1.SetActive(false);
        }

        if (!temp_b.activeSelf)
        {
            temp_b.SetActive(true);
            temp_b.transform.position = bulletSpawnPoint.position;
            temp_b.GetComponent<BulletControl>().SetValue(bulletSpawnPoint.position,bulletSpawnPoint.forward);
        }    
        else
        if (!temp_b1.activeSelf)
        {
            temp_b1.SetActive(true);
            temp_b1.transform.position = bulletSpawnPoint.position;
            temp_b1.GetComponent<BulletControl>().SetValue(bulletSpawnPoint.position, bulletSpawnPoint.forward);
        }

        magizne_remain--;
    }
    public void DropMagize()
    {
        magizne_remain = magizne;
    }    
    public void Recoil()
    {
        _recoilPosTarget += recoilPos + (handPose.GrabHandCount > 1 ? recoilPos_buf : Vector3.zero); 
        _recoilRotTarget += recoilRot + (handPose.GrabHandCount > 1 ? recoilRot_buf : Vector3.zero);

        _recoilPosTarget = Vector3.Lerp(_recoilPosTarget, Vector3.zero, returnSpeed * Time.deltaTime);
        _recoilRotTarget = Vector3.Lerp(_recoilRotTarget, Vector3.zero, returnSpeed * Time.deltaTime);

        _currentRecoilPosTarget = Vector3.Slerp(_currentRecoilPosTarget, _recoilPosTarget, snappiness * Time.deltaTime);
        _currentRecoilRotTarget = Vector3.Slerp(_currentRecoilRotTarget, _recoilPosTarget, snappiness * Time.deltaTime);
    }

    public void Update()
    {
        if (attackTimeoutDelta >= 0) attackTimeoutDelta -= Time.deltaTime;
        //Súng quay về sau khi giật
        if (_recoilPosTarget != Vector3.zero || _recoilRotTarget != Vector3.zero)
        {
            _recoilPosTarget = Vector3.Lerp(_recoilPosTarget, Vector3.zero, returnSpeed * Time.deltaTime);
            _recoilRotTarget = Vector3.Lerp(_recoilRotTarget, Vector3.zero, returnSpeed * Time.deltaTime);

            _currentRecoilPosTarget = Vector3.Slerp(_currentRecoilPosTarget, _recoilPosTarget, snappiness * Time.deltaTime);
            _currentRecoilRotTarget = Vector3.Slerp(_currentRecoilRotTarget, _recoilRotTarget, snappiness * Time.deltaTime);

            Model.localPosition = _currentRecoilPosTarget;
            Model.localEulerAngles = _currentRecoilRotTarget;
        }

        //Bắn súng và thực hiện giật
        if (handInput is not null)
        {
            CheckAttack();
            if (magizne_remain > 0)
            {
                if (gunType == GunType.Auto && attack && attackTimeoutDelta <= 0)
                {
                    attackTimeoutDelta = speed + speed_buf;
                    Fire();
                    Recoil();
                }

                if (gunType == GunType.NonAuto && attack)
                {
                    attack = false;
                    Fire();
                    Recoil();
                }
            }
        }    
    }
}
