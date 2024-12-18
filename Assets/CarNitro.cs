using System.Collections;
using System.Collections.Generic;
using UnityEngine;      
using UnityEngine.UI; // Để sử dụng UI  
using System.Diagnostics; // Để sử dụng Stopwatch  

public class CarControllerAI : MonoBehaviour  
{  
    public float nitroForce = 500f; // Lực nitro  
    private Rigidbody rb;  
    private AudioSource audioSource; // Biến AudioSource  
    private Stopwatch stopwatch; // Để đo thời gian  
    public Text timeText; // Text UI để hiển thị thời gian  
    private bool isRacing = true; // Biến trạng thái để theo dõi chế độ đua  
    private Vector3 startPosition; // Vị trí xuất phát  
    private bool isNitroActive = false; // Biến trạng thái nitro  
    private float nitroDuration = 4f; // Thời gian nitro tác dụng  
    private float nitroEndTime; // Thời gian kết thúc nitro  
    void Start()  
    {  
        rb = GetComponent<Rigidbody>(); // Lấy Rigidbody của xe  
        audioSource = GetComponent<AudioSource>(); // Lấy AudioSource của xe  
        stopwatch = new Stopwatch(); // Khởi tạo Stopwatch  
        stopwatch.Start(); // Bắt đầu đo thời gian  
        startPosition = transform.position; // Lưu vị trí xuất phát  
    }  
    void Update()  
    {  
        if (isRacing)  
        {  
            // Kiểm tra xem nitro có đang hoạt động không  
            if (isNitroActive && Time.time > nitroEndTime)  
            {  
                DeactivateNitro(); // Tắt nitro khi hết thời gian  
            }  
        }  
    }  


    // void OnCollisionEnter(Collision collision)  
    // {  
    //     if (collision.gameObject.CompareTag("Capsule")) // Kiểm tra va chạm với capsule  
    //     {  
    //         ApplyNitro(); // Gọi phương thức áp dụng nitro  
    //         PlayCollisionSound(); // Gọi phương thức phát âm thanh va chạm  

    //         // Gọi phương thức DisableCapsule từ script CapsuleController  
    //         CapsuleController capsuleController = collision.gameObject.GetComponent<CapsuleController>();  
    //         if (capsuleController != null)  
    //         {  
    //             capsuleController.DisableCapsule(); // Ẩn capsule  
    //         }  
    //     }  
    // }  

    void OnTriggerEnter(Collider other)  
    {  
        if (other.CompareTag("FinishLine")) // Kiểm tra va chạm với vạch đích  
        {  
            stopwatch.Stop(); // Dừng đồng hồ  
            float timeTaken = (float)stopwatch.Elapsed.TotalSeconds; // Lấy thời gian đã chạy  
            UnityEngine.Debug.Log("Thời gian hoàn thành vòng: " + timeTaken + " giây"); // In ra thời gian  

            // Hiển thị thời gian lên UI  
            if (timeText != null)  
            {  
                timeText.text = "Thời gian hoàn thành: " + timeTaken.ToString("F2") + " giây";  
            }  
            
            // Đặt trạng thái là không đua 
            isRacing = false;  

            // Di chuyển xe về vị trí xuất phát  
            transform.position = startPosition; // Quay lại vị trí xuất phát  

            rb.velocity = Vector3.zero; // Đặt tốc độ về 0  
            rb.angularVelocity = Vector3.zero; // Đặt vận tốc góc về 0  

            // Bắt đầu lại đồng hồ nếu cần  
            stopwatch.Reset();  
            stopwatch.Start();  
        }
        if (other.CompareTag("Capsule")) // Kiểm tra va chạm với capsule  
        {  
            ApplyNitro(); // Gọi phương thức áp dụng nitro  
            PlayCollisionSound(); // Gọi phương thức phát âm thanh va chạm  

            // Gọi phương thức DisableCapsule từ script CapsuleController  
            CapsuleController capsuleController = other.gameObject.GetComponent<CapsuleController>();  
            if (capsuleController != null)  
            {  
                capsuleController.DisableCapsule(); // Ẩn capsule  
            }  
        }    
    }  

    void ApplyNitro()  
    {  
        // Lấy hướng di chuyển hiện tại của xe  
        Vector3 forwardDirection = transform.forward; // Hướng đi của xe  
        nitroEndTime = Time.time + nitroDuration; // Đặt thời gian kết thúc nitro  
        rb.AddForce(forwardDirection * nitroForce, ForceMode.Acceleration); // Áp dụng lực nitro theo hướng xe  
    }  
    void DeactivateNitro()  
    {  
        isNitroActive = false; // Tắt nitro  
        // Bạn có thể thêm logic khác ở đây nếu cần  
    }  
    void PlayCollisionSound()  
    {  
        if (audioSource != null)  
        {  
            audioSource.Play(); // Phát âm thanh va chạm  
        }  
    }  

}