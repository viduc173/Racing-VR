using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Script chuyên dụng cho súng lục.
/// </summary>
public class HandGunInteractable : XRGrabInteractable
{
    #region General variable
    List<IXRSelectInteractor> m_Interactor = new();

    #endregion
    protected override void OnEnable()
    {
        base.OnEnable();
        selectEntered.AddListener(StartGrab);
        selectExited.AddListener(EndGrab);
    }

    protected override void OnDisable()
    {
        selectEntered.RemoveListener(StartGrab);
        selectExited.RemoveListener(EndGrab);
        base.OnDisable();
    }

    void StartGrab(SelectEnterEventArgs args)
    {
        args.interactorObject.transform.GetComponent<XRDirectInteractor>().selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
        GetComponent<HandGunSelectHandPose>().StartGrap(args);
    }

    void EndGrab(SelectExitEventArgs args)
    {
        args.interactorObject.transform.GetComponent<XRDirectInteractor>().selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.StateChange;
        GetComponent<HandGunSelectHandPose>().EndGrap(args);
    }

    // Use this instead of XR Grab Interactable to prevent hands fighting over objects.
    // Fighting hands can cause hand animations to break, and causes rapid-fire grab sounds.
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        bool isAlreadyGrabbed = false;

        if (isSelected && !interactor.Equals(firstInteractorSelecting))
        {
            var grabber = firstInteractorSelecting as XRDirectInteractor;
            if (grabber != null)
            {
                isAlreadyGrabbed = true;
                var s = new SelectEnterEventArgs();
                s.interactorObject = interactor;
                GetComponent<HandGunSelectHandPose>().StartFixGrap(s);
            }
        }

        return base.IsSelectableBy(interactor) && !isAlreadyGrabbed;
    }

}
