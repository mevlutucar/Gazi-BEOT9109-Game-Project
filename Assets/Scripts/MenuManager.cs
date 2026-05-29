using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
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
}