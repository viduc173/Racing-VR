using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingGlow : MonoBehaviour
{
    public Material glowMaterial; // Vật liệu phát sáng
    public float flashSpeed = 2.0f; // Tốc độ nhấp nháy
    private Color baseColor; // Màu cơ bản
    private Color emissionColor; // Màu phát sáng

    void Start()
    {
        // Lưu màu cơ bản và màu phát sáng
        baseColor = glowMaterial.color;
        emissionColor = glowMaterial.GetColor("_EmissionColor");
    }

    void Update()
    {
        // Tính toán độ sáng theo thời gian
        float intensity = Mathf.PingPong(Time.time * flashSpeed, 1.0f);
        glowMaterial.SetColor("_EmissionColor", emissionColor * intensity);
    }
}
