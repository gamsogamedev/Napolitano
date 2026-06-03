using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectorUIController : MonoBehaviour
{
    public void GoToLevel(string levelName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
    }
}
