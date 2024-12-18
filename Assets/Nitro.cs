using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nitro : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    [SerializeField] private float boostForce = 5000f;  
    [SerializeField] private float boostDuration = 2f;  
    
    // Thêm các biến cho animation xoay  
    [SerializeField] private float rotationSpeed = 100f; // Tốc độ xoay (độ/giây)  
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Trục xoay (mặc định là trục Y)  

    private void Update()  
    {  
        // Xoay capsule liên tục  
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);  
    }  

    private void OnTriggerEnter(Collider other)  
    {  
        if (other.CompareTag("Player"))  
        {  
            Rigidbody carRigidbody = other.GetComponent<Rigidbody>();  
            
            if (carRigidbody != null)  
            {  
                carRigidbody.AddForce(other.transform.forward * boostForce, ForceMode.Impulse);  
                Destroy(gameObject);  
            }  
        }  
    }  
    
}
