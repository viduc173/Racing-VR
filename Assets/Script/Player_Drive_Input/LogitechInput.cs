using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogitechInput : MonoBehaviour
{
    static LogitechGSDK.DIJOYSTATE2ENGINES rec;

    public static float GetAxis(string axisName)
    {
    #region 
        rec = LogitechGSDK.LogiGetStateUnity(0);
        switch (axisName)
        {
            case "Steering Horizontal": return rec.lX / 32760f;
            case "Gas Vertical" : return rec.lY/ -32760f;
            case "Clutch Vertical" : return rec.rglSlider[0] / -32760f;
            case "Brake Vertical" : return rec.lRz/ 32760f;
        }
        return 0f;
    }
    public static bool GetKeyTriggered(LogitechKeyCode gamecontroller, LogitechKeyCode keyCode)
    {
        if(LogitechGSDK.LogiButtonTriggered((int)gamecontroller, (int)keyCode))
        {
            return true;    
        }
        return false;
    }
    public static bool GetKeyPressed(LogitechKeyCode gamecontroller, LogitechKeyCode keyCode)
    {
        if(LogitechGSDK.LogiButtonIsPressed((int)gamecontroller, (int)keyCode))
        {
            return true;
        }
        return false;
    }
    public static bool GetKeyReleased(LogitechKeyCode gamecontroller, LogitechKeyCode keyCode)
    {
        if(LogitechGSDK.LogiButtonReleased((int)gamecontroller, (int)keyCode))
        {
            return true;
        }
        return false;
    }
    #endregion
}
