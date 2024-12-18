using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EVP;

public class PlayerDriveInputManager : MonoBehaviour
{
    private float _moveInput;
    private float _brakeInput;
    private float _steerInput;

    public float MoveInput { get { return _moveInput; } set { _moveInput = value; } }
    public float BrakeInput { get { return _brakeInput; } set { _brakeInput = value; } }
    public float SteerInput { get { return _steerInput; } set { _steerInput = value; } }

    public VehicleController controller;
    public CarControl controller_1;
    public Rigidbody Rb;

    [Header("Đèn xe xi nhan")]
    public List<GameObject> Left_Sign;
    public List<GameObject> Right_Sign;
    private bool left_sign = false;
    private bool right_sign = false;
    private bool signOn = false;
    private float sign_timeout = 0.4f;
    private float sign_timeout_delta = 0;

    private void Start()
    {
        controller = GetComponentInParent<VehicleController>();
        Rb = controller.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (left_sign)
        {
            Debug.Log(sign_timeout);
            sign_timeout_delta -= Time.deltaTime;
            if (sign_timeout_delta < 0)
            {
                foreach (var beam in Left_Sign)
                {
                    if (signOn) beam.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    else
                        beam.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                }
                signOn = !signOn;
                sign_timeout_delta = sign_timeout;
            }
        }
        else
        if (right_sign)
        {
            Debug.Log("Thoi gian la " + sign_timeout_delta);
            sign_timeout_delta -= Time.deltaTime;
            if (sign_timeout_delta < 0)
            {
                foreach (var beam in Right_Sign)
                {
                    if (signOn) beam.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    else
                        beam.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                }
                signOn = !signOn;
                sign_timeout_delta = sign_timeout;
            }
        }
    }

    public void SetValue(float Steer, float Move, float Brake, int gear) => SetValue(Steer, Move, Brake, 0, gear);
    public void SetValue(float Steer, float Move, float Brake, float HandBrake, int gear)
    {
        controller.steerInput = Steer;
        controller.throttleInput = Move;
        controller.brakeInput = Brake;
        controller.handbrakeInput = HandBrake;
    }

    public void TurnLeftSign()
    {
        if (left_sign)
            TurnOffLeftSign();
        else
            TurnOnLeftSign();
    }
    public void TurnRightSign()
    {
        if (right_sign)
            TurnOffRightSign();
        else
            TurnOnRightSign();
    }
    private void TurnOnLeftSign()
    {
        TurnOffRightSign();
        left_sign = true;
    }
    private void TurnOffLeftSign()
    {
        left_sign = false;
        foreach (var beam in Left_Sign)
        {
            beam.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        }
    }
    private void TurnOnRightSign()
    {
        TurnOffLeftSign();
        right_sign = true;
    }
    private void TurnOffRightSign()
    {
        right_sign = false;
        foreach (var beam in Right_Sign)
        {
            beam.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        }
    }
}
