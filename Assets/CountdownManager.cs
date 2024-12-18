using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    public GameObject countdownPanel; // Bảng điện tử chứa số đếm ngược
    public Text countdownText; // Text UI để hiển thị đếm ngược
    public Text shadowText; // Text UI cho hiệu ứng bóng
    public GameObject gameObjectsToStart; // Các đối tượng trong game sẽ bắt đầu sau khi đếm ngược kết thúc

    private void Start()
    {
        gameObjectsToStart.SetActive(false); // Ẩn các đối tượng khi bắt đầu
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        int countdown = 10; // Thời gian đếm ngược

        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();
            shadowText.text = countdown.ToString(); // Cập nhật văn bản bóng

            // Hiệu ứng bóng
            shadowText.color = new Color(0, 0, 0, 0.5f); // Màu đen với độ trong suốt
            shadowText.transform.localPosition = new Vector3(2, -2, 0); // Đặt vị trí bóng

            countdownText.transform.localScale = Vector3.one * 1.5f; // Tăng kích thước văn bản
            countdownText.color = Color.red; // Đổi màu văn bản

            yield return new WaitForSeconds(0.5f); // Chờ 0.5 giây

            countdownText.transform.localScale = Vector3.one; // Quay lại kích thước ban đầu
            countdownText.color = Color.white; // Đổi lại màu văn bản
            yield return new WaitForSeconds(0.5f); // Chờ 0.5 giây

            countdown--; // Giảm đếm ngược
        }

        countdownText.text = "Bắt đầu!"; // Hiển thị thông báo bắt đầu
        shadowText.text = "Bắt đầu!"; // Cập nhật văn bản bóng
        yield return new WaitForSeconds(1); // Chờ thêm 1 giây

        countdownText.gameObject.SetActive(false); // Ẩn số đếm ngược
        shadowText.gameObject.SetActive(false); // Ẩn bóng
        countdownPanel.SetActive(false); // Ẩn bảng điện tử
        gameObjectsToStart.SetActive(true); // Hiện các đối tượng trong game
    }
}
