using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Content.Interaction;

public class PlayerDriveInputFromLogitech : MonoBehaviour
{
    public PlayerDriveInputManager _carControl;
    public XRKnob _steerObject;
    /// <summary>
    /// là số đang được điều chỉnh trong hộp số
    /// </summary>
    [Range(-1, 4, order = 0)]
    public int gear;

    void Start()
    {
        _carControl = GetComponentInParent<PlayerDriveInputManager>();

        //Cái này mà không khởi tạo là không chạy được
        print(LogitechGSDK.LogiSteeringInitialize(false));
    }

    void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            LogitechGSDK.DIJOYSTATE2ENGINES rec;
            rec = LogitechGSDK.LogiGetStateUnity(0);

            var steer = LogitechInput.GetAxis("Steering Horizontal");
            var accel = (1 + LogitechInput.GetAxis("Gas Vertical")) / 2;
            var brake = Mathf.Clamp(1 - LogitechInput.GetAxis("Brake Vertical"), 0, 1);

            if (LogitechInput.GetKeyTriggered(0, LogitechKeyCode.Triangle)) gear += 1;
            else
            if (LogitechInput.GetKeyTriggered(0, LogitechKeyCode.Cross)) gear -= 1;

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
            _steerObject.value = (1 - steer) / 2;

            Debug.Log("Góc lái là " + steer);
            Debug.Log("Gas là " + accel);
            Debug.Log("Phanh là " + brake);
        }
    }
}
