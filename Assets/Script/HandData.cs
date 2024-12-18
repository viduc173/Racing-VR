using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandData : MonoBehaviour
{
    public enum HandType
    {
        left,
        right,
    }

    public Transform root;
    public Animator animator;
    public Transform[] fingerBones;
    public HandType handType;
}
