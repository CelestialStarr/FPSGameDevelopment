using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        // 加载第一关（改成你真实的第一关名字）
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
        // 在编辑器中无效，但打包后有效
        Debug.Log("Quit game.");
        Application.Quit();
    }
}
