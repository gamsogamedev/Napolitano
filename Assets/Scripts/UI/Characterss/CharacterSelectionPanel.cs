using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionPanel : MonoBehaviour
{
    [Header("Player Name UI")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Header("Character UI")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Image characterImage;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button selectButton;


    //Eventos
    public event Action OnPreviusPressed;
    public event Action OnNextPressed;
    public event Action OnSelectPressed;



    private void OnEnable()
    {
        prevButton.onClick.AddListener(OnPreviousClicked);
        nextButton.onClick.AddListener(OnNextClicked);
        selectButton.onClick.AddListener(OnSelectClicked);
    }

    private void OnDisable()
    {
        prevButton.onClick.RemoveListener(OnPreviousClicked);
        nextButton.onClick.RemoveListener(OnNextClicked);
        selectButton.onClick.RemoveListener(OnSelectClicked);
    }

    private void OnPreviousClicked()
    {
        OnPreviusPressed?.Invoke();
    }

    private void OnNextClicked()
    {
        OnNextPressed?.Invoke();
    }

    private void OnSelectClicked()
    {
        OnSelectPressed?.Invoke();
    }

    public void SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void SetCharacter(Character character)
    {
        characterNameText.text = character.characterName;
        characterImage.sprite = character.characterSkin;
    }

    public void SetInteractable(bool interactable)
    {
        prevButton.interactable = interactable;
        nextButton.interactable = interactable;
        selectButton.interactable = interactable;
    }
}
