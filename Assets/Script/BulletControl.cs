using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour
{
    private Vector3 org;
    public void SetValue(Vector3 org, Vector3 forward)
    {
        this.org = org;
        transform.forward = forward;
    }    

    void Update()
    {
        transform.position += transform.forward * Random.Range(200f,220f) * Time.deltaTime;
        if ((transform.position - org).magnitude > 30.0f) gameObject.SetActive(false);
    }
}
