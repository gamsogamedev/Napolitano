using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void GoToLobby()
    {
        SceneManager.LoadScene("SaveAndLobbyMultiplayer");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
