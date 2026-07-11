using TMPro;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectorUIController : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] private CustomButton[] levelButtons;

    [SerializeField] private GameObject statusLabel;

    private void Start()
    {
        ConfigureLevelButtons();
        ConfigureStatusLabel();
    }

    private void ConfigureStatusLabel()
    {
        statusLabel.SetActive(!NetworkManager.Singleton.LocalClient.IsSessionOwner);
    }

    private void ConfigureLevelButtons()
    {
        if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) return;
        int maxLevel = GetMaxLevel();

        for(int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].Interactable = i <= maxLevel;
        }
    }

    private int GetMaxLevel()
    {
        if(SessionManager.Instance.CurrentProfile == null)
        {
            return -1;
        }

        if(SessionManager.Instance.CurrentProfile == null)
        {
            return - 1;
        }
        return SessionManager.Instance.CurrentProfile.maxLevel;
    }
    public void GoToLevel(string levelName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
    }
}
