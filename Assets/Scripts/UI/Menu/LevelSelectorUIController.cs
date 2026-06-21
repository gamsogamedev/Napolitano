using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectorUIController : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] private Button[] levelButtons;

    [SerializeField] private TextMeshProUGUI statusLabel;

    private void Start()
    {
        ConfigureLevelButtons();
        ConfigureStatusLabel();
    }

    private void ConfigureStatusLabel()
    {
        bool isSessionOwner = NetworkManager.Singleton.LocalClient.IsSessionOwner;

        if (!isSessionOwner)
        {
            statusLabel.text = "Aguardando dono da sessão escolher uma fase...";
        }
    }

    private void ConfigureLevelButtons()
    {
        int maxLevel = GetMaxLevel();

        for(int i = 0; i < levelButtons.Length; i++)
        {
            bool unlocked = i <= maxLevel;
            levelButtons[i].interactable = unlocked;
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
