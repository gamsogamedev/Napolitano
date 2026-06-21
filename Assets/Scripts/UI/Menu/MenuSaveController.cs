using System;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSaveController : MonoBehaviour
{
    [Header("Titulo PlayerName")]
    [SerializeField] private TextMeshProUGUI titlePlayerName;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Buttons")]
    [SerializeField] private Button newPlayerButton;
    [SerializeField] private Button oldPlayerButton;

    [Header("Status text")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Panels")]
    [SerializeField] private GameObject savePanel;
    [SerializeField] private GameObject lobbyPanel;


    private void OnEnable()
    {
        newPlayerButton.onClick.AddListener(OnNewPlayerClicked);
        oldPlayerButton.onClick.AddListener(OnOldPlayerClicked);
    }

    private void OnDisable()
    {
        newPlayerButton.onClick.RemoveListener(OnNewPlayerClicked);
        oldPlayerButton.onClick.RemoveListener(OnOldPlayerClicked);
    }

    private void OnNewPlayerClicked()
    {
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Digite um nome válido";
            return;
        }

        if (PlayerProfileDatabase.ProfileExists(playerName))
        {
            statusText.text = "Este perfil já existe";
            return;
        }

        PlayerProfile profile = new PlayerProfile
        {
            playerName = playerName,
            playerSprite = Player.PlayerSprite.Strawberry,
            maxLevel = 0
        };

        PlayerProfileDatabase.SaveProfile(profile);

        Debug.Log("Perfil criado com sucesso");

        SessionManager.Instance.SetCurrentProfile(profile);

        OpenLobby();
    }

    private void OnOldPlayerClicked()
    {
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "Digite um nome válido";
            return;
        }

        if (!PlayerProfileDatabase.ProfileExists(playerName))
        {
            statusText.text = "Perfil não encontrado";
            return;
        }

        PlayerProfile profile = PlayerProfileDatabase.LoadProfile(playerName);

        Debug.Log("Perfil carregado: " + profile.playerName);

        SessionManager.Instance.SetCurrentProfile(profile);

        OpenLobby();
    }

    private void OpenLobby()
    {
        savePanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }
}