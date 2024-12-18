using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderSpace : MonoBehaviour
{
    public float LeftX;
    public float RightX;
    public float UpY;
    public float DownY;
    public float ForwardZ;
    public float BackWardZ;
    public bool DrawBorder = false;
    private Transform Origin;
    void Awake()
    {
        Origin = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        //Draw Line
        if (DrawBorder)
        {
            Debug.DrawLine(Origin.position, Origin.position + Origin.right * -LeftX);
            Debug.DrawLine(Origin.position, Origin.position + Origin.right * RightX);

            Debug.DrawLine(Origin.position, Origin.position + Origin.up * UpY);
            Debug.DrawLine(Origin.position, Origin.position + Origin.up * -DownY);

            Debug.DrawLine(Origin.position, Origin.position + Origin.forward * ForwardZ);
            Debug.DrawLine(Origin.position, Origin.position + Origin.up * -BackWardZ);
        }

        transform.localPosition =
            new Vector3(Mathf.Clamp(transform.localPosition.x, -LeftX, RightX),
                            Mathf.Clamp(transform.localPosition.y, -DownY, UpY),
                                Mathf.Clamp(transform.localPosition.z, -BackWardZ, ForwardZ));
    }
}
