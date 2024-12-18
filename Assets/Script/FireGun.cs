using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FireGun : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnPoint;
    public float fireSpeed = 100;
    void Start()
    {
        XRGrabInteractable grabbable = GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(FireBullet);
    }

    private void FireBullet(ActivateEventArgs arg0)
    {
        GameObject spawnBullet = Instantiate(bullet);
        spawnBullet.transform.position = spawnPoint.position;
        spawnBullet.transform.rotation = spawnPoint.rotation;
        spawnBullet.GetComponent<Rigidbody>().velocity = transform.forward * fireSpeed;
        Destroy(spawnBullet, 3.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(spawnPoint.position, spawnPoint.position + transform.forward * 100, Color.yellow);
    }
}
