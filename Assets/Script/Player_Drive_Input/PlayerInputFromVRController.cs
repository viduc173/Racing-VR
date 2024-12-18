using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerInputFromVRController : MonoBehaviour
{
    public InputActionProperty LeftHand_Move;
    public InputActionProperty RightHand_Move;
    public PlayerDriveInputManager _carControl;
    [Range(-1, 4, order = 0)]
    public int gear;

    private float accel = 0;
    private float brake = 0;
    private float steer = 0;
    private float _swithGearTimeout = -1;

    void Start()
    {
        _carControl = GetComponentInParent<PlayerDriveInputManager>();
    }

    public void HandleSteer(float value)
    {
        steer = -(value - 0.5f) / 0.5f;
    }

    void Update()
    {
        if (_swithGearTimeout >= 0) _swithGearTimeout -= Time.deltaTime;
        brake = 0;
        accel = 0;

        accel = LeftHand_Move.action.ReadValue<Vector2>().y;

        if (accel < 0)
        {
            brake = Mathf.Abs(accel);
            accel = 0;
        }

        if (RightHand_Move.action.ReadValue<Vector2>().y > 0 && _swithGearTimeout < 0)
        {
            gear += 1;
            _swithGearTimeout = 0.2f;
        }
        else
        if (RightHand_Move.action.ReadValue<Vector2>().y < 0 && _swithGearTimeout < 0)
        {
            gear -= 1;
            _swithGearTimeout = 0.2f;
        }

        gear = Mathf.Clamp(gear, -1, 4);

        switch (gear)
        {
            case -1: accel = Mathf.Clamp(accel / -1.5f, -1f, -0.2f); break;
            case 0: accel = 0; brake = 1; break;
            case 2: accel /= 1.25f; break;
            case 3: accel /= 1.5f; break;
            case 4: accel /= 1.75f; break;
        }

        if (gear > 0 || accel > 0) accel = Mathf.Clamp(accel, 0.2f, 1);

        _carControl.SetValue(steer, accel, brake, gear);
    }
}
