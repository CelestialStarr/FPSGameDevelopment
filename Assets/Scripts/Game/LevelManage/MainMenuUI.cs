using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        // ���ص�һ�أ��ĳ�����ʵ�ĵ�һ�����֣�
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
        // �ڱ༭������Ч�����������Ч
        Debug.Log("Quit game.");
        Application.Quit();
    }
}
