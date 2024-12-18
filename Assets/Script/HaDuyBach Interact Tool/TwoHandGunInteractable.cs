using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Script chuyên dụng cho các loại súng dùng 2 tay, có thể cầm đa điểm
/// </summary>
public class TwoHandGunInteractable : XRGrabInteractable
{
    HandGunSelectHandPose HandPose;
    protected override void OnEnable()
    {
        base.OnEnable();
        HandPose = GetComponent<HandGunSelectHandPose>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    void StartGrab(SelectEnterEventArgs args)
    {
        args.interactorObject.transform.GetComponent<XRDirectInteractor>().selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Sticky;
    }

    void EndGrab(SelectExitEventArgs args)
    {
        args.interactorObject.transform.GetComponent<XRDirectInteractor>().selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.StateChange;
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        StartGrab(args);

        var s = new SelectEnterEventArgs {interactorObject = args.interactorObject };

        if (HandPose.GrabHandCount > 0)
            HandPose.StartFixGrap(s);
        else
            HandPose.StartGrap(s);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        var s = new SelectExitEventArgs {interactorObject = args.interactorObject };

        if (HandPose.IsBeingFixHand(args.interactorObject))
            HandPose.EndFixGrap(s);
        else
            HandPose.EndGrap(s);

        EndGrab(args);
        base.OnSelectExiting(args);
    }
}
