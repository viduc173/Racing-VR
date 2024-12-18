using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : MonoBehaviour
{
    public float respawnTime = 3f; // Thời gian hồi lại
    private Vector3 originalPosition; // Vị trí ban đầu của capsule
    private Quaternion originalRotation; // Hướng ban đầu của capsule

    public Material capsuleMaterial; // Material của capsule
    public float rotationSpeed = 50f; // Tốc độ xoay
    public float emissionIntensity = 1f; // Độ phát sáng
    private Color originalColor; // Màu gốc của material

    void Start()
    {
        // Lưu vị trí và hướng ban đầu
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalColor = capsuleMaterial.GetColor("_EmissionColor");
    }

    void Update()
    {
        // Xoay capsule theo trục z
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // Nhấp nháy phát sáng
        float emission = Mathf.PingPong(Time.time * emissionIntensity, 1f);
        capsuleMaterial.SetColor("_EmissionColor", originalColor * emission);

        // Tạo hiệu ứng phát sáng xung quanh
        float alpha = Mathf.PingPong(Time.time * 0.5f, 1f); // Điều chỉnh độ nhấp nháy
        Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        capsuleMaterial.SetColor("_Color", newColor);
    }

    public void DisableCapsule()
    {
        gameObject.SetActive(false); // Ẩn capsule
        Invoke("RespawnCapsule", respawnTime); // Gọi phương thức hồi lại sau thời gian
    }

    void RespawnCapsule()
    {
        transform.position = originalPosition; // Đặt lại vị trí
        transform.rotation = originalRotation; // Đặt lại hướng
        gameObject.SetActive(true); // Hiện capsule
    }
}