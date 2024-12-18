using System.Collections;
using System.Collections.Generic; // Để sử dụng List
using UnityEngine;
using UnityEngine.UI;

public class FinishLine : MonoBehaviour
{
    public GameObject leaderboardUI; // UI bảng xếp hạng
    public Text leaderboardText; // Text hiển thị bảng xếp hạng

    private bool isPlayerFinished = false;
    private List<string> playerNames = new List<string>(); // Danh sách tên người chơi
    private List<int> playerScores = new List<int>(); // Danh sách điểm số

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerFinished)
        {
            isPlayerFinished = true;
            string playerName = other.gameObject.name; // Lấy tên người chơi từ GameObject
            int playerScore = 100; // Điểm số có thể được tính toán theo cách khác

            // Thêm tên và điểm vào danh sách
            playerNames.Add(playerName);
            playerScores.Add(playerScore);

            DisplayLeaderboard();
        }
    }

    private void DisplayLeaderboard()
    {
        leaderboardUI.SetActive(true); // Hiển thị bảng xếp hạng

        // Cập nhật nội dung của leaderboardText
        leaderboardText.text = "Bảng Xếp Hạng:\n";
        for (int i = 0; i < playerNames.Count; i++)
        {
            leaderboardText.text += $"{i + 1}. {playerNames[i]} - {playerScores[i]} điểm\n";
        }
    }
}
