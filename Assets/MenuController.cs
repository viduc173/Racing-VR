using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        // Đảm bảo tên scene trong LoadScene phải giống với tên file scene của bạn
        SceneManager.LoadScene("Simple"); // Thay "Simple" bằng tên chính xác của scene game
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
