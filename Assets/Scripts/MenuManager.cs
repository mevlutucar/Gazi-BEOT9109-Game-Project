using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        // Menü sahneleri (Ana Menü, Options, Credits) açýldýðý anda mouse'u görünür ve serbest yap
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Oyun içinden dönüldüðünde zamanýn donuk kalma ihtimaline karþý zamaný normal akýþýna al
        Time.timeScale = 1f;
    }

    public void LoadGameMainLevel()
    {
        SceneManager.LoadScene("GameMainLevel");
    }

    public void LoadOptionsMenu()
    {
        SceneManager.LoadScene("OptionsMenu");
    }

    public void LoadCreditsMenu()
    {
        SceneManager.LoadScene("CreditsMenu");
    }

    public void LoadGameMainMenu()
    {
        SceneManager.LoadScene("GameMainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}