using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimated : MonoBehaviour
{
    private Animator handAnimator;
    private HandInputValue handInput;    
    void Start()
    {
        handAnimator = GetComponent<Animator>();
        handInput = GetComponent<HandInputValue>();

        if (handAnimator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }

        if (handInput == null)
        {
            Debug.LogError("HandInputValue component not found on " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        handAnimator.SetFloat("Trigger", handInput.triggerValue);
        handAnimator.SetFloat("Grip", handInput.gridValue);
    }
}


