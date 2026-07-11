using System;
using TMPro;
using UI;
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
    [SerializeField] private CustomButton nextButton;
    [SerializeField] private CustomButton prevButton;
    [SerializeField] private CustomButton selectButton;


    //Eventos
    public event Action OnPreviusPressed;
    public event Action OnNextPressed;
    public event Action OnSelectPressed;

    public void OnPreviousClicked()
    {
        OnPreviusPressed?.Invoke();
    }

    public void OnNextClicked()
    {
        OnNextPressed?.Invoke();
    }

    public void OnSelectClicked()
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
        prevButton.Interactable = interactable;
        nextButton.Interactable = interactable;
        selectButton.Interactable = interactable;
    }
}
